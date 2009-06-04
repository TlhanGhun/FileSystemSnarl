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
using Snarl;

namespace FileSystemSnarl
{
    /// <summary>
    /// Interaction logic for Events2Snarl.xaml
    /// </summary>
    /// 

    public partial class FileSystemSnarlClass : Window
    {
        static int displayTime = 20;
        static IntPtr hwnd = IntPtr.Zero;
        static SnarlNetwork.SnarlNetwork myNetwork;
        static string host = "127.0.0.1";
        static int port = 9887;
        static string appName = "FileSystemSnarl";
        static bool local = true;
        static bool isRunning = false;
        static string version = "1.0b1";
        static string iconFileName = "FileSystemSnarl.ico";
        static string iconPath = "";
        static string toBeWatchedFolder = "";
        static string lastFilename = "";
        static string lastType = "";
        private WindowState m_storedWindowState = WindowState.Normal;
        
        static DateTime lastNotification = new DateTime();
        static System.IO.FileSystemWatcher watcher = new System.IO.FileSystemWatcher();
        private static NativeWindowApplication.snarlMsgWnd snarlComWindow;

        private System.Windows.Forms.NotifyIcon m_notifyIcon;

        public FileSystemSnarlClass()
        {
            InitializeComponent();

            lastNotification = DateTime.Now;
            
            this.mainWindow.Title = "FileSystemSnarl " + version;
            string[] commandLine = Environment.GetCommandLineArgs();
            iconPath = System.IO.Path.GetDirectoryName(commandLine[0]) + "\\" + iconFileName;

            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.Text = "FileSystemSnarl";
            m_notifyIcon.Icon = new System.Drawing.Icon(iconFileName);
            m_notifyIcon.DoubleClick += new EventHandler(m_notifyIcon_Click);

            if (commandLine.Length > 1)
            {
                parseCommandLineArguments(commandLine);
            }
        }

        ~FileSystemSnarlClass()
        {
            if (isRunning)
            {
                SnarlConnector.RevokeConfig(hwnd);

            }
        }


