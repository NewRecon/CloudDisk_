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
using System.Text.RegularExpressions;

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
            if (LoginTextBox.Text.Length > 0 && LoginPassword.Password.Length > 0)
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
                {
                    MessageBox.Show("Неправльный email или пароль");
                    LoginTextBox.Text = "";
                    LoginPassword.Password = "";
                }
            }
            else
            {
                MessageBox.Show("Не все поля заполнены!");
                LoginTextBox.Text = "";
                LoginPassword.Password = "";

            }
        }

        private async void SingButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginTextBox.Text.Length > 0 && LoginPassword.Password.Length > 0)
            {
                if (IsValidEmail(LoginTextBox.Text))
                {
                    if (await Controller.RegistrationAsync(LoginTextBox.Text, LoginPassword.Password))
                    {
                        BackVisible();
                        listViewSourse(await Controller.CreateDirectoryAsync(""));
                        UploadFile.Visibility = Visibility.Visible;
                        loginButton.Visibility = Visibility.Collapsed;
                        SingButton.Visibility = Visibility.Collapsed;
                        current_User.Visibility = Visibility.Visible;
                        current_User.Content = LoginTextBox.Text;
                        Out.Visibility = Visibility.Visible;
                        CreateDirectory.Visibility = Visibility.Visible;
                        viewList.Visibility = Visibility.Visible;
                        loginStackPanel.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MessageBox.Show("Email уже зарегистрирован");
                        LoginTextBox.Text = "";
                        LoginPassword.Password = "";
                    }
                }
                else
                {
                    MessageBox.Show("Не верный формат почты");
                    LoginTextBox.Text = "";
                    LoginPassword.Password = "";
                }
            }
            else
            {
                MessageBox.Show("Не все поля заполнены!");
                LoginTextBox.Text = "";
                LoginPassword.Password = "";
            }
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
                    if (buf.Contains("."))
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
        //является ли строка допустимым представлением адреса электронной почты
        public static bool IsValidEmail(string email)
        {
            string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
            Match isMatch = Regex.Match(email.ToLower(), pattern, RegexOptions.IgnoreCase);
            return isMatch.Success;
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
                saveFileDialog.FileName = selectedListView();
                if (saveFileDialog.ShowDialog() == true)
                {
                    if (selectedListView().Contains("."))
                        await Controller.DownloadFileAsync(saveFileDialog.FileName.Remove(saveFileDialog.FileName.LastIndexOf('\\') + 1), selectedListView(), CurrentDirrectory);
                    else
                        await Controller.DownloadDirectoryAsync(saveFileDialog.FileName.Remove(saveFileDialog.FileName.LastIndexOf('\\') + 1), selectedListView(), CurrentDirrectory);
                    viewList.Items.Clear();
                    listViewSourse(await Controller.ShowAllFileInfoAsync(CurrentDirrectory));
                    CreateDirectory.Visibility = Visibility.Visible;
                    CreateDirectory_TextBox.Visibility = Visibility.Collapsed;
                    CreateDirectory_Ok.Visibility = Visibility.Collapsed;
                }
            }
            else MessageBox.Show("Выбирите элемент для сохранения");
        }

        private async void UploadFile_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                await Controller.UploadFileAsync(CurrentDirrectory, ofd.FileName);
                viewList.Items.Clear();
                listViewSourse(await Controller.ShowAllFileInfoAsync(CurrentDirrectory));
                SaveFile.Visibility= Visibility.Visible;
                DeleteFile.Visibility= Visibility.Visible;
                CreateDirectory.Visibility = Visibility.Visible;
                CreateDirectory_TextBox.Visibility = Visibility.Collapsed;
                CreateDirectory_Ok.Visibility = Visibility.Collapsed;
            }

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
            CreateDirectory_TextBox.Text = "Новая папка";
            CreateDirectory_Ok.Visibility = Visibility.Collapsed;
            viewList.Items.Clear();
            listViewSourse(await Controller.ShowAllFileInfoAsync(CurrentDirrectory));
            SaveFile.Visibility = Visibility.Visible;
            DeleteFile.Visibility = Visibility.Visible;

        }
        private async void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            if (viewList.SelectedItem != null)
            {
                if (selectedListView().Contains("."))
                    await Controller.DeleteFileAsync(CurrentDirrectory, selectedListView());
                else
                    await Controller.DeleteDirectoryAsync(CurrentDirrectory, selectedListView());
                viewList.Items.Clear();
                listViewSourse(await Controller.ShowAllFileInfoAsync(CurrentDirrectory));
                CreateDirectory.Visibility = Visibility.Visible;
                CreateDirectory_TextBox.Visibility = Visibility.Collapsed;
                CreateDirectory_Ok.Visibility = Visibility.Collapsed;
                if(viewList.Items.Count==0)
                {
                    SaveFile.Visibility = Visibility.Collapsed;
                    DeleteFile.Visibility = Visibility.Collapsed;
                }
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
            CurrentDirrectory = "";
            Button_Back.Visibility = Visibility.Collapsed;
            viewList.Items.Clear();
        }
        private async void Button_Back_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (CurrentDirrectory != "")
            {
                viewList.Items.Clear();
                CurrentDirrectory = CurrentDirrectory.TrimEnd('\\');
                 CurrentDirrectory = CurrentDirrectory.Remove(CurrentDirrectory.LastIndexOf("\\")+1);
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
                CurrentDirrectory += $@"{buf}\";
                viewList.Items.Clear();
                listViewSourse(await Controller.ShowAllFileInfoAsync(CurrentDirrectory));
                BackVisible();
            }
        }
    }
}