using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;
using System.Security.Cryptography;

namespace Modification
{
    public partial class Form1 : Form
    {
        static string sConnectionStr = "Server=localhost; Port=5432; User Id=postgres; Password=1234765; Database=lab3uchbd";
        NpgsqlConnection sConnection = new NpgsqlConnection(sConnectionStr);

        public Form1()
        {
            InitializeComponent();
            InitializeListView();
        }

        private string GenSalt(int length)
        {
            RNGCryptoServiceProvider s = new RNGCryptoServiceProvider();
            var salt = new byte[length];
            s.GetBytes(salt);
            return Convert.ToBase64String(salt);
        }

        public string CalcHash(string text)
        {
            SHA1CryptoServiceProvider hash = new SHA1CryptoServiceProvider();
            return Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(text)));
        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        public void InitializeListView()
        {
            listView1.Columns.Add("Логин");
            listView1.Columns.Add("Хеш");
            listView1.Columns.Add("Соль");
            listView1.Columns.Add("Дата регистрации");
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.MultiSelect = false;
            sConnection.Open();
            using (var sCommand = new NpgsqlCommand())
            {
                sCommand.Connection = sConnection;
                sCommand.CommandText = "SELECT * FROM users";
                NpgsqlDataReader reader = sCommand.ExecuteReader();
                while (reader.Read())
                {
                    var item = new ListViewItem(new string[] { (string)reader["login"], (string)reader["hash"],
                        (string)reader["salt"], ((string)reader["regdate"]) });
                    listView1.Items.Add(item);
                }
                listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            sConnection.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Insert insert = new Insert();
            if (insert.ShowDialog() == DialogResult.OK)
            {
                sConnection.Open();
                using (var sCommand = new NpgsqlCommand())
                {
                    string salt = GenSalt(20);
                    string hash = CalcHash(insert.UserPass + salt);
                    sCommand.Connection = sConnection;
                    DateTime regdatee = DateTime.Now;
                    string regdate = regdatee.ToString();
                    sCommand.CommandText = "INSERT INTO users(login, hash, salt, regdate) VALUES (@login, @hash, @salt , @regdate)";
                    sCommand.Parameters.AddWithValue("login", insert.UserLogin);
                    sCommand.Parameters.AddWithValue("hash", hash);
                    sCommand.Parameters.AddWithValue("salt", salt);
                    sCommand.Parameters.AddWithValue("regdate", regdate);
                    sCommand.ExecuteNonQuery();
                    sCommand.CommandText = "SELECT regdate FROM users";
                    NpgsqlDataReader reader = sCommand.ExecuteReader();
                    if (reader.Read())
                    {
                        var item = new ListViewItem(new string[] { insert.UserLogin, hash, salt, regdate });
                        listView1.Items.Add(item);
                    }
                }
                sConnection.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Update update = new Update();
            string oldLogin = listView1.FocusedItem.Text;
            update.UserLogin = oldLogin;
            if (update.ShowDialog() == DialogResult.OK)
            {
                string salt = GenSalt(20);
                string hash = CalcHash(update.UserPass + salt);
                sConnection.Open();
                using (var sCommand = new NpgsqlCommand())
                {
                    sCommand.Connection = sConnection;
                    sCommand.CommandText = "UPDATE users set hash = @hash, salt = @salt WHERE login = @login";
                    sCommand.Parameters.AddWithValue("login", listView1.FocusedItem.Text);
                    sCommand.Parameters.AddWithValue("hash", hash);
                    sCommand.Parameters.AddWithValue("salt", salt);

                    MessageBox.Show($"Было изменено {sCommand.ExecuteNonQuery()} поле");
                    var item = new ListViewItem(new string[] { listView1.FocusedItem.Text, hash, salt, listView1.FocusedItem.SubItems[3].Text });
                    listView1.FocusedItem.Remove();
                    listView1.Items.Add(item);
                }
                sConnection.Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Удалить пользователя?", "Удаление пользователя", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                string login = listView1.FocusedItem.Text;
                sConnection.Open();
                using (var sCommand = new NpgsqlCommand())
                {
                    sCommand.Connection = sConnection;
                    sCommand.CommandText = "DELETE FROM users WHERE login = @login";
                    sCommand.Parameters.AddWithValue("@login", login);
                    sCommand.ExecuteScalar();
                }
                sConnection.Close();
                listView1.FocusedItem.Remove();
            }
        }
    }
}
