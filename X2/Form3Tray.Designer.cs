namespace X2
{
    partial class Form3Tray
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form3Tray));
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.DEBUG_BTN = new System.Windows.Forms.Button();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(12, 12);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(170, 17);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "uruchom przy starcie Windows";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "QA Cat";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 10000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // DEBUG_BTN
            // 
            this.DEBUG_BTN.Location = new System.Drawing.Point(536, 253);
            this.DEBUG_BTN.Name = "DEBUG_BTN";
            this.DEBUG_BTN.Size = new System.Drawing.Size(75, 23);
            this.DEBUG_BTN.TabIndex = 1;
            this.DEBUG_BTN.Text = "DEBUG";
            this.DEBUG_BTN.UseVisualStyleBackColor = true;
            this.DEBUG_BTN.Click += new System.EventHandler(this.DEBUG_BTN_Click);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(12, 35);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(163, 17);
            this.checkBox2.TabIndex = 2;
            this.checkBox2.Text = "testuj w dowolnym momencie";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // Form3Tray
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(623, 288);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.DEBUG_BTN);
            this.Controls.Add(this.checkBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form3Tray";
            this.ShowInTaskbar = false;
            this.Text = "QA Cat [Daemon]";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form3Tray_FormClosing);
            this.SizeChanged += new System.EventHandler(this.Form3Tray_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button DEBUG_BTN;
        private System.Windows.Forms.CheckBox checkBox2;
    }
}