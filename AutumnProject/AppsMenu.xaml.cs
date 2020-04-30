using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace autumn
{
    /// <summary>
    /// Interaction logic for appsmenu.xaml
    /// </summary>
    public partial class AppsMenu : Window
    {
        public AppsMenu(double top, double left)
        {
            InitializeComponent();
            this.Top = top - this.Height;
            this.Left = left;
        }

        public void AppsMenuWin_Loaded(object sender, RoutedEventArgs e)
        {
            gridMain.Margin = new Thickness(this.Left - this.Width, gridMain.Margin.Top, gridMain.Margin.Right, gridMain.Margin.Bottom);
            foreach (string file in Directory.GetFiles(Environment.GetEnvironmentVariable("appdata") + @"\Microsoft\Windows\Start Menu\Programs", "*.lnk", SearchOption.AllDirectories))
            {
                try
                {
                    ImageSource icn;
                    icn = winutils.GetIcon(file);
                    if (icn == null)
                        break;
                    LBItem itm = new LBItem { FileIcon = icn, FileName = System.IO.Path.GetFileNameWithoutExtension(file), FullPath = file };
                    this.DataContext = itm;
                    appsList.Items.Add(itm);
                }
                catch { MessageBox.Show("Error while adding " + file + "to the list.", "error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
            var sb = new Storyboard();
            var ta = new ThicknessAnimation();
            ta.BeginTime = new TimeSpan(0);
            ta.SetValue(Storyboard.TargetNameProperty, "gridMain");
            Storyboard.SetTargetProperty(ta, new PropertyPath(MarginProperty));
            ta.From = new Thickness(gridMain.Margin.Left, gridMain.Margin.Top, gridMain.Margin.Right, gridMain.Margin.Bottom);
            ta.To = new Thickness(this.Left + 8, gridMain.Margin.Top, gridMain.Margin.Right, gridMain.Margin.Bottom);
            ta.Duration = new Duration(TimeSpan.FromSeconds(1));
            sb.Children.Add(ta);
            sb.Begin(this);
        }

        public bool CloseShell { get; private set; }

        public class LBItem
        {
            public ImageSource FileIcon { get; set; }
            public string FileName { get; set; }
            public string FullPath { get; set; }
        }

        private void AppsMenuWin_Deactivated(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch { } //fast way to make it don't crash :P
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            CloseShell = true;
            this.Close();
        }

        private void appsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LBItem item = ((FrameworkElement)e.OriginalSource).DataContext as LBItem;

            if (item != null)
            {
                System.Diagnostics.Process.Start(item.FullPath);
            }
        }
    }
}
