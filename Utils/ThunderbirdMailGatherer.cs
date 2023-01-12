using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AirlinePlanChanges_MailCenter.AppData.Database;
using AirlinePlanChanges_MailCenter.Model;
using AirlinePlanChanges_MailCenter.View;
using HtmlAgilityPack;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MimeKit;
using MimeKit.Utils;

namespace AirlinePlanChanges_MailCenter.Utils
{
    internal class ThunderbirdMailGatherer
    {
        private readonly CurrentUserSettings _userSettings;

        internal ThunderbirdMailGatherer(CurrentUserSettings userSettings)
        {
            _userSettings = userSettings;
        }
        public Dictionary<UniqueId, MimeMessage> GetMailsFromImapDefined(string folderName, int year, int month, int day)
        {
            Dictionary<UniqueId, MimeMessage> list = new Dictionary<UniqueId, MimeMessage>();
            using (var client = new ImapClient())
            {
                client.Connect("remote.test.de", 993, true);
                client.Authenticate("test@test.de", "test");
                var folder = client.Inbox.GetSubfolder(folderName);
                folder.Open(FolderAccess.ReadOnly);
                var uids = folder.Search(SearchQuery.DeliveredAfter(new DateTime(year, month, day)));
                foreach (UniqueId uid in uids)
                {
                    var message = folder.GetMessage(uid);
                    list.Add(uid, message);
                }
            }
            return list;
        }

        public void PutMailsToImapDefined(string folderName, List<MimeMessage> messages)
        {
            using (var client = new ImapClient())
            {
                client.Connect("remote.test.de", 993, true);
                client.Authenticate("test@test.de", "test");
                var folder = client.Inbox.GetSubfolder(folderName);
                folder.Open(FolderAccess.ReadWrite);
                var mails = messages.GroupBy(c => c.MessageId).Select(s => s.First()).OrderBy(c => c.Date).ToList();
                var endDateTimeOffset = mails.First().Date;
                var startDateTimeOffset = mails.Last().Date;

                var uidsAfter = folder.Search(SearchQuery.SentAfter(new DateTime(endDateTimeOffset.Year, endDateTimeOffset.Month, endDateTimeOffset.Day)));
                var uidsCurrent = folder.Search(SearchQuery.SentOn(new DateTime(startDateTimeOffset.Year, startDateTimeOffset.Month, startDateTimeOffset.Day)));

                var beforeMails = new List<MimeMessage>();
                var currentMails = new List<MimeMessage>();
                foreach (var mimeMessage in mails)
                {
                    if (mimeMessage.Date.Date == startDateTimeOffset.Date)
                    {
                        currentMails.Add(mimeMessage);
                    }
                    else
                    {
                        beforeMails.Add(mimeMessage);
                    }
                }

                int before = uidsAfter.Count - uidsCurrent.Count;
                int current = uidsCurrent.Count;
                if (before == 0 && current == 0)
                {
                    foreach (var message in messages)
                    {
                        folder.Append(message);
                    }
                }
                else
                {
                    if (beforeMails.Count() != before)
                    {
                        foreach (var message in messages)
                        {
                            folder.Append(message);
                        }
                    }

                    if (currentMails.Count() != current)
                    {
                        var currs = new List<MimeMessage>();
                        foreach (var uniqueId in uidsCurrent)
                        {
                            currs.Add(folder.GetMessage(uniqueId));
                        }

                        foreach (var message in messages)
                        {

                            if (currs.FirstOrDefault(c => c.MessageId == message.MessageId) != null)
                            {
                                continue;
                            }

                            folder.Append(message);
                        }
                    }
                }
            }
        }

        public Tuple<List<MimeMessage>, int> GetMailsFromImap(string folderName)
        {
            List<MimeMessage> list = new List<MimeMessage>();
            using (var client = new ImapClient())
            {
                client.Connect(_userSettings.EmailSetting.ServerNameOfIncomeMessages, _userSettings.EmailSetting.ServerPortOfIncomeMessages, true);
                client.Authenticate(_userSettings.EmailSetting.EmailAddress, _userSettings.EmailSetting.EmailPassword);
                var folder = client.Inbox.GetSubfolder(folderName);
                folder.Open(FolderAccess.ReadOnly);
                var uids = folder.Search(SearchQuery.All);
                foreach (var uid in uids)
                {
                    var message = folder.GetMessage(uid);
                    list.Add(message);
                }
            }
            return new Tuple<List<MimeMessage>, int>(list, list.Count);
        }

