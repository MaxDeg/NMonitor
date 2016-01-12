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
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace NMonitor.WPF.ViewModels
{
	public class LogCollectionViewModel : ReactiveObject, IDisposable
	{
		private const int MaxItemInList = 100;
		private ReactiveList<LogEntry> logs;
		private IDisposable logsSource;
		private IDisposable logsSubscribtion;

		private CollectionStatus status;

		private bool showInfo = true;
		private bool showWarn = true;
		private bool showError = true;
		private bool showFatal = true;
		private string logger = string.Empty;

		public LogCollectionViewModel()
		{
			this.Parameters = new RabbitMQConfigurationViewModel();
			this.Status = CollectionStatus.NotConnected;
			this.Loggers = new ReactiveList<LogCollectionFilterViewModel<string>>();
			this.LogLevels = new List<LogCollectionFilterViewModel<LogLevel>>()
			{
				new LogCollectionFilterViewModel<LogLevel>(LogLevel.Info, "Info", false),
				new LogCollectionFilterViewModel<LogLevel>(LogLevel.Warn, "Warn", true),
				new LogCollectionFilterViewModel<LogLevel>(LogLevel.Error, "Error", true),
				new LogCollectionFilterViewModel<LogLevel>(LogLevel.Fatal, "Fatal", true),
			};

			this.logs = new ReactiveList<LogEntry>()
			{
				ChangeTrackingEnabled = true
			};
			this.logs.CountChanged.Subscribe(i =>
			{
				if (i > MaxItemInList)
					this.logs.RemoveRange(MaxItemInList - 1, i - MaxItemInList);
			});
			this.logs.ItemsAdded.Subscribe(l =>
			{
				if (!this.Loggers.Any(il => il.Value == l.Logger))
					this.Loggers.Add(new LogCollectionFilterViewModel<string>(l.Logger, l.Logger, true));
			});
			this.Logs = this.logs.CreateDerivedCollection(l => l, e => this.Loggers.Any(l => string.CompareOrdinal(l.Value, e.Logger) == 0)
																			&& this.LogLevels.Any(l => l.Value == e.Level));
			
			this.WhenAnyValue(
				t => t.Parameters.Host,
				t => t.Parameters.UserName,
				t => t.Parameters.Password,
				t => t.Parameters.Exchange,
				t => t.Parameters.ExchangeType,
				t => t.Parameters.RoutingKey,
				(h, u, p, e, et, r) => new RabbitMqParameters
				{
					Host = h,
					UserName = u,
					Password = p,
					Exchange = e,
					ExchangeType = et,
					RoutingKey = r
				})
				.Throttle(TimeSpan.FromSeconds(5))
				.ObserveOnDispatcher()
				.Subscribe(this.ConnectToLogCollection);
			
			//this.WhenAnyValue(
			//	t => t.ShowInfo,
			//	t => t.ShowWarn,
			//	t => t.ShowError,
			//	t => t.ShowFatal,
			//	t => t.Logger)
			//	.ObserveOnDispatcher()
			//	.Subscribe(x => this.Logs.Reset());
		}

		public IReactiveDerivedList<LogEntry> Logs { get; private set; }
		
		public RabbitMQConfigurationViewModel Parameters { get; set; }

		public CollectionStatus Status
		{
			get { return this.status; }
			set { this.RaiseAndSetIfChanged(ref this.status, value); }
		}

		public ReactiveList<LogCollectionFilterViewModel<string>> Loggers { get; private set; }

		public List<LogCollectionFilterViewModel<LogLevel>> LogLevels { get; private set; }

		public void Dispose()
		{
			this.logsSource?.Dispose();
		}

		private void ConnectToLogCollection(RabbitMqParameters parameters)
		{
			try
			{
				this.Status = CollectionStatus.Connecting;

				// Dispose existing
				this.logsSource?.Dispose();
				this.logsSubscribtion?.Dispose();

				var logs = new RabbitMqLogCollection(parameters);
				this.logsSource = logs;
				this.logsSubscribtion = logs.ObserveOnDispatcher()
											.Subscribe(l => this.logs.Insert(0, l));

				this.Status = CollectionStatus.Connected;
			}
			catch (Exception e)
			{
				this.Status = CollectionStatus.FailedToConnect;
			}
		}
	}
}
