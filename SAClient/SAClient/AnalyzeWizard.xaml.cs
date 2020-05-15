using System;
using System.Windows.Controls;
using CommonClassLib;
using CommonClassLib.Requests;
using System.Windows;

namespace SAClient
{
	/// <summary>
	/// Логика взаимодействия для AnalyzeWizard.xaml
	/// </summary>
	public partial class AnalyzeWizard
	{
        public AnalyzeRequest UserRequest;
	    public bool IsReady;
        
		public AnalyzeWizard()
		{
			InitializeComponent();
		    // Вставьте ниже код, необходимый для создания объекта.
		}


	    private void AnalyzeWizard_OnInitialized(object sender, EventArgs e)
	    {
            UserRequest = new AnalyzeRequest();
	    }

        private void AddAnalyzeRequest(object sender, RequestType type)
        {
            var cb = sender as CheckBox;
            var req = Resources["Request"] as AnalyzeRequest;
            if ((cb != null)&&(req!=null))
            {
                if (cb.IsChecked ?? false) req.Requests.Add(type);
                else req.Requests.Remove(type);
            }
        }

	    private void CbCY_OnChecked(object sender, RoutedEventArgs e)
	    {
	        AddAnalyzeRequest(sender, RequestType.CY);
	    }

	    private void CbPR_OnChecked(object sender, RoutedEventArgs e)
	    {
            AddAnalyzeRequest(sender, RequestType.PR);
	    }

	    private void CbClients_OnChecked(object sender, RoutedEventArgs e)
	    {
	        AddAnalyzeRequest(sender, RequestType.Customers);
	    }

	    private void CbLinks_OnChecked(object sender, RoutedEventArgs e)
	    {
	        AddAnalyzeRequest(sender, RequestType.Links);
	    }

	    private void Wizard_OnFinish(object sender, RoutedEventArgs e)
	    {
	        UserRequest = Resources["Request"] as AnalyzeRequest;
	        if (UserRequest == null)
	        {
	            IsReady = false;
	            return;
	        }
	        if ((UserRequest.Requests.Count <= 0) || (UserRequest.SiteUrl.Trim() == ""))
	        {
	            MessageBox.Show("Обязательно должен быть выбран хотя бы 1 анализ и введён адрес сайта.",
	                            "Ошибка при составлении запроса!");
	            IsReady = false;
	        } else IsReady = true;
	    }
	}
}