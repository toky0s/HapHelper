using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HapHelper
{
    internal class HapHtmlNode: BindableBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        public ObservableCollection<HapHtmlNode> ChildNodes { get; set; }
        public ObservableCollection<Tuple<string, string>> Attributes { get; set; }
    }
}
