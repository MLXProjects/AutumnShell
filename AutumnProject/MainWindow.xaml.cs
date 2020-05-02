using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Wilsons;
using System.IO;
using Microsoft.Win32;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace autumn
{
    public partial class MainWindow : Window
    {
        RegistryKey regpath;
        string wppath;
        private GlobalHooks globalHook;
        string explorerpath = @"C:\Windows\explorer.exe";
        RegistryKey winlogonpath;
        int AutoRestartShell;

        public MainWindow()
        {
            InitializeComponent();
            winlogonpath = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl);
            AutoRestartShell = (int)winlogonpath.GetValue("AutoRestartShell");
            if (AutoRestartShell != 0)
            {
                winlogonpath.SetValue("AutoRestartShell", 0, RegistryValueKind.DWord);
                AutoRestartShell = (int)winlogonpath.GetValue("AutoRestartShell");
            }
            foreach (Process proc in Process.GetProcessesByName("explorer"))
            {
                explorerpath = proc.MainModule.FileName;
                proc.CloseMainWindow();
                proc.Kill();
                proc.WaitForExit();
            }
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            globalHook = new Wilsons.GlobalHooks((new WindowInteropHelper(this)).Handle);
            ShowWindow((new WindowInteropHelper(this)).Handle, 0);
            ShowWindow((new WindowInteropHelper(this)).Handle, 8);
            globalHook.Shell.WindowActivated += new GlobalHooks.WindowEventHandler(Shell_WindowActivated);
            globalHook.Shell.WindowCreated += new GlobalHooks.WindowEventHandler(Shell_WindowCreated);
            globalHook.Shell.WindowDestroyed += new GlobalHooks.WindowEventHandler(Shell_WindowDestroyed);
            globalHook.Shell.Start();
            DesktopRefresh();
            UpdateWinList();
            ReadRegSettings();
        }

        #region desktop management

        public void ReadRegSettings()
        {
            regpath = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AutumnShell", RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl);
            if (regpath != null)
            {
                wppath = (string)regpath.GetValue("WallpaperPath");
            }
            else
            {
                try {
                    regpath = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\AutumnShell", RegistryKeyPermissionCheck.ReadWriteSubTree);
                    regpath.SetValue("WallpaperPath", @"");
                    }
                catch (Exception ex) { MessageBox.Show("Error saving settings: " + ex.ToString()); }
            }
            if (!String.IsNullOrWhiteSpace(wppath))
                winMain.Background = new ImageBrush(new BitmapImage(new Uri(wppath)));
        }

        public void DesktopRefresh()
        {
            foreach (string dir in Directory.GetDirectories(Environment.GetEnvironmentVariable("userprofile") + @"\Desktop"))
            {
                try
                {
                    ImageSource icn;
                    icn = winutils.BitmapToImageSource(autumn.Properties.Resources.folder);
                    string dirname = dir.Substring(System.IO.Path.GetDirectoryName(dir).Length + 1);
                    DesktopItem itm = new DesktopItem { FileIcon = icn, FileName = dirname, FullPath = dir };
                    this.DataContext = itm;
                    desktop.Items.Add(itm);
                }
                catch { MessageBox.Show("Error while adding " + dir + "to the list.", "error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
            foreach (string file in Directory.GetFiles(Environment.GetEnvironmentVariable("userprofile") + @"\Desktop"))
            {
                try
                {
                    ImageSource icn;
                    icn = winutils.GetIcon(file);
                    if (icn == null)
                        break;
                    DesktopItem itm = new DesktopItem { FileIcon = icn, FileName = System.IO.Path.GetFileNameWithoutExtension(file), FullPath = file };
                    this.DataContext = itm;
                    desktop.Items.Add(itm);
                }
                catch { MessageBox.Show("Error while adding " + file + "to the list.", "error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        protected void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DesktopItem item = ((FrameworkElement)e.OriginalSource).DataContext as DesktopItem;

            if (item != null)
            {
                bool isDirectory = Directory.Exists(item.FullPath);
                if (isDirectory)
                {
                    //this should call our custom file manager (being implemented).
                    filemgr FM = new filemgr(item.FullPath, false);
                    FM.Show();
                    //For now, we'll stop relying on original Explorer's FM
                    //Process.Start(explorerpath, item.FullPath);
                }
                else
                {
                    if (item.FullPath.Substring(item.FullPath.Length - 4) == ".lnk" && Directory.Exists(filemgr.GetShortcutTargetFile(item.FullPath)))
                        (new filemgr(filemgr.GetShortcutTargetFile(item.FullPath), false)).Show();
                    else
                        Process.Start(item.FullPath);
                }
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            (new Settings()).ShowDialog();
            ReadRegSettings();
        }

        public class DesktopItem
        {
            public ImageSource FileIcon { get; set; }
            public string FileName { get; set; }
            public string FullPath { get; set; }
        }
        #endregion desktop management

        #region taskbar
        public void UpdateWinList()
        {
            taskBar.Children.Clear();
            foreach (KeyValuePair<IntPtr, string> window in winutils.GetOpenWindows())
            {
                if (window.Key != ((new WindowInteropHelper(this)).Handle))
                {
                    Button appbtn = new Button();
                    System.Drawing.Image icn = winutils.GetSmallWindowIcon(window.Key);
                    using (var ms = new MemoryStream())
                    {
                        icn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        ms.Seek(0, SeekOrigin.Begin);
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                        Image img = new Image();
                        img.Source = bitmapImage;
                        StackPanel stackPnl = new StackPanel();
                        stackPnl.Background = Brushes.Transparent;
                        stackPnl.Children.Add(img);
                        appbtn.Content = stackPnl;
                    }
                    appbtn.Style = (Style)FindResource(ToolBar.ButtonStyleKey);
                    appbtn.ToolTip = window.Value;
                    appbtn.Width = 48;
                    appbtn.Tag = window.Key;
                    appbtn.Click += new RoutedEventHandler(appbtn_Click);
                    taskBar.Children.Add(appbtn);
                }
            }
        }

        private void appsBtn_Click(object sender, RoutedEventArgs e)
        {
            AppsMenu appsmenu = new AppsMenu(this.Height - bottomPanel.Height, this.Left);
            appsmenu.Show();
            appsmenu.Closed += (s, args) => { if (appsmenu.CloseShell) try { this.Close(); } catch { /* tried to close while close dialog present */ } };
        }

        public void appbtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            IntPtr hWnd = (IntPtr)btn.Tag;
            if (IsIconic(hWnd))
                ShowWindow(hWnd, 9);
            else ShowWindow(hWnd, 6);
            HighlightButton(GetForegroundWindow());
        }

        public void HighlightButton(IntPtr Handle)
        {
            foreach (Button appbtn in taskBar.Children)
            {
                if ((IntPtr)appbtn.Tag != Handle)
                {
                    if (appbtn.ToolTip.ToString().Substring(0, 8) == "Active: ")
                    {
                        appbtn.ToolTip = appbtn.ToolTip.ToString().Substring(8);
                        appbtn.Background = new SolidColorBrush(Color.FromArgb(200, Brushes.Blue.Color.R, Brushes.Blue.Color.G, Brushes.Blue.Color.B));                        
                    }
                }
                else
                {
                    appbtn.ToolTip = "Active: " + appbtn.ToolTip;
                    appbtn.Background = new SolidColorBrush(Color.FromArgb(200, Brushes.Cyan.Color.R, Brushes.Cyan.Color.G, Brushes.Cyan.Color.B));
                }
            }
        }
        #endregion taskbar

        #region winhooks
        public void Shell_WindowDestroyed(IntPtr Handle)
        {
            UpdateWinList();
        }

        public void Shell_WindowCreated(IntPtr Handle)
        {
            UpdateWinList();
        }

        public void Shell_WindowActivated(IntPtr Handle)
        {
            HighlightButton(Handle);
        }
        #endregion winhooks

        #region Windows API Helper Functions

        private const int WS_EX_TOOLWINDOW = 0x0080;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            //Set the window style to noactivate.
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE,
                GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr handle);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        [DllImport("User32")]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private string GetWindowName(IntPtr Hwnd)
        {
            // This function gets the name of a window from its handle
            StringBuilder Title = new StringBuilder(256);
            GetWindowText(Hwnd, Title, 256);

            return Title.ToString().Trim();
        }

        private string GetWindowClass(IntPtr Hwnd)
        {
            // This function gets the name of a window class from a window handle
            StringBuilder Title = new StringBuilder(256);
            RealGetWindowClass(Hwnd, Title, 256);

            return Title.ToString().Trim();
        }

        // API calls to give us a bit more information about the data we get from the hook
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder title, int size);
        [DllImport("user32.dll")]
        private static extern uint RealGetWindowClass(IntPtr hWnd, StringBuilder pszType, uint cchType);

        #endregion
        
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Do you want to close the shell?", "Close? ;-;", MessageBoxButton.OKCancel);
            if (res == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }


        private void winMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (e.Cancel == false)
            {
                globalHook.Shell.Stop();
                if (AutoRestartShell == 0)
                    winlogonpath.SetValue("AutoRestartShell", 1, RegistryValueKind.DWord);
                Process.Start(explorerpath);
            }
        }

    }
}
