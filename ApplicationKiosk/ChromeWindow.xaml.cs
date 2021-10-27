using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ApplicationKiosk
{
    /// <summary>
    /// Логика взаимодействия для ChromeWindow.xaml
    /// </summary>
    public partial class BrowserPage : Window
    {
        private Uri _adress = new Uri("https://www.google.com/");
        public BrowserPage(Uri adress)
        {
            _adress = adress;
            InitializeComponent();
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            browser.Navigate(_adress);
        }
    }
}
