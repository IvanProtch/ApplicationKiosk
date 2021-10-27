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
    /// Логика взаимодействия для AreaCustomizationWindow.xaml
    /// </summary>
    public partial class AreaCustomizationWindow : Window
    {
        private Rect _defaultRect;

        private Rect _rect;
        public Rect Rect
        {
            get
            {
                _rect = new Rect(Left, Top, Width, Height);
                return _rect;
            }
            private set { }
        }
        public AreaCustomizationWindow()
        {
            InitializeComponent();
            _defaultRect = Rect;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void accept_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Rect = _defaultRect;
            this.Close();
        }
    }
}
