using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Media;

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

            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, VK_F1); //CTRL + CAPS_LOCK
            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, VK_F2); //CTRL + CAPS_LOCK
            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, VK_F3); //CTRL + CAPS_LOCK
            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, VK_F4); //CTRL + CAPS_LOCK
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
        static int f1timeStartInt = 120;
        static int f2timeStartInt = 180;
        static int f3timeStartInt = 180;
        static int f4timeStartInt = 180;
        private int f1time = f1timeStartInt;
        private int f2time = f2timeStartInt;
        private int f3time = f3timeStartInt;
        private int f4time = f4timeStartInt;
        private bool f1active = false;
        private bool f2active = false;
        private bool f3active = false;
        private bool f4active = false;

        private void handleF1Timer()
        {
            if(!f1active)
            {
                f1Image.Visibility = Visibility.Hidden;
                f1active = true;
            f1time = f1timeStartInt;
            setF1Content();
            f1dt = new DispatcherTimer();
            f1dt.Interval = TimeSpan.FromSeconds(1);
            f1dt.Tick += F1dt_Tick;
            f1dt.Start();
            } else
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
        }



        private void handleF2Timer()
        {
            if (!f2active)
            {
                f2Image.Visibility = Visibility.Hidden;
                f2active = true;
                f2time = f2timeStartInt;
                setF2Content();
                f2dt = new DispatcherTimer();
                f2dt.Interval = TimeSpan.FromSeconds(1);
                f2dt.Tick += F2dt_Tick;
                f2dt.Start();
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
        }

        private void handleF3Timer()
        {
            if (!f3active)
            {
                f3Image.Visibility = Visibility.Hidden;
                f3active = true;
                f3time = f3timeStartInt;
                setF3Content();
                f3dt = new DispatcherTimer();
                f3dt.Interval = TimeSpan.FromSeconds(1);
                f3dt.Tick += F3dt_Tick;
                f3dt.Start();
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
        }

        private void handleF4Timer()
        {
            if (!f4active)
            {
                f4Image.Visibility = Visibility.Hidden;
                f4active = true;
                f4time = f3timeStartInt;
                setF4Content();
                f4dt = new DispatcherTimer();
                f4dt.Interval = TimeSpan.FromSeconds(1);
                f4dt.Tick += F4dt_Tick;
                f4dt.Start();
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
        }
    }
}
