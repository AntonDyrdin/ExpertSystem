namespace Dimension_Reducer
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.surfaceExcel = new System.Windows.Forms.RadioButton();
            this.surfacePlotly = new System.Windows.Forms.RadioButton();
            this.simpleTable = new System.Windows.Forms.RadioButton();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 241);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(176, 38);
            this.button1.TabIndex = 0;
            this.button1.Text = "Generate file";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 215);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(176, 20);
            this.textBox1.TabIndex = 1;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(12, 12);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(176, 20);
            this.textBox2.TabIndex = 3;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 38);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(176, 38);
            this.button2.TabIndex = 2;
            this.button2.Text = "Open file";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.surfaceExcel);
            this.groupBox1.Controls.Add(this.surfacePlotly);
            this.groupBox1.Controls.Add(this.simpleTable);
            this.groupBox1.Location = new System.Drawing.Point(12, 82);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(176, 127);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "3D mode";
            // 
            // surfaceExcel
            // 
            this.surfaceExcel.AutoSize = true;
            this.surfaceExcel.Location = new System.Drawing.Point(21, 71);
            this.surfaceExcel.Name = "surfaceExcel";
            this.surfaceExcel.Size = new System.Drawing.Size(103, 17);
            this.surfaceExcel.TabIndex = 2;
            this.surfaceExcel.Text = "surface for excel";
            this.surfaceExcel.UseVisualStyleBackColor = true;
            // 
            // surfacePlotly
            // 
            this.surfacePlotly.AutoSize = true;
            this.surfacePlotly.Checked = true;
            this.surfacePlotly.Location = new System.Drawing.Point(21, 48);
            this.surfacePlotly.Name = "surfacePlotly";
            this.surfacePlotly.Size = new System.Drawing.Size(102, 17);
            this.surfacePlotly.TabIndex = 1;
            this.surfacePlotly.TabStop = true;
            this.surfacePlotly.Text = "surfave for plotly";
            this.surfacePlotly.UseVisualStyleBackColor = true;
            // 
            // simpleTable
            // 
            this.simpleTable.AutoSize = true;
            this.simpleTable.Location = new System.Drawing.Point(21, 25);
            this.simpleTable.Name = "simpleTable";
            this.simpleTable.Size = new System.Drawing.Size(99, 17);
            this.simpleTable.TabIndex = 0;
            this.simpleTable.Text = "single Y column";
            this.simpleTable.UseVisualStyleBackColor = true;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(193, 286);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Dimension Reducer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton surfacePlotly;
        private System.Windows.Forms.RadioButton simpleTable;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.RadioButton surfaceExcel;
    }
}

