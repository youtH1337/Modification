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
    public partial class Insert : Form
    {
        static string sConnectionStr = "Server=localhost; Port=5432; User Id=postgres; Password=1234765; Database=lab3uchbd";

        public string UserLogin
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        public string UserPass
        {
            get { return textBox2.Text; }
        }
        public Insert()
        {
            InitializeComponent();
        }
        private void SwitchButton()
        {
            if (textBox1.TextLength > 0 && textBox2.TextLength > 0)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }
        private bool UsernameCheck()
        {
            using (var sConnection = new NpgsqlConnection(sConnectionStr))
            {
                sConnection.Open();
                using (var sCommand = new NpgsqlCommand())
                {
                    sCommand.Connection = sConnection;
                    sCommand.CommandText = "SELECT * FROM users WHERE login = @login";
                    sCommand.Parameters.AddWithValue("@login", textBox1.Text);
                    NpgsqlDataReader reader = sCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.HasRows == true)
                            return true;
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (UsernameCheck())
            {
                MessageBox.Show("Логин существует, повторите ввод");
                textBox1.Text = "";
            }
            else
                DialogResult = DialogResult.OK;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            SwitchButton();
        }
    }
}
