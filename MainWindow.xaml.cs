using MaterialDesignThemes.Wpf;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

            SerialPortNames = SerialPort.GetPortNames().ToList().Append("не выбран").ToArray();
            SelectedSerialPortName = SerialPortNames[0];
            TestRun = "";

            LeftPanelHide();
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


        private void LeftPanelHide()
        {
            LeftPanel.Width = new GridLength(15, GridUnitType.Pixel);
            MenuLeftPart.Visibility = Visibility.Collapsed;
            Menu.Padding = new Thickness(0, 0, 0, 0);
            MenuLeftRightBorder.CornerRadius = new CornerRadius(0);
            MenuArrow.Text = ">";
        }

        private void LeftPanelShow()
        {
            LeftPanel.Width = new GridLength(200, GridUnitType.Pixel);
            MenuLeftPart.Visibility = Visibility.Visible;
            Menu.Padding = new Thickness(10, 10, 0, 10);
            MenuLeftRightBorder.CornerRadius = new CornerRadius(20, 0, 0, 20);
            MenuArrow.Text = "<";
        }

        private void Border_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            if (MenuLeftPart.Visibility != Visibility.Collapsed)
            {
                LeftPanelHide();
            }
            else
            {
                LeftPanelShow();
            }
        }

        Random random = new Random();
        
        /*
        private InlineCollection _inlines;
        public InlineCollection Inlines
        {
            get => _inlines;
            set { _inlines = value; OnPropertyChanged("Inlines"); }
        }
        */

        private void _NextRun(string text, Color? frgcolor, Color? bckcolor, bool is_bold = false)
        {
            Dispatcher.Invoke(() =>
            {
                // убираем биндинг с последнего Run, сохраняя текст
                string run_text = TestRun;
                Paragraph prg = ConsoleLog.Document.Blocks.FirstBlock as Paragraph;
                Run run = prg.Inlines.LastInline as Run;
                BindingOperations.ClearBinding(run, Run.TextProperty);
                run.Text = run_text;
                
                run = new Run();
                if (frgcolor != null)
                    run.Foreground = new SolidColorBrush(frgcolor.Value);

                if (bckcolor != null)
                    run.Background = new SolidColorBrush(bckcolor.Value);

                if (is_bold)
                    run.FontWeight = FontWeights.ExtraBold;

                _ = run.SetBinding(Run.TextProperty, "TestRun");
                TestRun = text;
                prg.Inlines.Add(run);
            });
        }

        private void NextRun(string text)
        {
            _NextRun(text, null, null);
        }

        private void NextRun(string text, Color frgcolor)
        {
            _NextRun(text, frgcolor, null);
        }

        private void NextRun(string text, bool is_bold)
        {
            _NextRun(text, null, null, is_bold);
        }

        private void NextRun(string text, Color frgcolor, bool is_bold)
        {
            _NextRun(text, frgcolor, null, is_bold);
        }

        private void NextRun(string text, Color? frgcolor, Color bckcolor, bool is_bold)
        {
            _NextRun(text, frgcolor, bckcolor, is_bold);
        }

        private void NextRun(string text, Color? frgcolor, Color bckcolor)
        {
            _NextRun(text, frgcolor, bckcolor);
        }


        private enum ConsoleColor
        {
            Default = 0,
            Black = 30,
            Red = 31,
            Green = 32,
            Yellow = 33,
            Blue = 34,
            Magenta = 35,
            Cyan = 36,
            White = 37
        }

        private Color ConsoleColorToColor(ConsoleColor cc)
        {
            switch (cc)
            {
                case ConsoleColor.Default: return Color.FromRgb(0, 0, 0);
                case ConsoleColor.Black: return Color.FromRgb(0, 0, 0);
                case ConsoleColor.Red: return Color.FromRgb(255, 0, 0);
                case ConsoleColor.Green: return Color.FromRgb(0, 255, 0);
                case ConsoleColor.Yellow: return Color.FromRgb(255, 255, 0);
                case ConsoleColor.Blue: return Color.FromRgb(0, 0, 255);
                case ConsoleColor.Magenta: return Color.FromRgb(255, 0, 255);
                case ConsoleColor.Cyan: return Color.FromRgb(0, 255, 255);
                case ConsoleColor.White: return Color.FromRgb(255, 255, 255);
                default: return Color.FromRgb(0, 0, 0);
            }
        }

        private enum ConsoleTextWeight
        {
            Default = 0,
            Bold = 1
        }


        void ConsoleColorGet(uint color_num, ref ConsoleColor frgcolor, ref ConsoleColor bckcolor)
        {
            if (color_num >= 30 && color_num <= 37)
            {
                frgcolor = (ConsoleColor)color_num;
            }
            else if (color_num >= 40 && color_num <= 47)
            {
                bckcolor = (ConsoleColor)(color_num - 10);
            }
        }

        private static string ToLiteral(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!(sender is SerialPort serialPort))
                return;
            Random r = new Random();

            string receivedText = serialPort.ReadExisting().Replace("\r", "");


            int run_tail_len = TestRun.Length < 15 ? TestRun.Length : 15;
            string run_tail = TestRun.Substring(TestRun.Length - run_tail_len, run_tail_len); // хвост
            TestRun = TestRun.Substring(0, TestRun.Length - run_tail_len); // без хвоста

            receivedText = run_tail + receivedText;
            string[] parts = receivedText.Split(new string[] { "\x1b[" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length; ++i)
            {
                ConsoleTextWeight weight = ConsoleTextWeight.Default;
                ConsoleColor frgcolor = ConsoleColor.Default;
                ConsoleColor bckcolor = ConsoleColor.Default;

                try
                {
                    int del_len = 0;
                    if (parts[i].Length >= 2 && parts[i][1] == 'm')
                    {
                        // 0m
                        string bold_flag = parts[i].Substring(0, 1);
                        weight = uint.Parse(bold_flag) == 1 ? ConsoleTextWeight.Bold : ConsoleTextWeight.Default;
                        
                        del_len = 2;
                    }
                    else if (parts[i].Length >= 3 && parts[i][2] == 'm')
                    {
                        // 33m
                        string color_flag = parts[i].Substring(0, 2);
                        uint color_num = uint.Parse(color_flag);

                        ConsoleColorGet(color_num, ref frgcolor, ref bckcolor);
                        del_len = 3;
                    }
                    else if (parts[i].Length >= 5 && parts[i][4] == 'm')
                    {
                        // 1;33m
                        string bold_flag = parts[i].Substring(0, 1);
                        string color_flag = parts[i].Substring(2, 2);
                        
                        weight = uint.Parse(bold_flag) == 1 ? ConsoleTextWeight.Bold : ConsoleTextWeight.Default;
                        uint color_num = uint.Parse(color_flag);

                        ConsoleColorGet(color_num, ref frgcolor, ref bckcolor);
                        del_len = 5;
                    }
                    else if (parts[i].Length >= 6 && parts[i][5] == 'm')
                    {
                        // 33;40m
                        uint color_num_1 = uint.Parse(parts[i].Substring(0, 2));
                        uint color_num_2 = uint.Parse(parts[i].Substring(3, 2));
                        ConsoleColorGet(color_num_1, ref frgcolor, ref bckcolor);
                        ConsoleColorGet(color_num_2, ref frgcolor, ref bckcolor);
                        del_len = 6;
                    }
                    else if (parts[i].Length >= 8 && parts[i][7] == 'm')
                    {
                        // 1;33;40m
                        string bold_flag = parts[i].Substring(0, 1);
                        uint color_num_1 = uint.Parse(parts[i].Substring(2, 2));
                        uint color_num_2 = uint.Parse(parts[i].Substring(5, 2));
                        
                        weight = uint.Parse(bold_flag) == 1 ? ConsoleTextWeight.Bold : ConsoleTextWeight.Default;
                        ConsoleColorGet(color_num_1, ref frgcolor, ref bckcolor);
                        ConsoleColorGet(color_num_2, ref frgcolor, ref bckcolor);
                        del_len = 8;
                    }
                    else
                    {
                        throw new Exception("pass");
                    }

                    parts[i] = parts[i].Substring(del_len);
                    bool is_bold = weight == ConsoleTextWeight.Bold;
                    bool is_frgcolor_set = frgcolor != ConsoleColor.Default;
                    bool is_bckcolor_set = bckcolor != ConsoleColor.Default;

                    if (!is_frgcolor_set && !is_bckcolor_set)
                        NextRun(parts[i], is_bold);
                    if (is_frgcolor_set && !is_bckcolor_set)
                        NextRun(parts[i], ConsoleColorToColor(frgcolor), is_bold);
                    if (!is_frgcolor_set && is_bckcolor_set)
                        NextRun(parts[i], null, ConsoleColorToColor(bckcolor), is_bold);
                    if (is_frgcolor_set && is_bckcolor_set)
                        NextRun(parts[i], ConsoleColorToColor(frgcolor), ConsoleColorToColor(bckcolor), is_bold);
                }
                catch
                {
                    TestRun += parts[i];
                }
            }

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
            {
                comNameComboBox.Background = new SolidColorBrush(Color.FromArgb(100, 200, 100, 100));
                return;
            }
            
            comNameComboBox.Background = new SolidColorBrush(Color.FromArgb(0, 200, 100, 100));
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TestRun = "";
        }

        

        public BooleanToVisibilityConverterInverse converterInverse = new BooleanToVisibilityConverterInverse();
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverterInverse : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool is_collapsed = (bool)value;
            return is_collapsed ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return visibility == Visibility.Collapsed;
        }
    }
}
