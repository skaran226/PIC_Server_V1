namespace FPS
{
    partial class Validate
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
            this.Cancle = new System.Windows.Forms.Button();
            this.btn_Validate = new System.Windows.Forms.Button();
            this.textBoxValidate = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(320, 113);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "Enter Password";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // Cancle
            // 
            this.Cancle.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Yellow;
            this.Cancle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Cancle.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.Cancle.Location = new System.Drawing.Point(361, 309);
            this.Cancle.Name = "Cancle";
            this.Cancle.Size = new System.Drawing.Size(75, 34);
            this.Cancle.TabIndex = 5;
            this.Cancle.Text = "Cancle";
            this.Cancle.UseVisualStyleBackColor = true;
            this.Cancle.Click += new System.EventHandler(this.Cancle_Click);
            // 
            // btn_Validate
            // 
            this.btn_Validate.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn_Validate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Yellow;
            this.btn_Validate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Validate.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.btn_Validate.Location = new System.Drawing.Point(340, 227);
            this.btn_Validate.Name = "btn_Validate";
            this.btn_Validate.Size = new System.Drawing.Size(120, 50);
            this.btn_Validate.TabIndex = 4;
            this.btn_Validate.Text = "Authenticate";
            this.btn_Validate.UseVisualStyleBackColor = true;
            this.btn_Validate.Click += new System.EventHandler(this.btn_Validate_Click);
            // 
            // textBoxValidate
            // 
            this.textBoxValidate.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.textBoxValidate.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxValidate.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxValidate.Location = new System.Drawing.Point(318, 183);
            this.textBoxValidate.Name = "textBoxValidate";
            this.textBoxValidate.Size = new System.Drawing.Size(165, 19);
            this.textBoxValidate.TabIndex = 3;
            this.textBoxValidate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Validate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(800, 480);
            this.Controls.Add(this.Cancle);
            this.Controls.Add(this.btn_Validate);
            this.Controls.Add(this.textBoxValidate);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Validate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Validate";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Cancle;
        private System.Windows.Forms.Button btn_Validate;
        private System.Windows.Forms.TextBox textBoxValidate;

    }
}