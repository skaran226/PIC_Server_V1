using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Diagnostics;
using System.IO;

namespace FPS
{
    public partial class Form1 : Form
    {
        delegate void SetLabelTextCallback(Label lbPassLabel, string sPassText);
        delegate void SetLabelVisibleCallback(Label lbPassLabel, bool bVisible);
        delegate void SetButtonTextCallback(Button btPassButton, string sPassText);
        delegate void SetButtonColorCallback(Button btPassButton, Color cPassColor);
        delegate void SetButtonVisibleCallback(Button btPassButton, bool bVisible);
        delegate void SetTextBoxTextCallback(TextBox tbPassTextBox, string sPassText);
        delegate void SetTextBoxVisibleCallback(TextBox tbPassTextBox, bool bVisible);

        public Form1()
        {
            //Cursor.Hide();

            InitializeComponent();
        }

        public void SetLableText(Label lbPassLabel, string sPassText)
        {
            if (lbPassLabel.InvokeRequired)
            {
                SetLabelTextCallback d = new SetLabelTextCallback(SetLableText);
                this.Invoke(d, new object[] { lbPassLabel, sPassText });
            }
            else
            {
                lbPassLabel.Text = sPassText;
            }
        }

        public void SetButtonText(Button btPassButton, string sPassText)
        {
            if (btPassButton.InvokeRequired)
            {
                SetButtonTextCallback d = new SetButtonTextCallback(SetButtonText);
                this.Invoke(d, new object[] { btPassButton, sPassText });
            }
            else
            {
                btPassButton.Text = sPassText;
            }
        }

        public void SetButtonColor(Button btPassButton, Color cPassColor)
        {
            if (btPassButton.InvokeRequired)
            {
                SetButtonColorCallback d = new SetButtonColorCallback(SetButtonColor);
                this.Invoke(d, new object[] { btPassButton, cPassColor });
            }
            else
            {
                btPassButton.BackColor = cPassColor;
                btPassButton.FlatAppearance.MouseOverBackColor = cPassColor;
            }
        }

        public void SetButtonVisible(Button btPassButton, bool bVisible)
        {
            if (btPassButton.InvokeRequired)
            {
                SetButtonVisibleCallback d = new SetButtonVisibleCallback(SetButtonVisible);
                this.Invoke(d, new object[] { btPassButton, bVisible });
            }
            else
            {
                btPassButton.Visible = bVisible;
            }
        }

        public void SetTextBoxVisible(TextBox tbPassTextBox, bool bVisible)
        {
            if (tbPassTextBox.InvokeRequired)
            {
                SetTextBoxVisibleCallback d = new SetTextBoxVisibleCallback(SetTextBoxVisible);
                this.Invoke(d, new object[] { tbPassTextBox, bVisible });
            }
            else
            {
                tbPassTextBox.Visible = bVisible;
            }
        }

        public void SetLabelVisible(Label lbPassLabel, bool bVisible)
        {
            if (lbPassLabel.InvokeRequired)
            {
                SetLabelVisibleCallback d = new SetLabelVisibleCallback(SetLabelVisible);
                this.Invoke(d, new object[] { lbPassLabel, bVisible });
            }
            else
            {
                lbPassLabel.Visible = bVisible;
            }
        }

