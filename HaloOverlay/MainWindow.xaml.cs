using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Controls;
using System.Media;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Navigation;

namespace HaloOverlay
{
    public static class WindowsServices
    {
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;

        //Modifiers:
        private const uint MOD_NONE = 0x0000; //(none)
        private const uint MOD_ALT = 0x0001; //ALT
        private const uint MOD_CONTROL = 0x0002; //CTRL
        private const uint MOD_SHIFT = 0x0004; //SHIFT
        private const uint MOD_WIN = 0x0008; //WINDOWS

        // FOR MORE KEYCODES
        // https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes?redirectedfrom=MSDN
        private const uint VK_F1 = 0x70;
        private const uint VK_F2 = 0x71;
        private const uint VK_F3 = 0x72;
        private const uint VK_F4 = 0x73;
        private const uint VK_F10 = 0x79;

        public MainWindow()
        {
            InitializeComponent();
        }

        private IntPtr _windowHandle;
        private HwndSource _source;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, VK_F1);
            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, VK_F2);
            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, VK_F3);
            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, VK_F4);
            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, VK_F10);
            WindowsServices.SetWindowExTransparent(_windowHandle);
        }
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == VK_F1)
                            {
                                handleF1Timer();
                            }
                            if (vkey == VK_F2)
                            {
                                handleF2Timer();
                            }
                            if (vkey == VK_F3)
                            {
                                handleF3Timer();
                            }
                            if (vkey == VK_F4)
                            {
                                handleF4Timer();
                            }
                            if (vkey == VK_F10)
                            {
                                togglePopup();
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            base.OnClosed(e);
        }

        DispatcherTimer f1dt = new DispatcherTimer();
        DispatcherTimer f2dt = new DispatcherTimer();
        DispatcherTimer f3dt = new DispatcherTimer();
        DispatcherTimer f4dt = new DispatcherTimer();
        private int f1time;
        private int f2time;
        private int f3time;
        private int f4time;
        private bool f1active = false;
        private bool f2active = false;
        private bool f3active = false;
        private bool f4active = false;


        public int shotgunTimer = 120;
        public int swordTimer = 120;
        public int sniperTimer = 180;
        public int rocketTimer = 180;        

        
        private void handleF1Timer()
        {
            if (!f1active)
            {
                f1Image.Visibility = Visibility.Hidden;
                f1active = true;
                f1time = shotgunTimer;
                setF1Content();
                f1dt = new DispatcherTimer();
                f1dt.Interval = TimeSpan.FromSeconds(1);
                f1dt.Tick += F1dt_Tick;
                f1dt.Start();
                if (overlayCheck)
                {
                    playWarningSound("shotgunStarted");
                }
            }
            else
            {
                f1dt.Stop();
                f1Timer.Content = "";
                f1Image.Visibility = Visibility.Visible;
                f1active = false;
            }

        }

        private void F1dt_Tick(object sender, EventArgs e)
        {
            f1time--;
            setF1Content();
        }
        private void setF1Content()
        {
            TimeSpan time = TimeSpan.FromSeconds(f1time);
            f1Timer.Content = time.ToString(@"m\:ss");
            if (f1time == 0)
            {
                f1Timer.Foreground = new SolidColorBrush(Colors.LimeGreen);
                f1Timer.Content = "LIVE";
                f1dt.Stop();
                f1active = false;
            }
            else if (f1time >= 60)
            {
                f1Timer.Foreground = new SolidColorBrush(Colors.White);
            }
            else if (f1time < 60 && f1time > 30)
            {
                f1Timer.Foreground = new SolidColorBrush(Colors.Yellow);
            }
            else if (f1time <= 30)
            {
                f1Timer.Foreground = new SolidColorBrush(Colors.Red);
            }
            if(f1time == 30 && warningCheckBox.IsChecked == true)
            {
                playWarningSound("shotgunWarning");
            }
            if(f1time == 0 && alertCheckBox.IsChecked == true)
            {
                playWarningSound("shotgunAlert");
            }
        }



        private void handleF2Timer()
        {
            if (!f2active)
            {
                f2Image.Visibility = Visibility.Hidden;
                f2active = true;
                f2time = swordTimer;
                setF2Content();
                f2dt = new DispatcherTimer();
                f2dt.Interval = TimeSpan.FromSeconds(1);
                f2dt.Tick += F2dt_Tick;
                f2dt.Start();
                if (overlayCheck)
                {
                    playWarningSound("swordStarted");
                }
            }
            else
            {
                f2dt.Stop();
                f2Timer.Content = "";
                f2Image.Visibility = Visibility.Visible;
                f2active = false;
            }

        }

        private void F2dt_Tick(object sender, EventArgs e)
        {
            f2time--;
            setF2Content();
        }
        private void setF2Content()
        {
            TimeSpan time = TimeSpan.FromSeconds(f2time);
            f2Timer.Content = time.ToString(@"m\:ss");
            if (f2time == 0)
            {
                f2Timer.Foreground = new SolidColorBrush(Colors.LimeGreen);
                f2Timer.Content = "LIVE";
                f2dt.Stop();
                f2active = false;
            }
            else if (f2time >= 60)
            {
                f2Timer.Foreground = new SolidColorBrush(Colors.White);
            }
            else if (f2time < 60 && f2time > 30)
            {
                f2Timer.Foreground = new SolidColorBrush(Colors.Yellow);
            }
            else if (f2time <= 30)
            {
                f2Timer.Foreground = new SolidColorBrush(Colors.Red);
            }
            if (f2time == 30 && warningCheckBox.IsChecked == true)
            {
                playWarningSound("swordWarning");
            }
            if (f2time == 0 && alertCheckBox.IsChecked == true)
            {
                playWarningSound("swordAlert");
            }
        }

        private void handleF3Timer()
        {
            if (!f3active)
            {
                f3Image.Visibility = Visibility.Hidden;
                f3active = true;
                f3time = sniperTimer;
                setF3Content();
                f3dt = new DispatcherTimer();
                f3dt.Interval = TimeSpan.FromSeconds(1);
                f3dt.Tick += F3dt_Tick;
                f3dt.Start();
                if (overlayCheck)
                {
                    playWarningSound("sniperStarted");
                }
            }
            else
            {
                f3dt.Stop();
                f3Timer.Content = "";
                f3Image.Visibility = Visibility.Visible;
                f3active = false;
            }

        }

        private void F3dt_Tick(object sender, EventArgs e)
        {
            f3time--;
            setF3Content();
        }
        private void setF3Content()
        {
            TimeSpan time = TimeSpan.FromSeconds(f3time);
            f3Timer.Content = time.ToString(@"m\:ss");
            if (f3time == 0)
            {
                f3Timer.Foreground = new SolidColorBrush(Colors.LimeGreen);
                f3Timer.Content = "LIVE";
                f3dt.Stop();
                f3active = false;
            }
            else if (f3time >= 60)
            {
                f3Timer.Foreground = new SolidColorBrush(Colors.White);
            }
            else if (f3time < 60 && f3time > 30)
            {
                f3Timer.Foreground = new SolidColorBrush(Colors.Yellow);
            }
            else if (f3time <= 30)
            {
                f3Timer.Foreground = new SolidColorBrush(Colors.Red);
            }
            if (f3time == 30 && warningCheckBox.IsChecked == true)
            {
                playWarningSound("sniperWarning");
            }
            if (f3time == 0 && alertCheckBox.IsChecked == true)
            {
                playWarningSound("sniperAlert");
            }
        }

        private void handleF4Timer()
        {
            if (!f4active)
            {
                f4Image.Visibility = Visibility.Hidden;
                f4active = true;
                f4time = rocketTimer;
                setF4Content();
                f4dt = new DispatcherTimer();
                f4dt.Interval = TimeSpan.FromSeconds(1);
                f4dt.Tick += F4dt_Tick;
                f4dt.Start();
                if(overlayCheck)
                {
                    playWarningSound("rocketsStarted");
                }
            }
            else
            {
                f4dt.Stop();
                f4Timer.Content = "";
                f4Image.Visibility = Visibility.Visible;
                f4active = false;
            }

        }

        private void F4dt_Tick(object sender, EventArgs e)
        {
            f4time--;
            setF4Content();
        }
        private void setF4Content()
        {
            TimeSpan time = TimeSpan.FromSeconds(f4time);
            f4Timer.Content = time.ToString(@"m\:ss");
            if (f4time == 0)
            {
                f4Timer.Foreground = new SolidColorBrush(Colors.LimeGreen);
                f4Timer.Content = "LIVE";
                f4dt.Stop();
                f4active = false;
            }
            else if (f4time >= 60)
            {
                f4Timer.Foreground = new SolidColorBrush(Colors.White);
            }
            else if (f4time < 60 && f4time > 30)
            {
                f4Timer.Foreground = new SolidColorBrush(Colors.Yellow);
            }
            else if (f4time <= 30)
            {
                f4Timer.Foreground = new SolidColorBrush(Colors.Red);
            }
            if (f4time == 30 && warningCheckBox.IsChecked == true)
            {
                playWarningSound("rocketsWarning");
            }
            if (f4time == 0 && alertCheckBox.IsChecked == true)
            {
                playWarningSound("rocketsAlert");
            }
        }
        private void togglePopup()
        {
            mapPopup.IsOpen = !mapPopup.IsOpen;
        }
        public bool alertCheck = true;
        private void alertCheckChanged(object sender, RoutedEventArgs e)
        {
            alertCheck = !alertCheck;
        }
        public bool warningCheck = true;
        private void warningCheckChanged(object sender, RoutedEventArgs e)
        {
            warningCheck = !warningCheck;
        }
        public bool overlayCheck = false;
        private void overlayCheckChanged(object sender, RoutedEventArgs e)
        {
            overlayCheck = !overlayCheck;
            if(overlayCheck)
            {
                outerBorder.Visibility = Visibility.Hidden;
                warningCheckBox.IsEnabled = false;
                warningCheckBox.IsChecked = true;
                alertCheckBox.IsEnabled = false;
                alertCheckBox.IsChecked = true;
            } else
            {
                outerBorder.Visibility = Visibility.Visible;
                warningCheckBox.IsEnabled = true;
                alertCheckBox.IsEnabled = true;
            }
        }
        private void playWarningSound(string soundName)
        {
            string appFolderPath = AppDomain.CurrentDomain.BaseDirectory;
            string fileName = "sounds/" + soundName + ".wav";
            string filePath = Path.Combine(appFolderPath, fileName);
            Console.WriteLine(filePath);

            SoundPlayer player = new SoundPlayer(filePath);

            player.Load();
            player.Play();
        }

        public Button selectedButton;

        private void mapSelection(object sender, RoutedEventArgs e)
        {
            if(selectedButton != null)
            {
                selectedButton.BorderThickness = new Thickness(0);
            }
            clearAllTimers();
            selectedButton = (sender as Button);
            string name = selectedButton.Name.ToString();
            int[] mapTimers = getMapTimers(name);
            shotgunTimer = mapTimers[0];
            swordTimer = mapTimers[1];
            sniperTimer = mapTimers[2];
            rocketTimer = mapTimers[3];
            selectedButton.BorderThickness = new Thickness(3);
        }
        private int[] getMapTimers(string mapName)
        {
            switch (mapName) {
                case "BoardwalkBtn":
                    // No sword - plasma launcher 3 min
                   return new int[] {120, 180, 120, 180};
                case "BoneyardBtn":
                    //Validate Shotgun/Sword
                    return new int[] { 90, 120, 180, 180 };
                case "AsylumBtn":
                    // No Rockets
                    return new int[] { 90, 90, 90, 90 };
                case "TheCageBtn":
                    // No sword/rockets
                    return new int[] { 60, 60, 75, 90 };
                case "CountdownBtn":
                    // No sniper/rockets
                    return new int[] { 180, 180, 180, 180 };
                case "HemorrhageBtn":
                    // Validate Shotgun/Sword/Rockets
                    return new int[] { 90, 90, 120, 180 };
                case "ParadisoBtn":
                    // Validate Shotgun/Sword/Rockets
                    return new int[] { 120, 120, 180, 180 };
                case "PinnacleBtn":
                    // No Sword
                    return new int[] { 180, 180, 180, 180 };
                case "PowerhouseBtn":
                    // No Sword, maybe use Grav hammer/Grenade Launcher - 2min/90sec respectively
                    return new int[] { 180, 120, 180, 180 };
                case "ReflectionBtn":
                    // Should all be correct
                    return new int[] { 180, 180, 180, 180 };
                case "SpireBtn":
                    // Validate Shotgun/Sword, consider including vehicles
                    return new int[] { 90, 90, 120, 120 };
                case "SwordBaseBtn":
                    // No sniper, concussion rifle instead of rockets
                    return new int[] { 120, 120, 180, 180 };
                case "ZealotBtn":
                    // No shotgun/Sniper/Rockets. Concussion Rifle as rockets
                    return new int[] { 120, 120, 120, 180 };
                default:
                    return new int[] { 31, 31, 31, 31 };
            }

        }
        private void clearAllTimers()
        {
            f1dt.Stop();
            f1Timer.Content = "";
            f1Image.Visibility = Visibility.Visible;
            f1active = false;
            f2dt.Stop();
            f2Timer.Content = "";
            f2Image.Visibility = Visibility.Visible;
            f2active = false;
            f3dt.Stop();
            f3Timer.Content = "";
            f3Image.Visibility = Visibility.Visible;
            f3active = false;
            f4dt.Stop();
            f4Timer.Content = "";
            f4Image.Visibility = Visibility.Visible;
            f4active = false;
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }



    class timerObject {
        public int initialTime, currentTime;
        public Label label;
        public Image image;
        public DispatcherTimer dt = new DispatcherTimer();
        public bool active = false;
        
        public timerObject(int initTime, Label lab, Image img)
        {
            initialTime = initTime;
            label = lab;
            image = img;
        }
        public void handleTimer()
        {
            if (!active)
            {
                
                image.Visibility = Visibility.Hidden;
                active = true;
                currentTime = initialTime;
                setContent();
                dt = new DispatcherTimer();
                dt.Interval = TimeSpan.FromSeconds(1);
                dt.Tick += dt_Tick;
                dt.Start();
            }
            else
            {
                dt.Stop();
                label.Content = "";
                image.Visibility = Visibility.Visible;
                active = false;
            }
        }
        private void dt_Tick(object sender, EventArgs e)
        {
            currentTime--;
            setContent();
        }
        private void setContent()
        {
            TimeSpan time = TimeSpan.FromSeconds(currentTime);
            label.Content = time.ToString(@"m\:ss");
            if (currentTime == 0)
            {
                label.Foreground = new SolidColorBrush(Colors.LimeGreen);
                label.Content = "LIVE";
                dt.Stop();
                active = false;
            }
            else if (currentTime >= 60)
            {
                label.Foreground = new SolidColorBrush(Colors.White);
            }
            else if (currentTime < 60 && currentTime > 30)
            {
                label.Foreground = new SolidColorBrush(Colors.Yellow);
            }
            else if (currentTime <= 30)
            {
                label.Foreground = new SolidColorBrush(Colors.Red);
            }
        }
    }
    // Main Method 
    //public timerObject[] GenerateArray()
    //{
    //    // To declare an array of two objects 
    //    timerObject[] timerArray = new timerObject[4];

    //    // Initialize the objects 
    //    timerArray[0] = new timerObject(shotgunTimer, f1Timer, f1Image);
    //    timerArray[1] = new timerObject(swordTimer, f2Timer, f2Image);
    //    timerArray[2] = new timerObject(sniperTimer, f3Timer, f3Image);
    //    timerArray[3] = new timerObject(rocketTimer, f4Timer, f4Image);

    //    return timerArray;
    //}
}
