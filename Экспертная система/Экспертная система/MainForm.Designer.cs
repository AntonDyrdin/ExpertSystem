﻿namespace Экспертная_система
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.picBox = new System.Windows.Forms.PictureBox();
            this.b1 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.logBox = new System.Windows.Forms.RichTextBox();
            this.wipeLog = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.sell_button = new System.Windows.Forms.Button();
            this.bye_button = new System.Windows.Forms.Button();
            this.sum_name = new System.Windows.Forms.Label();
            this.deposit_2_name = new System.Windows.Forms.Label();
            this.deposit_1_name = new System.Windows.Forms.Label();
            this.sum_of_deposits = new System.Windows.Forms.TextBox();
            this.deposit_2_value = new System.Windows.Forms.TextBox();
            this.deposit_1_value = new System.Windows.Forms.TextBox();
            this.displayedWindow = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.picBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.displayedWindow)).BeginInit();
            this.SuspendLayout();
            // 
            // picBox
            // 
            this.picBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.picBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picBox.InitialImage = null;
            this.picBox.Location = new System.Drawing.Point(0, -3);
            this.picBox.Margin = new System.Windows.Forms.Padding(0);
            this.picBox.Name = "picBox";
            this.picBox.Size = new System.Drawing.Size(1200, 993);
            this.picBox.TabIndex = 37;
            this.picBox.TabStop = false;
            this.picBox.Click += new System.EventHandler(this.picBox_Click);
            this.picBox.DoubleClick += new System.EventHandler(this.picBox_DoubleClick);
            // 
            // b1
            // 
            this.b1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.b1.ForeColor = System.Drawing.Color.Cyan;
            this.b1.Location = new System.Drawing.Point(505, 3);
            this.b1.Name = "b1";
            this.b1.Size = new System.Drawing.Size(80, 40);
            this.b1.TabIndex = 39;
            this.b1.Text = "Запрет на покупку";
            this.b1.UseVisualStyleBackColor = true;
            this.b1.Click += new System.EventHandler(this.stop_buying_click);
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.Color.Red;
            this.button1.Location = new System.Drawing.Point(591, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(80, 40);
            this.button1.TabIndex = 42;
            this.button1.Text = "СТОП";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.RedClick);
            // 
            // logBox
            // 
            this.logBox.BackColor = System.Drawing.Color.Black;
            this.logBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logBox.DetectUrls = false;
            this.logBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.logBox.ForeColor = System.Drawing.Color.White;
            this.logBox.Location = new System.Drawing.Point(3, 135);
            this.logBox.Name = "logBox";
            this.logBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.logBox.Size = new System.Drawing.Size(668, 996);
            this.logBox.TabIndex = 52;
            this.logBox.Text = "";
            this.logBox.MouseEnter += new System.EventHandler(this.LogBox_MouseEnter);
            this.logBox.MouseLeave += new System.EventHandler(this.LogBox_MouseLeave);
            // 
            // wipeLog
            // 
            this.wipeLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(73)))), ((int)(((byte)(106)))));
            this.wipeLog.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.wipeLog.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.wipeLog.Location = new System.Drawing.Point(591, 46);
            this.wipeLog.Name = "wipeLog";
            this.wipeLog.Size = new System.Drawing.Size(80, 33);
            this.wipeLog.TabIndex = 60;
            this.wipeLog.Text = "Wipe";
            this.wipeLog.UseVisualStyleBackColor = false;
            this.wipeLog.Click += new System.EventHandler(this.wipeLog_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.Color.Transparent;
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AutoScroll = true;
            this.splitContainer1.Panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("splitContainer1.Panel1.BackgroundImage")));
            this.splitContainer1.Panel1.Controls.Add(this.picBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.sell_button);
            this.splitContainer1.Panel2.Controls.Add(this.bye_button);
            this.splitContainer1.Panel2.Controls.Add(this.sum_name);
            this.splitContainer1.Panel2.Controls.Add(this.deposit_2_name);
            this.splitContainer1.Panel2.Controls.Add(this.deposit_1_name);
            this.splitContainer1.Panel2.Controls.Add(this.sum_of_deposits);
            this.splitContainer1.Panel2.Controls.Add(this.deposit_2_value);
            this.splitContainer1.Panel2.Controls.Add(this.deposit_1_value);
            this.splitContainer1.Panel2.Controls.Add(this.displayedWindow);
            this.splitContainer1.Panel2.Controls.Add(this.logBox);
            this.splitContainer1.Panel2.Controls.Add(this.button1);
            this.splitContainer1.Panel2.Controls.Add(this.wipeLog);
            this.splitContainer1.Panel2.Controls.Add(this.b1);
            this.splitContainer1.Size = new System.Drawing.Size(1920, 1050);
            this.splitContainer1.SplitterDistance = 1200;
            this.splitContainer1.SplitterWidth = 20;
            this.splitContainer1.TabIndex = 61;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            // 
            // sell_button
            // 
            this.sell_button.BackColor = System.Drawing.Color.Black;
            this.sell_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sell_button.ForeColor = System.Drawing.Color.Red;
            this.sell_button.Location = new System.Drawing.Point(133, 32);
            this.sell_button.Name = "sell_button";
            this.sell_button.Size = new System.Drawing.Size(54, 22);
            this.sell_button.TabIndex = 69;
            this.sell_button.Text = "SELL";
            this.sell_button.UseVisualStyleBackColor = false;
            this.sell_button.Click += new System.EventHandler(this.sell_button_Click);
            // 
            // bye_button
            // 
            this.bye_button.BackColor = System.Drawing.Color.Black;
            this.bye_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bye_button.ForeColor = System.Drawing.Color.Blue;
            this.bye_button.Location = new System.Drawing.Point(40, 32);
            this.bye_button.Name = "bye_button";
            this.bye_button.Size = new System.Drawing.Size(54, 22);
            this.bye_button.TabIndex = 68;
            this.bye_button.Text = "BUY";
            this.bye_button.UseVisualStyleBackColor = false;
            this.bye_button.Click += new System.EventHandler(this.bye_button_Click);
            // 
            // sum_name
            // 
            this.sum_name.AutoSize = true;
            this.sum_name.BackColor = System.Drawing.Color.Transparent;
            this.sum_name.Font = new System.Drawing.Font("Century Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.sum_name.Location = new System.Drawing.Point(186, 13);
            this.sum_name.Name = "sum_name";
            this.sum_name.Size = new System.Drawing.Size(51, 16);
            this.sum_name.TabIndex = 67;
            this.sum_name.Text = "Сумма";
            // 
            // deposit_2_name
            // 
            this.deposit_2_name.AutoSize = true;
            this.deposit_2_name.BackColor = System.Drawing.Color.Transparent;
            this.deposit_2_name.Font = new System.Drawing.Font("Century Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.deposit_2_name.Location = new System.Drawing.Point(94, 13);
            this.deposit_2_name.Name = "deposit_2_name";
            this.deposit_2_name.Size = new System.Drawing.Size(33, 16);
            this.deposit_2_name.TabIndex = 66;
            this.deposit_2_name.Text = "USD:";
            // 
            // deposit_1_name
            // 
            this.deposit_1_name.AutoSize = true;
            this.deposit_1_name.BackColor = System.Drawing.Color.Transparent;
            this.deposit_1_name.Font = new System.Drawing.Font("Century Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.deposit_1_name.Location = new System.Drawing.Point(3, 13);
            this.deposit_1_name.Margin = new System.Windows.Forms.Padding(0);
            this.deposit_1_name.Name = "deposit_1_name";
            this.deposit_1_name.Size = new System.Drawing.Size(31, 16);
            this.deposit_1_name.TabIndex = 65;
            this.deposit_1_name.Text = "BTC:";
            // 
            // sum_of_deposits
            // 
            this.sum_of_deposits.BackColor = System.Drawing.Color.Black;
            this.sum_of_deposits.ForeColor = System.Drawing.Color.White;
            this.sum_of_deposits.Location = new System.Drawing.Point(238, 10);
            this.sum_of_deposits.Name = "sum_of_deposits";
            this.sum_of_deposits.Size = new System.Drawing.Size(54, 20);
            this.sum_of_deposits.TabIndex = 64;
            // 
            // deposit_2_value
            // 
            this.deposit_2_value.BackColor = System.Drawing.Color.Black;
            this.deposit_2_value.ForeColor = System.Drawing.Color.White;
            this.deposit_2_value.Location = new System.Drawing.Point(133, 10);
            this.deposit_2_value.Name = "deposit_2_value";
            this.deposit_2_value.Size = new System.Drawing.Size(54, 20);
            this.deposit_2_value.TabIndex = 63;
            // 
            // deposit_1_value
            // 
            this.deposit_1_value.BackColor = System.Drawing.Color.Black;
            this.deposit_1_value.ForeColor = System.Drawing.Color.White;
            this.deposit_1_value.Location = new System.Drawing.Point(40, 10);
            this.deposit_1_value.Name = "deposit_1_value";
            this.deposit_1_value.Size = new System.Drawing.Size(54, 20);
            this.deposit_1_value.TabIndex = 62;
            // 
            // displayedWindow
            // 
            this.displayedWindow.Location = new System.Drawing.Point(3, 84);
            this.displayedWindow.Maximum = 200;
            this.displayedWindow.Minimum = 3;
            this.displayedWindow.Name = "displayedWindow";
            this.displayedWindow.Size = new System.Drawing.Size(668, 45);
            this.displayedWindow.TabIndex = 61;
            this.displayedWindow.Value = 10;
            this.displayedWindow.Scroll += new System.EventHandler(this.displayedWindow_Scroll);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1904, 1011);
            this.Controls.Add(this.splitContainer1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Экспертная система";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picBox)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.displayedWindow)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.PictureBox picBox;
        private System.Windows.Forms.Button b1;
        private System.Windows.Forms.Button button1;
        public System.Windows.Forms.RichTextBox logBox;
        private System.Windows.Forms.Button wipeLog;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TrackBar displayedWindow;
        private System.Windows.Forms.Label sum_name;
        private System.Windows.Forms.Label deposit_2_name;
        private System.Windows.Forms.Label deposit_1_name;
        private System.Windows.Forms.TextBox sum_of_deposits;
        private System.Windows.Forms.TextBox deposit_2_value;
        private System.Windows.Forms.TextBox deposit_1_value;
        private System.Windows.Forms.Button sell_button;
        private System.Windows.Forms.Button bye_button;
    }
}

