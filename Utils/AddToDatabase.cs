using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using AirlinePlanChanges_MailCenter.AppData.Database.CallCenterDatabaseModels;

namespace AirlinePlanChanges_MailCenter.Utils
{
    internal class AddToDatabase
    {
        private readonly string _connectionString;
        public AddToDatabase(string connectionPath)
        {
            _connectionString = connectionPath;
        }

        public bool AddToFullReport(List<FullReport> report)
        {
            using (var connection=new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (var sbCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        sbCopy.BulkCopyTimeout = 0;
                        sbCopy.BatchSize = 100;
                        sbCopy.DestinationTableName = "FullReports";
                        var reader = report.AsDataTable();
                        reader.Columns.Remove("CustomerFullName");
                        sbCopy.WriteToServer(reader);
                    }
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + Environment.NewLine+e.HelpLink,"Error in Full Report saving in DB", MessageBoxButton.OK,MessageBoxImage.Error) ;
                    transaction.Rollback();
                    return false;
                }
                finally
                {
                    transaction.Dispose();
                    connection.Close();
                }
                return true;
            }
        }
    }
}
