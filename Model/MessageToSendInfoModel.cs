using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AirlinePlanChanges_MailCenter.Annotations;

namespace AirlinePlanChanges_MailCenter.Model
{
    public sealed class MessageToSendInfoModel:INotifyPropertyChanged
    {
        public string SourceMessageId { get; set; }
        public int Index { get; set; }
        public string Pnr { get; set; }
        public int BookingNumber { get; set; }
        public string ChangesDate { get; set; }
        public string Subject { get; set; }
        public string CustomerMail { get; set; }
        private bool? _isSelected;

        public bool? IsSelected
        {
            get => _isSelected ?? (_isSelected = IsChecked);
            set
            {
                if (value == null) _isSelected = IsChecked;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get
            {
                _isChecked = !String.IsNullOrWhiteSpace(CustomerMail) && BookingNumber != 0;
                return _isChecked;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
