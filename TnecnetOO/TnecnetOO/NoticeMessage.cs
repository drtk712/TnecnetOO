using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeanCloud.Realtime;
namespace TnecnetOO
{
    [AVIMMessageClassName("NoticeMessage")]
    [AVIMTypedMessageTypeInt(10)]
    class NoticeMessage : AVIMTypedMessage
    {
        public NoticeMessage() { }
        //通过通知的字符串的表示通知的信息==》TODO：修改成枚举类型
        [AVIMMessageFieldName("Notice")]
        public string Notice { get; set; }
    }
}
