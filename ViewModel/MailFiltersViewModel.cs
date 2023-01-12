using System.Collections.Generic;
using System.Collections.ObjectModel;
using AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels;

namespace AirlinePlanChanges_MailCenter.ViewModel
{
    internal class MailFiltersViewModel
    {
        public ObservableCollection<MailFilter> MailFilters { get; set; }

        public MailFiltersViewModel(IList<MailFilter> mails)
        {
            MailFilters=new ObservableCollection<MailFilter>();
            foreach (MailFilter mailFilter in mails)
            {
                MailFilters.Add(mailFilter);
            }
        }

        public MailFiltersViewModel()
        {
            MailFilters=new ObservableCollection<MailFilter>();
        }
    }
}
