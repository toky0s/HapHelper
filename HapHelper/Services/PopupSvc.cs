using Prism.Commands;
using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HapHelper.Services
{
    internal enum PopupType
    {
        Info,
        Error,
        Success
    }

    internal class PopupSvc: BindableBase
    {
        private string _popupTitle;
        public string PopupTitle
        {
            get { return _popupTitle; }
            set { SetProperty(ref _popupTitle, value); }
        }

        private string _popupCaption;
        public string PopupCaption
        {
            get { return _popupCaption; }
            set { SetProperty(ref _popupCaption, value); }
        }

        private PopupType _popupType;
        public PopupType PopupType
        {
            get { return _popupType; }
            set { SetProperty(ref _popupType, value); }
        }

        private bool _isOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
            set { SetProperty(ref _isOpen, value); }
        }

        private DelegateCommand _closePopupCmd;
        public DelegateCommand ClosePopupCmd =>
            _closePopupCmd ?? (_closePopupCmd = new DelegateCommand(ExecuteClosePopupCmd));

        void ExecuteClosePopupCmd()
        {
            IsOpen = false;
        }

        private DelegateCommand _openPopupCmd;
        public DelegateCommand OpenPopupCmd =>
            _openPopupCmd ?? (_openPopupCmd = new DelegateCommand(ExecuteOpenPopupCmd));

        void ExecuteOpenPopupCmd()
        {
            IsOpen = true;
        }

        public void Open()
        {
            ExecuteOpenPopupCmd();
        }

        public void Close()
        {
            ExecuteClosePopupCmd();
        }
    }
}
