namespace FPS
{
    partial class SetDataTime
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
            this.label1 = new System.Windows.Forms.Label();
            this.hours_txt_box = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.minutes_txt_box = new System.Windows.Forms.TextBox();
            this.am_btn = new System.Windows.Forms.Button();
            this.pm_btn = new System.Windows.Forms.Button();
            this.Ok_btn = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(287, 110);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(439, 32);
            this.label1.TabIndex = 1;
            this.label1.Text = "Set Auto Generate Reports Time";
            // 
            // hours_txt_box
            // 
            this.hours_txt_box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.hours_txt_box.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hours_txt_box.Location = new System.Drawing.Point(301, 188);
            this.hours_txt_box.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.hours_txt_box.MaxLength = 4;
            this.hours_txt_box.Name = "hours_txt_box";
            this.hours_txt_box.Size = new System.Drawing.Size(159, 30);
            this.hours_txt_box.TabIndex = 32;
            this.hours_txt_box.TextChanged += new System.EventHandler(this.hours_txt_box_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(191, 186);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 32);
            this.label2.TabIndex = 33;
            this.label2.Text = "Hours:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(484, 186);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(127, 32);
            this.label3.TabIndex = 35;
            this.label3.Text = "Minutes:";
            // 
            // minutes_txt_box
            // 
            this.minutes_txt_box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.minutes_txt_box.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.minutes_txt_box.Location = new System.Drawing.Point(620, 188);
            this.minutes_txt_box.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.minutes_txt_box.MaxLength = 4;
            this.minutes_txt_box.Name = "minutes_txt_box";
            this.minutes_txt_box.Size = new System.Drawing.Size(159, 30);
            this.minutes_txt_box.TabIndex = 34;
            // 
            // am_btn
            // 
            this.am_btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.am_btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.am_btn.Location = new System.Drawing.Point(385, 271);
            this.am_btn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.am_btn.Name = "am_btn";
            this.am_btn.Size = new System.Drawing.Size(100, 38);
            this.am_btn.TabIndex = 36;
            this.am_btn.Text = "AM";
            this.am_btn.UseVisualStyleBackColor = true;
            this.am_btn.Click += new System.EventHandler(this.am_btn_Click);
            // 
            // pm_btn
            // 
            this.pm_btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.pm_btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pm_btn.Location = new System.Drawing.Point(547, 271);
            this.pm_btn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pm_btn.Name = "pm_btn";
            this.pm_btn.Size = new System.Drawing.Size(100, 38);
            this.pm_btn.TabIndex = 36;
            this.pm_btn.Text = "PM";
            this.pm_btn.UseVisualStyleBackColor = true;
            this.pm_btn.Click += new System.EventHandler(this.pm_btn_Click);
            // 
            // Ok_btn
            // 
            this.Ok_btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Ok_btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Ok_btn.Location = new System.Drawing.Point(457, 346);
            this.Ok_btn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Ok_btn.Name = "Ok_btn";
            this.Ok_btn.Size = new System.Drawing.Size(124, 60);
            this.Ok_btn.TabIndex = 36;
            this.Ok_btn.Text = "OK";
            this.Ok_btn.UseVisualStyleBackColor = true;
            this.Ok_btn.Click += new System.EventHandler(this.Ok_btn_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(191, 271);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(123, 32);
            this.label4.TabIndex = 37;
            this.label4.Text = "Interval:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(505, 270);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(23, 32);
            this.label5.TabIndex = 38;
            this.label5.Text = "/";
            // 
            // SetDataTime
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1045, 543);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Ok_btn);
            this.Controls.Add(this.pm_btn);
            this.Controls.Add(this.am_btn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.minutes_txt_box);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.hours_txt_box);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "SetDataTime";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "s";
            this.Load += new System.EventHandler(this.SetDataTime_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox hours_txt_box;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox minutes_txt_box;
        private System.Windows.Forms.Button am_btn;
        private System.Windows.Forms.Button pm_btn;
        private System.Windows.Forms.Button Ok_btn;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
    }
}