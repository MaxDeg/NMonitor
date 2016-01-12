using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace NMonitor.WPF.ViewModels
{
    public class ChartsViewModel : ReactiveObject, IDisposable
    {
        private static readonly TimeSpan TimelineInterval = TimeSpan.FromSeconds(5);
        private static readonly int TimelineSize = (int)(TimeSpan.FromMinutes(10).Ticks / TimelineInterval.Ticks);

        private readonly ReactiveList<string> timeline;
        private readonly List<IDisposable> subscriptions;
        private readonly List<Action<IList<LogEntry>>> chartDefinitions;
        private IObservable<IList<LogEntry>> bufferedSource;

        public ChartsViewModel()
        {
            this.subscriptions = new List<IDisposable>();
            this.chartDefinitions = new List<Action<IList<LogEntry>>>();
            this.timeline = this.CreateLimitedSizeList<string>(TimelineSize);
            timeline.AddRange(Enumerable.Range(0, TimelineSize).Select(i => i.ToString()));

            this.Charts = new ReactiveList<ReactiveList<Tuple<string, ReactiveList<double>>>>();
        }

        public ReactiveList<string> Timeline
        {
            get { return this.timeline; }
        }

        public ReactiveList<ReactiveList<Tuple<string, ReactiveList<double>>>> Charts { get; private set; }

        public void SetSource(IObservable<LogEntry> source)
        {
            this.DisposeSubscriptions();

            this.bufferedSource = source.Buffer(TimelineInterval).ObserveOnDispatcher();
            this.bufferedSource.Subscribe(d => this.timeline.Add(DateTime.Now.Ticks.ToString("hh:mm:ss")));

            foreach (var definition in this.chartDefinitions)
                this.subscriptions.Add(this.bufferedSource.Subscribe(definition));
        }

        public void AddChart(Func<LogEntry, string> keySelector, Func<double, LogEntry, double> aggregator, Func<LogEntry, bool> filter)
        {
            var series = new ReactiveList<Tuple<string, ReactiveList<double>>>();
            Action<IList<LogEntry>> chartDefinition = logs =>
            {
                var lookup = logs.Where(filter).ToLookup(keySelector);
                foreach (var entries in lookup)
                {
                    var list = series.FirstOrDefault(s => string.CompareOrdinal(s.Item1, entries.Key) == 0);
                    if (list == null)
                    {
                        list = Tuple.Create(entries.Key, this.CreateLimitedSizeList<double>(TimelineSize));
                        series.Add(list);
                    }

                    list.Item2.Add(entries.Aggregate(0.0, aggregator));
                }
            };
            this.chartDefinitions.Add(chartDefinition);

            if (this.bufferedSource != null)
                this.subscriptions.Add(this.bufferedSource.Subscribe(chartDefinition));

            this.Charts.Add(series);
        }

        public void Dispose()
        {
            this.DisposeSubscriptions();
        }

        private ReactiveList<TValue> CreateLimitedSizeList<TValue>(int size)
        {
            var list = new ReactiveList<TValue>();
            list.CountChanged.Where(i => i > size).Subscribe(i => list.RemoveRange(size - 1, i - size));

            return list;
        }
        
        private void DisposeSubscriptions()
        {
            foreach (var subscription in this.subscriptions)
                subscription.Dispose();

            this.subscriptions.Clear();
        }
    }
}
