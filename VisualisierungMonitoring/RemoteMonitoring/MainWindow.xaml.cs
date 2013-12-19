using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
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


namespace RemoteMonitoring
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        private System.Windows.Forms.NotifyIcon MyNotifyIcon;

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
            popup1.IsOpen = true;
            
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            popup1.Placement = PlacementMode.AbsolutePoint;
            popup1.HorizontalOffset = Screen.PrimaryScreen.WorkingArea.Width - 220;
            popup1.VerticalOffset = Screen.PrimaryScreen.WorkingArea.Height - 80;
            popup1.Visibility = Visibility.Visible;
            popup1.IsOpen = true;
        }
    }
}
