using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMonitor.WPF.ViewModels
{
    public class ChartPointViewModel
    {
		public ChartPointViewModel()
		{
			this.Time = DateTime.Now;
		}

        public int Count { get; set; }

        public DateTime	Time { get; set; }
    }
}
