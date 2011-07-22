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

namespace BSQSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, TuioListener
    {
        TuioClient client;
        public MainWindow()
        {
            InitializeComponent();

            BSQSim.Init();

            Width = BSQSim.Bounding.Width;
            Height = BSQSim.Bounding.Height;

            this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);

            this.surfaceInkCanvas1.PreviewTouchUp += (e, a) => { surfaceInkCanvas1.Strokes.Clear(); };
            this.surfaceInkCanvas1.PreviewMouseLeftButtonUp += (e, a) => { surfaceInkCanvas1.Strokes.Clear(); };
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
                        new Point(tuioCursor.getScreenX(Convert.ToInt32(Width)), tuioCursor.getScreenY(Convert.ToInt32(Height))));
                }));
        }

        public void updateTuioCursor(TuioCursor tuioCursor)
        {
            this.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                (Action)(() =>
                {
                    BSQSim.UpdateTouchDevice((int)tuioCursor.getSessionID(),
                        new Point(tuioCursor.getScreenX(Convert.ToInt32(Width)), tuioCursor.getScreenY(Convert.ToInt32(Height))));
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
                    bttConnect.Content = "Connected";
                    lblContent.Content = "Simulator has working !";
                }
                catch (Exception e2)
                {
                    lblContent.Content = "Can't connect to TUIO host!\nPlease change another port !";
                    client = null;
                }
            }
            else
            {
                client.disconnect();
                client = null;
                lblContent.Content = "Disconnected !";
                bttConnect.Content = "Connect...";
            }
        }
    }
}
