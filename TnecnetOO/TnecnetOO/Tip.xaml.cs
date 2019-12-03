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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TnecnetOO
{
    /// <summary>
    /// Tip.xaml 的交互逻辑
    /// </summary>
    public partial class Tip : Window
    {
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public Tip()
        {
            InitializeComponent();
            double ScreenWidth = SystemParameters.PrimaryScreenWidth;
            double ScreenHeight = SystemParameters.PrimaryScreenHeight;
            Top = (int)(ScreenHeight - Height) - 40;
            Left = (int)(ScreenWidth - Width);
        }
        //不消失的tip
        public Tip(string tiptitle, string tipmessage) : this()
        {
            //设置tip的标题
            this.tiptitle.Text = tiptitle;
            //设置tip1的内容，如果太长了就截取前15个字符
            if (tipmessage.Length >= 20)
            {
                tip.Text = tipmessage.Substring(0, 15) + "...";
            }
            else
            {
                tip.Text = tipmessage;
            }
        }
        //定时消失的tip
        public Tip(string tiptitle, string tipmessage, int duration) : this()
        {
            //设置tip的标题
            this.tiptitle.Text = tiptitle;
            //设置tip1的内容，如果太长了就截取前15个字符
            if (tipmessage.Length >= 20)
            {
                tip.Text = tipmessage.Substring(0, 15) + "...";
            }
            else
            {
                tip.Text = tipmessage;
            }
            //设置tip存在时间
            dispatcherTimer.Tick += new EventHandler(CloseTip);
            dispatcherTimer.Interval = new TimeSpan(0, 0, duration);
            dispatcherTimer.Start();
        }
        //关闭tip
        private void CloseTip(object sender, EventArgs e)
        {
            //IsEnabled = false;
            grid.OpacityMask = this.Resources["ClosedBrush"] as LinearGradientBrush;
            Storyboard std = this.Resources["ClosedStoryboard"] as Storyboard;
            std.Completed += delegate { Close(); };

            std.Begin();
        }
        //点击tip即可关闭tip
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        //鼠标移到tip上，tip不再计时消失
        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (dispatcherTimer.IsEnabled)
            {
                dispatcherTimer.Stop();
            }
        }
        //鼠标移出tip，tip开始计时消失
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            dispatcherTimer.Start();
        }
    }
}
