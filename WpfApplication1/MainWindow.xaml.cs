using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using JetBrains.Annotations;
using WebApplication1.Models;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static HttpClient _client;
        private List<Setting> _items;
        private readonly string DatabaseFileName = "db.sqlite";
        private readonly string _databasePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)  + "\\" + "mighttrust_test1";
        private string _connectionString => string.Format("Data Source={0}\\{1};Version=3;", _databasePath, DatabaseFileName);

        public List<Setting> Items
        {
            get { return _items; }
            set
            {
                if (Equals(value, _items)) return;
                _items = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            //set datacontext to this, so listview could access Items
            DataContext = this;
            //load previous responce, if exists
            LoadFromDb();
        }

        private void CreateDb()
        {
            //database will be created in CommonAppicationData to avoid write restriction to db file in local folder under restricted user accounts

            if (!Directory.Exists(_databasePath))
                Directory.CreateDirectory(_databasePath);

            SQLiteConnection.CreateFile(_databasePath + "\\" + DatabaseFileName);
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                //assume what key and value will be no more than 1000 characters
                string sql = "CREATE TABLE items (key NVARCHAR(1000), value NVARCHAR(1000))";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
        private void LoadFromDb()
        {
            try
            {
                //create new db if not exist
                if (!File.Exists(_databasePath + "\\"+ DatabaseFileName))
                    CreateDb();

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    var q = "SELECT * FROM items";
                    using (var da = new SQLiteDataAdapter(q, connection))
                    {
                        using (var ds = new DataSet())
                        {
                            da.Fill(ds, "result");

                            //load items from db to Items collection
                            var items = new List<Setting>();
                            foreach (DataRow row in ds.Tables["result"].Rows)
                            {
                                items.Add(new Setting
                                {
                                    Key = row[0].ToString(),
                                    Value = row[1].ToString()
                                });
                            }
                            Items = items;
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void SaveToDb(List<Setting> items)
        {
            try
            {
                //create new db if not exist
                if (!File.Exists(_databasePath + "\\" + DatabaseFileName))
                    CreateDb();

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        //clear previous responce
                        var sql = "DELETE FROM items; ";

                        //insert items. parametrized to avoid sql-injections
                        for (int i = 0; i < items.Count; i++)
                        {
                            var item = items[i];
                            sql += string.Format(" INSERT INTO items (key,value) VALUES(@key{0}, @value{0});", i);
                            command.Parameters.AddWithValue(string.Format("@key{0}", i), item.Key);
                            command.Parameters.AddWithValue(string.Format("@value{0}", i), item.Value);
                        }
                        command.CommandText = sql;
                        
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void QueryButton_OnClick(object sender, RoutedEventArgs e)
        {
            //if client not initialized, or if user has changed url
            if (_client == null || _client.BaseAddress != Properties.Settings.Default.Uri)
            {
                _client = new HttpClient();
                _client.BaseAddress = Properties.Settings.Default.Uri;
                _client.DefaultRequestHeaders.Accept.Clear();
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            //form a query from url and saved searchfield
            var query = _client.BaseAddress.PathAndQuery + Properties.Settings.Default.SearchField;

            //lock window and let user know what query is processing
            IsEnabled = false;
            LockGrid.Visibility = Visibility.Visible;

            await UpdateItemsAsync(query);

            //unlock window
            IsEnabled = true;
            LockGrid.Visibility = Visibility.Hidden;
        }

        private async Task UpdateItemsAsync(string query)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(query);
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    var items = await response.Content.ReadAsAsync<List<Setting>>();
                    SaveToDb(items);
                    Items = items;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            //save url and searchfield
            Properties.Settings.Default.Save();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