        public Tuple<List<MimeMessage>, int> GetMailsFromFolder()
        {
            string dirPath = _userSettings.MozillaFtcFilePath;
            string inboxPath = Path.Combine(dirPath);
            List<MimeMessage> messagesList = new List<MimeMessage>();

            using (var stream = File.OpenRead(inboxPath))
            {
                MimeParser mimeParser = new MimeParser(stream, MimeFormat.Mbox);
                while (!mimeParser.IsEndOfStream)
                {
                    messagesList.Add(mimeParser.ParseMessage());
                }
            }
            CallCenterContext context = new CallCenterContext((new SqlConnectionStringBuilder
            {
                DataSource = _userSettings.DatabaseSetting.DatabaseDataSource,
                AttachDBFilename = _userSettings.DatabaseSetting.DatabaseLocalPathString,
                InitialCatalog = _userSettings.DatabaseSetting.DatabaseInitialCatalog,
                UserID = _userSettings.DatabaseSetting.DatabaseUserName,
                Password = _userSettings.DatabaseSetting.DatabasePassword,
                IntegratedSecurity = _userSettings.DatabaseSetting.DatabaseIntegratedSecurity
            }).ToString());

            var filters =
                context.MailFilterses.Select(c => new { Address = c.MailAddress, Thema = c.MailThemes }).ToList();
            return new Tuple<List<MimeMessage>, int>(messagesList, filters.Count);
        }

        public Dictionary<string, List<MimeMessage>> GetMails(DateTime? startDate, DateTime? endDate, MainPage mainPage, IProgress<int> progress, List<MimeMessage> messagesList)
        {
            Dictionary<string, List<MimeMessage>> result = new Dictionary<string, List<MimeMessage>>();
            int count = 0;
            CallCenterContext context = new CallCenterContext((new SqlConnectionStringBuilder
            {
                DataSource = _userSettings.DatabaseSetting.DatabaseDataSource,
                AttachDBFilename = _userSettings.DatabaseSetting.DatabaseLocalPathString,
                InitialCatalog = _userSettings.DatabaseSetting.DatabaseInitialCatalog,
                UserID = _userSettings.DatabaseSetting.DatabaseUserName,
                Password = _userSettings.DatabaseSetting.DatabasePassword,
                IntegratedSecurity = _userSettings.DatabaseSetting.DatabaseIntegratedSecurity
            }).ToString());
            var filters =
                context.MailFilterses.Select(c => new { Address = c.MailAddress, Thema = c.MailThemes }).ToList();
            foreach (var filter in filters)
            {
                if (filter.Address.Contains("eurowings.com"))
                {
                    var items = messagesList.Where(c =>
                        c.Subject.Contains(filter.Thema) && c.From.ToString().Contains(filter.Address) && c.Date >= startDate && c.Date <= endDate).ToList();
                    if (result.ContainsKey("GWI"))
                    {
                        result["GWI"].AddRange(items);
                        continue;
                    }
                    result.Add("GWI", items);
                }
                if (filter.Address.Contains("myreis.com"))
                {
                    var items = messagesList.Where(c =>
                        c.Subject.Contains(filter.Thema) && c.From.ToString().Contains(filter.Address) && c.Date >= startDate && c.Date <= endDate).ToList();
                    if (result.ContainsKey("AT"))
                    {
                        result["AT"].AddRange(items);
                        continue;
                    }
                    result.Add("AT", items);
                }
                progress.Report(++count);

            }
            return result;
        }


