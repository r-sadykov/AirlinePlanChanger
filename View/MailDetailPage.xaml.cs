using System;
using AirlinePlanChanges_MailCenter.Model;
using MimeKit;

namespace AirlinePlanChanges_MailCenter.View
{
    /// <summary>
    /// Логика взаимодействия для MailDetailPage.xaml
    /// </summary>
    public partial class MailDetailPage
    {
        public MailDetailPage()
        {
            InitializeComponent();
        }

        public MailDetailPage(Tuple<MimeMessage, MessageToSendInfoModel> item)
        {
            InitializeComponent();
            CustomerEMailAddressTextBox.Text = item.Item1.To.ToString();
            CopyMailAddressListTextBox.Text = item.Item1.Cc.ToString();
            EMailSubjectTextBox.Text = item.Item1.Subject;
            EMailBodyRichTextBox.NavigateToString(item.Item1.HtmlBody);
        }

        public MailDetailPage(string report)
        {
            InitializeComponent();
            EMailBodyRichTextBox.NavigateToString(report);
        }
    }
}
