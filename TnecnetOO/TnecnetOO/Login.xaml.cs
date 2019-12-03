using System;
using System.Collections.Generic;
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
using LeanCloud;
using Microsoft.Win32;

namespace TnecnetOO
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        string loginemail = "";
        string loginpassword = "";
        public Login()
        {
            InitializeComponent();
        }
        public Login(string username, string password) : this()
        {
            loginemail = username;
            loginpassword = password;
        }
        private async void Btnlogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AVUser user = await AVUser.LogInByEmailAsync(email.Text, password.Password);

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                //关闭界面
                this.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show("登录失败，原因" + err.Message);
            }
        }
        //注册按钮点击事件
        private void Btnregister_Click(object sender, RoutedEventArgs e)
        {
            Register register = new Register(email.Text, password.Password);
            register.Show();
            this.Close();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            email.Text = loginemail;
            password.Password = loginpassword;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Email_GotFocus(object sender, RoutedEventArgs e)
        {
            emailtip.FontSize = 14;
        }

        private void Email_LostFocus(object sender, RoutedEventArgs e)
        {
            emailtip.FontSize = 12;
        }

        private void Password_GotFocus(object sender, RoutedEventArgs e)
        {
            passwordtip.FontSize = 14;
        }

        private void Password_LostFocus(object sender, RoutedEventArgs e)
        {
            passwordtip.FontSize = 12;
        }
        //拖拽窗口
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        //按键快捷键
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //回车键登录
            if (e.Key == Key.Enter)
            {
                Btnlogin_Click(new object(), new RoutedEventArgs());
            }
        }
    }
}