        public ObservableCollection<Tuple<MimeMessage, MessageToSendInfoModel>> SetMessagesForSend(Dictionary<string, List<MimeMessage>> messagesToLookup, MainPage mainPage, IProgress<int> progress)
        {
            ObservableCollection<Tuple<MimeMessage, MessageToSendInfoModel>> toSend = new ObservableCollection<Tuple<MimeMessage, MessageToSendInfoModel>>();

            CallCenterContext context = new CallCenterContext((new SqlConnectionStringBuilder
            {
                DataSource = _userSettings.DatabaseSetting.DatabaseDataSource,
                AttachDBFilename = _userSettings.DatabaseSetting.DatabaseLocalPathString,
                InitialCatalog = _userSettings.DatabaseSetting.DatabaseInitialCatalog,
                UserID = _userSettings.DatabaseSetting.DatabaseUserName,
                Password = _userSettings.DatabaseSetting.DatabasePassword,
                IntegratedSecurity = _userSettings.DatabaseSetting.DatabaseIntegratedSecurity
            }).ToString());
            context.Database.CommandTimeout = 360;

            int count = 0;
            foreach (KeyValuePair<string, List<MimeMessage>> pair in messagesToLookup)
            {
                if (pair.Key.Equals("GWI"))
                {
                    foreach (var itemMessage in pair.Value)
                    {
                        try
                        {
                            var message = new MimeMessage();
                            message.From.Add(new MailboxAddress(_userSettings.EmailSetting.EmailUserName, _userSettings.EmailSetting.EmailAddress));
                            message.Subject = itemMessage.Subject;
                            var html = itemMessage.HtmlBody.Replace("\r", "").Replace("\n", "");

                            HtmlDocument hap = new HtmlDocument();
                            hap.LoadHtml(html);

                            var nodes = hap.DocumentNode.Descendants("span").Where(d =>
                                d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("Content"));
                            foreach (HtmlNode htmlNode in nodes)
                            {
                                HtmlNodeCollection htmlNodeChildNodes = htmlNode.ChildNodes;
                                for (int i = htmlNodeChildNodes.Count - 1; i >= 0; i--)
                                {
                                    if (htmlNodeChildNodes[i].InnerHtml.Contains("test. 18"))
                                    {
                                        htmlNodeChildNodes[i].Remove();
                                    }
                                    if (htmlNodeChildNodes[i].InnerHtml.Contains("10000 test"))
                                    {
                                        htmlNodeChildNodes[i].Remove();
                                    }
                                    if (htmlNodeChildNodes[i].InnerHtml.Contains("Germany"))
                                    {
                                        htmlNodeChildNodes[i].Remove();
                                    }
                                    if (htmlNodeChildNodes[i].Name.Equals("br") && i < 5 && i > 1)
                                    {
                                        htmlNodeChildNodes[i].Remove();
                                    }
                                }
                            }
                            nodes = hap.DocumentNode.Descendants("td").Where(d =>
                                d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("PNR"));
                            string pnr = String.Empty;
                            foreach (HtmlNode htmlNode in nodes)
                            {
                                pnr = htmlNode.InnerText;
                            }
                            using (TextWriter writer = new StringWriter())
                            {
                                hap.Save(writer);
                                html = writer.ToString();
                            }
                            BodyBuilder targetBuilder = new BodyBuilder { HtmlBody = html };
                            message.Body = targetBuilder.ToMessageBody();
                            if (String.IsNullOrWhiteSpace(pnr))
                            {
                                string subject = itemMessage.Subject;
                                pnr = subject.Replace(" ", "");
                                pnr = pnr.Substring(pnr.IndexOf('/') - 6, 6).Trim();
                            }
                            var order = context.FullReports.FirstOrDefault(c => c.FirstGdsBookingNumber.Contains(pnr));

                            if (order != null)
                            {
                                message.To.Add(new MailboxAddress(order.CustomerEmail));
                            }

                            int bookingNumber = 0;
                            if (order != null)
                            {
                                bookingNumber = order.BookingNumber;
                            }
                            string changesDate;

                            changesDate = itemMessage.Date.ToString("dd/MM/yyyy");
                            message.Cc.Add(new MailboxAddress("test@test.de"));
                            MessageToSendInfoModel model = new MessageToSendInfoModel
                            {
                                BookingNumber = bookingNumber,
                                Pnr = pnr,
                                Subject = message.Subject,
                                SourceMessageId = itemMessage.MessageId,
                                ChangesDate = changesDate,
                                Index = toSend.Count + 1,
                                CustomerMail = message.To.ToString()
                            };
                            toSend.Add(new Tuple<MimeMessage, MessageToSendInfoModel>(message, model));
                            progress.Report(++count);

                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                else if (pair.Key.Equals("AT"))
                {
                    ObservableCollection<Tuple<MimeMessage, MessageToSendInfoModel>> atMails = PrepareMailsFromAtFromMails(pair.Value, context, toSend.Count);
                    // ReSharper disable once UnusedVariable
                    var enumerable = toSend.Concat(atMails);
                }
            }
            return toSend;
        }

        public ObservableCollection<Tuple<MimeMessage, MessageToSendInfoModel>> PrepareMailsFromAtFromFile(List<AtPnrHierarchy> content)
        {
            ObservableCollection<Tuple<MimeMessage, MessageToSendInfoModel>> list = new ObservableCollection<Tuple<MimeMessage, MessageToSendInfoModel>>();
            CallCenterContext context = new CallCenterContext((new SqlConnectionStringBuilder
            {
                DataSource = _userSettings.DatabaseSetting.DatabaseDataSource,
                AttachDBFilename = _userSettings.DatabaseSetting.DatabaseLocalPathString,
                InitialCatalog = _userSettings.DatabaseSetting.DatabaseInitialCatalog,
                UserID = _userSettings.DatabaseSetting.DatabaseUserName,
                Password = _userSettings.DatabaseSetting.DatabasePassword,
                IntegratedSecurity = _userSettings.DatabaseSetting.DatabaseIntegratedSecurity
            }).ToString());
            context.Database.CommandTimeout = 360;

            List<GdsMailInfo> gdsMailList = new List<GdsMailInfo>();

            foreach (AtPnrHierarchy at in content)
            {
                GdsMailInfo gds = new GdsMailInfo();
                gds.Pnr = at.Pnr;
                foreach (var item in at.InfoRows)
                {
                    string line = item;
                    RouteInfoChanges info = new RouteInfoChanges();
                    //Берем первые два символа
                    string targetString = line.Substring(0, 2);
                    //Если эти два символа являются числовыми, то убираем два символа из строки, иначе убираем один символ
                    line = (Int32.TryParse(targetString, out int j)) ? line.Substring(2) : line.Substring(1);
                    //Следующие 2 символа - ИАТА код авиакомпании
                    info.AirlineCode = line.Substring(0, 2);
                    line = line.Substring(2);
                    targetString = String.Empty;
                    //Определяем номер рейса, посимвольно считывая данные со строки. Если символ не числовой, прекращаем считывание
                    for (int k = 0; k < line.Length; k++)
                    {
                        if (line[k] >= '0' && line[k] <= '9')
                        {
                            targetString += line[k];
                        }
                        else
                        {
                            break;
                        }
                    }
                    info.AirlineNumber = targetString;
                    line = line.Substring(targetString.Length);
                    //Класс бронирования
                    targetString = line.Substring(0, 1);
                    if (Int32.TryParse(targetString, out j) || targetString.Equals("-"))
                    {
                        info.AirlineClass = " ";
                    }
                    else
                    {
                        info.AirlineClass = targetString;
                    }
                    line = line.Substring(1);
                    //Дата вылета - 5 символов
                    info.DepartureDate = line.Substring(0, 5);
                    line = line.Substring(5);
                    //Убираем число
                    targetString = line.Substring(0, 1);
                    if (targetString.ToCharArray()[0] >= '0' && targetString.ToCharArray()[0] <= '9') line = line.Substring(1);
                    info.DepartureRoute = line.Substring(0, 6);
                    line = line.Substring(6);
                    targetString = line.Substring(0, 3);
                    info.Status = GdsMailInfo.GetStatus(targetString);
                    if (info.Status.Equals("NO DATA"))
                    {
                        continue;
                    }
                    Int32.TryParse(targetString.Substring(2), out j);
                    info.PassengersAmount = j;
                    line = line.Substring(3);
                    targetString = line.Substring(0, 4);
                    if (Int32.TryParse(targetString, out j))
                    {
                        info.DepartureTime = targetString;
                        line = line.Substring(4);
                    }
                    try
                    {
                        targetString = line.Substring(0, 4);
                        if (Int32.TryParse(targetString, out j))
                        {
                            info.ArrivalTime = targetString;
                        }
                    }
                    catch (ArgumentOutOfRangeException ex1)
                    {
                        Console.WriteLine(ex1.Message);
                    }
                    catch (IndexOutOfRangeException ex2)
                    {
                        Console.WriteLine(ex2.Message);
                    }
                    gds.RouteChanges.Add(info);
                }
                gdsMailList.Add(gds);
            }
            if (gdsMailList[0] == null)
            {
                return null;
            }
            int counter = 0;
            List<GdsMailInfo> sortedGdsMailList = gdsMailList.OrderBy(x => x.Pnr).ToList();
            foreach (GdsMailInfo gdsMailInfo in sortedGdsMailList)
            {
                var messageToSend = new MimeMessage();
                MessageToSendInfoModel model = new MessageToSendInfoModel();
                messageToSend.From.Add(new MailboxAddress(_userSettings.EmailSetting.EmailUserName, _userSettings.EmailSetting.EmailAddress));
                var order = context.FullReports.FirstOrDefault(c =>
                    c.FirstGdsBookingNumber.Equals(gdsMailInfo.Pnr));
                int bookingNumber = 0;
                string customerMail = String.Empty;
                string customerFullName = "Damen und Herren";
                if (order != null)
                {
                    bookingNumber = order.BookingNumber;
                    customerMail = order.CustomerEmail;
                    customerFullName = order.CustomerFullName;
                }
                messageToSend.Subject =
                    "Flugzeitenänderung PNR " + gdsMailInfo.Pnr + " / Referenznummer " +
                    bookingNumber;
                messageToSend.To.Add(new MailboxAddress(customerMail));
                BodyBuilder customerMessage = new BodyBuilder();
                string htmlText =
                    "<!DOCTYPE html><html lang=\'de\'><head><meta charset=\'UTF-8\'><title>test GmbH</title><style type=\'text/css\'> body{ background-color: #ffffff;font-family: Arial,serif;} .SmallColumn{ width: 50px;padding: 1px;} .BigColumn{ width: 100px; padding: 1px;} .OK{ background-color: green;} .CHANGED{ background-color: yellow; font-weight: bold;} .CANCELLED{ background-color: red; font-weight: bold; } </style></head><body><h2 style=\'font-size: xx-large; font-weight: bolder; color: red\'>Flugzeitenänderung!</h2><p>Sehr geehrte [Customer Full Name],</p><p>Ihre Flugdaten haben sich geändert!</p><p>Die Flugzeitenänderung bezieht sich auf alle, unter der eingegebener Referenznummer gebuchten Passagieren.</p><p>Bitte beachten Sie die Flugzeitenänderung auf folgender Strecke:</p><p style=\'font-size: xx-large; font-weight: bolder\'>INFORMATION:</p><table style=\'border: solid 3px\'><thead style=\'font-size: 14px; background-color: lightslategray; font-weight: bolder; text-align: center\'><tr><td class=\'SmallColumn\'>Airline<br/>code</td><td class=\'BigColumn\'>Airline<br/>number</td><td class=\'SmallColumn\'>Airline<br/>class</td><td class=\'BigColumn\'>Departure<br/>date</td><td class=\'BigColumn\'>Route</td><td class=\'BigColumn\'>Departure<br/>time</td><td class=\'BigColumn\'>Arrival<br/>time</td><td class=\'BigColumn\'>Status</td></tr></thead><tbody style=\'background-color: white; font-size: 14px\'>[TROW]</tbody></table><p>Bitte bestätigen Sie uns die Änderung unter:</p><p>E-Mail: test@test.de oder Service Center Hotline:</p><p>Mo. - Fr. 08:30 - 17:00, Sa. 08:00 - 14:00</p><p>Tel.+49 030 </p><p>Vielen Dank für Ihr Verständnis.</p></body></html>";
                string row = String.Empty;
                string body = htmlText.Replace("[Customer Full Name]", customerFullName);
                foreach (RouteInfoChanges routeInfoChangese in gdsMailInfo.RouteChanges)
                {
                    row += "<tr><td class=\'SmallColumn\'>" + routeInfoChangese.AirlineCode +
                           "</td><td class='BigColumn'>" + routeInfoChangese.AirlineNumber +
                           "</td><td class='SmallColumn'>" + routeInfoChangese.AirlineClass +
                           "</td><td class='BigColumn'>" + routeInfoChangese.DepartureDate +
                           "</td><td class='BigColumn'>" + routeInfoChangese.DepartureRoute +
                           "</td><td class='BigColumn'>" + routeInfoChangese.DepartureTime +
                           "</td><td class='BigColumn'>" + routeInfoChangese.ArrivalTime +
                           "</td><td class='BigColumn " + routeInfoChangese.Status + "'>" +
                           routeInfoChangese.Status + "</td></tr>";
                }
                body = body.Replace("[TROW]", row);
                customerMessage.HtmlBody = body;
                messageToSend.Body = customerMessage.ToMessageBody();
                messageToSend.Cc.Add(new MailboxAddress("test@test.de"));
                model.Pnr = gdsMailInfo.Pnr;
                model.BookingNumber = bookingNumber;
                model.Index = counter + 1;
                model.Subject = messageToSend.Subject;
                model.ChangesDate = gdsMailInfo.RouteChanges[0].DepartureDate;
                model.CustomerMail = messageToSend.To.ToString();
                list.Add(new Tuple<MimeMessage, MessageToSendInfoModel>(messageToSend, model));
                counter++;
            }
            return list;
        }

        public ObservableCollection<Tuple<MimeMessage, MessageToSendInfoModel>> PrepareMailsFromAtFromMails(List<MimeMessage> values, CallCenterContext context, int counter)
        {
            ObservableCollection<Tuple<MimeMessage, MessageToSendInfoModel>> list = new ObservableCollection<Tuple<MimeMessage, MessageToSendInfoModel>>();
            foreach (var message in values)
            {
                try
                {
                    List<GdsMailInfo> gdsMailList = new List<GdsMailInfo>();
                    using (TextReader reader = new StringReader(message.TextBody))
                    {
                        GdsMailInfo gds = null;
                        string tstr;
                        bool flag = false;
                        while ((tstr = reader.ReadLine()) != null)
                        {
                            string str = tstr.Replace(" ", "");
                            if (str.Contains("уважен"))
                            {
                                gdsMailList.Add(gds);
                                break;
                            }
                            if (String.IsNullOrWhiteSpace(str))
                            {
                                continue;
                            }
                            if (str.Length == 6)
                            {
                                if (flag)
                                {
                                    gdsMailList.Add(gds);
                                }
                                gds = new GdsMailInfo { Pnr = str };
                                flag = true;
                                continue;
                            }
                            if (!String.IsNullOrWhiteSpace(str) && flag)
                            {
                                string line = str.Replace(" ", "").Replace(".", "").Replace(",", "");
                                if (line.Contains("/") || line.Contains("\\") || line.Contains("!"))
                                {
                                    continue;
                                }
                                string targetString = line.Substring(0, 2);
                                line = (Int32.TryParse(targetString, out int j)) ? line.Substring(2) : line.Substring(1);

                                RouteInfoChanges info = new RouteInfoChanges();
                                targetString = line.Substring(0, 2);
                                info.AirlineCode = targetString;
                                line = line.Substring(2);
                                targetString = String.Empty;
                                for (int k = 0; k < line.Length; k++)
                                {
                                    if (line[k] >= '0' && line[k] <= '9')
                                    {
                                        targetString += line[k];
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                info.AirlineNumber = targetString;
                                line = line.Substring(targetString.Length);
                                targetString = line.Substring(0, 1);
                                info.AirlineClass = targetString;
                                line = line.Substring(1);
                                targetString = line.Substring(0, 5);
                                info.DepartureDate = targetString;
                                line = line.Substring(5);
                                targetString = line.Substring(0, 2);
                                if (Int32.TryParse(targetString, out j))
                                {
                                    info.PassengersAmount = j;
                                    line = line.Substring(2);
                                }
                                else
                                {
                                    targetString = line.Substring(0, 1);
                                    info.PassengersAmount = Int32.Parse(targetString);
                                    line = line.Substring(1);
                                }
                                targetString = line.Substring(0, 6);
                                info.DepartureRoute = targetString;
                                line = line.Substring(6);
                                targetString = line.Substring(0, 3);
                                info.Status = GdsMailInfo.GetStatus(targetString);
                                if (info.Status.Equals("NO DATA"))
                                {
                                    continue;
                                }
                                line = line.Substring(3);
                                targetString = line.Substring(0, 4);
                                info.DepartureTime = targetString;
                                line = line.Substring(4);
                                targetString = line.Substring(0, 4);
                                info.ArrivalTime = targetString;
                                gds.RouteChanges.Add(info);
                            }
                        }
                    }
                    if (gdsMailList[0] == null)
                    {
                        continue;
                    }
                    foreach (GdsMailInfo gdsMailInfo in gdsMailList)
                    {
                        var messageToSend = new MimeMessage();
                        MessageToSendInfoModel model = new MessageToSendInfoModel();
                        messageToSend.From.Add(new MailboxAddress(_userSettings.EmailSetting.EmailUserName, _userSettings.EmailSetting.EmailAddress));
                        var order = context.FullReports.FirstOrDefault(c =>
                            c.FirstGdsBookingNumber.Equals(gdsMailInfo.Pnr));
                        int bookingNumber = 0;
                        string customerMail = String.Empty;
                        string customerFullName = "Damen und Herren";
                        if (order != null)
                        {
                            bookingNumber = order.BookingNumber;
                            customerMail = order.CustomerEmail;
                            customerFullName = order.CustomerFullName;
                        }
                        messageToSend.Subject =
                            "Flugzeitenänderung PNR " + gdsMailInfo.Pnr + " / Referenznummer " +
                            bookingNumber;
                        messageToSend.To.Add(new MailboxAddress(customerMail));
                        //messageToSend.To.Add(new MailboxAddress("kparaskevopulo@berlogic.de"));
                        //messageToSend.To.Add(new MailboxAddress("Ruslan Sadykov", "rsadykov@berlogic.de"));
                        BodyBuilder customerMessage = new BodyBuilder();
                        string htmlText =
                            "<!DOCTYPE html><html lang=\'de\'><head><meta charset=\'UTF-8\'><title>test GmbH</title><style type=\'text/css\'> body{ background-color: #ffffff;font-family: Arial,serif;} .SmallColumn{ width: 50px;padding: 1px;} .BigColumn{ width: 100px; padding: 1px;} .OK{ background-color: green;} .CHANGED{ background-color: yellow; font-weight: bold;} .CANCELLED{ background-color: red; font-weight: bold; } </style></head><body><h2 style=\'font-size: xx-large; font-weight: bolder; color: red\'>Flugzeitenänderung!</h2><p>Sehr geehrte [Customer Full Name],</p><p>Ihre Flugdaten haben sich geändert!</p><p>Die Flugzeitenänderung bezieht sich auf alle, unter der eingegebener Referenznummer gebuchten Passagieren.</p><p>Bitte beachten Sie die Flugzeitenänderung auf folgender Strecke:</p><p style=\'font-size: xx-large; font-weight: bolder\'>INFORMATION:</p><table style=\'border: solid 3px\'><thead style=\'font-size: 14px; background-color: lightslategray; font-weight: bolder; text-align: center\'><tr><td class=\'SmallColumn\'>Airline<br/>code</td><td class=\'BigColumn\'>Airline<br/>number</td><td class=\'SmallColumn\'>Airline<br/>class</td><td class=\'BigColumn\'>Departure<br/>date</td><td class=\'BigColumn\'>Route</td><td class=\'BigColumn\'>Departure<br/>time</td><td class=\'BigColumn\'>Arrival<br/>time</td><td class=\'BigColumn\'>Status</td></tr></thead><tbody style=\'background-color: white; font-size: 14px\'>[TROW]</tbody></table><p>Bitte bestätigen Sie uns die Änderung unter:</p><p>E-Mail: test@test.de oder Service Center Hotline:</p><p>Mo. - Fr. 08:30 - 17:00, Sa. 08:00 - 14:00</p><p>Tel.+49...50</p><p>Vielen Dank für Ihr Verständnis.</p></body></html>";
                        string row = String.Empty;
                        string body = htmlText.Replace("[Customer Full Name]", customerFullName);
                        foreach (RouteInfoChanges routeInfoChangese in gdsMailInfo.RouteChanges)
                        {
                            row += "<tr><td class=\'SmallColumn\'>" + routeInfoChangese.AirlineCode +
                                   "</td><td class='BigColumn'>" + routeInfoChangese.AirlineNumber +
                                   "</td><td class='SmallColumn'>" + routeInfoChangese.AirlineClass +
                                   "</td><td class='BigColumn'>" + routeInfoChangese.DepartureDate +
                                   "</td><td class='BigColumn'>" + routeInfoChangese.DepartureRoute +
                                   "</td><td class='BigColumn'>" + routeInfoChangese.DepartureTime +
                                   "</td><td class='BigColumn'>" + routeInfoChangese.ArrivalTime +
                                   "</td><td class='BigColumn " + routeInfoChangese.Status + "'>" +
                                   routeInfoChangese.Status + "</td></tr>";
                        }
                        body = body.Replace("[TROW]", row);
                        customerMessage.HtmlBody = body;
                        messageToSend.Body = customerMessage.ToMessageBody();
                        messageToSend.Cc.Add(new MailboxAddress("test@test.de"));
                        model.Pnr = gdsMailInfo.Pnr;
                        model.BookingNumber = bookingNumber;
                        model.Index = counter + 1;
                        model.SourceMessageId = message.MessageId;
                        model.Subject = messageToSend.Subject;
                        model.ChangesDate = gdsMailInfo.RouteChanges[0].DepartureDate;
                        model.CustomerMail = messageToSend.To.ToString();
                        list.Add(new Tuple<MimeMessage, MessageToSendInfoModel>(messageToSend, model));
                    }
                }
                catch
                {
                    // ignored
                }
            }
            return list;
        }

        public string Send(MainPage mainPage, CurrentUserSettings toSend, List<Tuple<MimeMessage, MessageToSendInfoModel>> messagesToSend, Dictionary<string, List<MimeMessage>> messagesToLookup)
        {
            string report = "<!DOCTYPE html><html lang='en'><head><meta charset='UTF-8'><title>test GmbH</title></head><body><h3>Report</h3><table><thead><tr><td>#</td><td>Date</td><td>From</td><td>Subject</td><td>Message date</td><td>Pnr list</td><td>Status</td></tr></thead><tbody>";
            int count = 0;
            // ReSharper disable once NotAccessedVariable
            int baseCount = 0;
            var messagesIds = messagesToSend.Select(c => c.Item2.SourceMessageId).Distinct();
            var baseMessageValues = messagesToLookup.SelectMany(c => c.Value).ToList();
            using (var client = new SmtpClient())
            {
                client.Connect(toSend.EmailSetting.ServerNameOfOutcomeMessages, toSend.EmailSetting.ServerPortOfOutcomeMessages);
                client.Authenticate(toSend.EmailSetting.EmailAddress, toSend.EmailSetting.EmailPassword);

                foreach (string messagesId in messagesIds)
                {
                    MimeMessage baseMail = baseMessageValues.Find(c => c.MessageId == messagesId);
                    baseCount++;
                    List<Tuple<MimeMessage, MessageToSendInfoModel>> targetMail =
                    messagesToSend.Where(c => c.Item2.SourceMessageId == messagesId).ToList();
                    var messageToFoward = new MimeMessage();
                    messageToFoward.ResentFrom.Add(new MailboxAddress(_userSettings.EmailSetting.EmailUserName, _userSettings.EmailSetting.EmailAddress));
                    messageToFoward.ResentReplyTo.Add(new MailboxAddress("test@test.de"));
                    messageToFoward.ResentMessageId = MimeUtils.GenerateMessageId();
                    messageToFoward.ResentDate = DateTimeOffset.Now;
                    messageToFoward.Subject = baseMail.Subject;
                    var builder = new BodyBuilder();
                    builder.Attachments.Add(new MessagePart { Message = baseMail });
                    report +=
                        "<tr><td>" + count + "</td><td>" + DateTime.Now.ToLocalTime() + "</td><td>" + baseMail.From +
                        "</td><td>" + baseMail.Subject + "</td><td>" + baseMail.Date + "</td><td>";
                    foreach (Tuple<MimeMessage, MessageToSendInfoModel> tuple in targetMail)
                    {
                        builder.TextBody += "PNR: " + tuple.Item2.Pnr + ". Send status: ";
                        try
                        {
                            client.Send(tuple.Item1);
                            builder.TextBody += "OK" + Environment.NewLine;
                            count++;
                            report += tuple.Item2.Pnr + ",";
                            Task.Delay(1000).Wait();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            builder.TextBody += "FAILED" + Environment.NewLine;
                        }
                    }
                    messageToFoward.Body = builder.ToMessageBody();
                    report += "</td><td>";
                    try
                    {
                        client.Send(messageToFoward);
                        report += "OK</td></tr>";
                        Task.Delay(1000).Wait();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        report += "FAILED to RESEND</td></tr>";
                    }
                }

                client.Disconnect(true);
            }
            report += "</tbody></table></body></html>";
            mainPage.SuccessfulSendedMailsLabel.Content = count.ToString();
            return report;
        }
        public string Send(MainPage mainPage, CurrentUserSettings toSend, List<Tuple<MimeMessage, MessageToSendInfoModel>> messagesToSend)
        {
            string report = "<!DOCTYPE html><html lang='en'><head><meta charset='UTF-8'><title>test GmbH</title></head><body><h3>Report</h3><table><thead><tr><td>#</td><td>Date</td><td>From</td><td>Subject</td><td>Message date</td><td>Pnr list</td><td>Status</td></tr></thead><tbody>";
            int count = 0;
            using (var client = new SmtpClient())
            {
                client.Connect(toSend.EmailSetting.ServerNameOfOutcomeMessages, toSend.EmailSetting.ServerPortOfOutcomeMessages);
                client.Authenticate(toSend.EmailSetting.EmailAddress, toSend.EmailSetting.EmailPassword);
                var messageToFoward = new MimeMessage();
                messageToFoward.From.Add(new MailboxAddress(_userSettings.EmailSetting.EmailUserName, _userSettings.EmailSetting.EmailAddress));
                messageToFoward.To.Add(new MailboxAddress("test@test.de"));
                messageToFoward.MessageId = MimeUtils.GenerateMessageId();
                messageToFoward.Date = DateTimeOffset.Now;
                messageToFoward.Subject = "AT Mails from file sent";
                var builder = new BodyBuilder();
                report += "<tr><td>" + count + "</td><td>" + DateTime.Now.ToLocalTime() + "</td><td>test Travel</td><td>GDS FTC Mails</td><td>from File</td><td>";
                foreach (Tuple<MimeMessage, MessageToSendInfoModel> tuple in messagesToSend)
                {
                    builder.TextBody += "PNR: " + tuple.Item2.Pnr + ". Send status: ";
                    try
                    {
                        client.Send(tuple.Item1);
                        builder.TextBody += "OK" + Environment.NewLine;
                        count++;
                        report += tuple.Item2.Pnr + " - OK,<br/>";
                        Task.Delay(1000).Wait();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        builder.TextBody += "FAILED" + Environment.NewLine;
                        report += tuple.Item2.Pnr + " - FAILED<br/>";
                    }
                }
                messageToFoward.Body = builder.ToMessageBody();
                report += "</td><td>";
                try
                {
                    client.Send(messageToFoward);
                    report += "OK</td></tr>";
                    Task.Delay(1000).Wait();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    report += "FAILED to RESEND</td></tr>";
                }
                client.Disconnect(true);
            }
            report += "</tbody></table></body></html>";
            mainPage.SuccessfulSendedMailsLabel.Content = count.ToString();
            return report;
        }
    }
}
