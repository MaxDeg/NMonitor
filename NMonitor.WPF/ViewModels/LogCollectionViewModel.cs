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

using NLog;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace NMonitor.WPF.ViewModels
{
    public class LogCollectionViewModel : ReactiveObject, IDisposable
    {
        private const int MaxItemInList = 100;

        private ILogSource<RabbitMqParameters> logsSource;
        private ReactiveList<LogEntry> logs;
        private ChartsViewModel charts;
        private CollectionStatus status;

        public LogCollectionViewModel()
        {
            this.Status = CollectionStatus.NotConnected;
            this.Loggers = new ReactiveList<LogCollectionFilterViewModel<string>>() { ChangeTrackingEnabled = true };
            this.LogLevels = new ReactiveList<LogCollectionFilterViewModel<LogLevel>>() { ChangeTrackingEnabled = true };
            this.LogMachines = new ReactiveList<LogCollectionFilterViewModel<string>>() { ChangeTrackingEnabled = true };
            this.LogApplications = new ReactiveList<LogCollectionFilterViewModel<string>>() { ChangeTrackingEnabled = true };

            this.LogLevels.Add(new LogCollectionFilterViewModel<LogLevel>(LogLevel.Info, "Info"));
            this.LogLevels.Add(new LogCollectionFilterViewModel<LogLevel>(LogLevel.Warn, "Warn", true));
            this.LogLevels.Add(new LogCollectionFilterViewModel<LogLevel>(LogLevel.Error, "Error", true));
            this.LogLevels.Add(new LogCollectionFilterViewModel<LogLevel>(LogLevel.Fatal, "Fatal", true));

            this.logsSource = new RabbitMqLogCollection();
            this.logsSource
                .ObserveOnDispatcher()
                .Subscribe(this.AddToLogs);

            this.logs = new ReactiveList<LogEntry>() { ChangeTrackingEnabled = true };
            this.logs.CountChanged
                .Where(i => i > MaxItemInList)
                .Subscribe(i => this.logs.RemoveRange(MaxItemInList - 1, i - MaxItemInList));

            this.Logs = this.logs.CreateDerivedCollection(l => l, this.FilterLogList);

            this.Charts = new ChartsViewModel();
            this.Charts.SetSource(this.logsSource);

            this.LevelsChart = this.Charts.AddChart(l => l.Level.ToString(), (a, l) => a + 1.0, l => l.Level > LogLevel.Info);
            this.ApplicationsChart = this.Charts.AddChart(l => l.Application, (a, l) => a + 1.0, l => l.Level > LogLevel.Info);

            this.Parameters = new RabbitMQConfigurationViewModel();
            this.ConnectToLogCollection();

            this.Parameters.Changed
                .Buffer(() => this.ObservableForProperty(t => t.Parameters.IsEditing).Where(p => !p.Value))
                .Where(ps => ps.Any(p => p.PropertyName != nameof(this.Parameters.IsEditing)))
                .Subscribe(_ => this.ConnectToLogCollection());

            Observable.Merge(
                this.Loggers.ItemChanged.Select(_ => Unit.Default),
                this.LogLevels.ItemChanged.Select(_ => Unit.Default),
                this.LogApplications.ItemChanged.Select(_ => Unit.Default),
                this.LogMachines.ItemChanged.Select(_ => Unit.Default))
                .ObserveOnDispatcher()
                .Subscribe(_ => this.Logs.Reset());
        }

        public IReactiveDerivedList<LogEntry> Logs { get; private set; }

        public RabbitMQConfigurationViewModel Parameters { get; set; }

        public ChartsViewModel Charts
        {
            get { return this.charts; }
            private set { this.RaiseAndSetIfChanged(ref this.charts, value); }
        }

        public CollectionStatus Status
        {
            get { return this.status; }
            set { this.RaiseAndSetIfChanged(ref this.status, value); }
        }

        public ReactiveList<Tuple<string, ReactiveList<double>>> LevelsChart { get; private set; }

        public ReactiveList<Tuple<string, ReactiveList<double>>> ApplicationsChart { get; private set; }

        public ReactiveList<LogCollectionFilterViewModel<string>> Loggers { get; private set; }

        public ReactiveList<LogCollectionFilterViewModel<LogLevel>> LogLevels { get; private set; }

        public ReactiveList<LogCollectionFilterViewModel<string>> LogMachines { get; private set; }

        public ReactiveList<LogCollectionFilterViewModel<string>> LogApplications { get; private set; }

        public void Dispose()
        {
            this.logsSource?.Dispose();
            this.Charts?.Dispose();
        }

        private void ConnectToLogCollection()
        {
            Task.Run(() =>
            {
                try
                {
                    if (this.Parameters.IsEditing)
                        return;

                    this.Status = CollectionStatus.Connecting;

                    this.logsSource.Connect(new RabbitMqParameters
                    {
                        Host = this.Parameters.Host,
                        UserName = this.Parameters.UserName,
                        Password = this.Parameters.Password,
                        Exchange = this.Parameters.Exchange,
                        ExchangeType = this.Parameters.ExchangeType,
                        RoutingKey = this.Parameters.RoutingKey
                    });

                    this.Status = CollectionStatus.Connected;
                }
                catch (Exception)
                {
                    this.Status = CollectionStatus.FailedToConnect;
                }
            });
        }

        private bool FilterLogList(LogEntry entry)
        {
            return this.Loggers
                        .Where(l => l.IsSelected)
                        .Any(l => string.CompareOrdinal(l.Value, entry.Logger) == 0)
                        &&
                     this.LogLevels
                            .Where(l => l.IsSelected)
                            .Any(l => l.Value == entry.Level)
                        &&
                     this.LogMachines
                        .Where(l => l.IsSelected)
                        .Any(l => string.CompareOrdinal(l.Value, entry.Machine) == 0)
                        &&
                     this.LogApplications
                        .Where(l => l.IsSelected)
                        .Any(l => string.CompareOrdinal(l.Value, entry.Application) == 0);
        }

        private void AddToLogs(LogEntry entry)
        {
            if (!this.Loggers.Any(il => il.Value == entry.Logger))
                this.Loggers.Add(new LogCollectionFilterViewModel<string>(entry.Logger, entry.Logger, true));

            if (!this.LogMachines.Any(il => il.Value == entry.Machine))
                this.LogMachines.Add(new LogCollectionFilterViewModel<string>(entry.Machine, entry.Machine, true));

            if (!this.LogApplications.Any(il => il.Value == entry.Application))
                this.LogApplications.Add(new LogCollectionFilterViewModel<string>(entry.Application, entry.Application, true));

            this.logs.Insert(0, entry);
        }
    }
}
