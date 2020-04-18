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
    public partial class Update : Form
    {
        string sConnectionStr = "Server=localhost; Port=5432; User Id=postgres; Password=1234765; Database=lab3uchbd";

        public string UserLogin
        {
            set { textBox1.Text = value; }
        }

        public string UserPass
        {
            get { return textBox3.Text; }
        }
        public Update()
        {
            InitializeComponent();
        }
        public string CalcHash(string text)
        {
            SHA1CryptoServiceProvider hash = new SHA1CryptoServiceProvider();
            return Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(text)));
        }
        private void SwitchButton()
        {
            if (textBox1.TextLength > 0 && textBox2.TextLength > 0 && textBox3.TextLength > 0)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }
        private bool PasswordCheck()
        {
            using (var sConnection = new NpgsqlConnection(sConnectionStr))
            {
                sConnection.Open();
                using (var sCommand = new NpgsqlCommand())
                {
                    sCommand.Connection = sConnection;
                    sCommand.CommandText = "SELECT salt, hash FROM users where login= @login";
                    sCommand.Parameters.AddWithValue("login", textBox1.Text);
                    NpgsqlDataReader reader = sCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string hash = (string)reader["hash"];
                        string salt = ((string)reader["salt"]);
                        if (hash == CalcHash(textBox2.Text + salt))
                        {
                            return true;
                        }
                    }
                }
                sConnection.Close();
            }
            return false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            SwitchButton();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            SwitchButton();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            SwitchButton();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!PasswordCheck())
            {
                MessageBox.Show("Введен неверный пароль, повторите попытку ввода.");
                textBox2.Text = "";
            }
            else
                DialogResult = DialogResult.OK;
        }
    }
}
