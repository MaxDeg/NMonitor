/******************************************************************************
    Copyright 2015 Maxime Degallaix

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
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NMonitor.WPF.ViewModels
{
    public class LogCollectionViewModel : ReactiveObject, IDisposable
    {
        private const int MaxItemInList = 100;
        private readonly IDisposable logs;

        public LogCollectionViewModel(Dispatcher dispatcher)
        {
            var logs = new RabbitMqLogCollection(new RabbitMqParameters
            {
                Host = "localhost",
                UserName = "guest",
                Password = "guest",
                Exchange = "logs",
                ExchangeType = "topic"
            });
            this.logs = logs;

            this.Logs = new ReactiveList<LogEntry>();
            this.Logs.CountChanged.Subscribe(i =>
            {
                if (i > MaxItemInList)
                    this.Logs.RemoveRange(MaxItemInList - 1, i - MaxItemInList);
            });

            logs
                .ObserveOnDispatcher()
                .Subscribe(l => this.Logs.Insert(0, l));
        }

        public ReactiveList<LogEntry> Logs { get; private set; }

        public void Dispose()
        {
            this.logs?.Dispose();
        }
    }
}
