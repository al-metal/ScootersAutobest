using Bike18;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScootersAutobest
{
    public partial class Form1 : Form
    {
        Thread forms;
        nethouse nethouse = new nethouse();
        httpRequest webRequest = new httpRequest();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            #region Сохранение паролей
            Properties.Settings.Default.login = tbLogin.Text;
            Properties.Settings.Default.password = tbPassword.Text;
            Properties.Settings.Default.Save();
            #endregion

            #region Обработка сайта

            Thread tabl = new Thread(() => ActualSooters());
            forms = tabl;
            forms.IsBackground = true;
            forms.Start();

            #endregion
        }

        private void ActualSooters()
        {
            CookieContainer cookieNethouse = nethouse.CookieNethouse(tbLogin.Text, tbPassword.Text);
            if (cookieNethouse.Count == 1)
            {
                MessageBox.Show("Логин или пароль для сайта Nethouse введены не верно", "Ошибка логина/пароля");
                return;
            }

            string otv = webRequest.getRequest("https://bike18.ru/products/category/skutery-iz-yaponii?page=all");
            MatchCollection product = new Regex("(?<=<a href=\").*(?=\"><div class=\"-relative item-image\")").Matches(otv);
            for (int n = 0; product.Count > n; n++)
            {
                string urlTovar = product[n].ToString();
                List<string> listProduct = nethouse.GetProductList(cookieNethouse, urlTovar);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tbLogin.Text = Properties.Settings.Default.login;
            tbPassword.Text = Properties.Settings.Default.password;
        }
    }
}
