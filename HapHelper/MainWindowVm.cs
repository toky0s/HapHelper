using HapHelper.Services;

using HtmlAgilityPack;

using Prism.Commands;
using Prism.Mvvm;

using System;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace HapHelper
{
    internal class MainWindowVm : BindableBase
    {
        private PopupSvc _popupSvc;
        public PopupSvc PopupSvc
        {
            get { return _popupSvc; }
            set { SetProperty(ref _popupSvc, value); }
        }

        #region Html Source View Props
        private bool _isRawHtmlValid;
        public bool IsRawHtmlValid
        {
            get { return _isRawHtmlValid; }
            set
            {
                SetProperty(ref _isRawHtmlValid, value);
            }
        }

        private string _htmlSourceUrl;
        public string HtmlSourceUrl
        {
            get { return _htmlSourceUrl; }
            set 
            { 
                SetProperty(ref _htmlSourceUrl, value);
            }
        }

        private string _myRawHtml;
        public string MyRawHtml
        {
            get { return _myRawHtml; }
            set 
            { 
                SetProperty(ref _myRawHtml, value);
                if (_myRawHtml.Trim().Length == 0)
                {
                    HtmlParseErrors.Clear();
                }
            }
        }

        public ObservableCollection<HtmlParseError> HtmlParseErrors { get; set; }

        public DelegateCommand SendCommand { get; private set; }
        public DelegateCommand CheckCommand { get; private set; }
        #endregion

        public MainWindowVm()
        {
            PopupSvc = new PopupSvc();
            HtmlParseErrors = new ObservableCollection<HtmlParseError>();
            SendCommand = new DelegateCommand(SendHttpRequestGet, CanSend).ObservesProperty(() => HtmlSourceUrl);
            CheckCommand = new DelegateCommand(Check, CanCheck).ObservesProperty(() => MyRawHtml);
        }

        private async void SendHttpRequestGet()
        {
            if (_htmlSourceUrl is null || _htmlSourceUrl.Trim().Length == 0)
            {
                throw new ArgumentException("HTML URL can not be empty");
            }

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(HtmlSourceUrl);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    MyRawHtml = responseBody;
                }
                catch (Exception e)
                {
                    PopupSvc.PopupTitle = "Lỗi";
                    PopupSvc.PopupCaption = e.Message;
                    PopupSvc.Open();
                }
            }

            //HtmlWeb htmlWeb = new HtmlWeb();
            //HtmlDocument document = htmlWeb.Load(_htmlSourceUrl);
            //MyRawHtml = document.DocumentNode.OuterHtml;
            //Check();
        }

        private bool CanSend()
        {
            return !(_htmlSourceUrl is null)
                && _htmlSourceUrl.Trim().Length > 0;
        }

        private void Check()
        {
            HtmlParseErrors.Clear();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(MyRawHtml);
            foreach (HtmlParseError htmlParseError in document.ParseErrors)
            {
                HtmlParseErrors.Add(htmlParseError);
            }
            if (HtmlParseErrors.Count == 0)
            {
                IsRawHtmlValid = true;
            }
        }

        private bool CanCheck()
        {
            return !(MyRawHtml is null) 
                && MyRawHtml.Trim().Length > 0;
        }
    }
}
