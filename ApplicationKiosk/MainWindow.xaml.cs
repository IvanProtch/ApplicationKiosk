using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Interop;
using System.Threading;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using TestStack.White.WindowsAPI;

namespace ApplicationKiosk
{
    public class KioskButton : Button
    {
        public string SpecificName;
        protected IntPtr intPtr;
        public KioskButton()
        {
            this.FontSize = 35;
            this.FontWeight = FontWeights.DemiBold;
            this.Foreground = Brushes.DarkBlue;
            this.Background = Brushes.GhostWhite;
            //this.FontFamily = new FontFamily("segoe ui");
           
        }
        public Rect WindowRect { get; set; }

        const int SHOWWINDOW = 0x0040;
        const int SWP_NOSENDCHANGING = 0x0400;

        const int SW_SHOWMINIMIZED = 2;
        const int SW_SHOWMAXIMIZED = 3;
        const int SW_NORMAL = 1;
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hwnd, int intnCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool BringWindowToTop(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hwnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        protected static extern bool GetWindowInfo(IntPtr hWnd, ref WindowInfo pwi);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd,  int X, int Y, int nWidth, int nHeight, bool bRepaint);

        public void SetWindowPositionAndSelect(IntPtr hwnd)
        {
            ShowWindow(hwnd, SW_NORMAL);
            MoveWindow(hwnd, (int)this.ActualWidth, 0, System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - (int)this.ActualWidth, System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height, true);
            BringWindowToTop(hwnd);
            //SetWindowPos(hwnd, FindWindow(null, this.Name), (int)this.ActualWidth, 0, System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - (int)this.ActualWidth, System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height, SWP_NOSENDCHANGING);
        }

        public virtual XElement ToXElement() 
        {
            XElement xElement = new XElement("button");
            xElement.Add(new XAttribute("caption", Content.ToString()));
            //xElement.Add(new XAttribute("rectParams", WindowRect));
            return xElement;
        }

        public void CloseWindow()
        {
            ShowWindow(intPtr, SW_SHOWMINIMIZED);
        }
    }

    public class ProcessButton : KioskButton
    {
        private Process _process;
        private string _exeFilePath;
        private string _processName;
        private string _mainWindowHandle;

        public ProcessButton(string processName)
        {
            _process = Process.GetProcessesByName(processName).FirstOrDefault();

            if (Process.GetProcessesByName(processName).Length > 1)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show($"Существует несколько процессов с именем {processName}.\nВвести имя окна, чтобы явно указать запускаемое приложение?", "Ошибка создания кнопки вызова процесса", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if(messageBoxResult == MessageBoxResult.OK)
                {

                    GetWinNameDialog getWinNameDialog = new GetWinNameDialog();
                    if(getWinNameDialog.ShowDialog() == true)
                    {
                        _process = Process.GetProcessesByName(processName).FirstOrDefault(e => e.MainWindowTitle.Contains(getWinNameDialog.textBox.Text));
                    }
                }
            }
            if (_process == null)
                throw new Exception($"Не удалось найти процесс по наименованию {processName}");

            intPtr = _process.MainWindowHandle;

            _exeFilePath = _process.StartInfo.FileName == null ? "" : _process.StartInfo.FileName;
            _processName = processName;
            _mainWindowHandle = _process.MainWindowTitle == null ? "" : _process.MainWindowTitle;

            this.SpecificName = processName + _process.MainWindowTitle;
        }
        public ProcessButton(string processName, string mainWindowHandle, string exeFilePath)
        {
            _exeFilePath = exeFilePath;
            _processName = processName;
            if (mainWindowHandle == null)
                mainWindowHandle = string.Empty;
            _mainWindowHandle = mainWindowHandle;

            this.SpecificName = processName;
            _process = Process.GetProcessesByName(processName).FirstOrDefault(e => e.MainWindowTitle.Contains(mainWindowHandle));

            if (_process == null)
            {
                if (_exeFilePath != string.Empty)
                {
                    _process = Process.Start(_exeFilePath);
                    _process.WaitForInputIdle();
                }
            }
            if(_process != null)
                intPtr = _process.MainWindowHandle;
        }

