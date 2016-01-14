using LiveCharts;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NMonitor.WPF.Helpers
{
	public class ToSeriesConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var data = value as ReactiveList<Tuple<string, ReactiveList<double>>>;

			return new ReactiveObservableCollectionWrapper<Series>(data.CreateDerivedCollection(d => new LineSeries
			{
				Title = d.Item1,
				PrimaryValues = d.Item2,
				PointRadius = 0.0,
				StrokeThickness = 1.0
			}));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class ToLabelsConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var data = value as ReactiveList<Tuple<string, ReactiveList<double>>>;

			return new ReactiveObservableCollectionWrapper<string>(data.CreateDerivedCollection(d => d.Item1));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
