using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
    /// This is the source of the file manager, it's not yet implemented
    /// so I've only added a couple of things for the future.
    /// </summary>
    public partial class filemgr : Window
    {
        string path = "";
        bool isSpecial = false;
        int pathindex;

        public filemgr(string temppath, bool setSpecial)
        {
            InitializeComponent();
            path = temppath;
            isSpecial = setSpecial;
        }

        private void fmWin_Loaded(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(path))
            {
                ComboBoxItem curpath = new ComboBoxItem();
                curpath.Content = path;
                pathBox.Items.Add(curpath);
            }
        }

        public void GoPath(string gopath)
        {
            if (String.IsNullOrWhiteSpace(gopath))
                return;
            foreach (string dir in Directory.GetDirectories(gopath))
            {
                //left here
            }
        }
    }
}