        protected override void OnClick()
        {

            base.OnClick();
            bool exited = _process == null ? true : _process.HasExited;
            if (!exited)
                SetWindowPositionAndSelect(_process.MainWindowHandle);
            else
            {
                try
                {
                    _process = Process.Start(_exeFilePath);
                }
                catch (InvalidOperationException exc)
                {
                    var result = MessageBox.Show(exc.Message+"\nХотите запустить приложение и попробовать снова?", "Ошибка", MessageBoxButton.YesNo, MessageBoxImage.Error);

                    if(result == MessageBoxResult.Yes)
                    {
                        int step = 0;
                        while (exited)
                        {
                            step++;
                            Thread.Sleep(500);
                            _process = Process.GetProcessesByName(_processName).FirstOrDefault(e => e.MainWindowTitle.Contains(_mainWindowHandle));
                            exited = _process == null ? true : _process.HasExited;
                            if(step == 50)
                            {
                                MessageBox.Show("Истекло время ожидания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    if(_process != null)
                    {
                        Thread.Sleep(300);
                        SetWindowPositionAndSelect(_process.MainWindowHandle);
                    }
                }
            }

        }
        public override XElement ToXElement()
        {
            XElement xElement = base.ToXElement();
            if(_process != null)
            {
                xElement.Add(new XAttribute("processName", _process.ProcessName));
                xElement.Add(new XAttribute("mainWindowTitle", _process.MainWindowTitle));
            }
            else
            {
                xElement.Add(new XAttribute("processName", _processName));
                xElement.Add(new XAttribute("mainWindowTitle", _mainWindowHandle));
            }

            xElement.Add(new XAttribute("exePath", _exeFilePath));
            return xElement;
        }
    }
    public class InternetPageButton : KioskButton
    {
        private WindowInteropHelper _windowInteropHelper;
        private BrowserPage _browserPage;
        private Uri _adress;
        public InternetPageButton(Uri adress)
        {
            this.SpecificName = adress.ToString();

            if (adress == null)
                throw new Exception("Неверный адрес");
            _adress = adress;
            _browserPage = new BrowserPage(adress);
            _windowInteropHelper = new WindowInteropHelper(_browserPage);
            intPtr = _windowInteropHelper.Handle;

        }

        protected override void OnClick()
        {
            base.OnClick();

            if (!_browserPage.IsVisible)
            {
                _browserPage = new BrowserPage(_adress);
                _windowInteropHelper = new WindowInteropHelper(_browserPage);
            }

            _browserPage.Show();
            SetWindowPositionAndSelect(_windowInteropHelper.Handle);
        }

        public override XElement ToXElement()
        {
            XElement xElement = base.ToXElement();
            xElement.Add(new XAttribute("uri", _adress));
            return xElement;
        }
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Rect WindowRect { get; set; }

        private List<KioskButton> _createdButtons = new List<KioskButton>();

        private string _configPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\kioskConfig.xml";
        private XDocument _config = new XDocument();

        private KioskButton _selectedButton;
        private ButtonInitDialog _buttonInitDialog = new ButtonInitDialog();

        public MainWindow()
        {
            this.Top = 0;
            this.Left = 0;
            this.Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            InitializeComponent();

            try
            {
                if (!System.IO.File.Exists(_configPath))
                    System.IO.File.Create(_configPath);

                try
                {
                    _config = XDocument.Load(_configPath);
                }
                catch (Exception)
                {
                    _config = new XDocument();
                    _config.Add(new XComment("конфигурация открываемых окон"), new XElement("root"));
                }

                foreach (var xElem in _config.Root.Descendants())
                {
                    //var rectParams = xElem.Attribute("rectParams").Value.Replace(';', ',');
                    var name = xElem.Attribute("processName") ?? xElem.Attribute("uri");
                    var caption = xElem.Attribute("caption");
                    var winCaption = xElem.Attribute("mainWindowTitle");
                    var exePath = xElem.Attribute("exePath");

                    Uri adress = null;
                    KioskButton btn = null;
                    if (Uri.TryCreate(name.Value, UriKind.Absolute, out adress))
                    {
                        btn = new InternetPageButton(adress) { Content = caption.Value/*, WindowRect = Rect.Parse(rectParams)*/ };
                        _createdButtons.Add(btn);
                    }
                    else
                    {
                        btn = new ProcessButton(name.Value, winCaption?.Value, exePath?.Value) { Content = caption.Value/*, WindowRect = Rect.Parse(rectParams)*/ };
                        _createdButtons.Add(btn);
                    }
                    btn.MouseRightButtonDown += Btn_MouseRightButtonDown;

                }
                foreach (var createdButton in _createdButtons)
                {
                    button_stack.Children.Add(createdButton);
                }

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка инициализации", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void Btn_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _selectedButton = sender as KioskButton;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.Close();
            this._createdButtons.ForEach(btn => btn.CloseWindow());
            //_config.Save(_configPath);
        }


        protected override void OnLocationChanged(EventArgs e)
        {
            this.Top = 0;
            this.Left = 0;
        }

        private void MenuItem_winSizeChanged_Click(object sender, RoutedEventArgs e)
        {
            AreaCustomizationWindow area = new AreaCustomizationWindow();
            area.Show();
            area.Closed += Area_Closed;
        }

        private void Area_Closed(object sender, EventArgs e)
        {
            WindowRect = (sender as AreaCustomizationWindow).Rect;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            _buttonInitDialog.Show();
            _buttonInitDialog.ButtonWasCreated += ButtonInitDialog_ButtonWasInit;
        }

        private void ButtonInitDialog_ButtonWasInit(object sender, EventArgs e)
        {
            try
            {
                var createdButton = (sender as ButtonInitDialog).CreatedButton;

                if (_createdButtons.FirstOrDefault(item => item.SpecificName == createdButton.SpecificName || item.Content == createdButton.Content) != null)
                    throw new Exception($"Кнопка уже добавлена!");

                _createdButtons.Add(createdButton);
                button_stack.Children.Add(createdButton);
                createdButton.MouseRightButtonDown += Btn_MouseRightButtonDown;

                //_config.Root.Add(createdButton.ToXElement());
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка инициализации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            this.button_stack.Children.Remove(_selectedButton);
            _createdButtons.Remove(_selectedButton);
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            _config.Root.RemoveAll();
            foreach (var btn in _createdButtons)
            {
                _config.Root.Add(btn.ToXElement());
            }
            _config.Save(_configPath);
        }

        //private void MainWindow_SourceInitialized(object sender, EventArgs e)
        //{
        //    WindowInteropHelper helper = new WindowInteropHelper(this);
        //    HwndSource source = HwndSource.FromHwnd(helper.Handle);
        //    source.AddHook(WndProc);
        //}

        //private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        //{
        //    switch (msg)
        //    {
        //        case WM_SYSCOMMAND:
        //            int command = wParam.ToInt32() & 0xfff0;
        //            if (command == SC_MOVE)
        //            {
        //                handled = true;
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //    return IntPtr.Zero;
        //}
    }
}
