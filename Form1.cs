using Microsoft.Win32.TaskScheduler;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using usbipdtool.Properties;
using System.Management;
namespace usbipd
{
    public partial class Form1 : Form
    {
        public static Form1? Instance { get; private set; }
        string BUSID = "";
        string line ="";
        //bool EnableDeviceMonitor = true;
        List<string> busIds = new List<string>();
        private DateTime _lastDeviceEventTime = DateTime.MinValue;
        private readonly int _coolDownMs = 200;
        private ProcessStartInfo _usbipStartInfo;
        private ManagementEventWatcher? _usbDeviceWatcher;
        bool Process_check = false, Extern_SW = true;
        //config路径获取声明
        private readonly string _configPath = Path.Combine(Application.StartupPath, "config.xml");

        public Form1()
        {
            bool isNotRunning;  //互斥体判断
            System.Threading.Mutex instance = new System.Threading.Mutex(true, "MutexName", out isNotRunning);   //同步基元变量
            if (!isNotRunning)  // 如果不是未运行状态
            {
                Application.Exit();  // 退出应用程序
            }
            InitializeComponent();
            Instance = this; 
            //Console.OutputEncoding = Encoding.UTF8;
            //Console.InputEncoding = Encoding.UTF8;
            if (!CheckUsbIpdInstalled())
            {
                MessageBox.Show("未检测到usbipd-win工具，USB绑定/共享功能将无法使用。\n可使用PowerShell执行命令安装：winget install dorssel.usbipd-win");
                Application.Exit();
            }

            //创建配置文件有则不创建
            FileStream fs = new FileStream("config.txt", FileMode.OpenOrCreate, FileAccess.Write);
            //解除FileStream对象对文件的占用
            fs.Close();

            //判断自启状态
            if (GetTask("USBipd_TOOL"))
                自启动ToolStripMenuItem.Checked = true;
            else
                自启动ToolStripMenuItem.Checked = false;

            //激活usbipd
            ProcessStartInfo _usbipStart = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C net start usbipd",
                UseShellExecute = true,
                CreateNoWindow = true,
            };
            Process.Start(_usbipStart);

            _usbipStartInfo = new ProcessStartInfo
            {
                FileName = "usbipd.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            RUN_USBIPD();
            //this.Close();
            InitWmiDeviceMonitor();
        }
        #region WMI 热插拔监听
        private void InitWmiDeviceMonitor()
        {
            string query = "SELECT * FROM Win32_DeviceChangeEvent WHERE EventType=2 OR EventType=3";
            _usbDeviceWatcher = new ManagementEventWatcher(query);
            _usbDeviceWatcher.EventArrived += OnDeviceChanged;
            _usbDeviceWatcher.Start();
            //Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] USB设备WMI监听已启动");
        }
        private static void OnDeviceChanged(object sender, EventArrivedEventArgs e)
        {
                int eventType = Convert.ToInt32(e.NewEvent["EventType"]);
                DateTime now = DateTime.Now;
                // 冷却防抖：200ms内重复事件直接丢弃
                if ((now - Instance!._lastDeviceEventTime).TotalMilliseconds < Instance._coolDownMs)
                    return;
                Instance._lastDeviceEventTime = now;

            switch (eventType)
            {
                case 2:
                    //Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 检测USB设备插入，执行绑定逻辑");
                    Instance?.RUN_USBIPD();
                    break;
                case 3:
                    //Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] USB设备移除");
                    break;
            }
        }
        #endregion
        protected override void SetVisibleCore(bool value)
        {
            // 永远强制窗口不可见，无视外部Show()调用
            base.SetVisibleCore(false);
        }
        private bool CheckUsbIpdInstalled()
        {
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = "where";
                p.StartInfo.Arguments = "usbipd";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                return !string.IsNullOrWhiteSpace(output);
            }
            catch
            {
                return false;
            }
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        //const int WS_EX_APPWINDOW = 0x40000;
        //        //const int WS_EX_TOOLWINDOW = 0x80;
        //        //CreateParams cp = base.CreateParams;
        //        //// 清除任务栏显示标记
        //        //cp.ExStyle &= (~WS_EX_APPWINDOW);
        //        //// 设置工具窗口样式，Alt+Tab隐藏
        //        //cp.ExStyle |= WS_EX_TOOLWINDOW;
        //        //return cp;
        //        //CreateParams cp = base.CreateParams;
        //        //// 移除任务栏图标标记
        //        //const int WS_EX_APPWINDOW = 0x4000;
        //        //cp.ExStyle &= ~WS_EX_APPWINDOW;
        //        //return cp;
        //        const int WS_EX_APPWINDOW = 0x4000;
        //        const int WS_EX_NOACTIVATE = 0x08000000;
        //        CreateParams cp = base.CreateParams;
        //        // 移除任务栏图标标记
        //        cp.ExStyle &= ~WS_EX_APPWINDOW;
        //        // WS_EX_NOACTIVATE 辅助隐藏Alt+Tab，不修改标题栏样式
        //        cp.ExStyle |= WS_EX_NOACTIVATE;
        //        return cp;
        //    }
        //}
        private async void RUN_USBIPD()
        {
            Process_check = true;
            busIds.Clear();
            BUSID = Usbipd_Cmd("list");
            Console.WriteLine(BUSID);
            //MatchCollection USBID = Regex.Matches(BUSID, @"^([\d-]+)\s+\s+.*\s+(USB Serial Converter|USB.*\(COM)", RegexOptions.Multiline);
            //MatchCollection USBID = Regex.Matches(BUSID, @"^([\d-]+)\s+\s+.*\s+USB.*\(COM", RegexOptions.Multiline);
            StreamReader sr = new StreamReader("config.txt");
            line = sr.ReadToEnd();
            FileInfo fi = new FileInfo("config.txt");
            //Console.WriteLine("实际读取路径：" + fi.FullName);

            using StreamReader aw = new StreamReader(fi.FullName);
            line = aw.ReadToEnd();
            if (line == "")
            {
                //Console.WriteLine("未读取到config");
                Process_check = false;
                return;
            }
            string newLine = line.Replace("\r\n", "|");
            //string patternPart = Regex.Escape(BUSID);
            //Console.WriteLine(line);
            //Console.WriteLine(newLine);
            MatchCollection USBID = Regex.Matches(BUSID, @$"^(?!.*Attached)(\d+-\d+)\s.*(?:{newLine})", RegexOptions.Multiline);
            foreach (Match match in USBID)
            {
                if (match.Success)
                {
                    busIds.Add(match.Groups[1].Value);
                }
            }
            foreach (string busid in busIds.ToList())
            {
                //MessageBox.Show(busid);
                //Console.WriteLine($"bind --busid {busid} --force");
                Usbipd_Cmd($"bind --busid {busid} --force");
                //Console.WriteLine($"attach --wsl --busid {busid}");
                Usbipd_Cmd($"attach --wsl --busid {busid}");
            }
            Process_check = false;
        }

