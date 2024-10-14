using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace Bolt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // importing
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, IntPtr dwExtra);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        // class variables
        const uint lDown = 0x0002;
        const uint lUp = 0x0004;
        const uint rDown = 0x0008;
        const uint rUp = 0x0010;
        const uint mDown = 0x0020;
        const uint mUp = 0x0040;

        bool clickerEnabled = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private static readonly Regex _regex = new Regex("^[0-9]+$");
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_regex.IsMatch(e.Text);
        }

        private void ButtonStart(object sender, RoutedEventArgs e)
        {
            int[] clickConfig = GetChosenOptions();

            //TODO: maek fnuuy autoclickr start
        }
        private void ButtonStop(object sender, RoutedEventArgs e)
        {
            //TODO: maek fnuuy autoclickr stop
        }

        private int[] GetChosenOptions()
        {
            int[] options = new int[5];

            //-- Click Mode
            if (mpr.IsChecked == true)
            { options[0] = 1; }
            else if (mpo.IsChecked == true)
            { options[0] = 2; }
            else if (mro.IsChecked == true)
            { options[0] = 3; }
            else
            { MessageBox.Show("No click mode selected!"); }

            //-- Mouse Button
            if (ml.IsChecked == true)
            { options[1] = 1; }
            else if (mr.IsChecked == true)
            { options[1] = 2; }
            else if (mm.IsChecked == true)
            { options[1] = 3; }
            else
            { MessageBox.Show("No mouse button selected!"); }
            
            //-- Repeat time
            if (inf.IsChecked == true)
            { options[2] = 1; }
            else if (set.IsChecked == true)
            { options[2] = 2; }
            else
            { MessageBox.Show("No repeat mode selected!"); }

            //-- Interval
            if (options[2] == 2)
            { options[3] = int.Parse(tbRepeatCount.Text); }
            else
            { options[3] = int.MaxValue; }

            options[4] = 0 + int.Parse(tbIntervalMilli.Text) + (int.Parse(tbIntervalSec.Text) * 1000);

            return options;
        }
        private void lsClickMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageBox.Show(e.ToString());
        }
    }
}