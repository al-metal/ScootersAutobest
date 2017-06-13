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


            string otvAvtobest = webRequest.getRequestEncod("http://avtobest-moto.ru/?mc=sh&ct=2&ctype_moto=143&page=1");

            string pagesStr = new Regex("(?<=Страницы:).*(?=</div>)").Match(otvAvtobest).ToString();
            MatchCollection pages = new Regex("(?<=\">).*(?=</a>)").Matches(pagesStr);
            List<string> avtobest = new List<string>();
            int i = -1;
            do
            {
                i++;
                if (i != 0)
                {
                    otvAvtobest = webRequest.getRequestEncod("http://avtobest-moto.ru/?mc=sh&ct=2&ctype_moto=143&page=" + pages[i - 1].ToString());
                }

                MatchCollection tableAvtobestTovar = new Regex("(?<=<tr>)[\\w\\W]*?(?=</tr>)").Matches(otvAvtobest);
                foreach (Match s in tableAvtobestTovar)
                {
                    string str = s.ToString();
                    string art = new Regex("(?<=<td><span class=\"green\">).*?(?=</span>)").Match(str).ToString();
                    string url = new Regex("(?<=<td><a href=\").*(?=\">)").Match(str).ToString();
                    url = "http://avtobest-moto.ru" + url;
                    avtobest.Add(art + ";" + url);
                }

            } while (pages.Count > i);

            

            List<string> scooters = new List<string>();

            string otv = webRequest.getRequest("https://bike18.ru/products/category/skutery-iz-yaponii?page=all");
            MatchCollection product = new Regex("(?<=<a href=\").*(?=\"><div class=\"-relative item-image\")").Matches(otv);
            for (int n = 0; product.Count > n; n++)
            {
                string urlTovar = product[n].ToString();
                List<string> listProduct = nethouse.GetProductList(cookieNethouse, urlTovar);
                string article = listProduct[6].ToString();
                bool b = false;
                foreach (string str in avtobest)
                {
                    string[] s = str.Split(';');
                    string articleAB = s[0].ToString();
                    
                    if (article.Contains(articleAB))
                    {
                        b = true;
                        break;
                    }

                    if (!b)
                    {
                        nethouse.DeleteProduct(cookieNethouse, urlTovar);
                        break;
                    }
                        
                }

                if(b)
                scooters.Add(article);
            }

            

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tbLogin.Text = Properties.Settings.Default.login;
            tbPassword.Text = Properties.Settings.Default.password;
        }
    }
}
