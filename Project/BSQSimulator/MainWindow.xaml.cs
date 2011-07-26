using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Threading;
using BSQSIMLib;
using System.Windows.Input;
using TUIO;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace BSQSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, TuioListener
    {
        TuioClient client;
        System.Windows.Forms.NotifyIcon icon;

        int DISPLAY_W = 0;
        int DISPLAY_H = 0;

        public MainWindow()
        {
            DrawLogSystem("Start program");
            InitializeComponent();

            DrawLogSystem("Component init: Done");

            BSQSim.Init();

            DrawLogSystem("BSQSim lib init: Done");

            Width = DISPLAY_W = BSQSim.Bounding.Width;
            Height = DISPLAY_H = BSQSim.Bounding.Height;

            DrawLogSystem("Set width height: Done");

            SystemEvents.DisplaySettingsChanging += new EventHandler(SystemEvents_DisplaySettingsChanging);

            this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);

            this.surfaceInkCanvas1.PreviewTouchUp += (e, a) => { surfaceInkCanvas1.Strokes.Clear(); };
            this.surfaceInkCanvas1.PreviewMouseLeftButtonUp += (e, a) => { surfaceInkCanvas1.Strokes.Clear(); };
            this.StateChanged += new EventHandler(MainWindow_StateChanged);

            DrawLogSystem("Event init: Done");

            icon = new System.Windows.Forms.NotifyIcon();
            icon.BalloonTipText = "Minimized... Click tray icon to show";
            icon.BalloonTipTitle = "BSQSimulator";
            icon.Icon = new System.Drawing.Icon("Logo.ico");
            icon.Text = "BSQSimulator";
            icon.Click += new EventHandler(icon_Click);

            DrawLogSystem("Icontray init: Done");
            DrawLogSystem("BSQSimulator init ! Resolution: " + DISPLAY_W + " " + DISPLAY_H);
        }

        void DrawLogSystem(string _content)
        {
            StreamWriter writer = new StreamWriter("simLog.log", true);
            writer.WriteLine(DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString() + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Millisecond + " " + _content);
            writer.Close();
        }

        void SystemEvents_DisplaySettingsChanging(object sender, EventArgs e)
        {
            Width = DISPLAY_W = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;
            Height = DISPLAY_H = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height;

            DrawLogSystem("DisplaySettingsChanging: Done");
        }

        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if(WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                if(icon!=null)
                {
                    icon.ShowBalloonTip(500);
                    icon.Visible = true;
                }
            }else if(this.WindowState == WindowState.Normal)
            {
                icon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        void icon_Click(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        private void bttBehavior_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.Close();
                    break;
            }
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BSQSim.UnLoadSystem();
            if (client != null) client.disconnect();

            DrawLogSystem("End program");                
            DrawLogSystem("*----------------*");
        }

        /*
        private void RegisterForRawInput()
        {
            NativeMethods.RAWINPUTDEVICE[] rawInputDevices = new NativeMethods.RAWINPUTDEVICE[1];
            rawInputDevices[0].usagePage = 1;
            rawInputDevices[0].usage = 2;
            rawInputDevices[0].flags = NativeMethods.RawInputDeviceFlags.InputSink;
            WindowInteropHelper helper = new WindowInteropHelper(this);
            rawInputDevices[0].hwndTarget = helper.Handle;
            uint cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTDEVICE));
            if (!NativeMethods.RegisterRawInputDevices(rawInputDevices, 1, cbSize))
            {
                //MessageBox.Show(InputSimulatorResources.UnableToRegisterForRawMouseInput, InputSimulatorResources.InitializationErrorCaption, MessageBoxButton.OK, MessageBoxImage.Hand);
                Application.Current.Shutdown(1);
            }
        }
        public void SetupSimulator(Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            if (source != null)
            {
                source.AddHook(new HwndSourceHook(this.MessageProc));
            }
        }
        private IntPtr MessageProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0xff)
            {
                Microsoft.Surface.NativeWrappers.NativeMethods.RAWINPUT rawInput = new Microsoft.Surface.NativeWrappers.NativeMethods.RAWINPUT();
                if (Microsoft.Surface.NativeWrappers.NativeMethods.GetRawInputMouseData(lParam, ref rawInput))
                {
                    //this.ProcessRawMouseInput(rawInput);
                }
            }
            return IntPtr.Zero;
        }
        */

        public void addTuioObject(TuioObject tuioObject) { }
        public void updateTuioObject(TuioObject tuioObject) { }
        public void removeTuioObject(TuioObject tuioObject) { }
        public void addTuioCursor(TuioCursor tuioCursor)
        {
            this.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                (Action)(() =>
                {
                    BSQSim.AddTouchDevice((int)tuioCursor.getSessionID(),
                        new Point(tuioCursor.getScreenX(Convert.ToInt32(DISPLAY_W)), tuioCursor.getScreenY(Convert.ToInt32(DISPLAY_H))));
                }));
        }

        public void updateTuioCursor(TuioCursor tuioCursor)
        {
            this.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                (Action)(() =>
                {
                    BSQSim.UpdateTouchDevice((int)tuioCursor.getSessionID(),
                        new Point(tuioCursor.getScreenX(Convert.ToInt32(DISPLAY_W)), tuioCursor.getScreenY(Convert.ToInt32(DISPLAY_H))));
                }));
        }

        public void removeTuioCursor(TuioCursor tuioCursor)
        {
            this.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                (Action)(() =>
                {
                    BSQSim.RemoveTouchDevice((int)tuioCursor.getSessionID());
                }));
        }

        public void refresh(long timestamp)
        {
            //throw new NotImplementedException();
        }

        private void bttClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void bttConnect_Click(object sender, RoutedEventArgs e)
        {
            if (client == null)
            {
                try
                {
                    client = new TuioClient(int.Parse(txtPort.Text));
                    client.addTuioListener(this);
                    client.connect();
                    bttConnect.Content = "Disconnect";
                    lblContent.Content = "Simulator has working !";
                    DrawLogSystem("Simulator has working!");

                }
                catch (Exception e2)
                {
                    lblContent.Content = "Can't connect to TUIO host!\nPlease change another port !";
                    client = null;

                    DrawLogSystem("Simulator can't connect to TUIO host!");
                }
            }
            else
            {
                client.disconnect();
                client = null;
                lblContent.Content = "Simulator has stop working !";
                bttConnect.Content = "Connect...";
                DrawLogSystem("Simulator has stop working!");
            }
        }
    }
}
