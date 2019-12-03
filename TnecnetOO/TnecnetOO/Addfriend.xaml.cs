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
using System.Windows.Shapes;
using LeanCloud;
using LeanCloud.Realtime;

namespace TnecnetOO
{
    /// <summary>
    /// Addfriend.xaml 的交互逻辑
    /// </summary>
    public partial class Addfriend : Window
    {
        AVIMClient User;
        AVIMConversation Conversation;

        bool IsSuccess = false;
        public Addfriend()
        {
            InitializeComponent();
        }
        public Addfriend(AVIMClient user, AVIMConversation conversation) : this()
        {
            User = user;
            Conversation = conversation;
        }
        private async void Btnadd_Click(object sender, RoutedEventArgs e)
        {
            if (IsSuccess)
            {
                try
                {
                    AVUser friend = await AVUser.Query.WhereEqualTo("username", friendname.Text).FirstAsync();
                    //先检查对方是否已经为好友
                    if (Conversation.MemberIds.Contains(friend.ObjectId))
                    {
                        MessageBox.Show("你们已经是好友啦");
                    }

                    else
                    {
                        //不是好友再检测对方是否在线
                        List<string> friends = new List<string>
                        {
                            friend.ObjectId
                        };
                        if ((await User.PingAsync(targetClientIds: friends)).First().Item2)
                        {
                            //在线
                            await User.CreateConversationAsync(friend.ObjectId, isUnique: true);
                            MessageBox.Show("添加成功,等待对方确认");
                            Close();
                        }
                        else
                        {
                            //不在线
                            MessageBox.Show("对方不在线，请稍后重试");
                        }
                    }
                }
                catch (AVException error)
                {
                    MessageBox.Show(error.Message);
                }
            }
            else
            {
                Friendname_LostFocus(new object(), new RoutedEventArgs());
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void Friendname_LostFocus(object sender, RoutedEventArgs e)
        {
            IsSuccess = false;
            string WaringMessage = "";
            //需要输入好友的姓名
            if (friendname.Text.Length != 0)
            {
                //好友的姓名不能少于2个字符
                if (friendname.Text.Length >= 2)
                {
                    //好友的姓名只能是数字、英文字母或汉字
                    Regex rgx = new Regex(@"^[a-zA-Z0-9\u4e00-\u9fa5]+$");
                    if (rgx.IsMatch(friendname.Text))
                    {
                        //是否能查询到好友的姓名
                        try
                        {
                            await AVUser.Query.WhereEqualTo("username", friendname.Text).FirstAsync();
                            //查询到
                            IsSuccess = true;
                        }
                        catch (AVException error)
                        {
                            //没查询到
                            if (error.Code == AVException.ErrorCode.ObjectNotFound)
                            {
                                WaringMessage = "查无此人";
                            }
                            else
                            {
                                //其他异常
                                WaringMessage = error.Message;
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
                WaringMessage = "请输入要查询的姓名";
            }
            //根据上方判断显示结果
            if (IsSuccess)
            {
                friendnameimg.Source = new BitmapImage(new Uri("Icons/icons8-ok.png", UriKind.Relative));
                friendnamewarning.Visibility = Visibility.Collapsed;
            }
            else
            {
                friendnameimg.Source = new BitmapImage(new Uri("Icons/icons8-wrong.png", UriKind.Relative));
                friendnamewarning.Visibility = Visibility.Visible;
                friendnamewarning.Text = WaringMessage;
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
            //回车键q确定
            if (e.Key == Key.Enter)
            {
                Btnadd_Click(new object(), new RoutedEventArgs());
            }
        }
    }
}

