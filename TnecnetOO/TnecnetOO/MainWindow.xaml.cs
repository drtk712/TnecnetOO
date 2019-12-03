using System;
using System.Collections.Generic;
using System.Diagnostics;
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
namespace TnecnetOO
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //存放用户信息
        List<UserList> MyFriendsList = new List<UserList>();
        //存放离线用户信息
        List<AVIMMessageEventArgs> OfflineMessageList = new List<AVIMMessageEventArgs>();
        AVRealtime Realtime;
        AVIMClient User;
        AVIMConversation FriendsConversation;
        //存放好友信息的会话id
        public MainWindow()
        {
            InitializeComponent();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //初始化登录用户信息
            usernameheader.Text = AVUser.CurrentUser.Username[0].ToString();
            username.Text = AVUser.CurrentUser.Username;
            useremail.Text = AVUser.CurrentUser.Email;
            //初始化即时通讯服务
            try
            {
                Websockets.Net.WebsocketConnection.Link();
                Realtime = new AVRealtime(new AVRealtime.Configuration
                {
                    ApplicationId = "EthsHELtLfXL9XBqcFfzMrPO-gzGzoHsz",
                    ApplicationKey = "xODJ808fAD8hpLHlblQhk0t1",
                    //可以有
                    RTMRouter = new Uri("https://ethshelt.lc-cn-n1-shared.com"),
                    OfflineMessageStrategy = AVRealtime.OfflineMessageStrategy.Default
                });
                //注册自定义消息
                Realtime.RegisterMessageType<NoticeMessage>();
                //注册离线消息监听
                Realtime.OnOfflineMessageReceived += OnOfflineMessageReceived;
                //登录Client
                User = await Realtime.CreateClientAsync(AVUser.CurrentUser);
                if (AVUser.CurrentUser.Get<string>("Friends") == "null")
                {
                    //如果是第一次登录给用户添加Friends的ConversationId
                    AVIMConversation Myfriends = await User.CreateConversationAsync(member: null, name: AVUser.CurrentUser.Username + "的小伙伴们", isUnique: false);
                    AVUser.CurrentUser["Friends"] = Myfriends.ConversationId;
                    await AVUser.CurrentUser.SaveAsync();
                }
                //获取FriendsConversation
                FriendsConversation = await User.GetConversationAsync(AVUser.CurrentUser.Get<string>("Friends"));
                //上线后给所有的好友发送“我上线了”的notice
                NoticeMessage notice = new NoticeMessage
                {
                    Notice = "我上线啦"
                };
                await FriendsConversation.SendMessageAsync(notice);
                //绑定listbox数据
                BindingFriendListSrouce();
                //添加好友事件监听（好友邀请事件：同意，拒绝，好友邀请反馈事件：同意）
                User.OnInvited += OnInvited;
                //被删除事件监听
                User.OnKicked += OnKicked;
                //好友操作事件监听（删除好友，好友邀请反馈事件：拒绝）==》TODO：未完成
                //User.OnMembersLeft += OnMembersLeft;
                //User.OnMembersJoined += OnMembersJoined;
                User.OnMessageReceived += OnMessageReceived;

            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }
        //绑定好友信息
        private async void BindingFriendListSrouce(string newfriend = null)
        {
            //完全重新加载list
            if (newfriend == null)
            {
                //如果有好友的话
                if (FriendsConversation.MemberIds != null)
                {
                    //TODO:优化成add
                    MyFriendsList.Clear();
                    //显示自己的好友
                    foreach (var online in await User.PingAsync(targetClientIds: FriendsConversation.MemberIds))
                    {
                        if (online.Item1 != AVUser.CurrentUser.ObjectId)
                        {
                            var user = await AVUser.Query.GetAsync(online.Item1);
                            //将获取到的好友信息保存到列表中,根据client判断用户是否在线
                            MyFriendsList.Add(new UserList(user, online.Item2));
                        }
                    }
                    //如果有离线信息的话,将离线的消息保存==》TODO：离线消息顺序有变化，待解决
                    if (OfflineMessageList != null)
                    {
                        App.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            OfflineMessageList.ForEach((messgae) =>
                            {
                                MyFriendsList.ForEach((Friend) =>
                                {
                                    if (Friend.Friend.ObjectId == messgae.Message.FromClientId)
                                    {
                                        //将消息加入对应好友的消息列表
                                        Friend.MessageList.Add(new Message(messgae.Message));
                                        //在主页面对应用户上做提示
                                        Friend.Visibility = Visibility.Visible;
                                    }
                                });
                            });
                            //最后清空离线消息
                            OfflineMessageList = null;
                        }));
                    }
                }
            }
            //仅add
            else
            {
                List<string> friends = new List<string>
                {
                    newfriend
                };
                foreach (var online in await User.PingAsync(targetClientIds: friends))
                {
                    if (online.Item1 != AVUser.CurrentUser.ObjectId)
                    {
                        var user = await AVUser.Query.GetAsync(online.Item1);
                        //将获取到的好友信息保存到列表中,根据client判断用户是否在线
                        MyFriendsList.Add(new UserList(user, online.Item2));
                    }
                }
            }
            //将列表中的好友信息绑定到listbox里
            friendlist.ItemsSource = null;
            friendlist.ItemsSource = MyFriendsList;
        }
        //添加好友事件监听（好友邀请事件：同意，拒绝，好友邀请反馈事件：同意）
        private void OnInvited(object sender, AVIMOnInvitedEventArgs e)
        {
            //排除自身引发的事件
            if (e.InvitedBy != AVUser.CurrentUser.ObjectId)
            {
                App.Current.Dispatcher.Invoke((Action)(async () =>
                {
                    AVIMConversation InvitedConversation = await User.GetConversationAsync(e.ConversationId);
                    AVUser InvitedUser = await AVUser.Query.GetAsync(e.InvitedBy);
                    //好友邀请事件
                    if (InvitedConversation.IsUnique)
                    {
                        //同意
                        if (MessageBox.Show(InvitedUser.Username + "请求添加您为好友，是否通过？", "添加好友请求", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            //将对方加入自己的FriendsConversation
                            await User.InviteAsync(FriendsConversation, e.InvitedBy);
                            //重新获取FriendsConversation==》不必要
                            FriendsConversation = await User.GetConversationAsync(AVUser.CurrentUser.Get<string>("Friends"));
                            //重新绑定listbox数据
                            BindingFriendListSrouce(e.InvitedBy);
                        }
                        //拒绝
                        else
                        {
                            //拒绝即为删除邀请者创建的Conversation
                            try
                            {
                                List<string> members = new List<string>();
                                members.Add(AVUser.CurrentUser.ObjectId);
                                members.Add(InvitedUser.ObjectId);
                                await (await AVObject.GetQuery("_Conversation").WhereEqualTo("m", members).FirstAsync()).DeleteAsync();
                            }
                            catch
                            {
                                try
                                {
                                    List<string> members = new List<string>();
                                    members.Add(InvitedUser.ObjectId);
                                    members.Add(AVUser.CurrentUser.ObjectId);
                                    await (await AVObject.GetQuery("_Conversation").WhereEqualTo("m", members).FirstAsync()).DeleteAsync();
                                }
                                catch
                                {
                                    MessageBox.Show("给舔狗一条活路吧");
                                }
                            }
                            finally
                            {
                                new Tip("提醒", "已拒绝 " + InvitedUser.Username + "的好友邀请", 3);
                            }
                        }
                    }
                    //好友邀请反馈事件：同意
                    if (!InvitedConversation.IsUnique)
                    {
                        //将对方加入自己的FriendsConversation
                        if (!FriendsConversation.MemberIds.Contains(e.InvitedBy))
                        {
                            new Tip("提醒", "已成功添加 " + InvitedUser.Username);
                            await User.InviteAsync(FriendsConversation, e.InvitedBy);
                            //重新获取FriendsConversation==》不必要
                            FriendsConversation = await User.GetConversationAsync(AVUser.CurrentUser.Get<string>("Friends"));
                            //重新绑定listbox数据
                            BindingFriendListSrouce(e.InvitedBy);
                        }
                    }
                }));
            }
        }
        //好友操作事件监听（好友邀请反馈事件：拒绝）
        private void OnMembersLeft(object sender, AVIMOnMembersLeftEventArgs e)
        {
            //排除自身引发的事件
            if (e.LeftMembers.First() != AVUser.CurrentUser.ObjectId)
            {
                App.Current.Dispatcher.Invoke((Action)(async () =>
                {
                    AVIMConversation LeftConversation = await User.GetConversationAsync(e.ConversationId);
                    AVUser LeftUser = await AVUser.Query.GetAsync(e.LeftMembers.First());
                    //好友邀请反馈事件：拒绝
                    if (LeftConversation.IsUnique)
                    {
                        new Tip("提醒", LeftUser.Username + "拒绝了您的好友请求");
                    }
                }));
            }

        }
        ////被删除事件监听
        private void OnKicked(object sender, AVIMOnKickedEventArgs e)
        {
            //防止死循环
            if (FriendsConversation.MemberIds.Contains(e.KickedBy))
            {
                App.Current.Dispatcher.Invoke((Action)(async () =>
                {
                    //将对方从自己的FriendsConversation中移除
                    await FriendsConversation.RemoveMembersAsync(e.KickedBy);
                    //在MyFriendsList找到这个人,将其数据删除
                    foreach (var friend in MyFriendsList)
                    {
                        if (friend.Friend.ObjectId == e.KickedBy)
                        {
                            MyFriendsList.Remove(friend);
                            //新绑定listitemsource
                            friendlist.ItemsSource = null;
                            friendlist.ItemsSource = MyFriendsList;
                            new Tip("提示", friend.Name + "已经解除与你的好友关系").Show();
                            //重新获取FriendsConversation
                            FriendsConversation = await User.GetConversationAsync(AVUser.CurrentUser.Get<string>("Friends"));
                            break;
                        }
                    }
                }));
            }
        }
        //好友上线====弃用
        private void OnMembersJoined(object sender, AVIMOnMembersJoinedEventArgs e)
        {
            if (e.JoinedMembers.ElementAt(0) != AVUser.CurrentUser.ObjectId)
            {
                App.Current.Dispatcher.Invoke((Action)(() =>
                {
                    MyFriendsList.ForEach((Friend) =>
                    {
                        //找到上线的好友信息
                        if (Friend.Friend.ObjectId == e.JoinedMembers.ElementAt(0))
                        {
                            //将其设为上线
                            Friend.Online = true;
                            //MessageBox.Show(Friend.Name + "上线了");
                            new Tip("提示", Friend.Name + "上线了", 5).Show();
                        }
                    });
                }));
            }
        }
        //点击用户，打开聊天界面===弃用
        private void Friendlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (friendlist.SelectedIndex != -1)
            {
                //根据item的index获取用户对象
                UserList friend = MyFriendsList[friendlist.SelectedIndex];
                //判断是否已经打开聊天窗口
                if (friend.IsCheat)
                {
                    new Tip("提示", "正在和" + friend.Name + "聊天", 2).Show();
                }
                else
                {
                    //创建聊天窗口
                    new Tip("提示", "开始与" + friend.Name + "聊天", 2).Show();
                    friend.Visibility = Visibility.Hidden;
                    friend.IsCheat = true;
                    Main main = new Main(friend, Realtime, User);
                    main.Show();
                    GetWindow(main).Closing += (sender2, e2) =>
                    {
                        friend.IsCheat = false;
                    };
                }
                //重置选择的item
                friendlist.SelectedItem = null;
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
                    MyFriendsList.ForEach((Friend) =>
                    {
                        if (Friend.IsCheat == false && Friend.Friend.ObjectId == e.Message.FromClientId)
                        {
                            //将消息加入对应好友的消息列表
                            Friend.MessageList.Add(new Message(e.Message));
                            //弹出tip
                            if (e.Message is AVIMTextMessage)
                            {
                                new Tip(Friend.Name, ((AVIMTextMessage)e.Message).TextContent, 3).Show();
                            }
                            else
                            {
                                new Tip(Friend.Name, ((AVIMImageMessage)e.Message).TextContent, 3).Show();

                            }
                            //在主页面对应用户上做提示
                            Friend.Visibility = Visibility.Visible;
                        }
                    });
                }));
            }
            //判断是否为NoticeMessage
            if (e.Message is NoticeMessage)
            {
                App.Current.Dispatcher.Invoke((Action)(() =>
                {
                    MyFriendsList.ForEach((Friend) =>
                    {
                        //找到上线的好友信息
                        if (Friend.Friend.ObjectId == ((NoticeMessage)e.Message).FromClientId)
                        {
                            //好友上线
                            if (((NoticeMessage)e.Message).Notice == "我上线啦")
                            {
                                //将其设为上线
                                Friend.Online = true;
                                new Tip("提示", Friend.Name + "上线了", 5).Show();
                            }
                            //好友下线
                            if (((NoticeMessage)e.Message).Notice == "我下线啦")
                            {
                                //将其设为下线
                                Friend.Online = false;
                                new Tip("提示", Friend.Name + "下线了", 5).Show();
                            }
                        }
                    });
                }));
            }
        }
        //加载离线消息
        private void OnOfflineMessageReceived(object sender, AVIMMessageEventArgs e)
        {
            //判断消息类型是否为文本消息==》TODO：对其他类型的消息进行判断
            if (e.Message is AVIMTextMessage || e.Message is AVIMImageMessage)
            {
                OfflineMessageList.Add(e);
            }
        }
        //添加好友按钮点击事件
        private void Btnaddfriend_Click(object sender, RoutedEventArgs e)
        {
            Addfriend addfriend = new Addfriend(User, FriendsConversation);
            addfriend.Show();
        }
        //点击用户，打开聊天界面
        private void ListBoxItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            UserList friend = MyFriendsList[friendlist.SelectedIndex];
            //判断是否已经打开聊天窗口
            if (friend.IsCheat)
            {
                new Tip("提示", "正在和" + friend.Name + "聊天", 2).Show();
            }
            else
            {
                //创建聊天窗口
                new Tip("提示", "开始与" + friend.Name + "聊天", 2).Show();
                friend.Visibility = Visibility.Hidden;
                friend.IsCheat = true;
                Main main = new Main(friend, Realtime, User);
                main.Show();
                GetWindow(main).Closing += (sender2, e2) =>
                {
                    friend.IsCheat = false;
                };
            }
            //重置选择的item
            friendlist.SelectedItem = null;
        }
        //右键快捷菜单“开始聊天”
        private void ContextmenuChat_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem_PreviewMouseDoubleClick(sender, e as MouseButtonEventArgs);
        }
        //右键快捷菜单“删除好友”
        private void ContextMenuDelete_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)(async () =>
            {
                UserList friend = MyFriendsList[friendlist.SelectedIndex];
                if (MessageBox.Show("确定要删除" + friend.Name + "吗", "警告", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    //移除listitem，再重新绑定listitemsource
                    MyFriendsList.RemoveAt(friendlist.SelectedIndex);
                    friendlist.ItemsSource = null;
                    friendlist.ItemsSource = MyFriendsList;
                    new Tip("提示", "已成功解除与" + friend.Name + "的好友关系", 3).Show();
                    //将对方踢出自己的FriendsConversation，对方触发onkick事件
                    await User.KickAsync(FriendsConversation, member: friend.Friend.ObjectId);
                    //再将双方单独的_Conversation删除
                    try
                    {
                        List<string> members = new List<string>();
                        members.Add(AVUser.CurrentUser.ObjectId);
                        members.Add(friend.Friend.ObjectId);
                        await (await AVObject.GetQuery("_Conversation").WhereEqualTo("m", members).FirstAsync()).DeleteAsync();
                    }
                    catch
                    {
                        try
                        {
                            List<string> members = new List<string>();
                            members.Add(friend.Friend.ObjectId);
                            members.Add(AVUser.CurrentUser.ObjectId);
                            await (await AVObject.GetQuery("_Conversation").WhereEqualTo("m", members).FirstAsync()).DeleteAsync();
                        }
                        catch
                        {
                            MessageBox.Show("你早就被人家拉黑了");
                        }
                    }
                    //重新获取FriendsConversation
                    FriendsConversation = await User.GetConversationAsync(AVUser.CurrentUser.Get<string>("Friends"));
                }
            }));
        }
        //关闭页面时触发
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //下线操作
            //给所有好友发送“我下线啦”的消息
            NoticeMessage notice = new NoticeMessage
            {
                Notice = "我下线啦"
            };
            FriendsConversation.SendMessageAsync(notice);
            //关闭Client
            User.CloseAsync();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }
        //关闭页面
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        //窗口拖拽
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //进行拖拽
            DragMove();
        }
        //窗口靠到屏幕顶部自动上滚/下滚==》TODO：暂不启用，待优化
        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Top == 0)
            {
                while (Height < 550)
                {
                    System.Threading.Thread.Sleep(3);
                    Height += 20;
                }
                Height = 550;
            }
        }
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Top == 0)
            {
                System.Threading.Thread.Sleep(500);
                while (Height > 20)
                {
                    System.Threading.Thread.Sleep(3);
                    Height -= 20;
                }
                Height = 10;
            }
        }
    }
}