        private void onNewEventLogEntry(object source, EntryWrittenEventArgs e)
        {
            EventLog myEvent = (EventLog)source;


        }

  
        private void targetIP_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Net.IPAddress testIP;
            // System.Net.IPHostEntry hostInfo = System.Net.Dns.GetHostByName(temp.Text);
            // very slow in this way
            System.Windows.Controls.TextBox temp = (System.Windows.Controls.TextBox)sender;
            if (System.Net.IPAddress.TryParse(temp.Text, out testIP))
            {
                host = temp.Text;
                targetIP.Background = new System.Windows.Media.SolidColorBrush(
               System.Windows.Media.Color.FromRgb(255, 255, 255)
                                                  );
            }
            else
            {
                targetIP.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 150, 150)
                                                   );
            }
        }

        private void targetPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox temp = (System.Windows.Controls.TextBox)sender;
            try
            {
                int newPort = Convert.ToInt32(temp.Text);
                port = newPort;
                targetPort.Background = new System.Windows.Media.SolidColorBrush(
System.Windows.Media.Color.FromRgb(255, 255, 255)
                                   );
            }
            catch
            {
                targetPort.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 150, 150)
                                                   );
            }
        }

        private void fieldDisplayTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                System.Windows.Controls.TextBox temp = (System.Windows.Controls.TextBox)sender;
                int newValue = Convert.ToInt32(temp.Text);
                displayTime = newValue;
                fieldDisplayTime.Background = new System.Windows.Media.SolidColorBrush(
               System.Windows.Media.Color.FromRgb(255, 255, 255)
                                                  );
            }
            catch
            {
                fieldDisplayTime.Background = new System.Windows.Media.SolidColorBrush(
               System.Windows.Media.Color.FromRgb(255, 150, 150)
                                                  );
            }
        }

        private void radioButton1_Checked(object sender, RoutedEventArgs e)
        {

            System.Windows.Controls.RadioButton temp = (System.Windows.Controls.RadioButton)sender;
            if (temp.IsChecked == true)
            {
                local = true;
            }
            else
            {
                local = false;
            }
        }

        private void radioButton2_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton temp = (System.Windows.Controls.RadioButton)sender;
            if (temp.IsChecked == true)
            {
                local = false;
            }
            else
            {
                local = false;
            }
        }

    private static void OnChanged(object source, FileSystemEventArgs e)
    {
        if (e.Name.Length >= 5)
        {
            if (e.Name.Substring(0, 5) == "RECYC")
            {
                return;
            }
        }
        
       string alertClass = "File has been " + e.ChangeType.ToString().ToLower();

       if(lastFilename == e.Name && lastType == e.ChangeType.ToString()) {
           DateTime waitTime = lastNotification.AddSeconds(5);
           if (lastNotification < waitTime)
           {
               // we just had this one...
               return;
           }
       }
       
       int id = SnarlConnector.ShowMessageEx(alertClass,alertClass, e.Name + "\n\n" + e.ChangeType.ToString(), 10, iconPath, hwnd, Snarl.WindowsMessage.WM_USER + 11,"");
        lastType = e.ChangeType.ToString();
        lastFilename = e.Name;
        lastNotification = DateTime.Now;
        if (e.ChangeType.ToString() != "Deleted")
        {
            snarlComWindow.memoPath(id, e.FullPath);
        }
    }


   static void OnRenamed( object source, RenamedEventArgs e )
   {
       SnarlConnector.ShowMessageEx("File has been renamed", "File has been renamed", "File has been renamed from " + e.OldName + " to " + e.Name, 10, iconPath, hwnd, Snarl.WindowsMessage.WM_USER + 13,"");
   }



        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isRunning)
            {
                isRunning = true;
                startButton.Content = "Stop watching";
                startButton.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(150, 0, 0)
                                                   );

                radioButton1.IsEnabled = false;
                radioButton2.IsEnabled = false;
                targetIP.IsEnabled = false;
                targetPort.IsEnabled = false;

                textBoxPath.IsEnabled = false;
                checkBoxIncludeSubdirectories.IsEnabled = false;
                textBoxFilter.IsEnabled = false;



                Snarl.WindowsMessage winMsg = Snarl.WindowsMessage.WM_USER + 10;
                if (local)
                {

                    if (hwnd == IntPtr.Zero)
                    {
                        snarlComWindow = new NativeWindowApplication.snarlMsgWnd();
                        hwnd = snarlComWindow.Handle;
                    }
                    SnarlConnector.RegisterConfig(hwnd, "FileSystemSnarl", winMsg, iconPath);
                    SnarlConnector.RegisterAlert("FileSystemSnarl", "File has been created");
                    SnarlConnector.RegisterAlert("FileSystemSnarl", "File has been changed");
                    SnarlConnector.RegisterAlert("FileSystemSnarl", "File has been renamed");
                    SnarlConnector.RegisterAlert("FileSystemSnarl", "File has been delected");
                }
                else
                {
                    myNetwork = new SnarlNetwork.SnarlNetwork(host, port);
                    myNetwork.register(host, port, appName);
                    myNetwork.addClass(host, port, appName, "Log added", "Log added");
                }

                watcher.Path = textBoxPath.Text;
                ////watcher.NotifyFilter = System.IO.NotifyFilters.LastAccess | NotifyFilters.LastWrite
                //watcher.NotifyFilter =  NotifyFilters.LastWrite | NotifyFilters.Size
                //   | NotifyFilters.FileName | NotifyFilters.DirectoryName;

                NotifyFilters myFilter = new NotifyFilters();
                NotifyFilters emptyFilter = new NotifyFilters();
   
                if (checkBoxAttributes.IsChecked.Value)
                {
                    myFilter = NotifyFilters.Attributes;
                }
                if (checkBoxDirectoryName.IsChecked.Value)
                {
                    myFilter = myFilter | NotifyFilters.DirectoryName;
                }
                if (checkBoxFilename.IsChecked.Value)
                {
                    myFilter = myFilter | NotifyFilters.FileName;
                }
                if (checkBoxLastAccess.IsChecked.Value)
                {
                    myFilter = myFilter | NotifyFilters.LastAccess;
                }
                if (checkBoxLastWrite.IsChecked.Value)
                {
                    myFilter = myFilter | NotifyFilters.LastWrite;
                }
                if (checkBoxSize.IsChecked.Value)
                {
                    myFilter = myFilter | NotifyFilters.Size;
                }

                if (myFilter != emptyFilter)
                {
                    watcher.NotifyFilter = myFilter;
                }
                else
                {
                    watcher.NotifyFilter = NotifyFilters.FileName;
                    
                }
                
                watcher.Filter = textBoxFilter.Text;

                watcher.IncludeSubdirectories = checkBoxIncludeSubdirectories.IsChecked.Value;

                // Add event handlers.
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnChanged);
                watcher.Deleted += new FileSystemEventHandler(OnChanged);
                watcher.Renamed += new RenamedEventHandler(OnRenamed);

                watcher.EnableRaisingEvents = true;

            }
            else
            {
                isRunning = false;
                if (local)
                {
                    SnarlConnector.RevokeConfig(hwnd);
                }
                else
                {
                    myNetwork.unregister(host, port, appName);
                }

                watcher.EnableRaisingEvents = false;
                startButton.Content = "Start forwarding";
                startButton.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0, 150, 0)
                                                   );
                radioButton1.IsEnabled = true;
                radioButton2.IsEnabled = true;
                targetIP.IsEnabled = true;
                targetPort.IsEnabled = true;

                textBoxPath.IsEnabled = true;
                checkBoxIncludeSubdirectories.IsEnabled = true;
                textBoxFilter.IsEnabled = true;
            }
        }

        // TrayIcon stuff

        void OnClose(object sender, System.ComponentModel.CancelEventArgs args)
        {
            m_notifyIcon.Dispose();
            m_notifyIcon = null;
        }


        void OnStateChanged(object sender, EventArgs args)
        {
            if (WindowState == WindowState.Minimized)
            {
                if (isRunning && local)
                {
                    m_notifyIcon.Text = appName + " (local)";
                }
                else if (isRunning && !local)
                {
                    m_notifyIcon.Text = appName + " (" + host + ")";
                }
                else
                {
                    m_notifyIcon.Text = appName + " (not connected)";
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








  

        private void parseCommandLineArguments(string[] commandLine)
        {
            return;

            /*
            bool directStart = false;
            bool minimized = false;
            string temp = "";
            bool lastParam = false;
            for (int i = 1; i != commandLine.Length; ++i)
            {
                temp = "";
                if (i == commandLine.Length)
                {
                    lastParam = true;
                }
                switch (commandLine[i].ToUpper())
                {
                    case "-TYPE":
                    case "-T":
                        if (commandLine[++i].ToUpper() == "REMOTE")
                        {
                            radioButton2.IsChecked = true;
                        }
                        break;
                    case "-REMOTEIP":
                    case "-R":
                        if (lastParam)
                        {
                            MessageBox.Show("Missing parameter for remote IP");
                            break;
                        }
                        temp = commandLine[++i];
                        targetIP.Text = temp;
                        break;
                    case "-PORT":
                    case "-P":
                        if (lastParam)
                        {
                            MessageBox.Show("Missing parameter for remote port");
                            break;
                        }
                        temp = commandLine[++i];
                        targetPort.Text = temp;
                        break;
                    case "-DISPLAYTIME":
                    case "-D":
                        if (lastParam)
                        {
                            MessageBox.Show("Missing parameter for display time");
                            break;
                        }
                        temp = commandLine[++i];
                        fieldDisplayTime.Text = temp;
                        break;
                    case "-START":
                    case "-S":
                        directStart = true;
                        break;
                    case "-FILTERTYPE":
                        if (i + 2 >= commandLine.Length)
                        {
                            MessageBox.Show("Not enough parameters for filterType");
                            break;
                        }
                        filterType = true;
                        checkBoxFilterType.IsChecked = true;
                        temp = commandLine[++i].ToUpper();
                        if (temp == "SHOW")
                        {
                            radioButtonFilterTypeShowOnly.IsChecked = true;
                            textBoxFilterTypeShow.Text = commandLine[++i];
                        }
                        else if (temp == "DONT")
                        {
                            radioButtonFilterTypeAvoid.IsChecked = true;
                            textBoxFilterTypeDont.Text = commandLine[++i];
                        }
                        break;
                    case "-FILTERSOURCE":
                        if (i + 2 >= commandLine.Length)
                        {
                            MessageBox.Show("Not enough parameters for filterSource");
                            break;
                        }
                        checkBoxFilterSource.IsChecked = true;
                        temp = commandLine[++i].ToUpper();
                        if (temp == "SHOW")
                        {
                            radioButtonFilterSourceShow.IsChecked = true;
                            textBoxFilterSourceShow.Text = commandLine[++i];
                        }
                        else if (temp == "DONT")
                        {
                            radioButtonFilterSourceDont.IsChecked = true;
                            textBoxFilterSourceDont.Text = commandLine[++i]; ;
                        }
                        break;
                    case "-FILTERMSG":
                    case "-FILTERMESSAGE":
                        if (i + 2 >= commandLine.Length)
                        {
                            MessageBox.Show("Not enough parameters for filterMsg");
                            break;
                        }
                        checkBoxFilterMessage.IsChecked = true;
                        temp = commandLine[++i].ToUpper();
                        if (temp == "SHOW")
                        {
                            radioButtonFilterMsgShow.IsChecked = true;
                            textBoxFilterMsgShow.Text = commandLine[++i];
                        }
                        else if (temp == "DONT")
                        {
                            radioButtonFilterMsgDont.IsChecked = true;
                            textBoxFilterMsgDont.Text = commandLine[++i];
                        }
                        break;
                    case "-MINIMIZED":
                    case "-M":
                        minimized = true;
                        break;

                    default: MessageBox.Show("Invalid args!"); return;
                }
            }
            if (directStart)
            {
                startButton_Click(null, null);
            }
            if (minimized)
            {
                ShowTrayIcon(true);
                if (isRunning && local)
                {
                    m_notifyIcon.Text = appName + " (local)";
                }
                else if (isRunning && !local)
                {
                    m_notifyIcon.Text = appName + " (" + host + ")";
                }
                else
                {
                    m_notifyIcon.Text = appName + " (not connected)";
                }
                WindowState = WindowState.Minimized;
                mainWindow.Visibility = Visibility.Hidden;

            }
             * */
        }

        private void hideNow(object source, EventArgs e ) {
            WindowState = WindowState.Minimized;
            
        }

        private void chooseFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowser.Description = "Choose the folder to be monitored";
            folderBrowser.ShowNewFolderButton = true;
            System.Windows.Forms.DialogResult result = folderBrowser.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                toBeWatchedFolder = folderBrowser.SelectedPath;
                textBoxPath.Text = folderBrowser.SelectedPath;
            }

        }

        private void textFieldWatchedFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox temp = (System.Windows.Controls.TextBox)sender;
            if (Directory.Exists(temp.Text))
            {
                startButton.IsEnabled = true;
                startButton.Background = new System.Windows.Media.SolidColorBrush(
System.Windows.Media.Color.FromRgb(0, 150, 0)
                                   );
                startButton.Content = "Start watching";
            }
            else
            {
                startButton.IsEnabled = false;
                startButton.Background = new System.Windows.Media.SolidColorBrush(
System.Windows.Media.Color.FromRgb(150, 150, 150)
                                   );
                startButton.Content = "Choose folder first";
                
            }
        }
    }

}
