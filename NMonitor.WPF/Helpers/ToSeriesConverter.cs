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

using LiveCharts;
using LiveCharts.CoreComponents;
using ReactiveUI;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NMonitor.WPF.Helpers
{
    public class ToSeriesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var data = value as ReactiveList<Tuple<string, ReactiveList<int>>>;

            return new ReactiveObservableCollectionWrapper<Series>(data.CreateDerivedCollection(d => new LineSeries
            {
                Title = d.Item1,
                Values = d.Item2.AsChartValues<int>(),
                PointRadius = 0.0,
                StrokeThickness = 1.0
            }));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}