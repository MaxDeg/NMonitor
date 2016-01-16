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
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace NMonitor.WPF.Helpers
{
    public class PasswordBoxHelper : DependencyObject
    {
        public static readonly DependencyProperty Password = DependencyProperty.RegisterAttached("Password",
                                                                typeof(string),
                                                                typeof(PasswordBoxHelper),
                                                                new PropertyMetadata(string.Empty, OnPasswordChanged));

        public static string GetPassword(DependencyObject d)
        {
            return (string)d.GetValue(Password);
        }

        public static void SetPassword(DependencyObject d, SecureString str)
        {
            d.SetValue(Password, str);
        }

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox box = d as PasswordBox;

            // listen to PasswordBox change
            box.PasswordChanged -= HandlePasswordChanged;
            box.PasswordChanged += HandlePasswordChanged;

            box.Password = GetPassword(d);
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox box = sender as PasswordBox;
            box.SetValue(Password, box.Password);
        }
    }
}
