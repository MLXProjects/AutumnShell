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
using System.Windows.Shapes;
using Microsoft.Win32;

namespace autumn
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        RegistryKey regpath;
        string wppath;

        public Settings()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            regpath = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AutumnShell", RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl);
            if (regpath != null)
            {
                wppath = (string)regpath.GetValue("WallpaperPath");
            }
            else
            {
                regpath = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\AutumnShell", RegistryKeyPermissionCheck.ReadWriteSubTree);
                regpath.SetValue("WallpaperPath", @"");
            }
            wpBox.Text = wppath;
            if (!String.IsNullOrWhiteSpace(wppath))
                wpPreview.Source = new BitmapImage(new Uri(wppath));
        }

        public void SaveSettings()
        {
            try
            {
                if (regpath == null)
                    regpath = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AutumnShell", RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl);
                regpath.SetValue("WallpaperPath", wpBox.Text);
            }
            catch (Exception ex) { MessageBox.Show("Error saving settings: " + ex.ToString()); }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog wallpath = new OpenFileDialog();
            wallpath.Title = "Select a wallpaper image";
            wallpath.CheckFileExists = true;
            wallpath.DefaultExt = ".jpg";
            wallpath.AddExtension = true;
            wallpath.CheckPathExists = true;
            wallpath.Multiselect = false;
            wallpath.ShowDialog();
            if (!String.IsNullOrWhiteSpace(wallpath.FileName))
            {
                wpBox.Text = wallpath.FileName;
                wpPreview.Source = new BitmapImage(new Uri(wallpath.FileName));
                SaveSettings();
            }
        }
    }
}
