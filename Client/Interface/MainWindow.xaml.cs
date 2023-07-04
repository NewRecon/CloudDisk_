using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

using ControllerDLL;

namespace Interface
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public string CurrentDirrectory = "";
        Dictionary<string, Image> images = new Dictionary<string, Image>();
        public MainWindow()
        {          
            InitializeComponent();          
        }
        void addListViewEl(string str,string name)
        {
            if (File.Exists($"{str}.ico"))
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
                    ms.Close();
                    ms.Dispose();
                }

            }
            else
            {
                using (MemoryStream ms = new MemoryStream(File.ReadAllBytes($"unknown.ico")))
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
                    ms.Close();
                    ms.Dispose();
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (await Controller.AuthorizationAsync(LoginTextBox.Text, LoginPassword.Password))
            {
                BackVisible();
                viewList.Visibility = Visibility.Visible;
                listViewSourse(await Controller.ShowAllFileInfoAsync(""));
                loginStackPanel.Visibility = Visibility.Collapsed;
                if (viewList.Items.Count > 0)
                {
                    SaveFile.Visibility = Visibility.Visible;
                    DeleteFile.Visibility = Visibility.Visible;
                }
                UploadFile.Visibility = Visibility.Visible;
                loginButton.Visibility = Visibility.Collapsed;
                SingButton.Visibility = Visibility.Collapsed;
                current_User.Visibility = Visibility.Visible;
                current_User.Content = LoginTextBox.Text;
                Out.Visibility = Visibility.Visible;
                CreateDirectory.Visibility = Visibility.Visible;
            }
            else
                MessageBox.Show("Неправльный email или пароль");
        }

        private async void SingButton_Click(object sender, RoutedEventArgs e)
        {
            if (await Controller.RegistrationAsync(LoginTextBox.Text, LoginPassword.Password))
            {
                BackVisible();
                listViewSourse(await Controller.CreateMainDirectoryAsync());
                UploadFile.Visibility = Visibility.Visible;
                loginButton.Visibility = Visibility.Collapsed;
                SingButton.Visibility = Visibility.Collapsed;
                current_User.Visibility = Visibility.Visible;
                current_User.Content = LoginTextBox.Text;
                Out.Visibility = Visibility.Visible;
                CreateDirectory.Visibility = Visibility.Visible;
                viewList.Visibility = Visibility.Visible;
            }
            else
                MessageBox.Show("Email уже зарегистрирован");
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
                    buf = massin[i].Substring(massin[i].LastIndexOf('\\') + 1);
                    if(buf.Contains("."))
                    {
                        addListViewEl(buf.Substring(buf.LastIndexOf(".") + 1),buf);
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

            return (((TextBlock)s.Children[1]).Text).Substring(1);
        }
        //Удаление выбранного элемента из listview
        public void delSelectedListView()
        {
            viewList.Items.Remove(viewList.SelectedItem);
        }

        private async void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (viewList.SelectedItem != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "dcs";
                saveFileDialog.FileName = selectedListView();
                if (saveFileDialog.ShowDialog() == true)
                {
                    await Controller.DownloadFileAsync(saveFileDialog.FileName, selectedListView());                   
                }
            }
            viewList.Items.Clear();
            listViewSourse(await Controller.ShowAllFileInfoAsync(CurrentDirrectory));
        }

        private async void UploadFile_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                await Controller.UploadFileAsync(ofd.FileName, ofd.FileName.Substring(ofd.FileName.LastIndexOf('\\') + 1));
            }
            viewList.Items.Clear();
            listViewSourse(await Controller.ShowAllFileInfoAsync(CurrentDirrectory));
        }
        private void CreateDirectory_Click(object sender, RoutedEventArgs e)
        {
            CreateDirectory.Visibility = Visibility.Collapsed;
            CreateDirectory_TextBox.Visibility = Visibility.Visible;
            CreateDirectory_Ok.Visibility = Visibility.Visible;           
        }

        private async void CreateDirectory_Ok_Click(object sender, RoutedEventArgs e)
        {
            await Controller.CreateDirectoryAsync(CurrentDirrectory + "\\" + CreateDirectory_TextBox.Text);
            CreateDirectory.Visibility = Visibility.Visible;
            CreateDirectory_TextBox.Visibility = Visibility.Collapsed;
            CreateDirectory_Ok.Visibility = Visibility.Collapsed;
            viewList.Items.Clear();
            listViewSourse(await Controller.ShowAllFileInfoAsync(CurrentDirrectory));

        }
        private async void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            if (viewList.SelectedItem != null)
            {
                await Controller.DeleteFileAsync(CurrentDirrectory, selectedListView());
                viewList.Items.Clear();
                listViewSourse(await Controller.ShowAllFileInfoAsync(CurrentDirrectory));
            }
            else MessageBox.Show("Выбирите элемент для удаления");
        }

        private void Out_Click(object sender, RoutedEventArgs e)
        {
            current_User.Visibility = Visibility.Collapsed;
            Out.Visibility = Visibility.Collapsed;
            SaveFile.Visibility = Visibility.Collapsed;
            UploadFile.Visibility = Visibility.Collapsed;
            DeleteFile.Visibility = Visibility.Collapsed;
            viewList.Visibility = Visibility.Collapsed;
            loginButton.Visibility = Visibility.Visible;
            SingButton.Visibility = Visibility.Visible;
            CreateDirectory.Visibility = Visibility.Collapsed;
            LoginTextBox.Text = "";
            LoginPassword.Clear();
            loginStackPanel.Visibility = Visibility.Visible;
            CreateDirectory.Visibility= Visibility.Collapsed;
            CreateDirectory_Ok.Visibility= Visibility.Collapsed;
            CreateDirectory_TextBox.Visibility= Visibility.Collapsed;
        }
        private async void Button_Back_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (CurrentDirrectory != "")
            {
                viewList.Items.Clear();
                CurrentDirrectory = CurrentDirrectory.Remove(CurrentDirrectory.LastIndexOf("\\"));
                listViewSourse(await Controller.ShowAllFileInfoAsync(CurrentDirrectory));
                BackVisible();
            }
        }
        void BackVisible()
        {

            if (CurrentDirrectory.Length > 0)  Button_Back.Visibility = Visibility.Visible;
            else Button_Back.Visibility = Visibility.Collapsed;
        }
        private async void viewList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var a = ((((sender as ListView).SelectedItem) as StackPanel).Children[1]) as TextBlock;
            if(!(((((sender as ListView).SelectedItem) as StackPanel).Children[1]) as TextBlock).Text.Contains("."))
            {               
                string buf = a.Text.Substring(1);
                CurrentDirrectory += $@"\{buf}";
                /*if (CurrentDirrectory == null)
                    CurrentDirrectory += a.Text;
                else CurrentDirrectory += $@"\{a.Text}";*/
                viewList.Items.Clear();
                listViewSourse(await Controller.ShowAllFileInfoAsync(CurrentDirrectory));
                BackVisible();
            }
        }

    }
}