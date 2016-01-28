/******************************************************************************
    Copyright 2016 Maxime Degallaix

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
******************************************************************************/

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
        private static readonly TimeSpan TimelineInterval = TimeSpan.FromSeconds(2);
        private static readonly int TimelineSize = (int)(TimeSpan.FromMinutes(1).Ticks / TimelineInterval.Ticks);

        private readonly List<IDisposable> subscriptions;
        private readonly List<Action<IList<LogEntry>>> chartDefinitions;
        private ReactiveList<string> timeline;
        private IObservable<IList<LogEntry>> bufferedSource;

        public ChartsViewModel()
        {
            this.subscriptions = new List<IDisposable>();
            this.chartDefinitions = new List<Action<IList<LogEntry>>>();
            this.Timeline = this.CreateLimitedSizeList<string>(TimelineSize);
            this.Charts = new ReactiveList<ReactiveList<Tuple<string, ReactiveList<int>>>>();
        }

        public ReactiveList<string> Timeline
        {
            get { return this.timeline; }
            set { this.RaiseAndSetIfChanged(ref this.timeline, value); }
        }

        public ReactiveList<ReactiveList<Tuple<string, ReactiveList<int>>>> Charts { get; private set; }

        public void SetSource(IObservable<LogEntry> source)
        {
            this.DisposeSubscriptions();

            this.bufferedSource = source.Buffer(TimelineInterval).ObserveOnDispatcher();
            this.bufferedSource.Subscribe(d => this.timeline.Add(DateTime.Now.ToString("hh:mm:ss")));

            foreach (var definition in this.chartDefinitions)
                this.subscriptions.Add(this.bufferedSource.Subscribe(definition));
        }

        public ReactiveList<Tuple<string, ReactiveList<int>>> AddChart(Func<LogEntry, string> keySelector, Func<int, LogEntry, int> aggregator, Func<LogEntry, bool> filter)
        {
            var series = new ReactiveList<Tuple<string, ReactiveList<int>>>();
            Action<IList<LogEntry>> chartDefinition = logs =>
            {
                var lookup = logs.Where(filter).ToLookup(keySelector).ToDictionary(t => t.Key, t => t.AsEnumerable());
                foreach (var serie in series)
                {
                    IEnumerable<LogEntry> list;
                    if (lookup.TryGetValue(serie.Item1, out list))
                        lookup.Remove(serie.Item1);
                    else
                        list = Enumerable.Empty<LogEntry>();

                    serie.Item2.Add(list.Aggregate(0, aggregator));
                }

                foreach (var key in lookup.Keys)
                {
                    var list = Tuple.Create(key, this.CreateLimitedSizeList<int>(TimelineSize));

                    var firstSeries = series.FirstOrDefault();
                    if (firstSeries != null)
                        list.Item2.AddRange(Enumerable.Range(0, firstSeries.Item2.Count - 1).Select(i => 0));

                    list.Item2.Add(lookup[key].Aggregate(0, aggregator));

                    series.Add(list);
                }
            };
            this.chartDefinitions.Add(chartDefinition);

            if (this.bufferedSource != null)
                this.subscriptions.Add(this.bufferedSource.Subscribe(chartDefinition));

            this.Charts.Add(series);
            return series;
        }

        public void Dispose()
        {
            this.DisposeSubscriptions();
        }

        private ReactiveList<TValue> CreateLimitedSizeList<TValue>(int size)
        {
            var list = new ReactiveList<TValue>();
            list.CountChanged.Where(i => i > size).Subscribe(i => list.RemoveRange(0, i - size));

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
