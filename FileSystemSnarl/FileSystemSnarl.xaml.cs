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
        static bool filterType = false;
        static bool filterMsg = false;
        static bool filterSource = false;
        static bool filterTypeShow = true;
        static bool filterMsgShow = true;
        static bool filterSourceShow = true;
        static string filterTypeTextShow = "";
        static string filterMsgTextShow = "";
        static string filterSourceTextShow = "";
        static string filterTypeTextDont = "";
        static string filterMsgTextDont = "";
        static string filterSourceTextDont = "";
        static string lastFilename = "";
        static string lastType = "";
        private WindowState m_storedWindowState = WindowState.Normal;
        
        static DateTime lastNotification = new DateTime();


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
        string alertClass = "New file created";
        if (e.ChangeType.ToString() == "Changed")
        {
            alertClass = "File has been changed";
        }
        else if (e.ChangeType.ToString() == "Deleted")
        {
            alertClass = "File has been deleted";
        }
        // Specify what is done when a file is changed, created, or deleted.
       //Console.WriteLine("File: " +  e.FullPath + " " + e.ChangeType);

       if(lastFilename == e.Name && lastType == e.ChangeType.ToString()) {
           DateTime waitTime = lastNotification.AddSeconds(5);
           if (lastNotification < waitTime)
           {
               // we just had this one...
               return;
           }
       }
       
       SnarlConnector.ShowMessage(alertClass, e.Name + "\n\n" + e.ChangeType.ToString(), 10, iconPath, hwnd, Snarl.WindowsMessage.WM_USER + 13);
        lastType = e.ChangeType.ToString();
        lastFilename = e.Name;
        lastNotification = DateTime.Now;
    }


   static void OnRenamed( object source, RenamedEventArgs e )
   {
      // Specify what is done when a file is renamed.
      //Console::WriteLine( "File: {0} renamed to {1}", e->OldFullPath, e->FullPath );
       SnarlConnector.ShowMessage("File has been renamed", "File has been renamed from " + e.OldName + " to " + e.Name, 10, iconPath, hwnd, Snarl.WindowsMessage.WM_USER + 13);
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



                Snarl.WindowsMessage winMsg = new Snarl.WindowsMessage();
                if (local)
                {
                    System.Windows.Interop.HwndSourceParameters winParams = new System.Windows.Interop.HwndSourceParameters();

                    hwnd = new System.Windows.Interop.HwndSource(winParams).Handle;

                    SnarlConnector.RegisterConfig(hwnd, "FileSystemSnarl", winMsg, iconPath);
                    SnarlConnector.RegisterAlert("FileSystemSnarl", "Log added");
                }
                else
                {
                    myNetwork = new SnarlNetwork.SnarlNetwork(host, port);
                    myNetwork.register(host, port, appName);
                    myNetwork.addClass(host, port, appName, "Log added", "Log added");
                }


                // Create a new FileSystemWatcher and set its properties.
                System.IO.FileSystemWatcher watcher = new System.IO.FileSystemWatcher();
                //watcher.Path = "\\\\10.58.2.196\\fcdata\\Images\\20090602";
                watcher.Path = textFieldWatchedFolder.Text;
                /* Watch for changes in LastAccess and LastWrite times, and 
                   the renaming of files or directories. */
                //watcher.NotifyFilter = System.IO.NotifyFilters.LastAccess | NotifyFilters.LastWrite
                watcher.NotifyFilter =  NotifyFilters.LastWrite | NotifyFilters.Size
                   | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Only watch text files.
                //watcher.Filter = "*.txt";
                watcher.Filter = "*.*";
                watcher.IncludeSubdirectories = true;

                // Add event handlers.
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnChanged);
                watcher.Deleted += new FileSystemEventHandler(OnChanged);
                watcher.Renamed += new RenamedEventHandler(OnRenamed);

                watcher.EnableRaisingEvents = true;


                return;


                EventLog[] allLogs = EventLog.GetEventLogs();

 


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
                startButton.Content = "Start forwarding";
                startButton.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0, 150, 0)
                                                   );
                radioButton1.IsEnabled = true;
                radioButton2.IsEnabled = true;
                targetIP.IsEnabled = true;
                targetPort.IsEnabled = true;
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

        private void checkBoxFilterType_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox temp = (System.Windows.Controls.CheckBox)sender;
            if (temp.IsChecked.Value)
            {
                radioButtonFilterTypeShowOnly.IsEnabled = true;
                radioButtonFilterTypeAvoid.IsEnabled = true;
                textBoxFilterTypeDont.IsEnabled = true;
                textBoxFilterTypeShow.IsEnabled = true;
            }
            filterType = true;
        }

        private void checkBoxFilterType_Unchecked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox temp = (System.Windows.Controls.CheckBox)sender;
            if (!temp.IsChecked.Value)
            {
                radioButtonFilterTypeShowOnly.IsEnabled = false;
                radioButtonFilterTypeAvoid.IsEnabled = false;
                textBoxFilterTypeDont.IsEnabled = false;
                textBoxFilterTypeShow.IsEnabled = false;
            }
            filterType = false;
        }

        private void radioButtonFilterTypeShowOnly_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton temp = (System.Windows.Controls.RadioButton)sender;
            if (temp.IsChecked.Value)
            {
                filterTypeShow = true;
            }
            else
            {
                filterTypeShow = false;
            }
        }

        private void radioButtonFilterTypeAvoid_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton temp = (System.Windows.Controls.RadioButton)sender;
            if (temp.IsChecked.Value)
            {
                filterTypeShow = false;
            }
            else
            {
                filterTypeShow = true;
            }
        }

        private void textBoxFilterTypeShow_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox temp = (System.Windows.Controls.TextBox)sender;
            filterTypeTextShow = temp.Text;
        }

        private void textBoxFilterTypeDont_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox temp = (System.Windows.Controls.TextBox)sender;
            filterTypeTextDont = temp.Text;
        }

        private void checkBoxFilterSource_unchecked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox temp = (System.Windows.Controls.CheckBox)sender;
            if (!temp.IsChecked.Value)
            {
                radioButtonFilterSourceShow.IsEnabled = false;
                radioButtonFilterSourceDont.IsEnabled = false;
                textBoxFilterSourceDont.IsEnabled = false;
                textBoxFilterSourceShow.IsEnabled = false;
            }
            filterSource = false;
        }

        private void checkBoxFilterSource_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox temp = (System.Windows.Controls.CheckBox)sender;
            if (temp.IsChecked.Value)
            {
                radioButtonFilterSourceShow.IsEnabled = true;
                radioButtonFilterSourceDont.IsEnabled = true;
                textBoxFilterSourceDont.IsEnabled = true;
                textBoxFilterSourceShow.IsEnabled = true;
            }
            filterSource = true;
        }

        private void radioButtonFilterSourceShow_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton temp = (System.Windows.Controls.RadioButton)sender;
            if (temp.IsChecked.Value)
            {
                filterSourceShow = true;
            }
            else
            {
                filterSourceShow = false;
            }
        }

        private void radioButtonFilterSourceDont_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton temp = (System.Windows.Controls.RadioButton)sender;
            if (temp.IsChecked.Value)
            {
                filterSourceShow = false;
            }
            else
            {
                filterSourceShow = true;
            }
        }

        private void textBoxFilterSourceShow_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox temp = (System.Windows.Controls.TextBox)sender;
            filterSourceTextShow = temp.Text;
        }

        private void textBoxFilterSourceDont_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox temp = (System.Windows.Controls.TextBox)sender;
            filterSourceTextDont = temp.Text;
        }

        // Msg

        private void checkBoxFilterMessage_unchecked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox temp = (System.Windows.Controls.CheckBox)sender;
            if (!temp.IsChecked.Value)
            {
                radioButtonFilterMsgShow.IsEnabled = false;
                radioButtonFilterMsgDont.IsEnabled = false;
                textBoxFilterMsgDont.IsEnabled = false;
                textBoxFilterMsgShow.IsEnabled = false;
            }
            filterMsg = false;
        }

        private void checkBoxFilterMessage_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox temp = (System.Windows.Controls.CheckBox)sender;
            if (temp.IsChecked.Value)
            {
                radioButtonFilterMsgShow.IsEnabled = true;
                radioButtonFilterMsgDont.IsEnabled = true;
                textBoxFilterMsgDont.IsEnabled = true;
                textBoxFilterMsgShow.IsEnabled = true;
            }
            filterMsg = true;
        }

        private void radioButtonFilterMsgShow_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton temp = (System.Windows.Controls.RadioButton)sender;
            if (temp.IsChecked.Value)
            {
                filterMsgShow = true;
            }
            else
            {
                filterMsgShow = false;
            }
        }

        private void radioButtonFilterMsgDont_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton temp = (System.Windows.Controls.RadioButton)sender;
            if (temp.IsChecked.Value)
            {
                filterMsgShow = false;
            }
            else
            {
                filterMsgShow = true;
            }
        }

        private void textBoxFilterMsgShow_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox temp = (System.Windows.Controls.TextBox)sender;
            filterMsgTextShow = temp.Text;
        }

        private void textBoxFilterMsgDont_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox temp = (System.Windows.Controls.TextBox)sender;
            filterMsgTextDont = temp.Text;
        }

        private void parseCommandLineArguments(string[] commandLine)
        {
            return;
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
                textFieldWatchedFolder.Text = folderBrowser.SelectedPath;
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
            }
            else
            {
                startButton.IsEnabled = false;
                startButton.Background = new System.Windows.Media.SolidColorBrush(
System.Windows.Media.Color.FromRgb(150, 150, 150)
                                   );
            }
        }
    }

}
