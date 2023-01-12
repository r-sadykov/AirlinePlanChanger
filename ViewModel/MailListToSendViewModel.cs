using System;
using System.Collections.ObjectModel;
using AirlinePlanChanges_MailCenter.Annotations;
using AirlinePlanChanges_MailCenter.Model;
using MimeKit;

namespace AirlinePlanChanges_MailCenter.ViewModel
{
    public sealed class MailListToSendViewModel
    {
        [NotNull]
        public ObservableCollection<Tuple<MimeMessage,MessageToSendInfoModel>> MailToSendCollection { get; set; }

        public MailListToSendViewModel()
        {
            MailToSendCollection = new ObservableCollection<Tuple<MimeMessage, MessageToSendInfoModel>>();
        }
    }
}
