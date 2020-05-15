using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CommonClassLib;

namespace SAClient
{
	/// <summary>
	/// Логика взаимодействия для SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow
	{
	    public static UserSettings MySettings;
		public SettingsWindow()
		{
			InitializeComponent();
			// Вставьте ниже код, необходимый для создания объекта.
		}

        private void Window_Initialized(object sender, EventArgs e)
        {
            var provider = Resources["Source"] as ObjectDataProvider;
            if (provider == null) return;
            MySettings = provider.Data as UserSettings;
            if (MySettings == null) return;
            MySettings.Username = Properties.Settings.Default.Username;
            MySettings.Password = Properties.Settings.Default.Password;
            MySettings.ServerIP = Properties.Settings.Default.ServerIP;

            String[] buffer = Properties.Settings.Default.Analyze
                .Split(new[]{','}, Enum.GetValues(typeof(RequestType)).Length, StringSplitOptions.RemoveEmptyEntries);

            String lastParam = buffer[buffer.Count() - 1];
            if (lastParam.EndsWith(","))
            {
                int lastParamLength = buffer[buffer.Count() - 1].Count();
                buffer[buffer.Count() - 1] = lastParam.Substring(0, lastParamLength - 1);
            }
            for (int i = 0; i < buffer.Length; i++ )
            {
                buffer[i] = buffer[i].Trim();
                RequestType c;
                Enum.TryParse(buffer[i], out c);
                MySettings.Analyze[c] = true;
            }

            if (MySettings.Analyze[RequestType.CY]) CY.IsChecked = true;
            if (MySettings.Analyze[RequestType.PR]) PR.IsChecked = true;
            if (MySettings.Analyze[RequestType.Customers]) Customers.IsChecked = true;
            if (MySettings.Analyze[RequestType.Links]) Links.IsChecked = true;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Username = MySettings.Username;
            Properties.Settings.Default.Password = MySettings.Password;
            String buffer = MySettings.Analyze.Where(analyze => analyze.Value)
                .Aggregate("", (current, analyze) => current + (analyze.Key.ToString() + ","));
            Properties.Settings.Default.Analyze = buffer;
            Properties.Settings.Default.ServerIP = MySettings.ServerIP;
            Properties.Settings.Default.Save();
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

	    private void CY_OnClick(object sender, RoutedEventArgs e)
	    {
	        CheckBoxWork(sender, RequestType.CY);
	    }

	    private void PR_OnClick(object sender, RoutedEventArgs e)
	    {
            CheckBoxWork(sender, RequestType.PR);
	    }

	    private void Links_OnClick(object sender, RoutedEventArgs e)
	    {
            CheckBoxWork(sender, RequestType.Links);
	    }

	    private void Customers_OnClick(object sender, RoutedEventArgs e)
	    {
            CheckBoxWork(sender, RequestType.Customers);
	    }

        private void CheckBoxWork(object sender, RequestType type)
        {
            var cb = sender as CheckBox;
            if (cb != null) 
                MySettings.Analyze[type] = cb.IsChecked ?? false;
        }
	}
}