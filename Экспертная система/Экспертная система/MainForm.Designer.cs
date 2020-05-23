namespace Экспертная_система
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
            this.current_bid = new System.Windows.Forms.TextBox();
            this.current_ask = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
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
            this.picBox.Location = new System.Drawing.Point(0, -9);
            this.picBox.Margin = new System.Windows.Forms.Padding(0);
            this.picBox.Name = "picBox";
            this.picBox.Size = new System.Drawing.Size(1947, 1493);
            this.picBox.TabIndex = 37;
            this.picBox.TabStop = false;
            this.picBox.Click += new System.EventHandler(this.picBox_Click);
            this.picBox.DoubleClick += new System.EventHandler(this.picBox_DoubleClick);
            // 
            // b1
            // 
            this.b1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.b1.ForeColor = System.Drawing.Color.Cyan;
            this.b1.Location = new System.Drawing.Point(758, 4);
            this.b1.Margin = new System.Windows.Forms.Padding(4);
            this.b1.Name = "b1";
            this.b1.Size = new System.Drawing.Size(120, 60);
            this.b1.TabIndex = 39;
            this.b1.Text = "Запрет на покупку";
            this.b1.UseVisualStyleBackColor = true;
            this.b1.Click += new System.EventHandler(this.stop_buying_click);
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.Color.Red;
            this.button1.Location = new System.Drawing.Point(886, 4);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(120, 60);
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
            this.logBox.Location = new System.Drawing.Point(8, 203);
            this.logBox.Margin = new System.Windows.Forms.Padding(4);
            this.logBox.Name = "logBox";
            this.logBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.logBox.Size = new System.Drawing.Size(370, 1300);
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
            this.wipeLog.Location = new System.Drawing.Point(886, 69);
            this.wipeLog.Margin = new System.Windows.Forms.Padding(4);
            this.wipeLog.Name = "wipeLog";
            this.wipeLog.Size = new System.Drawing.Size(120, 50);
            this.wipeLog.TabIndex = 60;
            this.wipeLog.Text = "Wipe";
            this.wipeLog.UseVisualStyleBackColor = false;
            this.wipeLog.Click += new System.EventHandler(this.wipeLog_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.Color.Transparent;
            this.splitContainer1.Location = new System.Drawing.Point(3, 4);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AutoScroll = true;
            this.splitContainer1.Panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("splitContainer1.Panel1.BackgroundImage")));
            this.splitContainer1.Panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.splitContainer1.Panel1.Controls.Add(this.picBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.current_ask);
            this.splitContainer1.Panel2.Controls.Add(this.current_bid);
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
            this.splitContainer1.Size = new System.Drawing.Size(2880, 1500);
            this.splitContainer1.SplitterDistance = 1000;
            this.splitContainer1.SplitterWidth = 30;
            this.splitContainer1.TabIndex = 61;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            // 
            // sell_button
            // 
            this.sell_button.BackColor = System.Drawing.Color.Black;
            this.sell_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sell_button.ForeColor = System.Drawing.Color.Red;
            this.sell_button.Location = new System.Drawing.Point(200, 48);
            this.sell_button.Margin = new System.Windows.Forms.Padding(4);
            this.sell_button.Name = "sell_button";
            this.sell_button.Size = new System.Drawing.Size(81, 33);
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
            this.bye_button.Location = new System.Drawing.Point(60, 48);
            this.bye_button.Margin = new System.Windows.Forms.Padding(4);
            this.bye_button.Name = "bye_button";
            this.bye_button.Size = new System.Drawing.Size(81, 33);
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
            this.sum_name.Location = new System.Drawing.Point(279, 20);
            this.sum_name.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.sum_name.Name = "sum_name";
            this.sum_name.Size = new System.Drawing.Size(74, 21);
            this.sum_name.TabIndex = 67;
            this.sum_name.Text = "Сумма";
            // 
            // deposit_2_name
            // 
            this.deposit_2_name.AutoSize = true;
            this.deposit_2_name.BackColor = System.Drawing.Color.Transparent;
            this.deposit_2_name.Font = new System.Drawing.Font("Century Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.deposit_2_name.Location = new System.Drawing.Point(141, 20);
            this.deposit_2_name.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.deposit_2_name.Name = "deposit_2_name";
            this.deposit_2_name.Size = new System.Drawing.Size(48, 21);
            this.deposit_2_name.TabIndex = 66;
            this.deposit_2_name.Text = "USD:";
            // 
            // deposit_1_name
            // 
            this.deposit_1_name.AutoSize = true;
            this.deposit_1_name.BackColor = System.Drawing.Color.Transparent;
            this.deposit_1_name.Font = new System.Drawing.Font("Century Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.deposit_1_name.Location = new System.Drawing.Point(4, 20);
            this.deposit_1_name.Margin = new System.Windows.Forms.Padding(0);
            this.deposit_1_name.Name = "deposit_1_name";
            this.deposit_1_name.Size = new System.Drawing.Size(47, 21);
            this.deposit_1_name.TabIndex = 65;
            this.deposit_1_name.Text = "BTC:";
            // 
            // sum_of_deposits
            // 
            this.sum_of_deposits.BackColor = System.Drawing.Color.Black;
            this.sum_of_deposits.ForeColor = System.Drawing.Color.White;
            this.sum_of_deposits.Location = new System.Drawing.Point(357, 15);
            this.sum_of_deposits.Margin = new System.Windows.Forms.Padding(4);
            this.sum_of_deposits.Name = "sum_of_deposits";
            this.sum_of_deposits.Size = new System.Drawing.Size(79, 26);
            this.sum_of_deposits.TabIndex = 64;
            // 
            // deposit_2_value
            // 
            this.deposit_2_value.BackColor = System.Drawing.Color.Black;
            this.deposit_2_value.ForeColor = System.Drawing.Color.White;
            this.deposit_2_value.Location = new System.Drawing.Point(200, 15);
            this.deposit_2_value.Margin = new System.Windows.Forms.Padding(4);
            this.deposit_2_value.Name = "deposit_2_value";
            this.deposit_2_value.Size = new System.Drawing.Size(79, 26);
            this.deposit_2_value.TabIndex = 63;
            // 
            // deposit_1_value
            // 
            this.deposit_1_value.BackColor = System.Drawing.Color.Black;
            this.deposit_1_value.ForeColor = System.Drawing.Color.White;
            this.deposit_1_value.Location = new System.Drawing.Point(60, 15);
            this.deposit_1_value.Margin = new System.Windows.Forms.Padding(4);
            this.deposit_1_value.Name = "deposit_1_value";
            this.deposit_1_value.Size = new System.Drawing.Size(79, 26);
            this.deposit_1_value.TabIndex = 62;
            // 
            // displayedWindow
            // 
            this.displayedWindow.Location = new System.Drawing.Point(4, 126);
            this.displayedWindow.Margin = new System.Windows.Forms.Padding(4);
            this.displayedWindow.Maximum = 400;
            this.displayedWindow.Minimum = 10;
            this.displayedWindow.Name = "displayedWindow";
            this.displayedWindow.Size = new System.Drawing.Size(1002, 69);
            this.displayedWindow.TabIndex = 61;
            this.displayedWindow.Value = 10;
            this.displayedWindow.Scroll += new System.EventHandler(this.displayedWindow_Scroll);
            // 
            // current_bid
            // 
            this.current_bid.BackColor = System.Drawing.Color.Black;
            this.current_bid.ForeColor = System.Drawing.Color.White;
            this.current_bid.Location = new System.Drawing.Point(60, 90);
            this.current_bid.Margin = new System.Windows.Forms.Padding(4);
            this.current_bid.Name = "current_bid";
            this.current_bid.Size = new System.Drawing.Size(79, 26);
            this.current_bid.TabIndex = 63;
            // 
            // current_ask
            // 
            this.current_ask.BackColor = System.Drawing.Color.Black;
            this.current_ask.ForeColor = System.Drawing.Color.White;
            this.current_ask.Location = new System.Drawing.Point(202, 89);
            this.current_ask.Margin = new System.Windows.Forms.Padding(4);
            this.current_ask.Name = "current_ask";
            this.current_ask.Size = new System.Drawing.Size(79, 26);
            this.current_ask.TabIndex = 64;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Century Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(11, 91);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 21);
            this.label1.TabIndex = 66;
            this.label1.Text = "bid:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Century Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(151, 91);
            this.label2.Margin = new System.Windows.Forms.Padding(0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 21);
            this.label2.TabIndex = 66;
            this.label2.Text = "ask:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1924, 1050);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
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
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox current_ask;
        private System.Windows.Forms.TextBox current_bid;
    }
}

