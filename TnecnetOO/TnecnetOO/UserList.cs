using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using LeanCloud;
namespace TnecnetOO
{
    public class UserList : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        //构造函数，获取用户的基本信息
        public UserList(AVUser friend, Boolean online)
        {
            Friend = friend;
            Name = friend.Username;
            Online = online;
            Header = Name[0].ToString();
            IsCheat = false;
            Visibility = Visibility.Hidden;
        }
        //用户实例
        public AVUser Friend { get; set; }
        //用户名
        public string Name { get; set; }
        //头像，目前为用户名第一个字符
        public string Header { get; set; }
        //是否在线
        private bool online;
        public bool Online
        {
            get { return online; }
            set
            {
                online = value;
                if (Online)
                {
                    Background = new SolidColorBrush(Colors.LightGreen);
                    LastOnlineTime = "快来Q我吧";
                }
                else
                {
                    Background = new SolidColorBrush(Colors.LightGray);
                    LastOnlineTime = "离线时间：" + Friend.UpdatedAt.ToString();
                }
                Notify("Online");
                Notify("Background");
                Notify("LastOnlineTime");
            }
        }
        //在线：背景头像为绿色，否则为灰色
        public Brush Background { get; set; }
        //上次上线时间，如果在线就显示：来Q我吧
        public string LastOnlineTime { get; set; }
        //是否在与其聊天
        public bool IsCheat { get; set; }
        //存放该用户的聊天消息
        public List<Message> MessageList = new List<Message>();
        //提示红点是否可见
        private Visibility visibility;
        public Visibility Visibility
        {
            get { return visibility; }
            set
            {
                visibility = value;
                Notify("Visibility");
            }
        }

    }
}
