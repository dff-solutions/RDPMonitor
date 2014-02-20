using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using RemoteMonitor;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Window = System.Windows.Window;


namespace RemoteMonitoring
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        private System.Windows.Forms.NotifyIcon MyNotifyIcon;
        private const string PathServerDirectory = "\\\\192.168.2.22\\projects\\dff\\intraweb\\remote_status";
        private const string HipChatToken = "9826fa999d476cd7ae21aed8ff4063";
        private const string HipChatRoom = "RemoteDesktop";
        private const string HipChatUser = "RemoteInfoBot";

        public MainWindow()
        {
            InitializeComponent();
            MyNotifyIcon = new NotifyIcon();
            MyNotifyIcon.Icon = new System.Drawing.Icon(
                            @"C:\Users\mfassmann\Downloads\Deleket-Soft-Scraps-Button-Blank-Green.ico");
            MyNotifyIcon.MouseDoubleClick +=
                new System.Windows.Forms.MouseEventHandler
                    (MyNotifyIcon_MouseDoubleClick);
            MyNotifyIcon.Visible = true;
            var remoteMonitor = new RemoteMonitorWorker(PathServerDirectory);
            remoteMonitor.MyStatusChanged += RemoteMonitor_MyStatusChanged;
            remoteMonitor.StatusRemoteChanged += RemoteMonitor_StatusRemoteChanged;
            RemoteMonitor_StatusRemoteChanged(new object(), new EventArgs() );

        }

        private void MyNotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Visibility = Visibility.Visible;
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Visibility = Visibility.Collapsed;
        }

        private void OnClose(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Collapsed;

            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            timer.Interval = new TimeSpan(0, 0, 3);
            timer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            timer.Stop();    
            popup1.Placement = PlacementMode.AbsolutePoint;
            popup1.HorizontalOffset = Screen.PrimaryScreen.WorkingArea.Width - 220;
            popup1.VerticalOffset = Screen.PrimaryScreen.WorkingArea.Height - 80;
            popup1.Visibility = Visibility.Visible;
            popup1.IsOpen = false;
            
        }

        private void RemoteMonitor_StatusRemoteChanged(object sender, EventArgs e)
        {
            // The Work to perform on another thread
            ThreadStart start = delegate()
            {
             //This will work as its using the dispatcher
            DispatcherOperation op = Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action<string>(CreatePopup), RemoteMonitor.ServerClient.StatusReportAsStringGet(PathServerDirectory));

                DispatcherOperationStatus status = op.Status;
                while (status != DispatcherOperationStatus.Completed)
                {
                    status = op.Wait(TimeSpan.FromMilliseconds(1000));
                    if (status == DispatcherOperationStatus.Aborted)
                    {
                        // Alert Someone
                    }
                }
            };
            new Thread(start).Start();
        }

        private void CreatePopup(RemoteEventArgs e)
        {
            var popupText = new TextBlock();
            if (e == null || e.Info == null || e.Info.ServerList == null || e.Info.ServerList.Count == 0)
                popupText.Text = "Keine aktive .rdp Verbindung";
            else
            {
                popupText.Text = "aktive Verbindungen: \r\n";
                foreach (var server in e.Info.ServerList)
                {
                    popupText.Text += server + "\r\n";
                }    
                popupText.Text += e.Info.Username;
            }
            
            setPopup(popupText);
        }

        private void CreatePopup(string e)
        {
            var popupText = new TextBlock();
            popupText.Text = e.Replace("<br>","\r\n");
            popupText.TextWrapping = TextWrapping.Wrap;
            setPopup(popupText);
        }

        private void RemoteMonitor_MyStatusChanged(object sender, RemoteEventArgs e)
        {
            // The Work to perform on another thread
            ThreadStart start = delegate()
            {
                // ...

                // This will work as its using the dispatcher
                DispatcherOperation op = Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Action<RemoteEventArgs>(CreatePopup),
                    e);

                DispatcherOperationStatus status = op.Status;
                while (status != DispatcherOperationStatus.Completed)
                {
                    status = op.Wait(TimeSpan.FromMilliseconds(1000));
                    if (status == DispatcherOperationStatus.Aborted)
                    {
                        // Alert Someone
                    }
                }
            };
            new Thread(start).Start();
        }

        private void setPopup(TextBlock popupText)
        {
            popupText.TextAlignment = TextAlignment.Center;
            popupText.Background = Brushes.SteelBlue;
            popupText.Foreground = Brushes.White;
            popupText.Opacity = 0.9;
            popup1.HorizontalOffset = Screen.PrimaryScreen.WorkingArea.Width - 220;
            popup1.VerticalOffset = Screen.PrimaryScreen.WorkingArea.Height - 80;
            popup1.Placement = PlacementMode.AbsolutePoint;
            popup1.Child = popupText;
            popup1.Height = 80;
            popup1.Width = 220;
            popup1.IsOpen = true;
            var sound = new SoundPlayer(Assembly.GetExecutingAssembly().
                     GetManifestResourceStream("Voicbeep.wav"));
            sound.Play();
        }

        private void Popup1_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Visibility = Visibility.Visible;
        }

        private void Popup1_OnOpened(object sender, EventArgs e)
        {
            var timer = new DispatcherTimer();
            timer.Tick += (delegate { popup1.IsOpen = false; timer.Stop(); });
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Start();
            
        }
    }
}
