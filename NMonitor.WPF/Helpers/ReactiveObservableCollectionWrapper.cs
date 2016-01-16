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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMonitor.WPF.Helpers
{
    public class ReactiveObservableCollectionWrapper<TValue> : ObservableCollection<TValue>, IDisposable
    {
        private List<IDisposable> disposables;

        public ReactiveObservableCollectionWrapper(IReactiveCollection<TValue> list)
            : base(new List<TValue>())
        {
            this.disposables = new List<IDisposable>();

            this.disposables.Add(list.ItemsAdded.Subscribe(i => this.Add(i)));
            this.disposables.Add(list.ItemsRemoved.Subscribe(i => this.Remove(i)));
        }

        public void Dispose()
        {
            foreach (var disposable in this.disposables)
                disposable.Dispose();
        }
    }
}
