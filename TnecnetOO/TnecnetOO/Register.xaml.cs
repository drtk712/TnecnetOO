using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
namespace TnecnetOO
{
    /// <summary>
    /// Register.xaml 的交互逻辑
    /// </summary>
    public partial class Register : Window
    {
        string loginemail = "";
        string loginpassword = "";

        //ok图片
        BitmapImage OkImg = new BitmapImage(new Uri("Icons/icons8-ok.png", UriKind.Relative));
        //wrong图片
        BitmapImage WrongImg = new BitmapImage(new Uri("Icons/icons8-wrong.png", UriKind.Relative));
        bool UsernameHasWarning = true;
        bool PasswordHasWarning = true;
        bool EmailHasWarning = true;
        bool PhoneHasWarning = true;
        public Register()
        {
            InitializeComponent();
        }
        public Register(string username, string password) : this()
        {
            loginemail = username;
            loginpassword = password;


        }
        //注册按钮
        private async void Btnregister_Click(object sender, RoutedEventArgs e)
        {
            if (UsernameHasWarning)
            {
                Username_LostFocus(new object(), new RoutedEventArgs());
            }
            else if (PasswordHasWarning)
            {
                Password_LostFocus(new object(), new RoutedEventArgs());
            }
            else if (EmailHasWarning)
            {
                Email_LostFocus(new object(), new RoutedEventArgs());
            }
            else if (PhoneHasWarning)
            {
                Phone_LostFocus(new object(), new RoutedEventArgs());
            }
            else
            {
                var user = new AVUser
                {
                    Username = username.Text,
                    Password = password.Text,
                    Email = email.Text,
                    MobilePhoneNumber = phone.Text
                };
                try
                {
                    await user.SignUpAsync();
                    App.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        MessageBox.Show("注册成功");
                    }));
                    Login login = new Login(email.Text, password.Text);
                    login.Show();
                    this.Close();
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message);
                }
            }
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            email.Text = loginemail;
            password.Text = loginpassword;
            if (loginemail != "")
            {
                Email_LostFocus(new object(), new RoutedEventArgs());
            }
            if (loginpassword != "")
            {
                Password_LostFocus(new object(), new RoutedEventArgs());
            }
        }
        //关闭界面
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        //用户名输入验证
        private async void Username_LostFocus(object sender, RoutedEventArgs e)
        {
            bool IsSuccess = false;
            string WaringMessage = "";
            //需要输入用户名
            if (username.Text.Length != 0)
            {
                //用户名不能少于2个字符
                if (username.Text.Length >= 2)
                {
                    //用户名只能是数字、英文字母或汉字
                    Regex rgx = new Regex(@"^[a-zA-Z0-9\u4e00-\u9fa5]+$");
                    if (rgx.IsMatch(username.Text))
                    {
                        //用户名应为唯一的
                        try
                        {
                            await AVUser.Query.WhereEqualTo("username", username.Text).FirstAsync();
                            //用户名已经被注册
                            WaringMessage = "该用户名已被注册";
                        }
                        catch (AVException error)
                        {
                            //注册成功
                            if (error.Code == AVException.ErrorCode.ObjectNotFound)
                            {
                                IsSuccess = true;
                            }
                        }
                    }
                    else
                    {
                        WaringMessage = "请输入数字、英文字母或汉字";
                    }
                }
                else
                {
                    WaringMessage = "用户名不能少于2个字符";
                }
            }
            else
            {
                WaringMessage = "请输入用户名";
            }
            //根据上方判断显示结果
            if (IsSuccess)
            {
                usernameimg.Source = OkImg;
                usernamewarning.Visibility = Visibility.Collapsed;
                UsernameHasWarning = false;
            }
            else
            {
                usernameimg.Source = WrongImg;
                usernamewarning.Visibility = Visibility.Visible;
                usernamewarning.Text = WaringMessage;
                UsernameHasWarning = true;
            }
        }
        //密码输入验证
        private void Password_LostFocus(object sender, RoutedEventArgs e)
        {
            bool IsSuccess = false;
            string WaringMessage = "";
            //需要输入密码
            if (password.Text.Length != 0)
            {
                //密码不能少于6个字符
                if (password.Text.Length >= 6)
                {
                    //密码只能是数字、英文字母
                    Regex rgx = new Regex(@"^[a-zA-Z0-9]+$");
                    if (rgx.IsMatch(password.Text))
                    {
                        IsSuccess = true;
                    }
                    else
                    {
                        WaringMessage = "请输入数字、英文字母";
                    }
                }
                else
                {
                    WaringMessage = "密码不能少于6个字符";
                }
            }
            else
            {
                WaringMessage = "请输入密码";
            }
            //根据上方判断显示结果
            if (IsSuccess)
            {
                passwordimg.Source = OkImg;
                passwordwarning.Visibility = Visibility.Collapsed;
                PasswordHasWarning = false;
            }
            else
            {
                passwordimg.Source = WrongImg;
                passwordwarning.Visibility = Visibility.Visible;
                passwordwarning.Text = WaringMessage;
                PasswordHasWarning = true;
            }
        }
        //邮箱输入验证
        private async void Email_LostFocus(object sender, RoutedEventArgs e)
        {
            bool IsSuccess = false;
            string WaringMessage = "";
            //需要输入邮箱
            if (email.Text.Length != 0)
            {
                //邮箱的正则验证
                Regex rgx = new Regex(@"^\w+@\w{2,5}.com");
                if (rgx.IsMatch(email.Text))
                {
                    //邮箱应为唯一的
                    try
                    {
                        await AVUser.Query.WhereEqualTo("email", email.Text).FirstAsync();
                        //用户名已经被注册
                        WaringMessage = "该邮箱已被注册";
                    }
                    catch (AVException error)
                    {
                        //注册成功
                        if (error.Code == AVException.ErrorCode.ObjectNotFound)
                        {
                            IsSuccess = true;
                        }
                    }
                }
                else
                {
                    WaringMessage = "请输入正确的邮箱";
                }
            }
            else
            {
                WaringMessage = "请输入邮箱";
            }
            //根据上方判断显示结果
            if (IsSuccess)
            {
                emailimg.Source = OkImg;
                emailwarning.Visibility = Visibility.Collapsed;
                EmailHasWarning = false;
            }
            else
            {
                emailimg.Source = WrongImg;
                emailwarning.Visibility = Visibility.Visible;
                emailwarning.Text = WaringMessage;
                EmailHasWarning = true;
            }
        }
        //手机号输入验证
        private async void Phone_LostFocus(object sender, RoutedEventArgs e)
        {
            bool IsSuccess = false;
            string WaringMessage = "";
            //需要输入手机号
            if (phone.Text.Length != 0)
            {
                //手机号的正则验证
                Regex rgx = new Regex(@"^(13[0-9]|14[5|7]|15[0|1|2|3|5|6|7|8|9]|17[0|1|2|3|5|6|7|8|9|18[0|1|2|3|5|6|7|8|9])\d{8}$");
                if (rgx.IsMatch(phone.Text))
                {
                    //手机号应为唯一的
                    try
                    {
                        await AVUser.Query.WhereEqualTo("mobilePhoneNumber", phone.Text).FirstAsync();
                        //手机号已经被注册
                        WaringMessage = "该手机号已被注册";
                    }
                    catch (AVException error)
                    {
                        //注册成功
                        if (error.Code == AVException.ErrorCode.ObjectNotFound)
                        {
                            IsSuccess = true;
                        }
                    }
                }
                else
                {
                    WaringMessage = "请输入正确的手机号";
                }
            }
            else
            {
                WaringMessage = "请输入手机号";
            }
            //根据上方判断显示结果
            if (IsSuccess)
            {
                phoneimg.Source = OkImg;
                phonewarning.Visibility = Visibility.Collapsed;
                PhoneHasWarning = false;
            }
            else
            {
                phoneimg.Source = WrongImg;
                phonewarning.Visibility = Visibility.Visible;
                phonewarning.Text = WaringMessage;
                PhoneHasWarning = true;
            }
        }
        //拖拽窗口
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        //按键快捷键
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //回车键
            if (e.Key == Key.Enter)
            {
                Btnregister_Click(new object(), new RoutedEventArgs());
            }
        }
    }
}
