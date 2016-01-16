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

using NMonitor.WPF.Properties;
using ReactiveUI;
using System;
using System.Configuration;
using System.Reactive;
using System.Reactive.Linq;

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
        private bool isEditing;

        public RabbitMQConfigurationViewModel()
        {
            this.host = Settings.Default.RabbitMQHost;
            this.userName = Settings.Default.RabbitMQUser;
            this.password = Settings.Default.RabbitMQPassword;
            this.exchange = Settings.Default.RabbitMQExchange;
            this.exchangeType = Settings.Default.RabbitMQExchangeType;
            this.routingKey = Settings.Default.RabbitMQRoutingKey;
            this.IsEditing = string.IsNullOrWhiteSpace(Settings.Default.RabbitMQUser);

            this.Changed
                .Throttle(TimeSpan.FromSeconds(3))
                .Subscribe(_ =>
                {
                    Settings.Default.RabbitMQHost = this.Host;
                    Settings.Default.RabbitMQUser = this.UserName;
                    Settings.Default.RabbitMQPassword = this.Password;
                    Settings.Default.RabbitMQExchange = this.Exchange;
                    Settings.Default.RabbitMQExchangeType = this.ExchangeType;
                    Settings.Default.RabbitMQRoutingKey = this.RoutingKey;

                    Settings.Default.Save();
                });
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

        public bool IsEditing
        {
            get { return this.isEditing; }
            set { this.RaiseAndSetIfChanged(ref this.isEditing, value); }
        }
    }
}
