using LiveCharts;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMonitor.WPF.Helpers
{
	public class ChartValuesWrapper<T> : ChartValues<T>, IDisposable
	{
		private List<IDisposable> disposables;

		public ChartValuesWrapper(ReactiveList<T> list)
		{
			this.AddRange(list);

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
