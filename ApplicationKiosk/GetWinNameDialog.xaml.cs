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
    /// Логика взаимодействия для GetWinNameDialog.xaml
    /// </summary>
    public partial class GetWinNameDialog : Window
    {
        public GetWinNameDialog()
        {
            InitializeComponent();
            this.textBox.Foreground = Brushes.Gray;
            this.textBox.GotFocus += TextBox_GotFocus; ;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).Foreground = Brushes.Black;
            (sender as TextBox).Text = string.Empty;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
