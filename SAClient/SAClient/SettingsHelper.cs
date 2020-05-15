using System;
using System.Collections.Generic;
using CommonClassLib;
using System.ComponentModel;
using SAClient.Annotations;

namespace SAClient
{
    public static class SettingsHelper
    {
        public static String ReportTemplate = @".\ReportTemplate.html";
        public static String TempFilesDirectory = @".\TempFiles";
    }

    public class UserSettings : INotifyPropertyChanged
    {
        private String _username;
        private String _password;
        private String _serverIp;
        public String Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("Username");
            }
        }
        public String Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }
        public String ServerIP
        {
            get { return _serverIp; }
            set
            {
                _serverIp = value;
                OnPropertyChanged("ServerIP");
            }
        }
        public Dictionary<RequestType, bool> Analyze = new Dictionary<RequestType, bool>(); 

        public UserSettings()
        {
            foreach (RequestType r in Enum.GetValues(typeof (RequestType)))
            {
                Analyze.Add(r, false);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
