using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Configuration;
using Microsoft.Win32;

namespace memoryClear
{


    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("psapi.dll")]

        static extern int EmptyWorkingSet(IntPtr hwProc);

        private DateTime dt = DateTime.Now;

        private DateTime dt1;

        private string info = "";

        private int cishu = 0;

        Dictionary<string, float> infos;
        private void button1_Click(object sender, EventArgs e)
        {
            this.info = "在" + DateTime.Now + "清理了一次内存\r\n清理前剩余内存" + label3.Text;
            GC.Collect();
            this.label14.Text = "已经清理了" + ++cishu + "次";
            GC.WaitForPendingFinalizers();
            Process[] processes = Process.GetProcesses();
            dt = DateTime.Now;
            foreach (Process process in processes)
            {
                //以下系统进程没有权限，所以跳过，防止出错影响效率。  
                if ((process.ProcessName == "System") && (process.ProcessName == "Idle"))
                    continue;
                try
                {
                    EmptyWorkingSet(process.Handle);

                }
                catch
                {

                }
            }
            this.label7.Text = this.info;
        }

        private float shengyuneichun = 0;

        private void timer1_Tick(object sender, EventArgs e)
        {

            infos = Systeminfo();
            shengyuneichun = (infos["ROMY"] / 1024 / 1024);
            var zhongneichun = (infos["ROMN"] / 1024 / 1024);
            //显示内存
            this.label3.Text = shengyuneichun + "MB";
            this.label4.Text = zhongneichun + "MB";
            this.label13.Text = zhongneichun - shengyuneichun + "MB";
            float dangqian = ((zhongneichun - shengyuneichun) / zhongneichun * 100);
            string bianhua = dangqian.ToString("F2");
            string[] quzhi = bianhua.Split(new char[] { '.' });
            this.label8.Text = quzhi[0];
            this.label10.Text = "." + quzhi[1] + "%";
            //计算出进度条使用内存
            progressBar1.Value = int.Parse(quzhi[0]);
            剩余ToolStripMenuItem.Text = "剩余" + shengyuneichun.ToString("F2") + "MB";
            toolStripMenuItem3.Text = "占用" + bianhua + "%";
            //判断使用量更改颜色
            if (dangqian < 50)
            {
                this.label8.ForeColor = Color.Green;
                this.label10.ForeColor = Color.Green;
            }
            else if (dangqian >= 50 && dangqian <= 80)
            {
                this.label8.ForeColor = Color.Orange;
                this.label10.ForeColor = Color.Orange;
            }
            else
            {
                this.label8.ForeColor = Color.Red;
                this.label10.ForeColor = Color.Red;
            }

            label6.Text = dt1.ToString("t");
            dt1 = dt.AddMinutes(trackBar1.Value);
            //倒计时
            if (DateTime.Now.Hour == dt1.Hour && DateTime.Now.Minute == dt1.Minute && DateTime.Now.Second == dt1.Second)
            {
                dt = DateTime.Now;
                现在清理ToolStripMenuItem_Click(sender, e);
            }
            if (this.panel1.Visible)
            {
                timer2.Enabled = true;
            }

        }

        public static Dictionary<string, float> Systeminfo()
        {
            SystemInfo sys = new SystemInfo();
            Dictionary<string, float> info = new Dictionary<string, float>();
            info.Add("ROMY", sys.MemoryAvailable);
            info.Add("ROMN", sys.PhysicalMemory);
            return info;
        }

        private void 现在清理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
            notifyIcon1.BalloonTipTitle = "刚刚清理了内存";
            notifyIcon1.BalloonTipText = "时间  " + DateTime.Now.ToString("HH:mm:ss") + "\r\n剩余" + shengyuneichun + "MB";
            notifyIcon1.ShowBalloonTip(1000);
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            MouseEventArgs Mouse_e = (MouseEventArgs)e;
            //点鼠标右键,return   
            if (Mouse_e.Button != MouseButtons.Right)
            {
                //如果窗体是可见的，那么鼠标左击托盘区图标后，窗体为不可见  
                if (this.Visible == true)
                {
                    this.Visible = false;
                }
                else
                {
                    this.Visible = true;
                    this.Activate();
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //屏蔽错误
            try
            {
                e.Cancel = true;
            }
            catch (Exception)
            {

            }
            this.Hide();
            notifyIcon1.BalloonTipTitle = "内存管家";
            notifyIcon1.BalloonTipText = "持续为你监控内存";
            notifyIcon1.Icon = new System.Drawing.Icon("C:\\Windows\\System32\\PerfCenterCpl.ico");
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(1000);
        }

        private void 是ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                Form1_FormClosing(null, null);
            }
            this.panel1.Top += 5;
            if (panel1.Top >= 290)
            {
                timer2.Enabled = false;
                this.panel1.Visible = false;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label5.Text = trackBar1.Value + "分钟清理一次内存,下一次清理时间为";
            //添加时间
            dt1 = dt.AddMinutes(trackBar1.Value);
            label6.Text = dt1.ToString("t");
            //把设置的时间加进去
            Configuration config = ConfigurationManager.OpenExeConfiguration("memoryClear.exe");
            AppSettingsSection app = config.AppSettings;
            app.Settings["trackBar1"].Value = trackBar1.Value.ToString();
            config.Save(ConfigurationSaveMode.Modified);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.Icon = new System.Drawing.Icon("C:\\Windows\\System32\\PerfCenterCpl.ico");
            notifyIcon1.Visible = true;

            if (ConfigurationManager.AppSettings["trackBar1"] == null || ConfigurationManager.AppSettings["open"] == null || ConfigurationManager.AppSettings["openVisible"] == null)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration("memoryClear.exe");
                AppSettingsSection app = config.AppSettings;
                //先全部删除然后添加
                app.Settings.Remove("trackBar1");
                app.Settings.Remove("open");
                app.Settings.Remove("openVisible");
                //进度条长短，开机自启，开启后自动最小化
                app.Settings.Add("trackBar1", "10");
                app.Settings.Add("open", "false");
                app.Settings.Add("openVisible", "false");
                config.Save(ConfigurationSaveMode.Modified);
            }
            else
            {
                trackBar1.Value = int.Parse(ConfigurationManager.AppSettings["trackBar1"]);
                label5.Text = ConfigurationManager.AppSettings["trackBar1"] + "分钟清理一次内存,下一次清理时间为";
                this.checkBox1.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["open"]);
                this.checkBox2.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["openVisible"]);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.panel2.Visible == true)
            {
                this.button2.Text = "设置";
                this.panel2.Visible = false;
            }
            else
            {
                this.button2.Text = "确认";
                this.panel2.Visible = true;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration("memoryClear.exe");
            AppSettingsSection app = config.AppSettings;
            if (checkBox1.Checked)
            {
                //此方法把启动项加载到注册表中
                //获得应用程序路径
                string strAssName = Application.StartupPath + @"\" + Application.ProductName + @".exe";
                //获得应用程序名
                string ShortFileName = Application.ProductName;
                RegistryKey rgkRun = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (rgkRun == null)
                {
                    rgkRun = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                }
                rgkRun.SetValue(ShortFileName, strAssName);
            }
            else
            {
                string ShortFileName = Application.ProductName;
                RegistryKey rgkRun = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (rgkRun == null)
                {
                    rgkRun = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                }
                rgkRun.DeleteValue(ShortFileName, false);
            }
            app.Settings["open"].Value = this.checkBox1.Checked.ToString();
            config.Save(ConfigurationSaveMode.Modified);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration("memoryClear.exe");
            AppSettingsSection app = config.AppSettings;
            app.Settings["openVisible"].Value = this.checkBox2.Checked.ToString();
            config.Save(ConfigurationSaveMode.Modified);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }
    }
}
