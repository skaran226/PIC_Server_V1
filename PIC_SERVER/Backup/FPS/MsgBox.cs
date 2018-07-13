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
    delegate void SetTextCallback(string text);
    delegate void HideButtonCallback(Button bButton);
    delegate void ShowButtonCallback(Button bButton);

    public partial class MsgBox : Form
    {
        delegate void SetTextCallback(string text);

        public MsgBox()
        {
            InitializeComponent();
        }

        public void SetText1(string text)
        {
            if (this.label1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText1);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.label1.Text = text;
            }
        }

        public void ShowButton(Button bButton)
        {
            if (bButton.InvokeRequired)
            {
                ShowButtonCallback d = new ShowButtonCallback(ShowButton);
                this.Invoke(d, new object[] { bButton });
            }
            else
            {
                bButton.BringToFront();
                bButton.Show();
            }
        }

        public void HideButton(Button bButton)
        {
            if (bButton.InvokeRequired)
            {
                HideButtonCallback d = new HideButtonCallback(HideButton);
                this.Invoke(d, new object[] { bButton });
            }
            else
            {
                bButton.Hide();
            }
        }

        private void btYes_Click(object sender, EventArgs e)
        {
            if (Display.iChoice == 1)
            {
                Display.iChoice = 0;

                if (Display.iView == 3)
                {
                    DB.GenerateEodReport();
                }
                else if (Display.iView == 4)
                {
                    DB.GenerateCashReport();
                }
            }
            else if (Display.iChoice == 2)
            {
                Display.iChoice = 0;

                System.Diagnostics.Process.Start("shutdown", "-r -f -t 00");
            }

            Display.HideMessageBox();
        }

        private void btNo_Click(object sender, EventArgs e)
        {
            Display.HideMessageBox();
        }
    }
}
