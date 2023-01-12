using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AirlinePlanChanges_MailCenter.AppData.Database;
using AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels;
using AirlinePlanChanges_MailCenter.ViewModel;

namespace AirlinePlanChanges_MailCenter.View
{
    /// <summary>
    /// Interaction logic for MailFilters.xaml
    /// </summary>
    public partial class MailFilters
    {
        private readonly ConfigurationPage _configurationPage;
        private readonly MainWindow _mainWindow;
        private List<MailFilter> _mailFilters;
        private readonly string _connection;

        public MailFilters(ConfigurationPage configurationPage, MainWindow mainWindow, string connection)
        {
            InitializeComponent();
            _configurationPage = configurationPage;
            _mainWindow = mainWindow;
            _connection = connection;
            Refresh();
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            ConfigurationPage configuration=new ConfigurationPage(_configurationPage, _mainWindow);
            _mainWindow.MainWindowFrameFrame.Navigate(configuration);
        }

        private void AddFilter_OnClick(object sender, RoutedEventArgs e)
        {
            MailFilter filters =
                new MailFilter
                {
                    MailAddress = MailAddressesTextBox.Text,
                    MailThemes = ThemesTextBox.Text
                };
            if (String.IsNullOrWhiteSpace(filters.MailAddress) && String.IsNullOrWhiteSpace(filters.MailThemes))
            {
                return;
            }
            CallCenterContext context=new CallCenterContext(_connection);
            context.MailFilterses.Add(filters);
            context.SaveChanges();
            Refresh();
        }

        private void Refresh()
        {
            CallCenterContext context=new CallCenterContext(_connection);
            _mailFilters = context.MailFilterses.Any() ? context.MailFilterses.ToList() : new List<MailFilter>();
            MailFiltersViewModel model=new MailFiltersViewModel(_mailFilters);
            DataContext = model;
            FiltersListView.ItemsSource = model.MailFilters;
            MailAddressesTextBox.Text=String.Empty;
            ThemesTextBox.Text=String.Empty;
        }

        private void DeleteItemButton_OnClick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button) sender;
            MailFilter filterToRemove = (MailFilter) btn.DataContext;
            CallCenterContext context=new CallCenterContext(_connection);

            bool oldValidateOnSaveEnabled = context.Configuration.ValidateOnSaveEnabled;
            try
            {
                context.Configuration.ValidateOnSaveEnabled = false;
                context.MailFilterses.Attach(filterToRemove);
                context.Entry(filterToRemove).State = EntityState.Deleted;
                context.SaveChanges();
            }
            finally
            {
                context.Configuration.ValidateOnSaveEnabled = oldValidateOnSaveEnabled;
            }
            Refresh();
        }
    }
}
