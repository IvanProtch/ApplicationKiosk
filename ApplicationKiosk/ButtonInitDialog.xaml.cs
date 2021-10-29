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
    /// Логика взаимодействия для ButtonInitDialog.xaml
    /// </summary>
    public partial class ButtonInitDialog : Window
    {

        public event EventHandler ButtonWasCreated;

        private KioskButton _createdButton;
        public KioskButton CreatedButton
        {
            get
            {
                Uri adress = null;
                if (Uri.TryCreate(nameTextBox.Text, UriKind.Absolute, out adress))
                    _createdButton = new InternetPageButton(adress) { Content = captionTextBox.Text };
                else
                    _createdButton = new ProcessButton(nameTextBox.Text) { Content = captionTextBox.Text };
                return _createdButton;

            }
        }

        public ButtonInitDialog()
        {
            InitializeComponent();
            this.captionTextBox.Foreground = Brushes.Gray;
            this.nameTextBox.Foreground = Brushes.Gray;
            this.captionTextBox.GotFocus += CaptionTextBox_GotFocus;
            this.nameTextBox.GotFocus += CaptionTextBox_GotFocus;
        }

        private void CaptionTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).Foreground = Brushes.Black;
            (sender as TextBox).Text = string.Empty;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonWasCreated(this, e);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
