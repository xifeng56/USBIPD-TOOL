namespace usbipd
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            tuopan = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            自启动ToolStripMenuItem = new ToolStripMenuItem();
            运行 = new ToolStripMenuItem();
            退出ToolStripMenuItem = new ToolStripMenuItem();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tuopan
            // 
            tuopan.ContextMenuStrip = contextMenuStrip1;
            tuopan.Icon = (Icon)resources.GetObject("tuopan.Icon");
            tuopan.Text = "本软件为付费商用软件，著作权归 B 站 UP 主「雾都四季」所有，未经作者书面许可，禁止逆向、分发、倒卖";
            tuopan.Visible = true;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(24, 24);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { 自启动ToolStripMenuItem, 运行, 退出ToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(241, 127);
            // 
            // 自启动ToolStripMenuItem
            // 
            自启动ToolStripMenuItem.Name = "自启动ToolStripMenuItem";
            自启动ToolStripMenuItem.Size = new Size(240, 30);
            自启动ToolStripMenuItem.Text = "自启动";
            自启动ToolStripMenuItem.Click += 自启动ToolStripMenuItem_Click;
            // 
            // 运行
            // 
            运行.Checked = true;
            运行.CheckState = CheckState.Checked;
            运行.Name = "运行";
            运行.Size = new Size(240, 30);
            运行.Text = "运行";
            运行.Click += 运行_Click;
            // 
            // 退出ToolStripMenuItem
            // 
            退出ToolStripMenuItem.Name = "退出ToolStripMenuItem";
            退出ToolStripMenuItem.Size = new Size(240, 30);
            退出ToolStripMenuItem.Text = "退出";
            退出ToolStripMenuItem.Click += 退出ToolStripMenuItem_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1257, 635);
            Margin = new Padding(5, 4, 5, 4);
            Name = "Form1";
            ShowInTaskbar = false;
            Text = "Form1";
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private NotifyIcon tuopan;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem 退出ToolStripMenuItem;
        private ToolStripMenuItem 运行;
        private ToolStripMenuItem 自启动ToolStripMenuItem;
    }
}
