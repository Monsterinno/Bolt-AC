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
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;


namespace Bolt
{

    public partial class MainWindow : Window
    {
        //-- Thread Creation
        Thread hotkeyThread;
        Thread clickerSim;

        bool killThreads = false;

        //-- Importing Dll
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtra);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        //-- Getting the specific uint numbers for mouse input
        const uint lDown = 0x0002;
        const uint lUp = 0x0004;
        const uint rDown = 0x0008;
        const uint rUp = 0x0010;
        const uint mDown = 0x0020;
        const uint mUp = 0x0040;

        const int hotkey = 0x75;

        bool pPlusR = false; // Press and Release boolean

        static bool clickerEnabled = false;

        static int[] clickSettings = {};

        //-- Thread Creation
        Thread mainThread = Thread.CurrentThread;

        public MainWindow()
        {
            InitializeComponent();
            clickerSim = new Thread(RunClicker);
            clickerSim.Name = "clickerSimThread";
            hotkeyThread = new Thread(CheckHotkeyState);
            hotkeyThread.Name = "hotkeyThread";
            hotkeyThread.Start();
        }
        private void Window_Exit(object sender, EventArgs e)
        {
            killThreads = true;
        }

        //-- This is here entirely so that people cant input letters or special characters as arguements for the click settings bc that would be bad.
        private static readonly Regex _regex = new Regex("^[0-9]+$");
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_regex.IsMatch(e.Text);
        }

        //-- Runs whenever the button "Start Clicking" is pressed
        public void RunClicker()
        {
            for (int i = 0; i < clickSettings[3]; i++)
            {
                if (killThreads) { break; }

                if (clickerEnabled)
                {
                    MouseClick();
                }
                else
                {
                    break;
                }
                Thread.Sleep(clickSettings[4]);
            }

            this.Dispatcher.Invoke(() =>
            {
                if (DebugCB.IsChecked == true) { MessageBox.Show("Finished Clicking."); }
            });

            clickerEnabled = false;
        }
        //-- So the hotkey actually toggles the autoclicker
        private void CheckHotkeyState()
        {
            while (true)
            {
                if (killThreads) { break; }

                if (GetAsyncKeyState(hotkey)<0)
                {
                    if (clickerEnabled == false)
                    {
                        this.Dispatcher.Invoke(() =>
                        { 
                            EnableClicker();
                        });
                    }else
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            DisableClicker();
                        });  
                    }
                    Thread.Sleep(300);
                }

            }
        }
        //-- Event function connected to the button "Start Clicking"
        private void ButtonStart(object sender, RoutedEventArgs e)
        {
            EnableClicker();
        }

        //-- Event function connected to the button "Stop Clicking"
        private void ButtonStop(object sender, RoutedEventArgs e)
        {
            DisableClicker();
        }

        //-- These 2 functions are used for toggling the autoclicker
        private void EnableClicker()
        {
            clickSettings = GetChosenOptions();
            clickerEnabled = true;
            ChangeButtonStates(clickerEnabled);
            if (DebugCB.IsChecked == true) { ShowDebugInfo(); }
            clickerSim.Start();
        }
        private void DisableClicker()
        {
            clickerEnabled = false;
            ChangeButtonStates(clickerEnabled);
        }

        //-- A function dedicated to putting all the user's input for settings in a single int array
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

            //-- Repeat until
            if (inf.IsChecked == true)
            { options[2] = 1; }
            else if (set.IsChecked == true)
            { options[2] = 2; }
            else
            { MessageBox.Show("No repeat mode selected!"); }

            //-- Repeat Amount
            if (options[2] == 2)
            { options[3] = 0 + int.Parse(tbRepeatCount.Text); }
            else
            { options[3] = int.MaxValue; }

            //-- Interval
            if (int.TryParse(tbIntervalMilli.Text, out int result) == false)
            {
                if (int.TryParse(tbIntervalSec.Text, out result) == false)
                {
                    options[4] = 0;
                }
                else
                {
                    options[4] = 0 + (int.Parse(tbIntervalSec.Text) * 1000);
                }
            }
            else
            {
                if (int.TryParse(tbIntervalSec.Text, out result) == false)
                {
                    options[4] = 0 + int.Parse(tbIntervalMilli.Text);
                }else
                {
                    options[4] = 0 + int.Parse(tbIntervalMilli.Text) + (int.Parse(tbIntervalSec.Text) * 1000);
                }
            }

            if (options[4] < 20)
            {
                options[4] = 20;
            }

            return options;
        }

        private uint[] GetSpecificMouseButton()
        {
            uint[] returns = new uint[2];
            if (clickSettings[0] == 1)
            {
                pPlusR = true;
            }
            switch (clickSettings[1])
            {
                case 1:
                    returns[0] = lDown;
                    returns[1] = lUp;
                    break;
                case 2:
                    returns[0] = rDown;
                    returns[1] = rUp;
                    break;
                case 3:
                    returns[0] = mDown;
                    returns[1] = mUp;
                    break;
            }
            return returns;
        }

        //-- This function simulates the mouse input
        private void MouseClick()
        {
            uint[] mouseButtonStates = GetSpecificMouseButton();

            switch (mouseButtonStates[0])
            {
                case 1:
                    mouse_event((int)mouseButtonStates[0], 0, 0, 0, 0);
                    mouse_event((int)mouseButtonStates[1], 0, 0, 0, 0);
                    break;
                case 2:
                    mouse_event((int)mouseButtonStates[0], 0, 0, 0, 0);
                    break;
                case 3:
                    mouse_event((int)mouseButtonStates[1], 0, 0, 0, 0);
                    break;
            }
        }
        //-- Here so that you cant enable the autoclicker twice in a row.
        private void ChangeButtonStates(bool isClicking)
        {
            if (isClicking == true)
            {
                tbStartAC.IsEnabled = false;
                tbStopAC.IsEnabled = true;
            }
            else
            {
                tbStartAC.IsEnabled = true;
                tbStopAC.IsEnabled = false;
            }
        }

        private void ShowDebugInfo()
        {
            for (int i = 0; i < clickSettings.Length; i++)
            {
                MessageBox.Show(clickSettings[i].ToString());
            }
        }
    }
}