        private static bool ConsoleCloseHandler(int dwCtrlType)
        {
            return true;
        }
        bool GetTask(string taskName)
        {
            Microsoft.Win32.TaskScheduler.Task t = TaskService.Instance.GetTask(taskName);
            if (t == null) return false;
            return true;

        }
        private static void CreateScheduledTask()
        {
            TaskDefinition td = TaskService.Instance.NewTask();
            td.RegistrationInfo.Description = "USBipdtool自启计划";
            string exePath = Application.ExecutablePath;
            if(exePath is null)
            {
                //Console.WriteLine("无法找到当前可执行文件路径，计划任务创建失败");
                return;
            };
            string exeFolder = Path.GetDirectoryName(exePath)!;
            td.Principal.LogonType = TaskLogonType.InteractiveToken;
            td.Principal.RunLevel = TaskRunLevel.Highest;
            const string taskName = "USBipd_TOOL";
            td.Triggers.Add(new LogonTrigger());
            td.Actions.Add(new ExecAction(exePath, null, exeFolder));
            TaskService.Instance.RootFolder.RegisterTaskDefinition(taskName, td);
        }
            string Usbipd_Cmd(string args)
        {
            if (Extern_SW)
            {
                _usbipStartInfo.Arguments = args;

                using (Process process = new Process())
                {
                    process.StartInfo = _usbipStartInfo;
                    process.Start();
                    //process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    //process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    //Console.WriteLine(output);
                    if (!string.IsNullOrEmpty(error))
                        //Console.WriteLine(error);
                    return output + error;
                }
            }
            return "";
        }
        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _usbDeviceWatcher?.Stop();
            while (Process_check);
            Extern_SW = true;
            Usbipd_Cmd($"unbind --all");
            Extern_SW = false;
            Application.Exit();
        }
        private void 运行_Click(object sender, EventArgs e)
        {
            运行.Checked = !运行.Checked;
            //EnableDeviceMonitor = 运行.Checked;
            if (运行.Checked == false)
            {
                _usbDeviceWatcher?.Stop();
                Usbipd_Cmd("unbind --all");
            }
            else
            {
                _usbDeviceWatcher?.Start();
                RUN_USBIPD();
            }

        }
        //protected override void WndProc(ref Message m)
        //{
        //        if (!EnableDeviceMonitor) { base.WndProc(ref m); return; }
        //        const int WM_DEVICECHANGE = 0x219;
        //        const int DBT_DEVICEARRIVAL = 0x0007;
        //        base.WndProc(ref m);
        //        if (m.Msg == WM_DEVICECHANGE)
        //        {
        //            var now = DateTime.Now;
        //            if ((now - _lastDeviceEventTime).TotalMilliseconds < _coolDownMs)
        //            {
        //                return;
        //            }
        //            _lastDeviceEventTime = now;

        //            switch ((int)m.WParam)
        //            {
        //                case DBT_DEVICEARRIVAL:
        //                    {
        //                        RUN_USBIPD();
        //                    }
        //                    break;
        //            }
        //        }
        //}

        private void 自启动ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(!自启动ToolStripMenuItem.Checked)
            {
                CreateScheduledTask();
            }
            else
            {
                TaskService.Instance.RootFolder.DeleteTask("USBipd_TOOL");
            }
            if (GetTask("USBipd_TOOL"))
                自启动ToolStripMenuItem.Checked = true;
            else
                自启动ToolStripMenuItem.Checked = false;
        }
    }
}