using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using System.IO;
using FileSystemSnarl.Snarl;

namespace FileSystemSnarl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private WindowState m_storedWindowState = WindowState.Normal;
        private System.Windows.Forms.NotifyIcon m_notifyIcon;

        public MainWindow()
        {
            InitializeComponent();

            this.Title = "FileSystemSnarl " + AppController.Current.FormattedVersionString;

            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.Text = "FileSystemSnarl " + AppController.Current.FormattedVersionString;;
            m_notifyIcon.Icon = new System.Drawing.Icon("FileSystemSnarl.ico");
            m_notifyIcon.DoubleClick += new EventHandler(m_notifyIcon_Click);
        }
 
        private void targetIP_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Net.IPAddress testIP;
            TextBox textbox = sender as TextBox;
            if (System.Net.IPAddress.TryParse(textbox.Text, out testIP))
            {
                Properties.Settings.Default.snpIp = textbox.Text;
                targetIP.Background = Brushes.White;
            }
            else
            {
                targetIP.Background = Brushes.Red;
            }
        }

        private void targetPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            try
            {
                int newPort = Convert.ToInt32(textbox.Text);
                Properties.Settings.Default.snpPort = newPort;
                targetPort.Background = Brushes.White;
            }
            catch
            {
                targetPort.Background = Brushes.Red;
            }
        }

        private void fieldDisplayTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox textbox = sender as TextBox;
                int newValue = Convert.ToInt32(textbox.Text);
                Properties.Settings.Default.displayTime = newValue;
                fieldDisplayTime.Background = Brushes.White;
            }
            catch
            {
                fieldDisplayTime.Background = Brushes.Red;
            }
        }

        private void radioButtonUseLocalNotifications_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radiobutton = sender as RadioButton;
            if (radiobutton.IsChecked == true)
            {
                Properties.Settings.Default.snarlLocal = true;
            }
            else
            {
                Properties.Settings.Default.snarlLocal = false;
            }
        }

        private void radioButtonUseSnpNotifications_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radiobutton = sender as RadioButton;
            if (radiobutton.IsChecked == false)
            {
                Properties.Settings.Default.snarlLocal = true;
            }
            else
            {
                Properties.Settings.Default.snarlLocal = false;
            }
        }

  


        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            AppController.Current.startWatching();
        }

        #region tray icon

        void OnClose(object sender, System.ComponentModel.CancelEventArgs args)
        {
            Properties.Settings.Default.Save();
            try
            {
                m_notifyIcon.Dispose();
                m_notifyIcon = null;
            }
            catch {}
        }


        void OnStateChanged(object sender, EventArgs args)
        {
            if (WindowState == WindowState.Minimized)
            {
                if (AppController.Current.isRunning && Properties.Settings.Default.snarlLocal)
                {
                    m_notifyIcon.Text = "FileSystemSnarl (local)";
                }
                else if (AppController.Current.isRunning && !Properties.Settings.Default.snarlLocal)
                {
                    m_notifyIcon.Text = "FileSystemSnarl (" + Properties.Settings.Default.snpIp + ")";
                }
                else
                {
                    m_notifyIcon.Text = "FileSystemSnarl (not connected)";
                }
 
                Hide();

            }
            else
                m_storedWindowState = WindowState;
        }
        void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            CheckTrayIcon();
        }

        void m_notifyIcon_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = m_storedWindowState;
        }
        void CheckTrayIcon()
        {
            ShowTrayIcon(!IsVisible);
        }

        void ShowTrayIcon(bool show)
        {
            if (m_notifyIcon != null)
                m_notifyIcon.Visible = show;
        }  

        public void hideNow(object source, EventArgs e ) {
            WindowState = WindowState.Minimized;
            
        }
        #endregion

        private void chooseFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowser.Description = "Choose the folder to be monitored";
            folderBrowser.ShowNewFolderButton = true;
            System.Windows.Forms.DialogResult result = folderBrowser.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.folder = folderBrowser.SelectedPath;
                textBoxPath.Text = folderBrowser.SelectedPath;
            }

        }

        private void textFieldWatchedFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (Directory.Exists(textbox.Text))
            {
                startButton.IsEnabled = true;
                startButton.Background = Brushes.Green;
                startButton.Content = "Start watching";
            }
            else
            {
                startButton.IsEnabled = false;
                startButton.Background = Brushes.Gray;
                startButton.Content = "Choose folder first";
                
            }
        }

        private void buttonLicense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://tlhan-ghun.de");
            }
            catch { }
        }
    }

}
