using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPS
{
    public partial class Validate : Form
    {
        public Validate()
        {
            InitializeComponent();


        }

        private void btn_Validate_Click(object sender, EventArgs e)
        {
            if (textBoxValidate.Text.ToString() == "E892952932")
            {


                // MessageBox.Show("fine");
                this.Hide();
                Display.ChangeView(6);


                

            }
            else {

                Display.ShowMessageBox("Enter Wrong PassWord!!", 4);
            }
        }

       

        private void Cancle_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
