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
using System.Security;
using System.Runtime.InteropServices;
using Splat;

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
			this.Loggers = new ReactiveList<string>() { string.Empty };

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
				if (!this.Loggers.Contains(l.Logger))
					this.Loggers.Add(l.Logger);
			});
			this.Logs = this.logs.CreateDerivedCollection(l => l, this.IsFiltered);
			
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

			this.WhenAnyValue(
				t => t.ShowInfo,
				t => t.ShowWarn,
				t => t.ShowError,
				t => t.ShowFatal,
				t => t.Logger)
				.ObserveOnDispatcher()
				.Subscribe(x => this.Logs.Reset());
		}

		public IReactiveDerivedList<LogEntry> Logs { get; private set; }

		public ReactiveList<string> Loggers { get; private set; }

		public RabbitMQConfigurationViewModel Parameters { get; set; }

		public CollectionStatus Status
		{
			get { return this.status; }
			set { this.RaiseAndSetIfChanged(ref this.status, value); }
		}

		public bool ShowInfo
		{
			get { return this.showInfo; }
			set { this.RaiseAndSetIfChanged(ref this.showInfo, value); }
		}

		public bool ShowWarn
		{
			get { return this.showWarn; }
			set { this.RaiseAndSetIfChanged(ref this.showWarn, value); }
		}

		public bool ShowError
		{
			get { return this.showError; }
			set { this.RaiseAndSetIfChanged(ref this.showError, value); }
		}

		public bool ShowFatal
		{
			get { return this.showFatal; }
			set { this.RaiseAndSetIfChanged(ref this.showFatal, value); }
		}

		public string Logger
		{
			get { return this.logger; }
			set { this.RaiseAndSetIfChanged(ref this.logger, value); }
		}

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

		private bool IsFiltered(LogEntry entry)
		{
			if (!string.IsNullOrEmpty(this.Logger) && string.CompareOrdinal(entry.Logger, this.Logger) != 0)
				return false;

			switch (entry.Level)
			{
				case LogLevel.Info:
					return this.ShowInfo;

				case LogLevel.Warn:
					return this.ShowWarn;

				case LogLevel.Error:
					return this.ShowError;

				case LogLevel.Fatal:
					return this.ShowFatal;

				default:
					return false;
			}
		}
	}
}
