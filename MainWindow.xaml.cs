using MaterialDesignThemes.Wpf;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComConsole
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PaletteHelper _paletteHelper = new PaletteHelper();

        public bool ShowAdditionalUI { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
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


        private bool isLeftMenuClosed = false;
        private void Border_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            isLeftMenuClosed = !isLeftMenuClosed;
            if (isLeftMenuClosed)
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

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ConsoleLog.Text = ConsoleLog.Text + ConsoleCurrentLine.Text + "\n";
                ConsoleCurrentLine.Text = "";
                ConsoleScroll.ScrollToEnd();
            }
        }
    }
}
