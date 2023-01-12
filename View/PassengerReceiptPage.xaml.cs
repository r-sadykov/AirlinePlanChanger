using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AirlinePlanChanges_MailCenter.Model;
using AirlinePlanChanges_MailCenter.Utils;
using AirlinePlanChanges_MailCenter.Extensions;
using HtmlAgilityPack;
using MailKit;
using Microsoft.Win32;
using MimeKit;

namespace AirlinePlanChanges_MailCenter.View
{
    /// <summary>
    /// Interaction logic for PassengerReceiptPage.xaml
    /// </summary>
    public partial class PassengerReceiptPage
    {
        private readonly CurrentUserSettings _userSettings;
        private List<PassengerReceipt> _sepas;

        public PassengerReceiptPage()
        {
            InitializeComponent();
            DatePicker.DisplayDate = DateTime.Today;
        }
        internal PassengerReceiptPage(CurrentUserSettings currentUser)
        {
            InitializeComponent();
            _userSettings = currentUser;
            DatePicker.DisplayDate=DateTime.Today;
        }

        private void LoadMails_OnClick(object sender, RoutedEventArgs e)
        {
            ThunderbirdMailGatherer gatherer=new ThunderbirdMailGatherer(_userSettings);
            var date = DatePicker.SelectedDate;
            var selecteDateTime = date!=null ? DateTime.Parse(date.ToString()) : DateTime.Now;
            var year = selecteDateTime.Year;
            var month = selecteDateTime.Month;
            var day = selecteDateTime.Day;
            var mails = gatherer.GetMailsFromImapDefined("GWI Passenger Receipt ",year,month,day);
            _sepas = GetSepaMails(mails, out var messages);
            gatherer.PutMailsToImapDefined("GWI Passenger Receipt Erstattung", messages);
            DataGridReceipts.ItemsSource = _sepas;
        }

        private void SaveResults_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog {Filter = @"Excel Files(*.xlsx)|*.xlsx"};
            dialog.ShowDialog();
            if (dialog.CheckFileExists)
            {
                if (!string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    System.Data.DataTable dt =_sepas.AsDataTable();
                    dt.ExportToOxml(dialog.FileName, false);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    System.Data.DataTable dt = _sepas.AsDataTable();
                    dt.ExportToOxml(dialog.FileName, true);
                }
            }
        }

        private List<PassengerReceipt> GetSepaMails(Dictionary<UniqueId,MimeMessage> messages, out List<MimeMessage> toSendList)
        {
            List<PassengerReceipt> passengerReceipts=new List<PassengerReceipt>();
            toSendList=new List<MimeMessage>();
            foreach (var mimeMessage in messages)
            {
                var html = mimeMessage.Value.HtmlBody;
                HtmlDocument hap=new HtmlDocument();
                hap.LoadHtml(html);

                var nodes2 = hap.DocumentNode.Descendants("td").Where(d =>
                    d.Attributes.Contains("style") && d.Attributes["style"].Value
                        .Contains("line-height: 10px;font-size:10px;font-family:Arial,sans-serif;"));
                var sep = nodes2.Where(c => c.InnerText.Contains("SEPA Firmenlastschrift")).Select(c => c.ParentNode);
                var res = sep.Where(c => c.ChildNodes[1].InnerText.Contains('-')).ToList();

                if (res.Count>0)
                {
                    PassengerReceipt receipt = new PassengerReceipt();
                    receipt.ReceivedDate = mimeMessage.Value.Date.ToString();
                    var nodes = hap.DocumentNode.Descendants("td").Where(d =>
                        d.Attributes.Contains("style") && d.Attributes["style"].Value
                            .Contains("color:#000000;font-size:22px;line-height:15px;font-family:Arial,sans-serif;")).ToList();
                    var replace = nodes[0].InnerText.Replace("\r", "").Replace("\n", "").Trim();
                    receipt.Pnr = replace;
                    replace = res[0].ChildNodes[0].InnerText;
                    string tmp = replace.Substring(0, replace.IndexOf('-')).Trim();
                    replace= replace.Replace(tmp, "").Replace("-", "").Trim();
                    tmp = replace.Substring(0, replace.IndexOf('(')).Trim();
                    receipt.SepaCode = tmp;
                    replace = replace.Replace(tmp, "").Replace("(", "").Replace(")","").Trim();
                    receipt.SepaDate = replace;
                    receipt.SepaAmount = res[0].ChildNodes[1].InnerText.Replace("&nbsp;","").Replace("€","").Trim();
                    nodes = hap.DocumentNode.Descendants("td").Where(d =>
                        d.Attributes.Contains("style") && d.Attributes["style"].Value
                            .Contains("font-size:10px;font-family:Arial,sans-serif;")).ToList();
                    sep= nodes.Where(c => c.InnerText.Contains(@"Tag der Buchung")).Select(c => c.ParentNode).ToList();
                    foreach (var htmlNode in sep)
                    {
                        if (htmlNode.InnerText.Contains(@"Tag der Buchung"))
                        {
                            replace = htmlNode.InnerText.Substring(htmlNode.InnerText.IndexOf(@"Tag der Buchung", StringComparison.Ordinal));
                            replace = replace.Replace("\r", "").Replace("\n", "").Replace("&nbsp;", "").Trim();
                            replace = replace.Replace(@"Tag der Buchung:", "").Trim();
                            tmp = replace.Substring(0, replace.IndexOf(' ')).Trim();
                            receipt.BuchungDate = tmp;
                            replace = replace.Substring(tmp.Length).Trim().Replace(@"Änderungsdatum:", "").Trim();
                            receipt.ChangesDate = replace;
                            break;
                        }
                    }
                    sep = nodes.Where(c => c.InnerText.Contains(@"Name des Buchenden:")).ToList();
                    replace = sep.FirstOrDefault()?.InnerText;
                    replace = replace?.Substring(replace.IndexOf(@"Name des Buchenden:", StringComparison.Ordinal)).Replace("\r", "").Replace("\n", "")
                        .Trim().Replace(@"Name des Buchenden:","").Trim();
                    receipt.Passenger = replace;
                    passengerReceipts.Add(receipt);
                    toSendList.Add(mimeMessage.Value);
                }
            }
            return passengerReceipts.GroupBy(c=>c.Pnr).Select(s=>s.First()).ToList();
        }
    }
}
