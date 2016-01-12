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
		public static readonly DependencyProperty Password = DependencyProperty.RegisterAttached("Password", typeof(string), 
																typeof(PasswordBoxHelper), new PropertyMetadata(string.Empty, OnPasswordChanged));

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
