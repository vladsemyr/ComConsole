using MaterialDesignThemes.Wpf;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ComConsole
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly PaletteHelper _paletteHelper = new PaletteHelper();

        public string[] SerialPortNames { get; set; }
        public string SelectedSerialPortName { get; set; }

        private SerialPort _serialPort = null;

        public bool ShowAdditionalUI { get; set; }

        private string _testRun;
        public string TestRun
        {
            get => _testRun;
            set { _testRun = value; OnPropertyChanged("TestRun"); }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            SerialPortNames = SerialPort.GetPortNames();
            SelectedSerialPortName = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void ToggleBaseColour(bool isDark)
        {
            ITheme theme = _paletteHelper.GetTheme();
            IBaseTheme baseTheme = isDark ? new MaterialDesignDarkTheme() : (IBaseTheme)new MaterialDesignLightTheme();
            theme.SetBaseTheme(baseTheme);
            _paletteHelper.SetTheme(theme);
        }

        private void DarkModeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleBaseColour(true);
            if (ConsoleCurrentLineBorder != null)
                ConsoleCurrentLineBorder.Background = new SolidColorBrush(Color.FromArgb(0x15, 0xff, 0xff, 0xff));
        }

        private void DarkModeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleBaseColour(false);
            if (ConsoleCurrentLineBorder != null)
                ConsoleCurrentLineBorder.Background = new SolidColorBrush(Color.FromArgb(0x15, 0x00, 0x00, 0x00));
        }



        private void Border_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            if (MenuLeftPart.Visibility != Visibility.Collapsed)
            {
                LeftPanel.Width = new GridLength(15, GridUnitType.Pixel);
                MenuLeftPart.Visibility = Visibility.Collapsed;
                Menu.Padding = new Thickness(0, 0, 0, 0);
                MenuLeftRightBorder.CornerRadius = new CornerRadius(0);
                MenuArrow.Text = ">";
            }
            else
            {
                LeftPanel.Width = new GridLength(200, GridUnitType.Pixel);
                MenuLeftPart.Visibility = Visibility.Visible;
                Menu.Padding = new Thickness(10, 10, 0, 10);
                MenuLeftRightBorder.CornerRadius = new CornerRadius(20, 0, 0, 20);
                MenuArrow.Text = "<";
            }
        }


        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!(sender is SerialPort serialPort))
                return;

            string receivedText = serialPort.ReadExisting().Replace("\r", "");
            System.Random r = new System.Random();

            Run run = new Run(receivedText)
            {
                Foreground = new SolidColorBrush(Color.FromRgb((byte)r.Next(), (byte)r.Next(), (byte)r.Next())),
            };

            TestRun += receivedText;

            //(ConsoleLog.Document.Blocks.FirstBlock as Paragraph).Inlines.Add(run);
            Dispatcher.Invoke(() =>
            {
                ConsoleScroll.ScrollToEnd();
            });
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //int str = KeyInterop.VirtualKeyFromKey(e.Key);

            //_serialPort.BaseStream.WriteByte((byte)str);
            //return;

            if (e.Key == Key.Return)
            {
                if (!_serialPort.IsOpen)
                    return;

                _serialPort.WriteLine(ConsoleCurrentLine.Text);
                _serialPort.BaseStream.Flush();

                ConsoleCurrentLine.Text = "";
                ConsoleScroll.ScrollToEnd();
            }
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!(sender is ComboBox comNameComboBox))
                return;

            if (_serialPort is null)
                _serialPort = new SerialPort();

            _serialPort.Close();

            if (!(comNameComboBox.SelectedValue is string comName) || comName == "")
                return;

            _serialPort.PortName = comName;
            _serialPort.BaudRate = 115200;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            _serialPort.ReadTimeout = 1000;
            _serialPort.WriteTimeout = 1000;
            try
            {
                _serialPort.Open();
            }
            catch
            {

            }

            if (!_serialPort.IsOpen)
                return;
            
            _serialPort.DataReceived += _serialPort_DataReceived;
        }

        private void ConsoleLog_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            _serialPort.Write(e.Text);
            e.Handled = true;
        }

        private void ConsoleLog_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is RichTextBox rtb))
                return;

            if (e.Key == Key.Back)
                _serialPort.Write("\x08");

            if (e.Key == Key.Space)
            {
                _serialPort.Write(" ");
                //ConsoleLog_PreviewTextInput(null, new TextCompositionEventArgs(null, new TextComposition())
            }
            
            if (e.Key == Key.Enter)
            {
                _serialPort.Write("\n");
                e.Handled = true;
            }

            rtb.CaretPosition = rtb.CaretPosition.DocumentEnd;
        }

        private void TopmostToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Topmost = true;
        }

        private void TopmostToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Topmost = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TestRun = "";
        }
    }
}
