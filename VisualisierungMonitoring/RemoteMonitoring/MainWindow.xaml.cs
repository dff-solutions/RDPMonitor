#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using RemoteMonitor;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Cursors = System.Windows.Input.Cursors;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Point = System.Windows.Point;
using Window = System.Windows.Window;

#endregion

namespace RemoteMonitoring
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string[] _args;
        private System.Windows.Forms.NotifyIcon MyNotifyIcon;
        private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private const string PathServerDirectory = "\\\\192.168.2.22\\projects\\dff\\intraweb\\remote_status";
        private const string HipChatToken = "9826fa999d476cd7ae21aed8ff4063";
        private const string HipChatRoom = "RemoteDesktop";
        private const string HipChatUser = "RemoteInfoBot";
        private readonly RemoteMonitorWorker _remoteMonitor;
        private delegate void RenderServerListCallback();


        /*  Parameter:
         *  con <Adresse>[:<Port>] "Name der Verbindung"
         *  dcon <Adresse>[:<Port>] "Name der Verbindnung"
         */
        public MainWindow()
        {
            InitializeComponent();
            MyNotifyIcon = new NotifyIcon
            {
                Icon = Properties.Resources.green
            };
            MyNotifyIcon.MouseClick += MyNotifyIcon_MouseClick;
            MyNotifyIcon.Visible = true;

            contextMenu1 = new System.Windows.Forms.ContextMenu();
            menuItem1 = new System.Windows.Forms.MenuItem();
            contextMenu1.MenuItems.AddRange(
                new[] { menuItem1 }
                );
            menuItem1.Index = 0;
            menuItem1.Text = "Be&enden";
            menuItem1.DefaultItem = true;
            menuItem1.Click += CloseWindow;
            MyNotifyIcon.ContextMenu = contextMenu1;

            var rsi = createStatusInfoFromArgs();
            if (rsi != null)
            {
                _remoteMonitor = new RemoteMonitorWorker(PathServerDirectory, rsi);
                MyNotifyIcon.Visible = false;
                Environment.Exit(0);
            }

            _remoteMonitor = new RemoteMonitorWorker(PathServerDirectory);
            _remoteMonitor.MyStatusChanged += RemoteMonitor_MyStatusChanged;
            _remoteMonitor.StatusRemoteChanged += RemoteMonitor_StatusRemoteChanged;
            RemoteMonitor_StatusRemoteChanged(new object(), new EventArgs());
            RenderServerList();
            WindowState = WindowState.Minimized;
        }
        private RemoteStatusInfo createStatusInfoFromArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Count() <= 3) return null;
            
            var rsi = new RemoteStatusInfo();
            rsi.Username = UsernameGet();
            rsi.State = args[1] == "con" ? Status.RDPAktiv : args[1] == "dcon" ? Status.RDPInaktiv : Status.Error;
            var sl = new Server();
            var Ip = args[2].Split(':').Length >= 1 ? args[2].Split(':')[0] : string.Empty;
            var Port = args[2].Split(':').Length >= 2 ? args[2].Split(':')[1] : "3389";
            sl.Domain = Ip + ":" + Port;
            var Name = args.Skip(3);
            sl.Name = string.Join(", ", Name);
            sl.EnteredServerTime = new DateTime();
            rsi.ServerList.Add(sl);
            Console.WriteLine(rsi.State == Status.RDPAktiv ? "connected to " : "disconnected from" + sl.Name + " [" + sl.Domain + "]");

            return rsi;
        }

        private static string UsernameGet()
        {
            var username = string.Empty;
            var identity = WindowsIdentity.GetCurrent();
            if (identity == null) return null;

            var principal = new WindowsPrincipal(identity);
            username = principal.Identity.Name;

            if (username.Contains("\\"))
                username = username.Substring(username.IndexOf("\\") + 1);
            return username;
        }
        private void CloseWindow(object sender, EventArgs e)
        {
            MyNotifyIcon.Visible = false;
            Environment.Exit(0);
        }

        private void RenderServerList()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new RenderServerListCallback(RenderServerList));
                return;
            }
            if (ServerWrapPanel.Children.Count > 0)
                ServerWrapPanel.Children.RemoveRange(0, ServerWrapPanel.Children.Count);

            if (_remoteMonitor.ServerList != null && _remoteMonitor.ServerList.Count > 0)
                _remoteMonitor.ServerList.ForEach(
                    x => ServerWrapPanel.Children.Add(RenderServListItem(x.Name, x.Domain, x.State, x.Username)));
        }

        public Border RenderServListItem(string name, string domain, Status free, string username)
        {
            var border = new Border();
            border.Width = 120;
            border.Height = 100;
            border.CornerRadius = new CornerRadius(5);
            border.BorderThickness = new Thickness(1);
            border.BorderBrush = Brushes.LightSlateGray;
            border.Margin = new Thickness(5, 5, 0, 0);
            border.PreviewMouseLeftButtonDown += ServerListItem_MouseLeftButtonDown;
            border.Tag = new RemoteServerDbItem(domain, name, free, username);
            if (free == Status.RDPInaktiv)
                border.Cursor = Cursors.Hand;

            var lineargradientbrush = new LinearGradientBrush();
            lineargradientbrush.SpreadMethod = GradientSpreadMethod.Reflect;
            lineargradientbrush.MappingMode = BrushMappingMode.Absolute;
            lineargradientbrush.StartPoint = new Point(0, 0);
            lineargradientbrush.EndPoint = new Point(100, 120);
            lineargradientbrush.GradientStops.Add(
                new GradientStop(
                    free == Status.RDPInaktiv
                        ? Color.FromRgb(144, 238, 144)
                        : free == Status.RDPAktiv ? Color.FromRgb(238, 144, 144) : Color.FromRgb(238, 238, 144), 0));
            lineargradientbrush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 255, 255), 0.5));
            lineargradientbrush.GradientStops.Add(
                new GradientStop(
                    free == Status.RDPInaktiv
                        ? Color.FromRgb(144, 238, 144)
                        : free == Status.RDPAktiv ? Color.FromRgb(238, 144, 144) : Color.FromRgb(238, 238, 144), 1));
            border.Background = lineargradientbrush;

            var grid = new Grid();
            var border2 = new Border();
            border2.HorizontalAlignment = HorizontalAlignment.Center;
            border2.VerticalAlignment = VerticalAlignment.Top;
            border2.Height = 65;

            var tb1 = new TextBlock();
            tb1.TextAlignment = TextAlignment.Center;
            tb1.VerticalAlignment = VerticalAlignment.Center;
            tb1.FontSize = 13;
            tb1.FontWeight = FontWeights.Bold;
            tb1.TextWrapping = TextWrapping.Wrap;
            tb1.Text = name;
            border2.Child = tb1;

            grid.Children.Add(border2);

            var tb2 = new TextBlock();
            tb2.HorizontalAlignment = HorizontalAlignment.Center;
            tb2.Margin = new Thickness(0, 65, 0, 0);
            tb2.Text = free == Status.RDPInaktiv ? "- frei -" : username;

            grid.Children.Add(tb2);

            var tb3 = new TextBlock();
            tb3.HorizontalAlignment = HorizontalAlignment.Center;
            tb3.VerticalAlignment = VerticalAlignment.Bottom;
            tb3.FontSize = 10;
            tb3.Text = domain;

            grid.Children.Add(tb3);

            border.Child = grid;
            return border;
        }

        private void MyNotifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(e.Button != MouseButtons.Left) return;
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
            RenderServerList();
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
            RenderServerList();
        }

        private void ServerListItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (((RemoteServerDbItem)((Border)sender).Tag).State == Status.RDPInaktiv)
            {
                var P = new Process();
                P.StartInfo.FileName = "mstsc.exe";
                P.StartInfo.Arguments = "/v " + ((RemoteServerDbItem)((Border)sender).Tag).Domain;
                P.Start();
            }
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
