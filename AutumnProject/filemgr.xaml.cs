using System;
using Shell32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace autumn
{
    /// <summary>
    /// This is the file manager source, it's being implemented
    /// so expect some bugs and non-working things :P
    /// </summary>
    public partial class filemgr : Window
    {
        string path = "";
        bool isSpecial = false;

        public filemgr(string setpath, bool setSpecial)
        {
            InitializeComponent();
            path = setpath;
            isSpecial = setSpecial;
        }

        private void fmWin_Loaded(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(path))
            {
                ComboBoxItem curpath = new ComboBoxItem();
                curpath.Content = path;
                pathBox.Items.Add(curpath);
                pathBox.SelectedIndex = 0;
                GoPath(((ComboBoxItem)pathBox.SelectedItem).Content.ToString());
            }
        }

        public void GoPath(string gopath)
        {
            if (String.IsNullOrWhiteSpace(gopath) || !Directory.Exists(gopath))
                return;
            pathBox.Text = gopath;
            ComboBoxItem curpath = new ComboBoxItem();
            curpath.Content = pathBox.Text;
            pathBox.Items.Add(curpath);
            pathBox.SelectedItem = curpath;
            dirlist.Items.Clear();
            foreach (string dir in Directory.GetDirectories(gopath))
            {
                try
                {
                    ImageSource icn;
                    icn = winutils.BitmapToImageSource(autumn.Properties.Resources.folder);
                    string dirname = dir.Substring(System.IO.Path.GetDirectoryName(dir).Length + 1);
                    FileFolder itm = new FileFolder { FileIcon = icn, FileName = dirname, FullPath = System.IO.Path.GetFullPath(dir) };
                    this.DataContext = itm;
                    dirlist.Items.Add(itm);
                }
                catch { MessageBox.Show("Error while adding " + dir + "to the list.", "error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
            foreach (string file in Directory.GetFiles(gopath))
            {
                try
                {
                    ImageSource icn;
                    icn = winutils.GetIcon(file);
                    if (icn == null)
                        icn = winutils.BitmapToImageSource(autumn.Properties.Resources.file);
                    FileFolder itm = new FileFolder { FileIcon = icn, FileName = System.IO.Path.GetFileNameWithoutExtension(file), FullPath = System.IO.Path.GetFullPath(file) };
                    this.DataContext = itm;
                    dirlist.Items.Add(itm);
                }
                catch { MessageBox.Show("Error while adding " + file + "to the list.", "error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
            path = gopath;
        }

        private void goBtn_Click(object sender, RoutedEventArgs e)
        {
            GoPath(pathBox.Text);
        }

        public class FileFolder
        {
            public ImageSource FileIcon { get; set; }
            public string FileName { get; set; }
            public string FullPath { get; set; }
        }

        private void dirlist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileFolder item = ((FrameworkElement)e.OriginalSource).DataContext as FileFolder;
            if (item != null)
            {
                bool isDirectory = Directory.Exists(item.FullPath);
                if (isDirectory)
                {
                    GoPath(item.FullPath);
                }
                else
                {
                    if (item.FullPath.Substring(item.FullPath.Length - 4) == ".lnk" && Directory.Exists(GetShortcutTargetFile(item.FullPath)))
                        GoPath(GetShortcutTargetFile(item.FullPath));
                    else 
                        Process.Start(item.FullPath);
                }
            }
        }

        public static string GetShortcutTargetFile(string shortcutFilename)
        {
            string pathOnly = System.IO.Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = System.IO.Path.GetFileName(shortcutFilename);

            Shell shell = new Shell();
            Folder folder = shell.NameSpace(pathOnly);
            FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
                return link.Path;
            }

            return string.Empty;
        }
    }
}
