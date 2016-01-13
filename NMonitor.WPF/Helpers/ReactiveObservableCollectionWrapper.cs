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
