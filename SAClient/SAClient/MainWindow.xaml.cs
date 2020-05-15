using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommonClassLib;
//using ClientClasses;
using CommonClassLib.Responces;
using HtmlAgilityPack;
using System.Diagnostics;
using CommonClassLib.Requests;
using System.Windows.Threading;

namespace SAClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IDisposable
    {
        private AnalyzeWizard _analyzeWizard;
        private SettingsWindow _settingsWindow;
        private ClientDbWorkProvider _provider;
        private ClientConnectionManager _connectionManager;
        private DispatcherTimer _updateTimer;
        private bool _waitingToAuthorize;
        private Request _requestPending;
        private bool _pendingRequestIsUpdating;

        public MainWindow()
        {
            InitializeComponent();

        }

        private void Analyze_OnClick(object sender, RoutedEventArgs e)
        {
            _analyzeWizard = new AnalyzeWizard();
            if (_analyzeWizard.ShowDialog() ?? false)
            {
                TrySendRequest(RequestGeneratingType.FromWizard);
            }
        }

        private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (TbQuick.Text.Equals("Быстрый анализ...")) TbQuick.Text = "";
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (TbQuick.Text.Trim().Length == 0) TbQuick.Text = "Быстрый анализ...";
        }

        private void SaveReport_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog { DefaultExt = ".html", Filter = "Отчет (.html)|*.html" };
            if (dlg.ShowDialog() == true)
            {
                string filename = dlg.FileName;
                var responce = LbLocalReports.SelectedItem as AnalyzeResponce;
                if (responce == null) throw new Exception();

                String tmpFile = String.Format(@"{0}\{1}{2}.html",
                    SettingsHelper.TempFilesDirectory, responce.Url, responce.LastUpdate.ToShortDateString());


                try
                {
                    File.Copy(tmpFile, filename, true);
                    MessageBox.Show("Успешно сохранено");
                }
                catch (Exception)
                {
                    MessageBox.Show("Возникли проблемы с сохранением.");
                }
            }
        }

        private void Settings_OnClick(object sender, RoutedEventArgs e)
        {
            _settingsWindow = new SettingsWindow();
            _settingsWindow.ShowDialog();

        }

        private void DeleteReport_OnClick(object sender, RoutedEventArgs e)
        {

            try
            {
                _provider.DeleteReport((AnalyzeResponce)LbLocalReports.SelectedItem);
                LbLocalReports.SelectedIndex = 0;
            }
            catch (Exception)
            {
                MessageBox.Show("Во время удаления отчета возникла ошибка!");
            }
        }

        private void LbLocalReports_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var responce = ((ListBox)sender).SelectedItem as AnalyzeResponce;
                if (responce != null) PrepareReport(responce);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникла ошибка при работе с шаблоном отчета!");
            }

        }

        private void PrepareReport(AnalyzeResponce responce)
        {
            String path = Path.GetFullPath(SettingsHelper.ReportTemplate);
            var doc = new HtmlDocument();
            doc.Load(path);
            doc.GetElementbyId("SiteUrl").InnerHtml = responce.Url;
            doc.GetElementbyId("LastUpdate").InnerHtml = responce.LastUpdate.ToShortDateString();
            doc.GetElementbyId("CY").InnerHtml = responce.Results[RequestType.CY] != "" ? responce.Results[RequestType.CY] : "-";
            doc.GetElementbyId("PR").InnerHtml = responce.Results[RequestType.PR] != "" ? responce.Results[RequestType.PR] : "-";
            doc.GetElementbyId("Links").InnerHtml = responce.Results[RequestType.Links] != "" ? responce.Results[RequestType.Links] : "-";
            doc.GetElementbyId("Clients").InnerHtml = responce.Results[RequestType.Customers] != "" ? responce.Results[RequestType.Customers] : "-";

            if (!Directory.Exists(SettingsHelper.TempFilesDirectory))
                Directory.CreateDirectory(SettingsHelper.TempFilesDirectory);

            string fileName = responce.LastUpdate.ToShortDateString();
            fileName = PrepareFileName(fileName);

            //String tmpFile = String.Format(@"{0}\{1}{2}.html", SettingsHelper.TempFilesDirectory, responce.Url, responce.LastUpdate.ToShortDateString());
            String tmpFile = String.Format(@"{0}\{1}{2}.html", SettingsHelper.TempFilesDirectory, responce.Url, fileName);
            doc.Save(tmpFile, Encoding.Unicode);

            ReportBrowser.Navigate(Path.GetFullPath(tmpFile));

        }

        private string PrepareFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return fileName;

            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            foreach (char c in invalid)
            {
                fileName = fileName.Replace(c.ToString(), "_");
            }

            return fileName;
        }

        private void MainWindow_OnInitialized(object sender, EventArgs e)
        {
            _provider = new ClientDbWorkProvider(Properties.Settings.Default.ClientDBConnectionString);
            LbLocalReports.ItemsSource = _provider.Reports;
            LbLocalReports.SelectedIndex = 0;
            _connectionManager = new ClientConnectionManager(_provider);
            _settingsWindow = new SettingsWindow();
            _updateTimer = new DispatcherTimer(DispatcherPriority.Normal) { Interval = new TimeSpan(0, 0, 3) };
            _updateTimer.Tick += updateTimer_Tick;

            _connectionManager.ResponceReceived += _connectionManager_ResponceReceived;
            _updateTimer.Start();
        }

        void updateTimer_Tick(object sender, EventArgs e)
        {
            _provider.Update();
        }


        void _connectionManager_ResponceReceived(object sender, EventArgs e)
        {
            if (_waitingToAuthorize)
            {
                if (!_connectionManager.Responce.Result)
                    MessageBox.Show(((CommandResponce)_connectionManager.Responce).Message, "Ошибка авторизации");
                else
                {
                    _connectionManager.SendRequest(_requestPending, _pendingRequestIsUpdating);
                    _waitingToAuthorize = false;

                }
            }
            else
            {
                MessageBox.Show(
                    "Сервер прислал отчет по сайту!",
                    "Получен ответ от сервера!", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);

            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Directory.Exists(SettingsHelper.TempFilesDirectory))
                foreach (var file in Directory.GetFiles(SettingsHelper.TempFilesDirectory))
                {
                    File.Delete(file);
                }
            Application app = Application.Current;
            app.Shutdown();
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            Application app = Application.Current;
            app.Shutdown();
        }

        private void Help_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(@".\Help.pdf");
        }

        private void TrySendRequest(RequestGeneratingType type)
        {
            if (!_connectionManager.IsConnected)
            {
                _connectionManager.ConnectToServer(Properties.Settings.Default.Username,
                                                   Properties.Settings.Default.Password,
                                                   Properties.Settings.Default.ServerIP);
                _waitingToAuthorize = true;
                _requestPending = CreateRequest(type);
            }
            else
            {
                _connectionManager.SendRequest(CreateRequest(type), _pendingRequestIsUpdating);
            }
        }

        private AnalyzeRequest CreateRequest(RequestGeneratingType type)
        {
            AnalyzeRequest req = null;
            switch (type)
            {
                case RequestGeneratingType.FromWizard:
                    req = _analyzeWizard.UserRequest;
                    break;
                case RequestGeneratingType.QuickRequest:
                    var requests = new SortedSet<RequestType>();
                    var settings = SettingsWindow.MySettings;
                    if (settings == null) return null;
                    foreach (var node in settings.Analyze)
                    {
                        if (node.Value) requests.Add(node.Key);
                    }

                    req = new AnalyzeRequest { SiteUrl = TbQuick.Text, Requests = requests };
                    break;
                case RequestGeneratingType.UpdateReport:
                    var report = LbLocalReports.SelectedItem as AnalyzeResponce;
                    if (report == null) return null;
                    req = new AnalyzeRequest
                        {
                            SiteUrl = report.Url,
                            Requests = new SortedSet<RequestType>(report.Results.Keys)
                        };
                    _pendingRequestIsUpdating = true;
                    break;
            }
            return req;
        }

        private enum RequestGeneratingType
        {
            FromWizard,
            QuickRequest,
            UpdateReport
        }

        private void TbQuick_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (TbQuick.Text.Trim() != "")
                {
                    TrySendRequest(RequestGeneratingType.QuickRequest);
                    TbQuick.Text = "";
                }
                else
                {
                    TbQuick.Text = "";
                    MessageBox.Show("Необходимо ввести адрес сайта!", "Ошибка!");
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TrySendRequest(RequestGeneratingType.UpdateReport);
        }

        public void Dispose()
        {
            _provider.Dispose();
        }
    }
}
