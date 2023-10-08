using AngleSharp.Html.Parser;
using AngleSharp.Html;

using HapHelper.Services;

using HtmlAgilityPack;

using Prism.Commands;
using Prism.Mvvm;

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;

using AngleSharp.Html.Dom;
using HAP_HtmlParseError = HtmlAgilityPack.HtmlParseError;
using System.Windows.Input;
using AngleSharp.Dom;

namespace HapHelper
{
    internal class MainWindowVm : BindableBase
    {
        private Cursor _ui_Cursor;
        public Cursor UI_Cursor
        {
            get { return _ui_Cursor; }
            set { SetProperty(ref _ui_Cursor, value); }
        }

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

        public ObservableCollection<HAP_HtmlParseError> HtmlParseErrors { get; set; }

        public DelegateCommand SendCommand { get; private set; }
        public DelegateCommand CheckCommand { get; private set; }
        public DelegateCommand PrettyCommand { get; private set; }
        public DelegateCommand RunCommand { get; private set; }
        #endregion

        #region Document Node
        private string _documentNodeInnerHtml;
        public string DocumentNodeInnerHtml
        {
            get { return _documentNodeInnerHtml; }
            set { SetProperty(ref _documentNodeInnerHtml, value); }
        }
        #endregion

        #region Pretty View
        public ObservableCollection<HapHtmlNode> HapNodes { get; set; }
        #endregion

        public MainWindowVm()
        {
            PopupSvc = new PopupSvc();
            HtmlParseErrors = new ObservableCollection<HAP_HtmlParseError>();
            HapNodes = new ObservableCollection<HapHtmlNode>();
            SendCommand = new DelegateCommand(SendHttpRequestGet, CanSend).ObservesProperty(() => HtmlSourceUrl);
            CheckCommand = new DelegateCommand(Check, CanCheck).ObservesProperty(() => MyRawHtml);
            PrettyCommand = new DelegateCommand(Pretty, CanCheck).ObservesProperty(() => MyRawHtml);
            RunCommand = new DelegateCommand(Run, CanRun).ObservesProperty(() => MyRawHtml).ObservesProperty(() => IsRawHtmlValid);

            UI_Cursor = Cursors.Arrow;
        }

        private bool CanRun()
        {
            return MyRawHtml != null 
                && MyRawHtml.Trim().Length > 0 
                && IsRawHtmlValid;
        }

        private void Run()
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(MyRawHtml);
            DocumentNodeInnerHtml = htmlDocument.DocumentNode.InnerHtml;
            HapNodes.Add(GetHapHtmlNode(htmlDocument.DocumentNode));
        }

        private HapHtmlNode GetHapHtmlNode(HtmlNode htmlNode)
        {
            HapHtmlNode hapHtmlNode = new HapHtmlNode
            {
                Name = htmlNode.Name,
                Attributes = new ObservableCollection<Tuple<string, string>>(),
                ChildNodes = new ObservableCollection<HapHtmlNode>(),
            };
            foreach (var attr in htmlNode.Attributes)
            {
                Tuple<string, string> hapAttr = Tuple.Create(attr.Name, attr.Value);
                hapHtmlNode.Attributes.Add(hapAttr);
            }
            foreach (var node in htmlNode.ChildNodes)
            {

            hapHtmlNode.ChildNodes.Add(GetHapHtmlNode(node));

            }
            return hapHtmlNode;
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

        private void Pretty()
        {
            UI_Wait();
            HtmlParser parser = new HtmlParser();
            IHtmlDocument document = parser.ParseDocument(MyRawHtml);
            using (StringWriter writer = new StringWriter())
            {
                document.ToHtml(writer, new PrettyMarkupFormatter
                {
                    Indentation = "  ",
                    NewLine = "\n"
                });
                MyRawHtml = writer.ToString();
            }
            UI_RemoveWait();
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
            foreach (HAP_HtmlParseError htmlParseError in document.ParseErrors)
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

        private void UI_Wait()
        {
            UI_Cursor = Cursors.Wait;
        }

        private void UI_RemoveWait()
        {
            UI_Cursor = Cursors.Arrow;
        }
    }
}