        public void SetTextBoxText(TextBox tbPassTextBox, string sPassText)
        {
            if (tbPassTextBox.InvokeRequired)
            {
                SetTextBoxTextCallback d = new SetTextBoxTextCallback(SetTextBoxText);
                this.Invoke(d, new object[] { tbPassTextBox, sPassText });
            }
            else
            {
                tbPassTextBox.Text = sPassText;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (this.button1.Text != "")
            {
                Display.SelectButton(1);
                //this.button1.BackColor = Color.Yellow;
                //this.button1.FlatAppearance.MouseOverBackColor = Color.Yellow;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.button2.Text != "")
            {
                Display.SelectButton(2);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.button3.Text != "")
            {
                Display.SelectButton(3);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (this.button4.Text != "")
            {
                Display.SelectButton(4);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (this.button5.Text != "")
            {
                Display.SelectButton(5);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (this.button6.Text != "")
            {
                Display.SelectButton(6);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (this.button7.Text != "")
            {
                Display.SelectButton(7);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (this.button8.Text != "")
            {
                Display.SelectButton(8);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (this.button9.Text != "")
            {
                Display.SelectButton(9);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (this.button10.Text != "")
            {
                Display.SelectButton(10);
            }
        }

        private void btPrint_Click(object sender, EventArgs e)
        {
            if (Display.iButtonSelected > 0)
            {
                if (Display.iView == 1)
                {
                    if (Display.lColors[Display.iButtonSelected - 1] == Color.Orange)
                    {
                        TRAN_MGR.TRANs[Display.iButtonSelected - 1].Print();
                    }
                }
                else if (Display.iView == 2)
                {
                    DB.PrintReceipt(Display.iButtonSelected - 1);
                }
                else if (Display.iView == 3)
                {
                    DB.PrintEodReport(Display.iButtonSelected - 1);
                }
                else if (Display.iView == 4)
                {
                    DB.PrintCashReport(Display.iButtonSelected - 1);
                }
            }
        }

        private void btPending_Click(object sender, EventArgs e)
        {
            Display.ChangeView(1);
        }

        private void btCompleted_Click(object sender, EventArgs e)
        {
            Display.ChangeView(2);

        }

        private void btEOD_Click(object sender, EventArgs e)
        {
            Display.ChangeView(3);
        }

        private void btCash_Click(object sender, EventArgs e)
        {
            Display.ChangeView(4);
        }

        private void btStatus_Click(object sender, EventArgs e)
        {
            //CenCom.bStatusRequest = true;
            Display.ChangeView(5);
        }

        private void btConfigure_Click(object sender, EventArgs e)
        {
            Display.ChangeView(6);
        }

        private void btUpdate_Click(object sender, EventArgs e)
        {
            Process p = null;
            try
            {
                string targetDir;
                targetDir = string.Format(@"D:\");
                p = new Process();
                p.StartInfo.WorkingDirectory = targetDir;
                p.StartInfo.FileName = "RunMe.bat";
                p.Start();
            }
            catch
            {
                Display.ShowMessageBox("Error", 3);
            }
        }

        private void btRestart_Click(object sender, EventArgs e)
        {
            Display.ShowMessageBox("Are you sure?\n\n", 10, 2);
        }

        private void btGenerate_Click(object sender, EventArgs e)
        {
            if (Display.iView == 3)
            {
                Display.ShowMessageBox("Are you sure?\n\n", 10, 1);
            }
            else if (Display.iView == 4)
            {
                Display.ShowMessageBox("Are you sure?\n\n", 10, 1);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Process.Start("control.exe", "date/time");
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btSave_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("SAVE CONFIG");

            int iPic, iPump, iCash, iBills, iGrade1, iGrade2, iGrade3, iGrade4;
            string sHeader, sFooter;

            try
            {
                iPic = Convert.ToInt16(Display.screen1.tbPicNum.Text);
                if (iPic >= 1 && iPic <= 8)
                {
                    FileAccess.sSettings = FileAccess.sSettings.Replace("<PICNUM>" + Convert.ToString(CenCom.iPicCount), "<PICNUM>" + iPic);
                }
                iPump = Convert.ToInt16(Display.screen1.tbPumpNum.Text);
                if (iPump >= 1 && iPump <= 36)
                {
                    FileAccess.sSettings = FileAccess.sSettings.Replace("<PUMPNUM>" + Convert.ToString(CenCom.iPumpCount), "<PUMPNUM>" + iPump);
                }
                iCash = Convert.ToInt16(Display.screen1.tbMaxCash.Text);
                if (iCash >= 20 && iPump <= 200)
                {
                    FileAccess.sSettings = FileAccess.sSettings.Replace("<MAXCASH>" + Convert.ToString(CenCom.iMaxCash), "<MAXCASH>" + iCash);
                }
                iBills = Convert.ToInt16(Display.screen1.tbMaxBills.Text);
                if (iBills >= 10 && iBills <= 100)
                {
                    FileAccess.sSettings = FileAccess.sSettings.Replace("<MAXBILLS>" + Convert.ToString(CenCom.iMaxBills), "<MAXBILLS>" + iBills);
                }
            }
            catch
            {

                Debug.WriteLine("Invalid String to Convert");
            }

            FileAccess.sSettings = FileAccess.sSettings.Replace("<HEADER>" + Printer.sHeader, "<HEADER>" + Display.screen1.tbHeader.Text);
            FileAccess.sSettings = FileAccess.sSettings.Replace("<FOOTER>" + Printer.sFooter, "<FOOTER>" + Display.screen1.tbFooter.Text);

            Display.GetConfig();

            File.WriteAllText("settings.txt", FileAccess.sSettings);
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            Display.GetConfig();
        }

        private void btDateTime_Click(object sender, EventArgs e)
        {
            Process.Start("control.exe", "date/time");
        }

        private void btSave_Click_1(object sender, EventArgs e)
        {
            Debug.WriteLine("SAVE CONFIG");

            int iPic, iPump, iCash, iBills, iGrade1, iGrade2, iGrade3, iGrade4;
            string sHeader, sFooter;

            try
            {
                iPic = Convert.ToInt16(Display.screen1.tbPicNum.Text);
                if (iPic >= 1 && iPic <= 8)
                {
                    FileAccess.sSettings = FileAccess.sSettings.Replace("<PICNUM>" + Convert.ToString(CenCom.iPicCount), "<PICNUM>" + iPic);
                }
                iPump = Convert.ToInt16(Display.screen1.tbPumpNum.Text);
                if (iPump >= 1 && iPump <= 36)
                {
                    FileAccess.sSettings = FileAccess.sSettings.Replace("<PUMPNUM>" + Convert.ToString(CenCom.iPumpCount), "<PUMPNUM>" + iPump);
                }
                iCash = Convert.ToInt16(Display.screen1.tbMaxCash.Text);
                if (iCash >= 20 && iPump <= 200)
                {
                    FileAccess.sSettings = FileAccess.sSettings.Replace("<MAXCASH>" + Convert.ToString(CenCom.iMaxCash), "<MAXCASH>" + iCash);
                }
                iBills = Convert.ToInt16(Display.screen1.tbMaxBills.Text);
                if (iBills >= 10 && iBills <= 100)
                {
                    FileAccess.sSettings = FileAccess.sSettings.Replace("<MAXBILLS>" + Convert.ToString(CenCom.iMaxBills), "<MAXBILLS>" + iBills);
                }
            }
            catch
            {

                Debug.WriteLine("Invalid String to Convert");
            }

            FileAccess.sSettings = FileAccess.sSettings.Replace("<HEADER>" + Printer.sHeader, "<HEADER>" + Display.screen1.tbHeader.Text);
            FileAccess.sSettings = FileAccess.sSettings.Replace("<FOOTER>" + Printer.sFooter, "<FOOTER>" + Display.screen1.tbFooter.Text);

            Display.GetConfig();
        }

        private void btCancel_Click_1(object sender, EventArgs e)
        {
            Display.GetConfig();
        }

        private void btShutDown_Click(object sender, EventArgs e)
        {
            Display.ShowMessageBox("Are you sure?\n\n", 10, 3);
        }

        private void btPageUp_Click(object sender, EventArgs e)
        {
            int iButtonIndex;
            int iTranIndex;

            DB.iPage++;

            iButtonIndex = 0;
            for (iTranIndex = (10 * (DB.iPage - 1)); iTranIndex < (10 * DB.iPage); iTranIndex++)
            {
                if (iTranIndex < DB.lCompletedTrans.Count)
                {
                    iButtonIndex++;
                    Display.UpdateButtonText(iButtonIndex, "PUMP: " + DB.lCompletedTrans[iTranIndex].sPump + " @ " + DB.lCompletedTrans[iTranIndex].sShowTime + "\nPAID: $" + DB.lCompletedTrans[iTranIndex].sDeposit + "  CHANGE: $" + DB.lCompletedTrans[iTranIndex].sChange);
                }
            }

            if (DB.lCompletedTrans.Count <= 10 * DB.iPage)
            {
                Display.screen1.SetButtonVisible(Display.screen1.btPageUp, false);
            }

            if (DB.iPage == 2)
            {
                Display.screen1.SetButtonVisible(Display.screen1.btPageDown, true);
            }
        }

        private void btLoggingSFC_Click(object sender, EventArgs e)
        {
            if (CenCom.iLoggingSFC == 0)
            {
                CenCom.iLoggingSFC = 1;
                Display.screen1.SetButtonText(Display.screen1.btLoggingSFC, "LOGGING\nSFC - ON");
                Display.screen1.SetButtonColor(Display.screen1.btLoggingSFC, Color.Green);
            }
            else
            {
                CenCom.iLoggingSFC = 0;
                Display.screen1.SetButtonText(Display.screen1.btLoggingSFC, "LOGGING\nSFC - OFF");
                Display.screen1.SetButtonColor(Display.screen1.btLoggingSFC, Color.White);
            }
        }

        private void btPageDown_Click(object sender, EventArgs e)
        {
            int iButtonIndex;
            int iTranIndex;

            DB.iPage--;

            iButtonIndex = 0;
            for (iTranIndex = (10 * (DB.iPage - 1)); iTranIndex < (10 * DB.iPage); iTranIndex++)
            {
                if (iTranIndex < DB.lCompletedTrans.Count)
                {
                    iButtonIndex++;
                    Display.UpdateButtonText(iButtonIndex, "PUMP: " + DB.lCompletedTrans[iTranIndex].sPump + " @ " + DB.lCompletedTrans[iTranIndex].sShowTime + "\nPAID: $" + DB.lCompletedTrans[iTranIndex].sDeposit + "  CHANGE: $" + DB.lCompletedTrans[iTranIndex].sChange);
                }
            }

            if (DB.lCompletedTrans.Count > 10 * DB.iPage)
            {
                Display.screen1.SetButtonVisible(Display.screen1.btPageUp, true);
            }

            if (DB.iPage == 1)
            {
                Display.screen1.SetButtonVisible(Display.screen1.btPageDown, false);
            }
        }

        private void btLoggingPIC_Click(object sender, EventArgs e)
        {
            if (CenCom.iLoggingPIC == 0)
            {
                CenCom.iLoggingPIC = 1;
                Display.screen1.SetButtonText(Display.screen1.btLoggingPIC, "LOGGING\nPIC - ON");
                Display.screen1.SetButtonColor(Display.screen1.btLoggingPIC, Color.Green);
            }
            else
            {
                CenCom.iLoggingPIC = 0;
                Display.screen1.SetButtonText(Display.screen1.btLoggingPIC, "LOGGING\nPIC - OFF");
                Display.screen1.SetButtonColor(Display.screen1.btLoggingPIC, Color.White);
            }
        }

        private void btDownloadLogs_Click(object sender, EventArgs e)
        {
            FileAccess.CopyLogs();
            Display.ShowMessageBox("Copy Success", 3);
        }

        private void btDownloadReports_Click(object sender, EventArgs e)
        {
            FileAccess.CopyReports();
            Display.ShowMessageBox("Copy Success", 3);
        }

        private void btDownloadData_Click(object sender, EventArgs e)
        {
            FileAccess.CopyData();
            Display.ShowMessageBox("Copy Success", 3);
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            if (Display.iButtonSelected > 0)
            {
                Display.ShowMessageBox("Are you sure?\n\n", 10, 4);
            }
        }

    }

    static class ExtendForm
    {
        delegate void MyHideCallback(Form form);
        delegate void MyShowCallback(Form form);

        public static void HideThis(this Form form)
        {
            try
            {
                if (form.InvokeRequired)
                {
                    MyHideCallback d = new MyHideCallback(HideThis);
                    form.Invoke(d, new object[] { form });
                }
                else
                {
                    form.Hide();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static void ShowThis(this Form form)
        {
            try
            {
                if (form.InvokeRequired)
                {
                    MyShowCallback d = new MyShowCallback(ShowThis);
                    form.Invoke(d, new object[] { form });
                }
                else
                {
                    //Form.ActiveForm.ActiveMdiChild.HideThis();
                    form.Show();
                    form.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
