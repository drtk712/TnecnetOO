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
using LeanCloud.Realtime;
using Microsoft.Win32;

namespace TnecnetOO
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window
    {
        //存放消息信息的列表
        AVRealtime Realtime;
        AVIMClient User;
        AVIMConversation Conversation;
        UserList InviteFriend;
        public Main()
        {
            InitializeComponent();
        }
        public Main(UserList invitefriend, AVRealtime realtime, AVIMClient user) : this()
        {
            InviteFriend = invitefriend;
            Realtime = realtime;
            User = user;
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Title = InviteFriend.Name;
            friendname.Text = InviteFriend.Name;
            if (InviteFriend.Online)
            {
                friendname.Foreground = new SolidColorBrush(Colors.LimeGreen);
            }
            else
            {
                friendname.Foreground = new SolidColorBrush(Colors.DarkGray);
            }
            try
            {
                //邀请别人的话就创建新的会话==》TODO：不创建新的会话，并且可以查看聊天记录
                Conversation = await User.CreateConversationAsync(InviteFriend.Friend.ObjectId, name: "conmunicate", isUnique: true);

                //监听消息
                User.OnMessageReceived += OnMessageReceived;
                //绑定消息
                received.ItemsSource = InviteFriend.MessageList;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }
        //用户接收消息
        private void OnMessageReceived(object sender, AVIMMessageEventArgs e)
        {
            //判断消息类型是否为文本消息==》TODO：对其他类型的消息进行判断
            if (e.Message is AVIMTextMessage || e.Message is AVIMImageMessage)
            {
                App.Current.Dispatcher.Invoke((Action)(() =>
                {
                    InviteFriend.MessageList.Add(new Message(e.Message));
                    //TODO:
                    received.ItemsSource = null;
                    received.ItemsSource = InviteFriend.MessageList;
                    //聚焦最新的消息
                    received.ScrollIntoView(received.Items.GetItemAt(InviteFriend.MessageList.Count - 1));
                }));
            }
        }
        //用户发送消息,==》TODO：用户发送其他类型的消息
        private async void Btnsend_Click(object sender, RoutedEventArgs e)
        {
            Image a = new Image();
            if (send.Text != "")
            {
                try
                {
                    AVIMTextMessage textMessage = new AVIMTextMessage();
                    textMessage.TextContent = send.Text;
                    await Conversation.SendMessageAsync(textMessage);
                    InviteFriend.MessageList.Add(new Message(textMessage));
                    //TODO:
                    received.ItemsSource = null;
                    received.ItemsSource = InviteFriend.MessageList;
                    //聚焦最新的消息
                    received.ScrollIntoView(received.Items.GetItemAt(InviteFriend.MessageList.Count - 1));
                    send.Text = "";
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
            }
        }
        //回车键发送消息
        private void Send_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Btnsend_Click(sender, new RoutedEventArgs());
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //取消注册的监听
            User.OnMessageReceived -= OnMessageReceived;
        }
        //选择图片，发送图片
        private async void Btnsendimage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "ALL Image Files|*.*";
            if ((bool)openFile.ShowDialog())
            {
                var image = new AVFile(openFile.SafeFileName, openFile.OpenFile());
                await image.SaveAsync();

                var imageMessage = new AVIMImageMessage
                {
                    File = image,
                    TextContent = "[图片]"
                };
                await Conversation.SendMessageAsync(imageMessage);
                InviteFriend.MessageList.Add(new Message(imageMessage));
                //TODO:
                received.ItemsSource = null;
                received.ItemsSource = InviteFriend.MessageList;
                //聚焦最新的消息
                received.ScrollIntoView(received.Items.GetItemAt(InviteFriend.MessageList.Count - 1));
            }
            openFile.OpenFile().Close();


        }
        //上拉到顶部刷新出历史聊天记录
        private async void Received_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer viewer = e.OriginalSource as ScrollViewer;
            if (viewer != null)
            {
                if (viewer.VerticalOffset == 0 && Conversation != null)
                {
                    //加载历史记录
                    var earliestMessages = await Conversation.QueryMessageAsync(limit: 20);
                    List<Message> earliestMessageList = new List<Message>();
                    foreach (AVIMMessage message in earliestMessages)
                    {
                        earliestMessageList.Add(new Message(message));
                    }
                    InviteFriend.MessageList.InsertRange(0, earliestMessageList);
                    received.ItemsSource = null;
                    received.ItemsSource = InviteFriend.MessageList;
                    //聚焦最新的消息
                    received.ScrollIntoView(received.Items.GetItemAt(InviteFriend.MessageList.Count - 1));
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
