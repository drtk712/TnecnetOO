using LeanCloud;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TnecnetOO
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //初始化云服务器
            AVClient.Initialize("EthsHELtLfXL9XBqcFfzMrPO-gzGzoHsz", "xODJ808fAD8hpLHlblQhk0t1", "https://ethshelt.lc-cn-n1-shared.com");

            Login login = new Login();
            login.Show();
        }
    }
}

