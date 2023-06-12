using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Interface
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<string, Image> images = new Dictionary<string, Image>();
        
        public MainWindow()
        {
            string strtest = "/home/leonid/Рабочий стол/serverTEST/publish/шабанов/DZ_WinForms_week_1_1.pdf;241366;/home/leonid/Рабочий стол/serverTEST/publish/шабанов/Battle.net-Setup.exe;4838352;/home/leonid/Рабочий стол/serverTEST/publish/шабанов/котик (1).txt;27;/home/leonid/Рабочий стол/serverTEST/publish/шабанов/котик;27;/home/leonid/Рабочий стол/serverTEST/publish/шабанов/tsetup-x64.4.6.5.exe;40488912;";
            InitializeComponent();


            listViewSourse(strtest);

        }

        void addListViewEl(string str,string name)
        {
            using (MemoryStream ms = new MemoryStream(File.ReadAllBytes($"{str}.ico")))
            {
                StackPanel s = new StackPanel();
                s.Orientation = Orientation.Horizontal;
                Image img = new Image();
                TextBlock tb = new TextBlock();
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                img.Source = bitmapImage;
                img.Height = 16; img.Width = 16;

                tb.FontSize = 16;
                tb.Text = $" {name}";
                s.Children.Add(img);
                s.Children.Add(tb);
                viewList.Items.Add(s);
                viewList.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SingUpStackPanel.Visibility = Visibility.Collapsed;
            loginStackPanel.Visibility = Visibility.Visible;
        }

        private void SingButton_Click(object sender, RoutedEventArgs e)
        {
            SingUpStackPanel.Visibility = Visibility.Visible;
            loginStackPanel.Visibility = Visibility.Collapsed;
        }
        void listViewSourse(string str)
        {
            var massin = str.Split(';');
            string buf = "";
            for (int i = 0; i < massin.Length-1; i++) 
            {
                if(i%2 == 0)
                {
                    buf = massin[i].Substring(massin[i].LastIndexOf('/') + 1);
                    if(buf.Contains("."))
                    {
                        addListViewEl(buf.Substring(buf.LastIndexOf(".")+1),buf);
                    }
                    else
                    {
                        addListViewEl("folder",buf);
                    }
                }
            }


        }

        //Возвращает текст выбранного элемента listview
        public string selectedListView()
        {
            var s = (StackPanel)viewList.SelectedItem;

            return ((TextBlock)s.Children[1]).Text;
        }
        //Удаление выбранного элемента из listview
        public void delSelectedListView()
        {
            viewList.Items.Remove(viewList.SelectedItem);
        }


    }
}
