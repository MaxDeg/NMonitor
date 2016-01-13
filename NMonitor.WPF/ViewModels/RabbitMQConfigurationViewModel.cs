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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace NMonitor.WPF.ViewModels
{
	public class RabbitMQConfigurationViewModel : ReactiveObject
	{
		private string host;
		private string userName;
		private string password;
		private string exchange;
		private string exchangeType;
		private string routingKey;

		public RabbitMQConfigurationViewModel()
		{
			this.host = "sony-dev";
			this.userName = "sony.app";
			this.password = "711F1'7R48~4BxM";
			this.exchange = "sony.logs";
			//this.host = "localhost";
   //         this.userName = "guest";
   //         this.password = "guest";
   //         this.exchange = "logs";
            this.exchangeType = "topic";
			this.routingKey = "#";
		}

		public string Host
		{
			get { return this.host; }
			set { this.RaiseAndSetIfChanged(ref this.host, value); }
		}
		public string UserName
		{
			get { return this.userName; }
			set { this.RaiseAndSetIfChanged(ref this.userName, value); }
		}
		public string Password
		{
			get { return this.password; }
			set { this.RaiseAndSetIfChanged(ref this.password, value); }
		}

		public string Exchange
		{
			get { return this.exchange; }
			set { this.RaiseAndSetIfChanged(ref this.exchange, value); }
		}
		public string ExchangeType
		{
			get { return this.exchangeType; }
			set { this.RaiseAndSetIfChanged(ref this.exchangeType, value); }
		}
		public string RoutingKey
		{
			get { return this.routingKey; }
			set { this.RaiseAndSetIfChanged(ref this.routingKey, value); }
		}
	}
}
