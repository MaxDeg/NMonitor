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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NMonitor.WPF.Helpers
{
    public class CombiningConverter : IValueConverter
    {
        public IValueConverter Converter1 { get; set; }

        public IValueConverter Converter2 { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object convertedValue = this.Converter1.Convert(value, targetType, parameter, culture);
            return this.Converter2.Convert(convertedValue, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object convertedValue = this.Converter1.ConvertBack(value, targetType, parameter, culture);
            return this.Converter2.ConvertBack(convertedValue, targetType, parameter, culture);
        }
    }
}
