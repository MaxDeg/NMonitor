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

using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NMonitor
{
    public sealed class RabbitMqLogCollection : IObservable<LogEntry>, IDisposable
    {
        private IConnection connection;
        private IModel channel;
        private EventingBasicConsumer consumer;

        private Subject<LogEntry> asyncSubject;

        public RabbitMqLogCollection(RabbitMqParameters parameters)
        {
            this.asyncSubject = new Subject<LogEntry>();

            var factory = new ConnectionFactory()
            {
                HostName = parameters.Host,
                UserName = parameters.UserName,
                Password = parameters.Password
            };

            this.connection = factory.CreateConnection();
            this.channel = connection.CreateModel();

            this.channel.ExchangeDeclare(exchange: parameters.Exchange, type: parameters.ExchangeType, durable: true);

            var queueName = this.channel.QueueDeclare().QueueName;
            this.channel.QueueBind(queue: queueName, exchange: parameters.Exchange, routingKey: parameters.RoutingKey ?? "*");

            this.consumer = new EventingBasicConsumer(this.channel);
            this.consumer.Received += this.OnReceived;

            this.channel.BasicConsume(queue: queueName, noAck: true, consumer: this.consumer);
        }

        public void Dispose()
        {
            this.asyncSubject?.OnCompleted();

            if (this.consumer != null)
                this.consumer.Received -= this.OnReceived;

            this.channel?.Dispose();
            this.connection?.Dispose();
        }

        private void OnReceived(object model, BasicDeliverEventArgs ea)
        {
            this.asyncSubject.OnNext(JsonConvert.DeserializeObject<LogEntry>(Encoding.UTF8.GetString(ea.Body)));
        }

        public IDisposable Subscribe(IObserver<LogEntry> observer)
        {
            return this.asyncSubject.Subscribe(observer);
        }
    }
}
