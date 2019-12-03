using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LeanCloud;
using LeanCloud.Realtime;
namespace TnecnetOO
{
    public class Message
    {
        public Message(IAVIMMessage textmessage)
        {
            if (textmessage is AVIMTextMessage)
            {
                Text = ((AVIMTextMessage)textmessage).TextContent;
                Image = null;
            }
            else
            {
                Text = ((AVIMImageMessage)textmessage).TextContent;
                //Image=new BitmapImage(((AVIMImageMessage)textmessage).File.Url, new System.Net.Cache.RequestCachePolicy());
                Image = new BitmapImage(((AVIMImageMessage)textmessage).File.Url);

            }

            if (textmessage.FromClientId != AVUser.CurrentUser.ObjectId)
            {
                Alignment = HorizontalAlignment.Left;
                Background = new SolidColorBrush(Colors.LightGray);
            }
            else
            {
                Alignment = HorizontalAlignment.Right;
                Background = new SolidColorBrush(Colors.LightGreen);
            }

        }
        //消息里的文本信息
        public String Text { get; set; }
        //消息的水平对其方式
        public HorizontalAlignment Alignment { get; set; }
        //消息的背景颜色
        public Brush Background { get; set; }
        //图片消息
        public ImageSource Image { get; set; }
    }
}
