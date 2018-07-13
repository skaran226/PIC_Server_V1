using System;
using System.Collections.Generic;
using System.Text;

using System.IO.Ports;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Management;
using System.Data.OleDb;
using System.Drawing;

namespace FPS
{
    public static class CenCom
    {
        public static bool bLogMaster = false;
        public static int iWait = 1;
        public static string sOS = "XP";

        public static bool bStartup = true;
        public static bool bTestMode = false;

        [DllImport("Kernel32.dll")]
        public static extern bool SetLocalTime(ref SYSTEMTIME Time);

        public static int iStatusRequest = 0;
        public static bool bPrintRequest = false;
        public static bool bTestRequest = false;
        public static string sStatus; //to report status (system test)
        static int iStatusCheckCount = 5;//wait...
        public static int iOverallStatus = 0;
        public static bool bForceCheck = false;

        public static int iMsgBoxTimer = 0;
        public static int iScreenTimer = 0;

        public static int iMonitoringLevel = 0;

        public static int i485Monitor = 0;
        public static int iPrintTimer = 0;
        public static int iPrinterWait = 5;

        public static int iLoggingSFC = 0;
        public static int iLoggingPIC = 0;
        public static int iLoggingPTR = 0;
        public static int iTempLoggingSFC;
        public static int iTempLoggingPIC;
        public static int iTempLoggingPTR;

        public static DateTime dtLastCheckup;
        public static TimeSpan tsDuration;
        public static TimeSpan tsLimit = TimeSpan.FromSeconds(9);
        public static string sHealthCheck;//from settings.txt
        public static DateTime dtServerRequest;
        public static TimeSpan tsServerWait;
        public static TimeSpan tsServerLimit = TimeSpan.FromSeconds(9);

        public static int iPicCount = 1;
        public static int iPumpCount = 16;
        public static int iMaxCash;
        public static int iMaxBills;
        public static string sGrade1;
        public static string sGrade2;
        public static string sGrade3;
        public static string sGrade4;
        public static string sGrade5;
        public static string sGrade6;

        public static bool bOK = false;
        public static bool bOverall = false;

        static TimerCallback timerDelegate = new TimerCallback(CheckStatus);
        static System.Threading.Timer stateTimer = new System.Threading.Timer(timerDelegate, null, 1000, 1000);
        
        public struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;

            public void FromDateTime(string sTime)
            {
                wYear = Convert.ToUInt16("20" + sTime.Substring(0, 2));
                wMonth = Convert.ToUInt16(sTime.Substring(2, 2));
                wDay = Convert.ToUInt16(sTime.Substring(4, 2));
                wHour = Convert.ToUInt16(sTime.Substring(6, 2));
                wMinute = Convert.ToUInt16(sTime.Substring(8, 2));
                wSecond = Convert.ToUInt16(sTime.Substring(10, 2));
            }
        }
        public static void ChangeDateTime(string sNewTime)
        {
            SYSTEMTIME st = new SYSTEMTIME();

            st.FromDateTime(sNewTime);
            //Call Win32 API to set time
            SetLocalTime(ref st);
        }

        public static void StartUp()
        {
            FileAccess.ReadSettings();

            Display.Init();

            ProcessNextScreen();

            Display.ShowMessageBox("Please wait while loading...", iWait);//PD - rev 19
        }

        public static string Format(int iNum, int iChars)
        {
            int iLimit = 10;
            string sReturn = Convert.ToString(iNum);

            while (iChars > 1)
            {
                if (iNum < iLimit)
                {
                    sReturn = "0" + sReturn;
                }
                iLimit = iLimit * 10;
                iChars--;
            }
            return sReturn;
        }

        public static void UpdateOkStatus()
        {
            int i;
            bOK = true;

            Debug.WriteLine("CENCOM UPDATE OK");

            if (SFC.iStatus == 0 || RS485.iStatus == 0 || Printer.iStatus == 0)
            {
                bOK = false;
            }

            Debug.WriteLine(bOK);

            UpdateOverallStatus();
        }

        public static void UpdateOverallStatus()
        {
            int i;
            bOverall = true;

            Debug.WriteLine("UPDATE OVERALL ");

            if (bOK == false)
            {
                bOverall = false;
            }
            if (bOverall == true)
            {
                for (i = 1; i <= CenCom.iPicCount; i++)
                {
                    if (!PIC_MGR.PICs[i - 1].bOK)
                    {
                        bOverall = false;
                        break;
                    }
                }
            }

            if (Display.iView == 5)
            {
                Display.UpdateButtonList();
            }
            if (bOverall)
            {
                Display.screen1.SetButtonColor(Display.screen1.btStatus, Color.White);
            }
            else
            {
                Display.screen1.SetButtonColor(Display.screen1.btStatus, Color.Red);
            }
        }

        public static void CheckStatus(Object stateInfo)
        {
            if (iMsgBoxTimer > 0)
            {
                iMsgBoxTimer--;
                Debug.WriteLine("MSG BOX TIMER:  ->  " + iMsgBoxTimer);


                if (iMsgBoxTimer == 0)
                {
                    if (bStartup)
                    {
                        dtLastCheckup = DateTime.Now;

                        RS485.Init();
                        SFC.Init();

                        PIC_MGR.Init();
                        PUMP_MGR.Init();

                        for (int i = 1; i <= CenCom.iPicCount; i++)
                        {
                            DB.CheckForAvailableCassette(i);
                        }
                        
                        //Web.Init();
                        //Web.SendStatus();

                        Printer.Init();

                        bStartup = false;

                        Display.HideMessageBox();
                    }
                    else
                    {
                        Display.HideMessageBox();
                    }
                }
            }
            else if (iScreenTimer > 0)
            {
                iScreenTimer--;
                Debug.WriteLine("SCREEN TIMER:  ->  " + iScreenTimer);
                if (iScreenTimer == 0)
                {
                    //if (Display.iCurrentScreen == 21 || (Display.iCurrentScreen == 0 && Display.iScreenMode == 1) || Display.iCurrentScreen == 8)//added screen 8 to time out receipt request
                    //{
                    //    CenCom.bTestMode = false;//otherwise won't respond to goto screen requests from SSC
                    //    ProcessNextScreen();
                    //}
                }
            }

            if (bStartup)
            {
                //don't check status until system is ready...
            }
            else
            {
                if (true)//update SFC status while monitoring in case goes offline (overallstatus won't be update if bmonitor = true)
                {
                    //SFC.CheckStatus();
                    //PIC_MGR.CheckStatus();

                    if (iStatusCheckCount == 0)
                    {
                        if (Printer.bPrinting == false)//if screen = 21... should not need if not in test mode
                        {
                            iStatusCheckCount++;//Win7 placed here to avoid this not happening if failure on opening a port

                            //tsDuration = DateTime.Now - dtLastCheckup;
                            //Debug.WriteLine("TIME: " + tsDuration);

                            //if (tsDuration > tsLimit)
                            //{
                            //    CenCom.dtLastCheckup = DateTime.Now;
                            //    CenCom.tsLimit = TimeSpan.FromMinutes(Convert.ToInt32(CenCom.sHealthCheck));

                            //    Debug.WriteLine("SEND STATUS - TIMER");
                            //    Web.SendStatus();
                            //}
                            //else if (Web.bWaitingForResponse && Web.iStatus == 1)
                            //{
                            //    tsServerWait = DateTime.Now - dtServerRequest;
                            //    Debug.WriteLine("SERVER WAIT: " + tsServerWait);

                            //    if (tsServerWait > tsServerLimit)
                            //    {
                            //        Web.iStatus = 0;
                            //    }
                            //}

                            RS485.bWaitingForResponse = true;
                            SFC.bWaitingForResponse = true;//removed from SFC.checkstatus due to problems when monitoring SFC

                            if (Printer.iType == 3)
                            {
                                //Printer.CheckStatusUSB();
                            }
                            else
                            {
                                Printer.CheckStatus();//PD - 13.1
                            }
                        }
                    }
                    else if (iStatusCheckCount == 1)
                    {
                        iStatusCheckCount++;
                    }
                    else if (iStatusCheckCount == 2)
                    {
                        iStatusCheckCount++;
                        bForceCheck = false;

                        if (SFC.bWaitingForResponse == true && SFC.iStatus > 0)
                        {
                            SFC.iStatus = 0;
                            PUMP_MGR.bLock = false;
                            UpdateOkStatus();
                            Debug.WriteLine("SFC Offline");

                            //Debug.WriteLine("SEND STATUS - SFC OFFLINE");
                            //if (Web.iStatus == 1) { Web.SendStatus(); }
                        }
                        if (RS485.bWaitingForResponse == true && RS485.iStatus > 0)
                        {
                            RS485.iStatus = 0;
                            UpdateOkStatus();
                            Debug.WriteLine("RS485 Offline");
                        }
                        if (Printer.iType < 3)
                        {
                            if (Printer.bWaitingForResponse == true && Printer.iStatus > 0)
                            {
                                Printer.iStatus = 0;
                                UpdateOkStatus();
                                Printer.bError = false;//so know when offline vs. jam
                                Debug.WriteLine("Printer Offline");
                            }
                            else if (Printer.bWaitingForResponse == true && Printer.bError == true)
                            {
                                Printer.bError = false;//clear jam msg if go offline
                            }
                        }
                    }
                    else
                    {
                        iStatusCheckCount++;

                        if (iStatusCheckCount == 10)
                        {
                            iStatusCheckCount = 0;

                            Debug.WriteLine("overall status: " + iOverallStatus);
                            //Debug.WriteLine("web status: " + Web.iStatus);
                            Debug.WriteLine("SFC status: " + SFC.iStatus);
                            Debug.WriteLine("ptr status: " + Printer.iStatus);
                        }
                    }

                }

                if (bPrintRequest == true)//won't set until already have data to print so check after know customer wants car wash... force check when enter screen 1 for reg. receipt...so bforcecheck should be false by now
                {
                    bPrintRequest = false;

                    Debug.WriteLine("trying to print................");

                    if (Printer.iType > 2)
                    {
                        //Printer.PrintUSB(Printer.sReceipt);
                    }
                    else
                    {
                        Debug.WriteLine("PRINT-> " + Printer.sReceipt);
                        Printer.Print(Printer.sReceipt);
                    }
                }
                else if (bTestRequest == true && bForceCheck == false)
                {
                    bTestRequest = false;
                    Debug.WriteLine("trying to test................" + Printer.iStatus);

                    if (Printer.iStatus == 1)//PD- prevent printing while already printing
                    {
                        Display.ShowMessageBox("Printing.  Please wait.", 0);
                        //Printer.iStatus = 2;//don't need to set this for testing
                        Printer.iStatus = 2;//PD- rev. 16 - put back in to prevent multiple prints...
                        if (Printer.iType == 3)
                        {
                            //Printer.TestUSB();
                        }
                        else { Printer.Test(); }
                    }
                    else
                    {
                        if (Printer.iType > 1)
                        {
                            if (Printer.bError)
                            {
                                //sStatus = "Paper Out";
                                sStatus = Printer.sStatus;
                            }
                            else
                            {
                                sStatus = "Printer Offline";
                            }
                        }
                        else
                        {
                            sStatus = "Printer Offline";
                        }
                        //Display.GotoScreen(22, 0);
                        Display.ShowMessageBox(sStatus, 3);
                    }
                }
                else if (Printer.bPrinting)//make sure don't get stuck printing....
                {
                    Debug.WriteLine("PRINT TIMER          :" + iPrintTimer);
                    iPrintTimer++;

                    if (iPrintTimer > iPrinterWait)//PD - 13.1 (change to 5 from 10)
                    {
                        Printer.bPrinting = false;

                        if (bTestMode)
                        {
                            Printer.iStatus = 1;//PD- rev 16
                            Display.HideMessageBox();
                        }
                        else
                        {
                            Printer.iStatus = 1;//PD-just set to 1... let checkstatus change to offline...                           
                        }
                    }
                }

                if (iStatusRequest > 0 && bForceCheck == false)
                {
                    sStatus = "";

                    if (iStatusRequest == 1)
                    {
                        //if (Web.iStatus == 0) { sStatus = sStatus + "Server Offline\n"; }
                        if (SFC.iStatus == 0) { sStatus = sStatus + "SFC Offline\n"; }
                        if (RS485.iStatus == 0) { sStatus = sStatus + "PIC Offline\n"; }
                        if (Printer.iStatus == 0) { sStatus = sStatus + "Printer Offline\n"; }

                        if (sStatus == "") { sStatus = "System OK"; }

                    }
                    else if (iStatusRequest <= iPicCount + 1)
                    {
                        //if (bAlarmOn) { sStatus = sStatus + "Alarm On\n"; }
                        //if (bMainDoorOpen) { sStatus = sStatus + "Door Open\n"; }
                        //if (bVaultDoorOpen) { sStatus = sStatus + "Vault Door Open\n"; }

                        //if (CardReader.iStatus == 0) { sStatus = sStatus + "Card Reader Offline\n"; }

                        if (PIC_MGR.PICs[iStatusRequest-2].iStatus_CAS == 0) { sStatus = sStatus + "Cash Acceptor Offline\n"; }
                        else if (PIC_MGR.PICs[iStatusRequest - 2].iStatus_CAS == 11) { sStatus = sStatus + "Bill Jammed\n"; }
                        else if (PIC_MGR.PICs[iStatusRequest - 2].iStatus_CAS == 12) { sStatus = sStatus + "Cassette Full\n"; }
                        else if (PIC_MGR.PICs[iStatusRequest - 2].iStatus_CAS == 13) { sStatus = sStatus + "Cassette Removed\n"; }

                        if (PIC_MGR.PICs[iStatusRequest - 2].iStatus_PTR == 0) { sStatus = sStatus + "Printer Offline\n"; }

                        if (PIC_MGR.PICs[iStatusRequest - 2].iStatus_EPP == 0) { sStatus = sStatus + "EPP Offline\n"; }

                        if (sStatus == "") { sStatus = "System OK"; }
                    }

                    iStatusRequest = 0;

                    Display.ShowMessageBox(sStatus, 3);
                }

            }
        }

        public static void ImmediateCheck()
        {
            if (iStatusCheckCount > 1)//only if didn't just check..
            {
                bForceCheck = true;
                iStatusCheckCount = 0;
            }
        }

        public static void Beep(int iTimes)
        {
            //iBeep = iTimes;
        }

        public static void ProcessNextScreen()
        {
            Debug.WriteLine("PROCESS NEXT SCREEN");

            Display.GotoScreen(1, 0);
        }

    }

    public static class RS485
    {
        public static int iPortNum = 0;

        static SerialPort Port485 = new SerialPort();

        public static bool bWaitingForResponse = false;
        public static int iStatus = 0;

        static string sReceive = "";
        static string sTransmit = "";
        static string sPreviousReceive = "";
        static string sPreviousTransmit = "";
        static string sPreviousTransmit2 = "";//MYPIC

        //maintain through multiple data received events
        //static int iESC = 0;
        static bool bReceive = false;
        static int iCheck = 0;
        //static int iCheckVal1, iCheckVal2;

        public static string sDataToSend;
        public static int iMsgSeq;

        static int iPicNum = 0;

        public static void Init()
        {
            Port485.BaudRate = 9600;
            Port485.StopBits = StopBits.One;
            Port485.Parity = Parity.None;
            Port485.DataBits = 8;
            Port485.PortName = "COM" + iPortNum;
            Port485.Handshake = Handshake.None;
            //Port485.ReceivedBytesThreshold = 1;
            Port485.ReadTimeout = 3000;//may need to change...
            //Port485.RtsEnable = true;//pd- need for advantech rs485

            Port485.DataReceived += new SerialDataReceivedEventHandler(Port485_DataReceived);

            try
            {
                Port485.Open();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                Debug.WriteLine(ex.Message);
            }

            CenCom.bStartup = false;
        }

        static void Port485_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int iBytesToRead = Port485.BytesToRead;
            byte[] comBuffer = new byte[iBytesToRead];
            byte bCurrentByte;
            int i;

            Port485.Read(comBuffer, 0, iBytesToRead);

            for (i = 0; i < iBytesToRead; i++)
            {
                bCurrentByte = comBuffer[i];

                //if (bCurrentByte == 0xF0 && iESC == 0)
                //{
                //    iESC = 1;
                //} 

                if (bCurrentByte >= 0xB1 && bCurrentByte <= (0xB0 + PIC_MGR.PICs.Count) && iCheck == 0)
                {
                    Debug.WriteLine("bReceive is ON!");
                    
                    iPicNum = bCurrentByte - 0xB0;
                    bReceive = true;
                    iCheck = 0;
                    sReceive = "";

                    CenCom.i485Monitor = 0;

                    if (iStatus < 1)//happens only if was offline
                    {
                        iStatus = 1;
                        CenCom.UpdateOkStatus();
                        Debug.WriteLine("RS485 Online");

                        bWaitingForResponse = false;//pd- don't trigger block below if just switched to online (send cas now, not cancel)
                    }
                    else if (bWaitingForResponse == true)//happens each status check....
                    {
                        bWaitingForResponse = false;
                        Debug.WriteLine("RS485 WaitingForResponse Off");
                    }
                }

                if (bReceive == true)
                {
                    sReceive = sReceive + Convert.ToString((char)bCurrentByte);

                    if (iCheck > 0)
                    {
                        iCheck++;
                        if (iCheck > 2)
                        {
                            iCheck = 0;
                            bReceive = false;

                            if (CenCom.iLoggingPIC == 1)
                            {
                                LogMessage();
                            }

                            //if (ValidateMessage())
                            if (true)
                            {
                                PIC_MGR.PICs[iPicNum - 1].ProcessMessage(sReceive);
                                //MyProcessMessage();
                                //CenCom.MyUpdateStatus();
                                //if (iTransmitKey < 0xFF) { MyTransmitKey(); }

                                //sPreviousTransmit2 = sTransmit;
                                //MySendResponse();
                                //sTransmit = "";
                            }
                            else
                            {
                                Debug.WriteLine("CRC FAILURE!!!");
                                //MySendError(1, "");
                                //MySendResponse();
                            }

                            //Debug.WriteLine("UNLOCK PIC LOCK!!!");
                            //PIC_MGR.bLock = false;
                        }
                    }
                    else if (bCurrentByte == 0xF3)
                    {
                        iCheck = 1;
                    }
                }
                else
                {
                    //IGNORE BYTE
                }
            }
        }

        static bool ValidateMessage()
        {
            string sMyCRC, sSentCRC;

            Debug.WriteLine("VALIDATE MESSAGE");

            sSentCRC = sReceive.Substring(sReceive.Length - 2, 2);
            sMyCRC = Crc.ComputeChecksum2(sReceive.Substring(0, sReceive.Length - 2));

            Debug.Write("My CRC = ");
            foreach (char c in sMyCRC)
            {
                Debug.Write(String.Format("{0:X2}", (byte)c) + " ");
            }

            if (sSentCRC == sMyCRC)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //public static string MyGotoMain()
        //{
        //    return Convert.ToString((char)0x11) + Convert.ToString((char)0x0A) + Convert.ToString((char)0);
        //}

        public static void MyReceiveKey(int iKey)
        {
            //iTransmitKey = iKey;
        }

        //public static void MyTransmitKey()
        //{
        //    sTransmit = sTransmit + Convert.ToString((char)0x05) + Convert.ToString((char)iTransmitKey);
        //    iTransmitKey = 0xFF;
        //}

        public static void MyReceiveInput(string sInput)
        {
            //sTransmit = sTransmit + Convert.ToString((char)0x03) + Convert.ToString((char)sInput.Length) + sInput;
        }

        public static void MyReceiveBillValue()
        {
            //sTransmit = sTransmit + Convert.ToString((char)0x09) + Convert.ToString((char)CashAcceptor.iBillVal);
        }

        //public static void MyReceiveCardData(string sTrack1, string sTrack2)
        //{
        //    sTransmit = sTransmit + Convert.ToString((char)0x07) + Convert.ToString((char)sTrack1.Length) + sTrack1 + Convert.ToString((char)sTrack2.Length) + sTrack2;
        //}

        public static void MyReceiveConfig()
        {
            //sTransmit = sTransmit + Convert.ToString((char)0x10) + CenCom.iType + CenCom.iProtocol + EPP.iType + CardReader.iType + CashAcceptor.iType + Printer.iType + FileAccess.sVersion;
        }

        public static void MyReceiveStatus(int iComponent, int iStatus)
        {
            //if (iComponent == 4 && iStatus < 0)
            //{
            //    iStatus = iStatus + 19;
            //}
            //else if (iComponent == 4 && iStatus == 4)
            //{
            //    //if (CenCom.iProtocol == 2)
            //    //{
            //        CenCom.MySetString(6, "$" + CashAcceptor.iBillVal + ".00");
            //    //}
            //    MySendBillValue();
            //}
            //sTransmit = sTransmit + Convert.ToString((char)(0x10 + iComponent)) + Convert.ToString((char)iStatus);
        }

        public static void MyReceiveArmor()
        {
            //sTransmit = sTransmit + Convert.ToString((char)0x0A);
        }

        public static void MyReceiveError(int iCode, string sError)
        {
            //sTransmit = sTransmit + Convert.ToString((char)0x0E) + Convert.ToString((char)iCode) + Convert.ToString((char)sError.Length) + sError;
        }

        public static void MyStartTransaction(int iPump, int iType)
        {
            Debug.WriteLine("Start Transaction - Pump = " + iPump + "Type = " + iType);

            //CenCom.bTranType = true;

            //sTransmit = sTransmit + Convert.ToString((char)0x01) + Convert.ToString((char)iPump) + Convert.ToString((char)iType);
        }

        public static void MySendRequest(int iPIC, string sParam)
        {
            string sMsg = "";
            int i;
            byte[] bbTransmit;

            Debug.Write("SEND RESPONSE: ");

            sMsg = Convert.ToString((char)(0xA0 + iPIC)) + sParam + Convert.ToString((char)0xF3);
            sMsg = sMsg + Crc.ComputeChecksum2(sMsg);
            sTransmit = sMsg;

            bbTransmit = new byte[sTransmit.Length];

            i = 0;
            foreach (char c in sTransmit)
            {
                Debug.Write(String.Format("{0:X2}", (byte)c) + " ");
                bbTransmit[i] = (byte)c;
                i++;
            }
            Debug.WriteLine(">>>");

            //Port485.Write(bbTransmit, 0, bbTransmit.Length);

            try
            {
                if (Port485.IsOpen == false)
                {
                    Port485.Open();

                    Debug.WriteLine("485 Port not opened... Trying to open port...");
                }
                else
                {
                    Port485.DiscardOutBuffer();
                    Port485.Write(bbTransmit, 0, bbTransmit.Length);
                    if (CenCom.iLoggingPIC == 1)
                    {
                        WriteLogData_TX(Environment.NewLine + "TX> " + DateTime.Now.ToString("HH:mm:ss:ffff") + " -- ");
                        for (i = 0; i < bbTransmit.Length; i++)
                        {
                            WriteLogData_TX(String.Format("{0:X2}", bbTransmit[i]) + " ");
                        }
                    }
                 }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("485 ERROR: ");
                Debug.WriteLine(ex.Message);
            }
        }



        static void LogMessage()
        {
            if (sPreviousReceive != sReceive)
            {
                WriteLogData_RX(Environment.NewLine + "RX> " + DateTime.Now.ToString("HH:mm:ss:ffff") + " -- ");
                foreach (char c in sReceive)
                {
                    WriteLogData_RX(String.Format("{0:X2}", (byte)c) + " ");
                }
            }
            else
            {
                WriteLogData_RX("R");
            }

            sPreviousReceive = sReceive;
        }

        static void LogResponse()
        {
            if (sPreviousTransmit != sTransmit)
            {
                WriteLogData_RX(Environment.NewLine + "RX> " + DateTime.Now.ToString("HH:mm:ss:ffff") + " -- ");
                foreach (char c in sTransmit)
                {
                    WriteLogData_RX(String.Format("{0:X2}", (byte)c) + " ");
                }
            }
            else
            {
                WriteLogData_RX("R");
            }

            sPreviousTransmit = sTransmit;
        }

        //public static void SendConfig()
        //{
        //    string sSend;
        //    sSend = GetCAS() + GetCRS() + GetPRS() + GetGNS() + "DSS001AKBS001A";//PD - rev17b3
        //    sSend = GetGNS() + "GNR000" + GetCRS() + GetCAS() + "DSS001AKBS001A" + GetPRS();//PD - rev19
        //    //Debug.WriteLine(sSend);
        //    SendData(sSend);
        //}

        //public static void PumpGas(string sData)
        //{
        //    string sSend = "";

        //    if (CenCom.iProtocol == 1)
        //    {
        //        sSend = "KBF001a" + "KBN00" + (sData.Length + 1) + "B" + sData;//length should never exceed 9 digits

        //        Debug.WriteLine("Auth Pump: " + sSend);
        //        SendData(sSend);
        //    }
        //    else
        //    {
        //        RS485.MyStartTransaction(Convert.ToInt16(CenCom.sPump), 1);
        //    }
        //}

        //public static void GetReceipt(string sData)
        //{
        //    string sSend = "";

        //    sSend = "KBF001b" + "KBN00" + (sData.Length + 1) + "B" + sData;//length should never exceed 9 digits

        //    Debug.WriteLine("Get Receipt: " + sSend);
        //    SendData(sSend);
        //}

        //public static void GetReceiptPart1()
        //{
        //    string sSend = "";

        //    if (CenCom.iProtocol == 1)
        //    {
        //        sSend = "KBF001b";

        //        Debug.WriteLine("Get Receipt - Part 1: " + sSend);
        //        SendData(sSend);
        //    }
        //    else
        //    {
        //        RS485.MyStartTransaction(Convert.ToInt16(CenCom.sPump), 2);
        //    }
        //}

        //public static void GetReceiptPart2(string sData)
        //{
        //    string sSend = "";

        //    sSend = "KBN00" + (sData.Length + 1) + "B" + sData;//length should never exceed 9 digits

        //    Debug.WriteLine("Get Receipt - Part 2: " + sSend);
        //    SendData(sSend);
        //}

        //public static void SendInput(string sData)
        //{
        //    string sSend = "";

        //    if (CenCom.iProtocol == 1)
        //    {
        //        //Debug.WriteLine("Trying to reserve pump " + CenCom.sPump);
        //        //SendData("DPTI1Nn01KBF001aKBN002B" + CenCom.sPump);
        //        sSend = "KBN00" + (sData.Length + 1) + "B" + sData;//length should never exceed 9 digits

        //        Debug.WriteLine("Sending Input: " + sSend);
        //        SendData(sSend);
        //    }
        //    else
        //    {
        //        MySendInput(sData);
        //    }
        //}

        //public static void SendCardData()
        //{
        //    string sLengthCR1;
        //    string sLengthCR2;

        //    //CardReader.sTrack1 = "";
        //    //CardReader.sTrack2 = ";65000950000000005=0012?0";

        //    if (CenCom.iProtocol == 1)
        //    {
        //        sLengthCR1 = FormatNum(CardReader.sTrack1.Length, 3);
        //        sLengthCR2 = FormatNum(CardReader.sTrack2.Length, 3);

        //        Debug.WriteLine("SEND CARD DATA");
        //        Debug.WriteLine("CR1= " + CardReader.sTrack1);
        //        Debug.WriteLine("CR2= " + CardReader.sTrack2);
        //        Debug.WriteLine("PAN= " + CardReader.sPAN);

        //        SendData("CR1" + sLengthCR1 + CardReader.sTrack1 + "CR2" + sLengthCR2 + CardReader.sTrack2 + "CRS001E");
        //    }
        //    else
        //    {
        //        MySendCardData(CardReader.sTrack1, CardReader.sTrack2);
        //    }
        //}

        //public static string GetCardData()
        //{
        //    string sLengthCR1;
        //    string sLengthCR2;

        //    //CardReader.sTrack1 = "";
        //    //CardReader.sTrack2 = ";65000950000000005=0012?0";

        //    sLengthCR1 = FormatNum(CardReader.sTrack1.Length, 3);
        //    sLengthCR2 = FormatNum(CardReader.sTrack2.Length, 3);

        //    Debug.WriteLine("SEND CARD DATA");
        //    Debug.WriteLine("CR1= " + CardReader.sTrack1);
        //    Debug.WriteLine("CR2= " + CardReader.sTrack2);
        //    Debug.WriteLine("PAN= " + CardReader.sPAN);

        //    return ("CR1" + sLengthCR1 + CardReader.sTrack1 + "CR2" + sLengthCR2 + CardReader.sTrack2 + "CRS001E");
        //}

        //public static void SendPINData()
        //{
        //    string sLengthKBP;

        //    if (CenCom.iProtocol == 1)
        //    {
        //        sLengthKBP = FormatNum(EPP.EPB.Length + EPP.KSN.Length + 1, 3);

        //        Debug.WriteLine("SEND PIN DATA");
        //        Debug.WriteLine("EPB= " + EPP.EPB);
        //        Debug.WriteLine("KSN= " + EPP.KSN);

        //        SendData("KBP" + sLengthKBP + "B" + EPP.EPB + EPP.KSN);
        //    }
        //    else
        //    {
        //        MySendInput(EPP.EPB + EPP.KSN);
        //    }
        //}

        //public static void RequestArmoredReport()
        //{
        //    if (CenCom.iProtocol == 1)
        //    {
        //        SendData("GNI000GNA000");
        //    }
        //    else
        //    {
        //        MySendArmor();
        //    }
        //}

        //static void SendData(string sString)
        //{
        //    if (iStatus == 1)
        //    {
        //        sDataToSend = sDataToSend + sString;
        //    }
        //    else
        //    {
        //        Debug.WriteLine("RS485 offline, string not sent: " + sString);
        //    }

        //}

        static void WriteLogData_RX(string sData)
        {
            File.AppendAllText("LOGS\\RS_" + String.Format("{0:D2}", DateTime.Today.Day) + ".txt", sData);
        }

        public static void WriteLogData_TX(string sData)
        {
            File.AppendAllText("LOGS\\RS_" + String.Format("{0:D2}", DateTime.Today.Day) + ".txt", sData);

        }
    }

    public static class SFC
    {
        public static int iPortNum = 0;
        static int count = 0;

        public static int iBytesRead = 0;
        public static int iLength = 0;
        public static int iCmd = 0;
        public static int iResult = 0;

        public static SerialPort PortSFC = new SerialPort();

        public static bool bWaitingForResponse = false;
        public static int iStatus = 0;
        public static int iPreviousStatus = 0;
        public static bool bMonitor = false;
        public static int iStep = 0;
        public static string sKioskTransId = "";
        public static string sServerTransId = "";
        public static string sPower = "";
        public static Int32 iPower = 0;
        public static bool bEsc = false;
        public static bool bReset = false;


        static int iPump = 0;
        static string sPump = "";
        static int iIndex = 0;
        static int iPumpState;

        static bool bCheck = false;
        static int iAuthRequest = 0;
        static int iCompleteRequest = 0;

        static string sGrade = "";
        static string sPrice = "";
        static string sVolume = "";
        static string sBought = "";

        public static void Init()
        {
            Debug.WriteLine("SFC INIT COM" + iPortNum);

            PortSFC.BaudRate = 4800;
            PortSFC.Parity = Parity.Odd;
            PortSFC.StopBits = StopBits.One;
            PortSFC.DataBits = 7;
            PortSFC.PortName = "COM" + iPortNum;
            PortSFC.Handshake = Handshake.None;

            PortSFC.ReadTimeout = 1000;//may need to change...

            PortSFC.DataReceived += new SerialDataReceivedEventHandler(PortSFC_DataReceived);

            try
            {
                PortSFC.Open();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                Debug.WriteLine(ex.Message);
            }
        }

        public static byte Checksum(byte[] byPassbuffer)
        {
            int i;
            int iSum = 0;

            for (i = 1; i < byPassbuffer.Length - 1; i++)
            {
                iSum = iSum ^ byPassbuffer[i];
            }

            return (byte)iSum;
        }

        private static void PortSFC_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int iBytesToRead = PortSFC.BytesToRead;
            int i;
            byte[] comBuffer = new byte[iBytesToRead];
            byte bCurrentByte;

            Debug.WriteLine("SFC Data Received");
            Debug.WriteLine("SFC bytes to read: " + iBytesToRead);

            if (CenCom.iLoggingSFC == 1)
            {
                WriteLogData_RX(Environment.NewLine + "RX> " + DateTime.Now.ToString("HH:mm:ss:ffff") + " -- ");
            }

            if (iStatus < 1)//PD - Rely on status cmd
            {
                iStatus = 1;
                CenCom.UpdateOkStatus();

            }

            if (bWaitingForResponse == true)//if get ANY response link is up
            {
                bWaitingForResponse = false;
            }

            PortSFC.Read(comBuffer, 0, iBytesToRead);

            for (i = 0; i < iBytesToRead; i++)
            {
                if (CenCom.iLoggingSFC == 1)
                {
                    WriteLogData_RX(String.Format("{0:X2}", comBuffer[i]) + " ");
                }

                if (iBytesRead == 0 && comBuffer[i] == 2)
                {
                    iBytesRead++;
                }
                else if (iBytesRead == 1)
                {
                    iBytesRead++;
                    iCmd = comBuffer[i];
                }
                else if (iBytesRead == 2 || iBytesRead == 3)
                {
                    iBytesRead++;

                    sPump = sPump + Convert.ToChar(comBuffer[i]);

                    if (iBytesRead == 4)
                    {
                        iPump = Convert.ToInt16(sPump);
                    }
                }
                else if (iCmd == 0x4B && iPump == 0 && iBytesRead >= 4 && iBytesRead < 40)
                {

                    iBytesRead++;

                    if (iBytesRead - 4 <= CenCom.iPumpCount)
                    {
                        iIndex = iBytesRead - 5;
                        PUMP_MGR.PUMPs[iIndex].GetStateResponse(comBuffer[i] - 48);
                    }

                }
                else if (iCmd == 0x41 && iBytesRead >= 4 && iBytesRead < 28)
                {

                    iBytesRead++;

                    if (iBytesRead == 5)
                    {
                        sGrade = Convert.ToString((char)comBuffer[i]);
                    }
                    else if (iBytesRead >= 7 && iBytesRead <= 14)
                    {
                        sVolume = sVolume + Convert.ToString((char)comBuffer[i]);
                    }
                    else if (iBytesRead >= 15 && iBytesRead <= 22)
                    {
                        sBought = sBought + Convert.ToString((char)comBuffer[i]);
                    }
                    else if (iBytesRead >= 24 && iBytesRead <= 27)
                    {
                        sPrice = sPrice + Convert.ToString((char)comBuffer[i]);
                    }
                    else if (iBytesRead == 28)
                    {
                        PUMP_MGR.PUMPs[iPump - 1].GetReportResponse(sGrade, sVolume, sBought, sPrice);

                        sGrade = "";
                        sPrice = "";
                        sVolume = "";
                        sBought = "";
                    }

                }
                else if (comBuffer[i] == 3)
                {
                    bCheck = true;
                }
                else if (bCheck)
                {
                    Debug.WriteLine("SFC - CHECKSUM");

                    //if (true)
                    //{
                    //    //if (iIndex == 1 && iPumpStates[iPump - 1] == 0x33)
                    //    //{
                    //    //    GetSalesReport(iPump);
                    //    //}
                    //}

                    bCheck = false;
                    iBytesRead = 0;
                    iCmd = 0;
                    iPump = 0;
                    sPump = "";
                    iIndex = 0;
                    iAuthRequest = 0;
                    iCompleteRequest = 0;

                    PUMP_MGR.bLock = false;
                }
                else if (iAuthRequest > 0)
                {
                    if (comBuffer[i] == 6)
                    {
                        PUMP_MGR.PUMPs[iAuthRequest - 1].AuthorizationResponse(true);
                    }
                    else
                    {

                        PUMP_MGR.PUMPs[iAuthRequest - 1].AuthorizationResponse(false);

                    }
                    iAuthRequest = 0;

                }
                else if (iCompleteRequest > 0)
                {
                    if (comBuffer[i] == 6)
                    {

                        PUMP_MGR.PUMPs[iCompleteRequest - 1].CompleteResponse(true);
                    }
                    else
                    {

                        PUMP_MGR.PUMPs[iCompleteRequest - 1].CompleteResponse(false);

                    }
                    iCompleteRequest = 0;

                    PUMP_MGR.bLock = false;
                }
                else
                {
                    bEsc = false;
                    iBytesRead = 0;
                    iLength = 0;
                    iCmd = 0;
                    iPump = 0;
                    sPump = "";
                    bCheck = false;
                    iIndex = 0;
                    iAuthRequest = 0;
                    iCompleteRequest = 0;
                }
            }
        }

        public static void ShowStatus()
        {
            //if (CenCom.bTestMode == false)
            //{
            //    if (iStatus == 0)
            //    {
            //        Display.screen20.SetText1("SFC Offline");
            //    }
            //    else if (iStatus == 1)
            //    {
            //        Display.screen20.SetText1("Please Connect Vehicle");
            //    }
            //    Display.GotoScreen(20, 0);
            //}
        }

        //public static void CheckStatus()
        //{
        //    //if (iState == 0) {
        //        GetState(0);
        //    //}
        //}

        public static void GetState(int iPassNum)
        {
            int i;
            byte[] bSend;
            string sNum;

            Debug.WriteLine("GET STATUS PUMP " + iPassNum);

            sNum = CenCom.Format(iPassNum, 2);

            bSend = new byte[6];
            bSend[0] = 2;
            bSend[1] = 0x4B;
            bSend[2] = (byte)sNum[0];
            bSend[3] = (byte)sNum[1];
            bSend[4] = 3;
            bSend[5] = Checksum(bSend);

            //Debug.WriteLine("CHECKSUM = " + bSend[5]);

            try
            {
                if (PortSFC.IsOpen == false)
                {
                    PortSFC.Open();

                    Debug.WriteLine("SFC Port not opened... Trying to open port...");
                }
                else
                {
                    PortSFC.DiscardOutBuffer();
                    PortSFC.Write(bSend, 0, bSend.Length);
                    if (CenCom.iLoggingSFC == 1)
                    {
                        WriteLogData_TX(Environment.NewLine + "TX> " + DateTime.Now.ToString("HH:mm:ss:ffff") + " -- ");
                        for (i = 0; i < bSend.Length; i++)
                        {
                            WriteLogData_TX(String.Format("{0:X2}", bSend[i]) + " ");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SFC ERROR: ");
                Debug.WriteLine(ex.Message);
            }
        }

        public static void Authorize(int iPassNum, int iPassAmount)
        {
            int i;
            byte[] bSend;
            string sNum;
            string sAmount;

            Debug.WriteLine("AUTHORIZE PUMP");

            sNum = CenCom.Format(iPassNum, 2);
            sAmount = CenCom.Format(iPassAmount, 3);

            bSend = new byte[17];
            bSend[0] = 2;
            bSend[1] = 0x50;
            bSend[2] = (byte)sNum[0];
            bSend[3] = (byte)sNum[1];
            bSend[4] = 0x30;
            bSend[5] = 0x31;
            bSend[6] = 0x30;
            bSend[7] = 0x30;
            bSend[8] = 0x30;
            bSend[9] = (byte)sAmount[0];
            bSend[10] = (byte)sAmount[1];
            bSend[11] = (byte)sAmount[2];
            bSend[12] = 0x30;
            bSend[13] = 0x30;
            bSend[14] = 0x30;
            bSend[15] = 3;
            bSend[16] = Checksum(bSend);

            Debug.WriteLine("AUTH CHECKSUM = " + bSend[16]);

            try
            {
                if (PortSFC.IsOpen == false)
                {
                    PortSFC.Open();

                    Debug.WriteLine("SFC Port not opened... Trying to open port...");
                }
                else
                {
                    PortSFC.DiscardOutBuffer();
                    PortSFC.Write(bSend, 0, bSend.Length);
                    if (CenCom.iLoggingSFC == 1)
                    {
                        WriteLogData_TX(Environment.NewLine + "TX> " + DateTime.Now.ToString("HH:mm:ss:ffff") + " -- ");
                        for (i = 0; i < bSend.Length; i++)
                        {
                            WriteLogData_TX(String.Format("{0:X2}", bSend[i]) + " ");
                        }
                    }

                    iAuthRequest = iPassNum;

                }
                //return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SFC ERROR: ");
                Debug.WriteLine(ex.Message);
                //return false;
            }
        }

        public static void GetSalesReport(int iPassNum)
        {
            int i;
            byte[] bSend;
            string sNum = "";

            sNum = CenCom.Format(iPassNum, 2);

            Debug.WriteLine("GET SALES REPORT FOR PUMP" + sNum);

            bSend = new byte[7];
            bSend[0] = 2;
            bSend[1] = 0x41;
            bSend[2] = (byte)sNum[0];
            bSend[3] = (byte)sNum[1];
            bSend[4] = 0x30;
            bSend[5] = 3;
            bSend[6] = Checksum(bSend);

            Debug.WriteLine("SALES REPORT CHECKSUM = " + bSend[6]);

            try
            {
                if (PortSFC.IsOpen == false)
                {
                    PortSFC.Open();

                    Debug.WriteLine("SFC Port not opened... Trying to open port...");
                }
                else
                {
                    PortSFC.DiscardOutBuffer();
                    PortSFC.Write(bSend, 0, bSend.Length);
                    if (CenCom.iLoggingSFC == 1)
                    {
                        WriteLogData_TX(Environment.NewLine + "TX> " + DateTime.Now.ToString("HH:mm:ss:ffff") + " -- ");
                        for (i = 0; i < bSend.Length; i++)
                        {
                            WriteLogData_TX(String.Format("{0:X2}", bSend[i]) + " ");
                        }
                    }

                    //iState = 2;
                }
                //return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SFC ERROR: ");
                Debug.WriteLine(ex.Message);
                //return false;
            }
        }

        public static void CompleteSale(int iPassNum)
        {
            int i;
            byte[] bSend;
            string sNum = "";

            sNum = CenCom.Format(iPassNum, 2);

            Debug.WriteLine("COMPLETE SALE ON PUMP" + sNum);

            bSend = new byte[7];
            bSend[0] = 2;
            bSend[1] = 0x52;
            bSend[2] = (byte)sNum[0];
            bSend[3] = (byte)sNum[1];
            bSend[4] = 3;
            bSend[5] = Checksum(bSend);

            Debug.WriteLine("COMPLETE SALE CHECKSUM = " + bSend[6]);

            try
            {
                if (PortSFC.IsOpen == false)
                {
                    PortSFC.Open();

                    Debug.WriteLine("SFC Port not opened... Trying to open port...");
                }
                else
                {
                    PortSFC.DiscardOutBuffer();
                    PortSFC.Write(bSend, 0, bSend.Length);
                    if (CenCom.iLoggingSFC == 1)
                    {
                        WriteLogData_TX(Environment.NewLine + "TX> " + DateTime.Now.ToString("HH:mm:ss:ffff") + " -- ");
                        for (i = 0; i < bSend.Length; i++)
                        {
                            WriteLogData_TX(String.Format("{0:X2}", bSend[i]) + " ");
                        }
                    }

                    iCompleteRequest = iPassNum;
                   
                    //iState = 2;
                }
                //return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SFC ERROR: ");
                Debug.WriteLine(ex.Message);
                //return false;
            }
        }

        static void WriteLogData_RX(string sData)
        {
            File.AppendAllText("LOGS\\EV_RX_" + String.Format("{0:D2}", DateTime.Today.Day) + ".txt", sData);
        }

        static void WriteLogData_TX(string sData)
        {
            File.AppendAllText("LOGS\\EV_TX_" + String.Format("{0:D2}", DateTime.Today.Day) + ".txt", sData);
        }
    }

    public static class Display
    {
        public static int iCurrentScreen;
        public static int iScreenMode;
        public static int iMaxInput;
        public static int iMinInput;

        public static bool bMsgBoxShowing = false;

        public static Form1 screen1 = new Form1();
        public static MsgBox msgBox = new MsgBox();

        public static int iType = 1;

        public static int iChoice = 0;

        public static int iButtonSelected = 0;
        public static int iView = 0;
        public static List<Color> lColors = new List<Color>();

        public static void Init()
        {
            iCurrentScreen = 0;
            iScreenMode = 0;
            iMaxInput = 2;

            for (int i = 0; i < 10; i++)
            {
                lColors.Add(Color.White);
            }

            //if (iType == 2)
            //{
            //    myParent.Width = 804;
            //    myParent.Height = 604;
            //}
            //else
            //{
            //    myParent.Width = 804;
            //    myParent.Height = 484;
            //}

            //screen1.MdiParent = myParent;
            //msgBox.MdiParent = myParent;

            screen1.StartPosition = FormStartPosition.CenterScreen;
            msgBox.StartPosition = FormStartPosition.CenterScreen;

            ChangeView(1);
            screen1.ShowThis();
        }

        public static void ChangeView(int iNewView)
        {
            if (iView != iNewView)
            {
                screen1.SetButtonColor(screen1.btPending, Color.White);
                screen1.SetButtonColor(screen1.btCompleted, Color.White);
                screen1.SetButtonColor(screen1.btEOD, Color.White);
                screen1.SetButtonColor(screen1.btCash, Color.White);
                screen1.SetButtonColor(screen1.btConfigure, Color.White);

                if (CenCom.bOverall)
                {
                    screen1.SetButtonColor(screen1.btStatus, Color.White);
                }
                else
                {
                    screen1.SetButtonColor(screen1.btStatus, Color.Red);
                }

                screen1.SetButtonVisible(screen1.btPrint, false);
                screen1.SetButtonVisible(screen1.btGenerate, false);
                screen1.SetButtonVisible(screen1.btClear, false);
                screen1.SetButtonVisible(screen1.btLoggingPIC, false);
                screen1.SetButtonVisible(screen1.btLoggingSFC, false);
                screen1.SetButtonVisible(screen1.btDownloadLogs, false);
                screen1.SetButtonVisible(screen1.btDownloadReports, false);
                screen1.SetButtonVisible(screen1.btDownloadData, false);
                screen1.SetButtonVisible(screen1.btPageUp, false);
                screen1.SetButtonVisible(screen1.btPageDown, false);
                screen1.SetButtonVisible(screen1.btUpdate, false);

                if (iNewView == 1)
                {
                    screen1.SetButtonColor(screen1.btPending, Color.Yellow);
                    screen1.SetButtonVisible(screen1.btPrint, true);
                    screen1.SetButtonVisible(screen1.btClear, true);
                }
                else if (iNewView == 2)
                {
                    screen1.SetButtonColor(screen1.btCompleted, Color.Yellow);
                    screen1.SetButtonVisible(screen1.btPrint, true);
                    screen1.SetButtonVisible(screen1.btPageUp, true);
                }
                else if (iNewView == 3)
                {
                    screen1.SetButtonColor(screen1.btEOD, Color.Yellow);
                    screen1.SetButtonVisible(screen1.btPrint, true);
                    screen1.SetButtonVisible(screen1.btGenerate, true);
                }
                else if (iNewView == 4)
                {
                    screen1.SetButtonColor(screen1.btCash, Color.Yellow);
                    screen1.SetButtonVisible(screen1.btPrint, true);
                    screen1.SetButtonVisible(screen1.btGenerate, true);
                }
                else if (iNewView == 5)
                {
                    screen1.SetButtonColor(screen1.btStatus, Color.Yellow);
                    screen1.SetButtonVisible(screen1.btLoggingPIC, true);
                    screen1.SetButtonVisible(screen1.btLoggingSFC, true);
                    screen1.SetButtonVisible(screen1.btDownloadLogs, true);
                    screen1.SetButtonVisible(screen1.btDownloadReports, true);
                    screen1.SetButtonVisible(screen1.btDownloadData, true);
                }
                else if (iNewView == 6)
                {
                    screen1.SetButtonColor(screen1.btConfigure, Color.Yellow);
                    screen1.SetButtonVisible(screen1.btUpdate, true);
                }

                iView = iNewView;
                iButtonSelected = 0;
                UpdateButtonList();
            }
        }

        public static void SelectButton(int iIndex)
        {
            if (iView == 5)
            {
                CenCom.iStatusRequest = iIndex;
            }

            if (iButtonSelected != iIndex)
            {
                iButtonSelected = iIndex;

                screen1.SetButtonColor(screen1.button1, lColors[0]);
                screen1.SetButtonColor(screen1.button2, lColors[1]);
                screen1.SetButtonColor(screen1.button3, lColors[2]);
                screen1.SetButtonColor(screen1.button4, lColors[3]);
                screen1.SetButtonColor(screen1.button5, lColors[4]);
                screen1.SetButtonColor(screen1.button6, lColors[5]);
                screen1.SetButtonColor(screen1.button7, lColors[6]);
                screen1.SetButtonColor(screen1.button8, lColors[7]);
                screen1.SetButtonColor(screen1.button9, lColors[8]);
                screen1.SetButtonColor(screen1.button10, lColors[9]);

                if (iIndex == 1)
                {
                    screen1.SetButtonColor(screen1.button1, Color.Yellow);
                }
                else if (iIndex == 2)
                {
                    screen1.SetButtonColor(screen1.button2, Color.Yellow);
                }
                else if (iIndex == 3)
                {
                    screen1.SetButtonColor(screen1.button3, Color.Yellow);
                }
                else if (iIndex == 4)
                {
                    screen1.SetButtonColor(screen1.button4, Color.Yellow);
                }
                else if (iIndex == 5)
                {
                    screen1.SetButtonColor(screen1.button5, Color.Yellow);
                }
                else if (iIndex == 6)
                {
                    screen1.SetButtonColor(screen1.button6, Color.Yellow);
                }
                else if (iIndex == 7)
                {
                    screen1.SetButtonColor(screen1.button7, Color.Yellow);
                }
                else if (iIndex == 8)
                {
                    screen1.SetButtonColor(screen1.button8, Color.Yellow);
                }
                else if (iIndex == 9)
                {
                    screen1.SetButtonColor(screen1.button9, Color.Yellow);
                }
                else if (iIndex == 10)
                {
                    screen1.SetButtonColor(screen1.button10, Color.Yellow);
                }
            }
        }
        public static void UpdateButtonColor(int iIndex, int iColor)
        {
            if (iColor == 0)
            {
                lColors[iIndex - 1] = Color.White;
            }
            else if (iColor == 1)
            {
                lColors[iIndex - 1] = Color.Orange;
            }
            else if (iColor == 2)
            {
                lColors[iIndex - 1] = Color.Red;
            }

            if (iButtonSelected != iIndex)
            {
                if (iIndex == 1)
                {
                    screen1.SetButtonColor(screen1.button1, lColors[iIndex - 1]);
                }
                else if (iIndex == 2)
                {
                    screen1.SetButtonColor(screen1.button2, lColors[iIndex - 1]);
                }
                else if (iIndex == 3)
                {
                    screen1.SetButtonColor(screen1.button3, lColors[iIndex - 1]);
                }
                else if (iIndex == 4)
                {
                    screen1.SetButtonColor(screen1.button4, lColors[iIndex - 1]);
                }
                else if (iIndex == 5)
                {
                    screen1.SetButtonColor(screen1.button5, lColors[iIndex - 1]);
                }
                else if (iIndex == 6)
                {
                    screen1.SetButtonColor(screen1.button6, lColors[iIndex - 1]);
                }
                else if (iIndex == 7)
                {
                    screen1.SetButtonColor(screen1.button7, lColors[iIndex - 1]);
                }
                else if (iIndex == 8)
                {
                    screen1.SetButtonColor(screen1.button8, lColors[iIndex - 1]);
                }
                else if (iIndex == 9)
                {
                    screen1.SetButtonColor(screen1.button9, lColors[iIndex - 1]);
                }
                else if (iIndex == 10)
                {
                    screen1.SetButtonColor(screen1.button10, lColors[iIndex - 1]);
                }
            }
        }

        public static void GetConfig()
        {
            Debug.WriteLine("GET CONFIG");
            //Debug.WriteLine(FileAccess.sConfig.IndexOf("</PICNUM>") - FileAccess.sConfig.IndexOf("<PICNUM>") - 8);
            screen1.SetTextBoxText(screen1.tbPicNum, FileAccess.sConfig.Substring(FileAccess.sConfig.IndexOf("<PICNUM>") + 8, (FileAccess.sConfig.IndexOf("</PICNUM>") - FileAccess.sConfig.IndexOf("<PICNUM>") - 8)));
            screen1.SetTextBoxText(screen1.tbPumpNum, FileAccess.sConfig.Substring(FileAccess.sConfig.IndexOf("<PUMPNUM>") + 9, (FileAccess.sConfig.IndexOf("</PUMPNUM>") - FileAccess.sConfig.IndexOf("<PUMPNUM>") - 9)));
            screen1.SetTextBoxText(screen1.tbMaxCash, FileAccess.sConfig.Substring(FileAccess.sConfig.IndexOf("<MAXCASH>") + 9, (FileAccess.sConfig.IndexOf("</MAXCASH>") - FileAccess.sConfig.IndexOf("<MAXCASH>") - 9)));
            screen1.SetTextBoxText(screen1.tbMaxBills, FileAccess.sConfig.Substring(FileAccess.sConfig.IndexOf("<MAXBILLS>") + 10, (FileAccess.sConfig.IndexOf("</MAXBILLS>") - FileAccess.sConfig.IndexOf("<MAXBILLS>") - 10)));
            screen1.SetTextBoxText(screen1.tbGrade1, FileAccess.sConfig.Substring(FileAccess.sConfig.IndexOf("<GRADE1>") + 8, (FileAccess.sConfig.IndexOf("</GRADE1>") - FileAccess.sConfig.IndexOf("<GRADE1>") - 8)));
            screen1.SetTextBoxText(screen1.tbGrade2, FileAccess.sConfig.Substring(FileAccess.sConfig.IndexOf("<GRADE2>") + 8, (FileAccess.sConfig.IndexOf("</GRADE2>") - FileAccess.sConfig.IndexOf("<GRADE2>") - 8)));
            screen1.SetTextBoxText(screen1.tbGrade3, FileAccess.sConfig.Substring(FileAccess.sConfig.IndexOf("<GRADE3>") + 8, (FileAccess.sConfig.IndexOf("</GRADE3>") - FileAccess.sConfig.IndexOf("<GRADE3>") - 8)));
            screen1.SetTextBoxText(screen1.tbGrade4, FileAccess.sConfig.Substring(FileAccess.sConfig.IndexOf("<GRADE4>") + 8, (FileAccess.sConfig.IndexOf("</GRADE4>") - FileAccess.sConfig.IndexOf("<GRADE4>") - 8)));
            screen1.SetTextBoxText(screen1.tbGrade5, FileAccess.sConfig.Substring(FileAccess.sConfig.IndexOf("<GRADE5>") + 8, (FileAccess.sConfig.IndexOf("</GRADE5>") - FileAccess.sConfig.IndexOf("<GRADE5>") - 8)));
            screen1.SetTextBoxText(screen1.tbGrade6, FileAccess.sConfig.Substring(FileAccess.sConfig.IndexOf("<GRADE6>") + 8, (FileAccess.sConfig.IndexOf("</GRADE6>") - FileAccess.sConfig.IndexOf("<GRADE6>") - 8)));
            screen1.SetTextBoxText(screen1.tbHeader, FileAccess.sConfig.Substring(FileAccess.sConfig.IndexOf("<HEADER>") + 8, (FileAccess.sConfig.IndexOf("</HEADER>") - FileAccess.sConfig.IndexOf("<HEADER>") - 8)));
            screen1.SetTextBoxText(screen1.tbFooter, FileAccess.sConfig.Substring(FileAccess.sConfig.IndexOf("<FOOTER>") + 8, (FileAccess.sConfig.IndexOf("</FOOTER>") - FileAccess.sConfig.IndexOf("<FOOTER>") - 8)));

            if (CenCom.iPicCount != Convert.ToInt16(screen1.tbPicNum.Text) || CenCom.iPumpCount != Convert.ToInt16(screen1.tbPumpNum.Text))
            {
                screen1.SetLableText(screen1.lbWarning, "RESTART REQUIRED");
            }
            else
            {
                screen1.SetLableText(screen1.lbWarning, "");
            }

            CenCom.iMaxCash = Convert.ToInt16(screen1.tbMaxCash.Text);
            CenCom.iMaxBills = Convert.ToInt16(screen1.tbMaxBills.Text);
            CenCom.sGrade1 = screen1.tbGrade1.Text;
            CenCom.sGrade2 = screen1.tbGrade2.Text;
            CenCom.sGrade3 = screen1.tbGrade3.Text;
            CenCom.sGrade4 = screen1.tbGrade4.Text;
            CenCom.sGrade5 = screen1.tbGrade5.Text;
            CenCom.sGrade6 = screen1.tbGrade6.Text;
            Printer.sHeader = screen1.tbHeader.Text;
            Printer.sFooter = screen1.tbFooter.Text;
        }

        public static void UpdateButtonList()
        {
            int i;

            for (i = 1; i <= 10; i++)
            {
                Display.UpdateButtonText(i, "");
                Display.UpdateButtonColor(i, 0);
            }

            if (iView == 6)
            {
                GetConfig();

                screen1.SetButtonVisible(screen1.button1, false);
                screen1.SetButtonVisible(screen1.button2, false);
                screen1.SetButtonVisible(screen1.button3, false);
                screen1.SetButtonVisible(screen1.button4, false);
                screen1.SetButtonVisible(screen1.button5, false);
                screen1.SetButtonVisible(screen1.button6, false);
                screen1.SetButtonVisible(screen1.button7, false);
                screen1.SetButtonVisible(screen1.button8, false);
                screen1.SetButtonVisible(screen1.button9, false);
                screen1.SetButtonVisible(screen1.button10, false);

                screen1.SetTextBoxVisible(screen1.tbPicNum, true);
                screen1.SetTextBoxVisible(screen1.tbPumpNum, true);
                screen1.SetTextBoxVisible(screen1.tbMaxCash, true);
                screen1.SetTextBoxVisible(screen1.tbMaxBills, true);
                screen1.SetTextBoxVisible(screen1.tbGrade1, true);
                screen1.SetTextBoxVisible(screen1.tbGrade2, true);
                screen1.SetTextBoxVisible(screen1.tbGrade3, true);
                screen1.SetTextBoxVisible(screen1.tbGrade4, true);
                screen1.SetTextBoxVisible(screen1.tbGrade5, true);
                screen1.SetTextBoxVisible(screen1.tbGrade6, true);
                screen1.SetTextBoxVisible(screen1.tbHeader, true);
                screen1.SetTextBoxVisible(screen1.tbFooter, true);
                screen1.SetButtonVisible(screen1.btSave, true);
                screen1.SetButtonVisible(screen1.btCancel, true);
                screen1.SetButtonVisible(screen1.btDateTime, true);
                screen1.SetButtonVisible(screen1.btUpdate, true);
                screen1.SetButtonVisible(screen1.btRestart, true);
                screen1.SetLabelVisible(screen1.lbPicNum, true);
                screen1.SetLabelVisible(screen1.lbPumpNum, true);
                screen1.SetLabelVisible(screen1.lbMaxCash, true);
                screen1.SetLabelVisible(screen1.lbMaxBills, true);
                screen1.SetLabelVisible(screen1.lbGrade1, true);
                screen1.SetLabelVisible(screen1.lbGrade2, true);
                screen1.SetLabelVisible(screen1.lbGrade3, true);
                screen1.SetLabelVisible(screen1.lbGrade4, true);
                screen1.SetLabelVisible(screen1.lbGrade5, true);
                screen1.SetLabelVisible(screen1.lbGrade6, true);
                screen1.SetLabelVisible(screen1.lbHeader, true);
                screen1.SetLabelVisible(screen1.lbFooter, true);
                screen1.SetLabelVisible(screen1.lbWarning, true);
            }
            else
            {
                screen1.SetButtonVisible(screen1.button1, true);
                screen1.SetButtonVisible(screen1.button2, true);
                screen1.SetButtonVisible(screen1.button3, true);
                screen1.SetButtonVisible(screen1.button4, true);
                screen1.SetButtonVisible(screen1.button5, true);
                screen1.SetButtonVisible(screen1.button6, true);
                screen1.SetButtonVisible(screen1.button7, true);
                screen1.SetButtonVisible(screen1.button8, true);
                screen1.SetButtonVisible(screen1.button9, true);
                screen1.SetButtonVisible(screen1.button10, true);

                screen1.SetTextBoxVisible(screen1.tbPicNum, false);
                screen1.SetTextBoxVisible(screen1.tbPumpNum, false);
                screen1.SetTextBoxVisible(screen1.tbMaxCash, false);
                screen1.SetTextBoxVisible(screen1.tbMaxBills, false);
                screen1.SetTextBoxVisible(screen1.tbGrade1, false);
                screen1.SetTextBoxVisible(screen1.tbGrade2, false);
                screen1.SetTextBoxVisible(screen1.tbGrade3, false);
                screen1.SetTextBoxVisible(screen1.tbGrade4, false);
                screen1.SetTextBoxVisible(screen1.tbGrade5, false);
                screen1.SetTextBoxVisible(screen1.tbGrade6, false);
                screen1.SetTextBoxVisible(screen1.tbHeader, false);
                screen1.SetTextBoxVisible(screen1.tbFooter, false);
                screen1.SetButtonVisible(screen1.btSave, false);
                screen1.SetButtonVisible(screen1.btCancel, false);
                screen1.SetButtonVisible(screen1.btDateTime, false);
                screen1.SetButtonVisible(screen1.btUpdate, false);
                screen1.SetLabelVisible(screen1.lbPicNum, false);
                screen1.SetLabelVisible(screen1.lbPumpNum, false);
                screen1.SetLabelVisible(screen1.lbMaxCash, false);
                screen1.SetLabelVisible(screen1.lbMaxBills, false);
                screen1.SetLabelVisible(screen1.lbGrade1, false);
                screen1.SetLabelVisible(screen1.lbGrade2, false);
                screen1.SetLabelVisible(screen1.lbGrade3, false);
                screen1.SetLabelVisible(screen1.lbGrade4, false);
                screen1.SetLabelVisible(screen1.lbGrade5, false);
                screen1.SetLabelVisible(screen1.lbGrade6, false);
                screen1.SetLabelVisible(screen1.lbHeader, false);
                screen1.SetLabelVisible(screen1.lbFooter, false);
                screen1.SetLabelVisible(screen1.lbWarning, false);
            }

            if (iView == 1)
            {
                if (TRAN_MGR.TRANs.Count > 0)
                {
                    for (i = 0; i < TRAN_MGR.TRANs.Count; i++)
                    {
                        if (TRAN_MGR.TRANs[i].bCompleted)
                        {
                            Display.UpdateButtonText(i + 1, "PUMP: " + TRAN_MGR.TRANs[i].iPump + " @ " + TRAN_MGR.TRANs[i].sShowTime + "\nPAID: $" + TRAN_MGR.TRANs[i].iDeposited + "  CHANGE: $" + TRAN_MGR.TRANs[i].sChange);
                            Display.UpdateButtonColor(i + 1, 1);
                        }
                        else if (TRAN_MGR.TRANs[i].bAuthorized)
                        {
                            Display.UpdateButtonText(i + 1, "PUMP: " + TRAN_MGR.TRANs[i].iPump + " DEPOSIT: " + TRAN_MGR.TRANs[i].iDeposited);
                        }
                        else
                        {
                            Display.UpdateButtonText(i + 1, "PUMP: " + TRAN_MGR.TRANs[i].iPump + " RESERVED");
                        }
                    }
                }
            }
            else if (iView == 2)
            {
                DB.UpdateCompletedTransView();
            }
            else if (iView == 3)
            {
                DB.UpdateEodView();
            }
            else if (iView == 4)
            {
                DB.UpdateCashView();
            }
            else if (iView == 5)
            {
                if (!CenCom.bOK)
                {
                    Display.UpdateButtonColor(1, 2);
                }
                Display.UpdateButtonText(1, "FPS STATUS");

                for (i = 1; i <= CenCom.iPicCount; i++)
                {
                    if (!PIC_MGR.PICs[i - 1].bOK)
                    {
                        Display.UpdateButtonColor(i + 1, 2);
                    }
                    Display.UpdateButtonText(i + 1, "PIC " + i + " STATUS");
                }
            }
        }

        public static void UpdateButtonText(int iIndex, string sLabel)
        {
            if (iIndex == 1)
            {
                screen1.SetButtonText(screen1.button1, sLabel);
            }
            else if (iIndex == 2)
            {
                screen1.SetButtonText(screen1.button2, sLabel);
            }
            else if (iIndex == 3)
            {
                screen1.SetButtonText(screen1.button3, sLabel);
            }
            else if (iIndex == 4)
            {
                screen1.SetButtonText(screen1.button4, sLabel);
            }
            else if (iIndex == 5)
            {
                screen1.SetButtonText(screen1.button5, sLabel);
            }
            else if (iIndex == 6)
            {
                screen1.SetButtonText(screen1.button6, sLabel);
            }
            else if (iIndex == 7)
            {
                screen1.SetButtonText(screen1.button7, sLabel);
            }
            else if (iIndex == 8)
            {
                screen1.SetButtonText(screen1.button8, sLabel);
            }
            else if (iIndex == 9)
            {
                screen1.SetButtonText(screen1.button9, sLabel);
            }
            else if (iIndex == 10)
            {
                screen1.SetButtonText(screen1.button10, sLabel);
            }
        }

        public static void GotoScreen(int iNextScreen, int iMode)
        {
            Debug.WriteLine("GOTOSCREEN " + iNextScreen + ", " + iMode);

            iCurrentScreen = iNextScreen;
            iScreenMode = iMode;

            if (CenCom.bTestMode == false)
            {
                CenCom.iScreenTimer = 0;//put here for safety and to handle nav from screen 3,4,5.
            }
            else
            {
                CenCom.iScreenTimer = 60;//also set on any key press if btestmode on processkey handler
            }

            //screen1.SetText1("Pump Status");
            //screen1.ShowThis();

            //if (iNextScreen == 0)
            //{
            //    CenCom.sInput = "";
            //    screen0.SetText2("");

            //    if (iMode == 1)
            //    {
            //        CenCom.iScreenTimer = 60;

            //        iMinInput = 4;
            //        iMaxInput = 4;

            //        screen0.SetText1("Enter Passcode\n\n\n\n\nPress ENTER when done");
            //    }
            //    else if (iMode == 4)
            //    {
            //        CenCom.sInput = CenCom.sChargerID;
            //        iMinInput = 5;
            //        iMaxInput = 5;

            //        screen0.SetText1("Enter Charger ID\n\n\n\n\nPress OK when done");

            //        screen0.SetText2(CenCom.sChargerID);
            //    }
            //    else
            //    {
            //        iMaxInput = 2;

            //        screen0.SetText1("\nOut of Service");

            //    }
            //    screen0.ShowThis();
            //}
            //else if (iNextScreen == 1)
            //{
            //    CenCom.sInput = "";
            //    CenCom.bCardRequest = true;

            //    if (CenCom.iBrand == 2)
            //    {
            //        screen1.HideImage(screen1.pictureBox1);
            //        screen1.ShowImage(screen1.pictureBox2);
            //    }
            //    else
            //    {
            //        screen1.HideImage(screen1.pictureBox2);
            //        screen1.ShowImage(screen1.pictureBox1);
            //    }
                
            //    CenCom.ImmediateCheck();//check status of printer to know if can print receipt + report updated status of other components

            //    screen1.ShowThis();

            //    if (CenCom.bStartup)
            //    {
            //        msgBox.BringToFront();
            //    }
            //}
        }

        public static void ShowMessageBox(string sText, int iTimer)
        {
            bMsgBoxShowing = true;

            CenCom.iMsgBoxTimer = iTimer;
            msgBox.SetText1(sText);

            msgBox.HideButton(msgBox.btYes);
            msgBox.HideButton(msgBox.btNo);
            msgBox.ShowThis();
        }

        public static void ShowMessageBox(string sText, int iTimer, int iPassChoiceNum)
        {
            bMsgBoxShowing = true;

            if (iPassChoiceNum > 0)
            {
                iChoice = iPassChoiceNum;
                msgBox.ShowButton(msgBox.btYes);
                msgBox.ShowButton(msgBox.btNo);
            }
            else
            {
                msgBox.HideButton(msgBox.btYes);
                msgBox.HideButton(msgBox.btNo);
            }

            CenCom.iMsgBoxTimer = iTimer;
            msgBox.SetText1(sText);
            msgBox.ShowThis();
        }

        public static void HideMessageBox()
        {
            bMsgBoxShowing = false;

            msgBox.SetText1("");
            msgBox.HideThis();
        }
    }

    public static class Printer
    {
        public static int iPortNum = 0;

        static SerialPort PortPTR = new SerialPort();

        public static bool bWaitingForResponse = false;
        public static int iStatus = 0;

        public static string sReceipt;

        public static bool bPrinting = false;

        static int iPrintCheckCount = 0;

        public static int iType = 1;

        static string sRead = "";

        public static bool bError = false;
        public static string sStatus = "";

        //USB-public static ManagementObjectSearcher searcher;

        public static string sHeader;
        public static string sFooter;

        public static void Init()
        {
            if (iType == 4)
            {
                PortPTR.BaudRate = 9600;
                PortPTR.StopBits = StopBits.One;
                PortPTR.Parity = Parity.None;
                PortPTR.DataBits = 8;
                PortPTR.PortName = "COM" + iPortNum;
                PortPTR.Handshake = Handshake.None;

                CenCom.iPrinterWait = 1;
            }
            //else if (iType == 3)
            //{
            //    ManagementScope MyScope = new ManagementScope();
            //    ManagementScope scope = new ManagementScope(@"\root\cimv2");
            //    scope.Connect();

            //    searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");

            //    PrinterUSB.OpenPrinter("MyPrinter");
            //    CenCom.iPrinterWait = 1;
            //}
            else if (iType == 2)
            {
                PortPTR.BaudRate = 115200;
                PortPTR.StopBits = StopBits.One;
                PortPTR.Parity = Parity.None;
                PortPTR.DataBits = 8;
                PortPTR.PortName = "COM" + iPortNum;
                PortPTR.Handshake = Handshake.RequestToSend;

                CenCom.iPrinterWait = 1;
            }
            else
            {
                //PortPTR.BaudRate = 1200;
                //PortPTR.StopBits = StopBits.One;
                //PortPTR.Parity = Parity.None;
                //PortPTR.DataBits = 8;
                //PortPTR.PortName = "COM" + iPortNum;
                //PortPTR.Handshake = Handshake.None;

                //CenCom.iPrinterWait = 6;

                Debug.WriteLine("PRINTER INIT*******************");
                //PortPTR.BaudRate = 9600;
                PortPTR.BaudRate = 38400;
                PortPTR.StopBits = StopBits.One;
                PortPTR.Parity = Parity.None;
                PortPTR.DataBits = 8;
                PortPTR.PortName = "COM" + iPortNum;
                //PortPTR.Handshake = Handshake.None;
                PortPTR.Handshake = Handshake.XOnXOff;

                CenCom.iPrinterWait = 1;
            }

            //PortPTR.ReceivedBytesThreshold = 1;
            PortPTR.ReadTimeout = 1000;//may need to change...

            PortPTR.DataReceived += new SerialDataReceivedEventHandler(PortPTR_DataReceived);

            try
            {
                PortPTR.Open();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                Debug.WriteLine(ex.Message);
            }
        }

        static void PortPTR_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int iBytesToRead = PortPTR.BytesToRead;
            int i;
            byte[] comBuffer = new byte[iBytesToRead];
            byte bCurrentByte;

            Debug.WriteLine("Printer Data Received");
            Debug.WriteLine("Printer bytes to read: " + iBytesToRead);

            if (iType == 2)
            {
                if (iBytesToRead > 3)
                {
                    PortPTR.Read(comBuffer, 0, iBytesToRead);

                    bCurrentByte = 0;
                    for (i = 0; i < iBytesToRead; i++)
                    {
                        bCurrentByte = comBuffer[i];
                        Debug.WriteLine("PRINTER JAM?????-" + String.Format("{0:X2}", bCurrentByte));
                        if (bCurrentByte == 0x80)
                        {
                            break;
                        }
                    }

                    if ((bCurrentByte & 0x80) != 0x80)
                    {
                        Printer.bError = true;
                        Printer.sStatus = "Printer Error";
                        Debug.WriteLine("Printer Error");
                    }
                    else if ((bCurrentByte & 0x04) == 0x04)
                    {
                        Printer.bError = true;
                        Printer.sStatus = "Printer Temp";
                        Debug.WriteLine("Printer Temp");
                    }
                    else if ((bCurrentByte & 0x40) == 0x40)
                    {
                        Printer.bError = true;
                        Printer.sStatus = "Paper Out";
                        Debug.WriteLine("Paper Out");
                    }
                    else if ((bCurrentByte & 0x10) == 0x10)
                    {
                        Printer.bError = true;
                        Printer.sStatus = "Paper Jam";
                        Debug.WriteLine("Paper Jam");
                    }
                    else if ((bCurrentByte & 0x20) == 0x20)
                    {
                        Printer.bError = false;
                        Printer.sStatus = "Paper Low";
                        Debug.WriteLine("Paper Low");
                    }
                    else if ((bCurrentByte & 0x08) == 0x08)
                    {
                        Printer.bError = false;
                        Printer.sStatus = "Printout Lost";
                        Debug.WriteLine("Printout Lost");
                    }
                    else if ((bCurrentByte & 0x02) == 0x02)
                    {
                        Printer.bError = false;
                        Printer.sStatus = "Paper In Chute";
                        Debug.WriteLine("Paper In Chute");
                    }
                    else  //if ((ptrCharReadArray[0] & 0x80) == 0x80)
                    {
                        Printer.bError = false;
                        Printer.sStatus = "";
                        Debug.WriteLine("Printer OK");
                    }

                    //if (bCurrentByte == 0x80)
                    if (bError == false)
                    {
                        if (iStatus < 1)//distinguish offline from jam - only trigger if offline
                        {
                            iStatus = 1;
                            CenCom.UpdateOkStatus();
                            Debug.WriteLine("Printer Online");

                            //Debug.WriteLine("SEND STATUS - PRINTER ONLINE");
                            //Web.SendStatus();

                            //RS485.SendPRS();
                            Config();
                        }
                    }
                    else
                    {
                        if (iStatus > 0)
                        {
                            iStatus = 0;
                            Debug.WriteLine("Printer Jam");
                            //RS485.SendPRS();
                        }
                    }
                }
            }
            else
            {
                if (iStatus < 1)
                {
                    iStatus = 1;
                    CenCom.UpdateOkStatus();
                    Debug.WriteLine("Printer Online");

                    Config();
                }

                Debug.WriteLine("Purge Printer: ");
                try
                {
                    sRead = PortPTR.ReadExisting();
                    if (CenCom.iLoggingPTR == 1)
                    {
                        WriteLogData_RX(sRead);//would fill up log
                        for (i = 0; i < sRead.Length; i++)//PD- caused H3.exe failure
                        {
                            WriteLogData_RX(String.Format("{0:X2}", (byte)Convert.ToChar(sRead.Substring(i, 1))) + " ");
                        }
                    }
                }
                catch
                {
                }
            }

            if (bWaitingForResponse == true)
            {
                bWaitingForResponse = false;
                Debug.WriteLine("Printer WaitingForResponse Off");
            }
        }

        public static void CheckStatus()
        {
            int i;
            byte[] bSend = new byte[3];

            //if (iType == 4)
            //{
                bSend = new byte[] { 0x1B, 0x76 };
            //}
            //else
            //{
            //    bSend = new byte[] { 28, 114, 255 };
            //}

            bWaitingForResponse = true;

            try
            {
                if (PortPTR.IsOpen == false)
                {
                    PortPTR.Open();
                }
                //if (iType == 4)
                //{
                    PortPTR.Write(bSend, 0, 2);
                //}
                //else
                //{
                //    PortPTR.Write(bSend, 0, 3);
                //}
                if (CenCom.iLoggingPTR == 1)
                {
                    WriteLogData_TX(Environment.NewLine + "TX> " + DateTime.Now.ToString("HH:mm:ss:ffff") + " -- ");
                    for (i = 0; i < 3; i++)
                    {
                        WriteLogData_TX(String.Format("{0:X2}", bSend[i]) + " ");
                    }
                }
                Debug.WriteLine("Checking Printer status", DateTime.Now.ToString("h:mm:ss.fff"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static void Config()
        {
            int i;
            byte[] bSend = new byte[9];

            //if (iType == 4)
            //{
                bSend = new byte[] { 0x1B, 0x21, 0x00, 0x1D, 0x4C, 0x30, 0x00 };
            //}
            //else
            //{
            //    bSend = new byte[] { 28, 57, 95, 27, 33, 34, 29, 97, 22 };
            //}

            try
            {
                if (PortPTR.IsOpen == false)
                {
                    PortPTR.Open();
                }

                //if (iType == 4)
                //{
                    PortPTR.Write(bSend, 0, 7);
                //}
                //else
                //{
                //    PortPTR.Write(bSend, 0, 9);
                //}

                if (CenCom.iLoggingPTR == 1)
                {
                    WriteLogData_TX(Environment.NewLine + "TX> " + DateTime.Now.ToString("HH:mm:ss:ffff") + " -- ");
                    for (i = 0; i < 9; i++)
                    {
                        WriteLogData_TX(String.Format("{0:X2}", bSend[i]) + " ");
                    }
                }
                Debug.WriteLine("Configuring Printer.................");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static bool Test()
        {
            byte[] bSend = new byte[1];
            byte[] bSend10 = new byte[] { 10 };
            byte[] bSendCut = new byte[] { 29, 86, 48 };

            int i, j, k = 0;

            bPrinting = true;
            iPrintCheckCount = 0;
            CenCom.iPrintTimer = 0;

            try
            {
                if (PortPTR.IsOpen == false)
                {
                    PortPTR.Open();
                }

                k = 15;
                for (i = 0; i < 30; i++)
                {
                    for (j = 0; j < 27; j++)
                    {
                        bSend[0] = (byte)(k + 33);
                        PortPTR.Write(bSend, 0, 1);
                        if (k < 93) { k++; }
                        else { k = 0; }
                    }
                    PortPTR.Write(bSend10, 0, 1);
                }

                for (i = 0; i <= 8; i++)
                {
                    PortPTR.Write(bSend10, 0, 1);
                }

                //if (iType == 1)
                //{
                //    PortPTR.Write(bSendCut, 0, 3);
                //}
                Debug.WriteLine("Testing Printer");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        static void WriteLogData_RX(string sData)
        {
            File.AppendAllText("LOGS\\PR_RX_" + String.Format("{0:D2}", DateTime.Today.Day) + ".txt", sData);
        }

        public static bool Print(string sPrint)
        {
            byte[] bSend = new byte[1];
            byte[] bSendCut = new byte[] { 29, 86, 48 };

            int i = 0;

            bPrinting = true;
            iPrintCheckCount = 0;
            CenCom.iPrintTimer = 0;

            try
            {
                if (PortPTR.IsOpen == false)
                {
                    PortPTR.Open();
                }
                if (CenCom.iLoggingPTR == 1)
                {
                    WriteLogData_TX(Environment.NewLine + "TX> " + DateTime.Now.ToString("HH:mm:ss:ffff") + " -- ");
                }
                for (i = 0; i < sPrint.Length; i++)
                {
                    bSend[0] = (byte)Convert.ToChar(sPrint.Substring(i, 1));
                    PortPTR.Write(bSend, 0, 1);
                    if (CenCom.iLoggingPTR == 1)
                    {
                        WriteLogData_TX(sPrint.Substring(i, 1));
                    }
                }

                if (iType == 1)
                {
                    PortPTR.Write(bSendCut, 0, 3);
                }
                if (CenCom.iLoggingPTR == 1)
                {
                    for (i = 0; i < 3; i++)
                    {
                        WriteLogData_TX(String.Format("{0:X2}", bSendCut[i]) + " ");
                    }
                }

                Debug.WriteLine("Printed Receipt");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        static void WriteLogData_TX(string sData)
        {
            File.AppendAllText("LOGS\\PR_TX_" + String.Format("{0:D2}", DateTime.Today.Day) + ".txt", sData);
        }
    }

    public static class FileAccess
    {
        public static string sSettings;
        public static string sVersion;
        public static string sConfig;

        public static void ReadSettings()
        {
            sSettings = File.ReadAllText("settings.txt");
            RS485.iPortNum = Convert.ToInt16(sSettings.Substring(sSettings.IndexOf("<PIC_PORT>") + 10, 1));
            SFC.iPortNum = Convert.ToInt16(sSettings.Substring(sSettings.IndexOf("<SFC_PORT>") + 10, 1));
            Printer.iPortNum = Convert.ToInt16(sSettings.Substring(sSettings.IndexOf("<PTR_PORT>") + 10, 1));
            sVersion = sSettings.Substring(sSettings.IndexOf("<VERSION>") + 9, 9);
            Printer.iType = Convert.ToInt16(sSettings.Substring(sSettings.IndexOf("<PTR_TYPE>") + 10, 1));
            //CenCom.sHealthCheck = sSettings.Substring(sSettings.IndexOf("<CHECK>") + 7, 4);

            sConfig = File.ReadAllText("C:/config.txt");
            CenCom.iPicCount = Convert.ToInt16(sConfig.Substring(FileAccess.sConfig.IndexOf("<PICNUM>") + 8, (FileAccess.sConfig.IndexOf("</PICNUM>") - FileAccess.sConfig.IndexOf("<PICNUM>") - 8)));
            CenCom.iPumpCount = Convert.ToInt16(sConfig.Substring(FileAccess.sConfig.IndexOf("<PUMPNUM>") + 9, (FileAccess.sConfig.IndexOf("</PUMPNUM>") - FileAccess.sConfig.IndexOf("<PUMPNUM>") - 9)));
            CenCom.iMaxCash = Convert.ToInt16(sConfig.Substring(FileAccess.sConfig.IndexOf("<MAXCASH>") + 9, (FileAccess.sConfig.IndexOf("</MAXCASH>") - FileAccess.sConfig.IndexOf("<MAXCASH>") - 9)));
            CenCom.iMaxBills = Convert.ToInt16(sConfig.Substring(FileAccess.sConfig.IndexOf("<MAXBILLS>") + 10, (FileAccess.sConfig.IndexOf("</MAXBILLS>") - FileAccess.sConfig.IndexOf("<MAXBILLS>") - 10)));
            CenCom.sGrade1 = sConfig.Substring(FileAccess.sConfig.IndexOf("<GRADE1>") + 8, (FileAccess.sConfig.IndexOf("</GRADE1>") - FileAccess.sConfig.IndexOf("<GRADE1>") - 8));
            CenCom.sGrade2 = sConfig.Substring(FileAccess.sConfig.IndexOf("<GRADE2>") + 8, (FileAccess.sConfig.IndexOf("</GRADE2>") - FileAccess.sConfig.IndexOf("<GRADE2>") - 8));
            CenCom.sGrade3 = sConfig.Substring(FileAccess.sConfig.IndexOf("<GRADE3>") + 8, (FileAccess.sConfig.IndexOf("</GRADE3>") - FileAccess.sConfig.IndexOf("<GRADE3>") - 8));
            CenCom.sGrade4 = sConfig.Substring(FileAccess.sConfig.IndexOf("<GRADE4>") + 8, (FileAccess.sConfig.IndexOf("</GRADE4>") - FileAccess.sConfig.IndexOf("<GRADE4>") - 8));
            CenCom.sGrade5 = sConfig.Substring(FileAccess.sConfig.IndexOf("<GRADE5>") + 8, (FileAccess.sConfig.IndexOf("</GRADE5>") - FileAccess.sConfig.IndexOf("<GRADE5>") - 8));
            CenCom.sGrade6 = sConfig.Substring(FileAccess.sConfig.IndexOf("<GRADE6>") + 8, (FileAccess.sConfig.IndexOf("</GRADE6>") - FileAccess.sConfig.IndexOf("<GRADE6>") - 8));
            Printer.sHeader = sConfig.Substring(FileAccess.sConfig.IndexOf("<HEADER>") + 8, (FileAccess.sConfig.IndexOf("</HEADER>") - FileAccess.sConfig.IndexOf("<HEADER>") - 8));
            Printer.sFooter = sConfig.Substring(FileAccess.sConfig.IndexOf("<FOOTER>") + 8, (FileAccess.sConfig.IndexOf("</FOOTER>") - FileAccess.sConfig.IndexOf("<FOOTER>") - 8));

            if (CenCom.bLogMaster)
            {
                CenCom.iLoggingSFC = Convert.ToInt16(sSettings.Substring(sSettings.IndexOf("<LOG_SFC>") + 9, 1));
                CenCom.iLoggingPIC = Convert.ToInt16(sSettings.Substring(sSettings.IndexOf("<LOG_PIC>") + 9, 1));
                CenCom.iLoggingPTR = Convert.ToInt16(sSettings.Substring(sSettings.IndexOf("<LOG_PTR>") + 9, 1));
            }
        }

        public static void CopyLogs()
        {
            DateTime dt = DateTime.Now;
            string sFileName = "";
            string sDestFile = "";
            string sSourcePath = @"C:\Documents and Settings\Administrator\My Documents\FPS\LOGS";
            string sTargetPath = @"D:\LOGS\" + "FPS" + "_" + dt.Year + "_" + dt.Month + "_" + dt.Day + "_" + dt.Hour + "_" + dt.Minute + "_" + dt.Second;

            if (!System.IO.Directory.Exists(sTargetPath))
            {
                Debug.WriteLine(sTargetPath);
                System.IO.Directory.CreateDirectory(sTargetPath);
            }

            if (System.IO.Directory.Exists(sSourcePath))
            {
                string[] files = System.IO.Directory.GetFiles(sSourcePath);

                foreach (string s in files)
                {
                    sFileName = System.IO.Path.GetFileName(s);
                    sDestFile = System.IO.Path.Combine(sTargetPath, sFileName);
                    System.IO.File.Copy(s, sDestFile, true);
                    System.IO.File.Delete(s);
                }
            }
        }

        public static void CopyData()
        {
            DateTime dt = DateTime.Now;
            string sFileName = "";
            string sDestFile = "";
            string sSourcePath = @"C:\DB";
            string sTargetPath = @"D:\DB\" + "DB" + "_" + dt.Year + "_" + dt.Month + "_" + dt.Day + "_" + dt.Hour + "_" + dt.Minute + "_" + dt.Second;

            if (!System.IO.Directory.Exists(sTargetPath))
            {
                Debug.WriteLine(sTargetPath);
                System.IO.Directory.CreateDirectory(sTargetPath);
            }

            if (System.IO.Directory.Exists(sSourcePath))
            {
                string[] files = System.IO.Directory.GetFiles(sSourcePath);

                foreach (string s in files)
                {
                    sFileName = System.IO.Path.GetFileName(s);
                    sDestFile = System.IO.Path.Combine(sTargetPath, sFileName);
                    System.IO.File.Copy(s, sDestFile, true);
                    System.IO.File.Delete(s);
                }
            }
        }

        public static void CopyReports()
        {
            DateTime dt = DateTime.Now;
            string sFileName = "";
            string sDestFile = "";
            string sSourcePath;
            string sTargetPath;

            sSourcePath = @"C:\REPORTS\EOD";
            sTargetPath = @"D:\REPORTS\" + "EOD" + "_" + dt.Year + "_" + dt.Month + "_" + dt.Day + "_" + dt.Hour + "_" + dt.Minute + "_" + dt.Second;

            if (!System.IO.Directory.Exists(sTargetPath))
            {
                Debug.WriteLine(sTargetPath);
                System.IO.Directory.CreateDirectory(sTargetPath);
            }

            if (System.IO.Directory.Exists(sSourcePath))
            {
                string[] files = System.IO.Directory.GetFiles(sSourcePath);

                foreach (string s in files)
                {
                    sFileName = System.IO.Path.GetFileName(s);
                    sDestFile = System.IO.Path.Combine(sTargetPath, sFileName);
                    System.IO.File.Copy(s, sDestFile, true);
                    System.IO.File.Delete(s);
                }
            }

            sSourcePath = @"C:\REPORTS\CASH";
            sTargetPath = @"D:\REPORTS\" + "CASH" + "_" + dt.Year + "_" + dt.Month + "_" + dt.Day + "_" + dt.Hour + "_" + dt.Minute + "_" + dt.Second;

            if (!System.IO.Directory.Exists(sTargetPath))
            {
                Debug.WriteLine(sTargetPath);
                System.IO.Directory.CreateDirectory(sTargetPath);
            }

            if (System.IO.Directory.Exists(sSourcePath))
            {
                string[] files = System.IO.Directory.GetFiles(sSourcePath);

                foreach (string s in files)
                {
                    sFileName = System.IO.Path.GetFileName(s);
                    sDestFile = System.IO.Path.Combine(sTargetPath, sFileName);
                    System.IO.File.Copy(s, sDestFile, true);
                    System.IO.File.Delete(s);
                }
            }
        }
    }

    public static class PIC_MGR
    {
        public static List<PIC> PICs = new List<PIC>();

        public static int iCurrentPIC = 1;
        static int iCurrentState = 0;

        static bool bSkip = false;

        public static DateTime dtPrintRequest;
        static TimeSpan tsPrinterWait;
        static TimeSpan tsPrinterLimit = TimeSpan.FromSeconds(2);

        static DateTime dtLastRequest;
        static TimeSpan tsWait;
        static TimeSpan tsLimit = TimeSpan.FromMilliseconds(500);

        static TimerCallback timerDelegate = new TimerCallback(Run);
        static System.Threading.Timer PicTimer = new System.Threading.Timer(timerDelegate, null, 200, 200);

        public static void Init()
        {
            int i;
            PIC PIC_Obj;

            Debug.WriteLine("RUN:::");

            dtLastRequest = DateTime.Now;

            Debug.WriteLine("HALLA:::" + iCurrentPIC + PICs.Count + "mm" + CenCom.iPicCount);

            for (i = 1; i <= CenCom.iPicCount; i++)
            {
                PIC_Obj = new PIC(i);
                PICs.Add(PIC_Obj);
            }

            if (CenCom.iPicCount > 0)
            {
                GotoNextPIC();
                //Run();
            }
        }

        static void Run(Object stateInfo)
        {
            //if (!bLock || RS485.iStatus == 0)
            //{
            //    Debug.WriteLine("LOCK PIC LOCK!!!");
            //    bLock = true;

                tsWait = DateTime.Now - dtLastRequest;
                Debug.WriteLine("TIME: " + tsWait);

                if (tsWait > tsLimit)//don't go too fast...
                {
                    dtLastRequest = DateTime.Now;

                    Debug.WriteLine("HALLA:::" + iCurrentPIC + PICs.Count);

                    if (PICs.Count > 0)
                    {
                        PICs[iCurrentPIC - 1].SendRequest();
                    }

                    GotoNextPIC();
                }
            

            //}

            //Run();

            //iCurrentState = PICs[iCurrentPIC - 1].iState;

            //Debug.WriteLine("RUN - PIC " + iCurrentPIC + " STATE = " + iCurrentState);
            //if (iCurrentState == 0)
            //{
            //    //Get Status

                
            //}
            //else if (iCurrentState == 1)
            //{
                
            //}
            //else if (iCurrentState == 2)
            //{
            //    //if (PUMP_MGR.PUMPs[PICs[iCurrentPIC - 1].iPump - 1].bReserved)
            //    //{
            //    //    Debug.WriteLine("PUMP " + PUMP_MGR.PUMPs[PICs[iCurrentPIC - 1].iPump - 1] + " RESERVED");
            //    //}

            //}
        }

        static void GotoNextPIC()
        {
            if (bSkip)
            {
                bSkip = false;
            }
            else
            {
                bSkip = true;

                if (iCurrentPIC < CenCom.iPicCount)
                {
                    iCurrentPIC++;
                }
                else
                {
                    iCurrentPIC = 1;
                }
            }
        }
    }

    public class PIC
    {
        public int iPump;
        public int iTranType;
        public int iTranState;
        public int iDeposit;
        public int iBillCount;
        //public string sTranTime;
        //public DateTime dtReserved;

        int iPIC;
        string sRequest = "";

        public string sReceipt;

        public bool bOK = false;

        public int iStatus_CAS;
        public int iStatus_PTR;
        public int iStatus_EPP;
        int iStatus_CRD;
        int iPrevious;

        int iBillValue;
        int iStackAttempt = 0;
        int iReturnAttempt = 0;
        bool bCancel = false;
        bool bStack = false;
        bool bReturn = false;

        DateTime dtLastRequest;
        TimeSpan tsWait;
        TimeSpan tsLimit = TimeSpan.FromMilliseconds(600);

        DateTime dtNewScreen;
        TimeSpan tsScreenWait;
        TimeSpan tsScreenLimit = TimeSpan.FromMilliseconds(0);

        public PIC(int iPassNum)
        {
            this.iPIC = iPassNum;
            iPump = 0;
            iTranType = 0;
            iTranState = 0;
            iDeposit = 0;
            iBillCount = 0;

            iStatus_CAS = 0;
            iStatus_PTR = 0;
            iStatus_EPP = 0;
            iStatus_CRD = 0;

            sRequest = "";
            sReceipt = "";

            bOK = false;
        }

        public void Init()
        {
            iPump = 0;
            iTranType = 0;
            iTranState = 0;
            iDeposit = 0;
            iBillCount = 0;
            iBillValue = 0;
            iStackAttempt = 0;
            iReturnAttempt = 0;
            bCancel = false;
            bStack = false;
            bReturn = false;

            sRequest = "";
            sReceipt = "";

            dtLastRequest = DateTime.Now;
        }

        public void ProcessMessage(string sPassReceive)
        {
                Debug.WriteLine("PROCESS PIC MESSAGE: " + sPassReceive);

                int i;
                int iCmd = 0;

                Debug.Write("PROCESS MSG: ");
                foreach (char c in sPassReceive)
                {
                    Debug.Write(String.Format("{0:X2}", (byte)c) + " ");
                }
            
                if (iTranState == 0 && SFC.iStatus == 1 && RS485.iStatus == 1)
                {
                    if (SFC.iStatus == 1 && RS485.iStatus == 1 && iStatus_EPP == 1 && iStatus_CAS == 1)
                    {
                        GotoMain();
                    }
                    else
                    {
                        GetStatusUpdate(0);
                    }
                }

                i = 1;
                while (i < (sPassReceive.Length - 4))
                {
                    iCmd = sPassReceive[i];

                    try
                    {
                        if (iCmd == 0x01)
                        {
                            StartTransaction(sPassReceive[i+1], sPassReceive[i+2]);
                            i = i + 3;
                        }
                        else if (iCmd == 0x03)//Numeric Entry
                        {
                            //
                        }
                        else if (iCmd == 0x05)//Key Press
                        {
                            ProcessKey(sPassReceive[i + 1]);
                            i = i + 2;
                        }
                        else if (iCmd == 0x09)//New Bill
                        {
                            SetBillValue(sPassReceive[i + 1]);
                            i = i + 2;
                        }
                        else if (iCmd == 0x11)//EPP Status
                        {
                            UpdateStatus_EPP(sPassReceive[i + 1]);
                            i = i + 2;
                        }
                        else if (iCmd == 0x13)//CR Status
                        {
                            UpdateStatus_CDR(sPassReceive[i + 1]);
                            i = i + 2;
                        }
                        else if (iCmd == 0x14)//CA Status
                        {
                            UpdateStatus_CAS(sPassReceive[i + 1]);
                            i = i + 2;
                        }
                        else if (iCmd == 0x15)//PTR Status
                        {
                            UpdateStatus_PTR(sPassReceive[i + 1]);
                            i = i + 2;
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            
        }

        public void SendRequest()
        {
            tsWait = DateTime.Now - dtLastRequest;
            Debug.WriteLine("TIME: " + tsWait);

            //if (tsWait > tsLimit)
            //{
            //    dtLastRequest = DateTime.Now;

            ////if (sRequest == "")
            ////{
            ////    sRequest = "Set PIC " + iPIC + "Request to Default";
            ////}

            if (tsScreenLimit != TimeSpan.FromMilliseconds(0))
            {
                tsScreenWait = DateTime.Now - dtNewScreen;

                if (tsScreenWait > tsScreenLimit)
                {
                    tsScreenLimit = TimeSpan.FromMilliseconds(0);
                    if (iTranState == 3)
                    {
                        AuthorizationRequest(iDeposit);
                    }
                    else
                    {
                        Debug.WriteLine("1111");
                        CancelTransaction();
                    }
                }
            }

            if (iStackAttempt > 0)
            {
                if (iStackAttempt < 5)
                {
                    StackBill();
                }
                else
                {
                    iStackAttempt = 0;
                    ShowMessage(87, 0);
                    ReturnBill();
                }
            }
            else if (iReturnAttempt > 0)
            {
                if (iReturnAttempt < 5)
                {
                    ReturnBill();
                }
                else
                {
                    iReturnAttempt = 0;
                    Debug.WriteLine("2222");
                    CancelTransaction();
                }
            }

            RS485.MySendRequest(iPIC, sRequest);
            Debug.WriteLine("SEND PIC REQUEST: " + sRequest);

            sRequest = "";

            //}
        }

        void GotoMain()
        {
            iTranState = 1;
            sRequest = sRequest + Convert.ToString((char)0x11) + Convert.ToString((char)0x0A) + Convert.ToString((char)1);
        }

        void GotoFinal(bool bPassAuth)
        {
            iTranState = 4;

            if (bPassAuth)
            {
                ShowMessage(99, 5);
            }
            else
            {
                if (bCancel)
                {
                    ShowMessage(100, 5);//Change
                }
                else
                {
                    ShowMessage(100, 5);
                }
                TRAN_MGR.CancelSale(PUMP_MGR.PUMPs[iPump - 1].sTranId, iDeposit);
            }
        }

        void UpdateOkStatus()
        {
            int i;
            bOK = true;

            Debug.WriteLine("PIC " + iPIC + " UPDATE OK");

            if (iStatus_CAS == 0 || iStatus_CAS == 11 || iStatus_CAS == 12 || iStatus_CAS == 13 || iStatus_EPP == 0 || iStatus_PTR == 0)
            {
                bOK = false;
            }

            Debug.WriteLine(iStatus_CAS + "-" + iStatus_EPP + "-" + iStatus_PTR);
            Debug.WriteLine(bOK);

            CenCom.UpdateOverallStatus();
        }

        void UpdateStatus_EPP(int iPassNewState)
        {
            Debug.WriteLine("UPDATE EPP STATUS-----------------" + iStatus_EPP + iPassNewState);
            if (iStatus_EPP != iPassNewState)
            {
                iStatus_EPP = iPassNewState;
                UpdateOkStatus();
            }
        }

        void UpdateStatus_CDR(int iPassNewState)
        {
            //Debug.WriteLine("UPDATE CDR STATUS-----------------" + iStatus_CDR + iPassNewState);
            //if (iStatus_CDR != iPassNewState)
            //{
            //    iStatus_CDR = iPassNewState;
            //}
        }

        void UpdateStatus_PTR(int iPassNewState)
        {
            Debug.WriteLine("UPDATE PTR STATUS-----------------" + iStatus_PTR + iPassNewState);

            if (iStatus_PTR != iPassNewState)
            {
                if (iPassNewState == 2)
                {
                    ShowMessage(70, 5);
                }

                iPrevious = iStatus_PTR;
                iStatus_PTR = iPassNewState;
                if (iPassNewState == 0 || (iPassNewState == 1 && iPrevious == 0))
                {
                    UpdateOkStatus();
                }

                if (iPassNewState == 1 || iPrevious == 2)
                {
                    //PIC_MGR.bLock = false;
                    GotoMain();
                }
            }
        }

        void PrintReceipt()
        {
            Debug.WriteLine("PRINT RECEIPT: " + sReceipt);

            sRequest = sRequest + Convert.ToString((char)0x05) + sReceipt + Convert.ToString((char)0xF0);
        }

        void SetBillValue(int iPassNewBill)
        {
            iBillValue = iPassNewBill;
        }

        void UpdateStatus_CAS(int iPassNewState)
        {
           if (iStatus_CAS != iPassNewState) {//PIC should not update if this is not true

                iStackAttempt = 0;
                iReturnAttempt = 0;

                if (iTranState == 2 || iTranState == 3)
                {
                    UpdateScreenTimer(30);

                    if (iPassNewState == 4)//Check for max bill/cash limit
                    {
                        if ((iBillCount + 1) > CenCom.iMaxBills)
                        {
                            ShowMessage(23, 0);
                            bReturn = true;
                            ReturnBill();
                        }
                        else if ((iDeposit + iBillValue) > CenCom.iMaxCash)
                        {
                            ShowMessage(24, 0);
                            bReturn = true;
                            ReturnBill();
                        }
                        else
                        {
                            ShowMessage(86, 0);
                            bStack = true;
                            StackBill();
                        }
                    }
                    else if (iStatus_CAS == 4 || iStatus_CAS == 5 && (iPassNewState == 2 || iPassNewState == 3 || iPassNewState == 7))
                    {
                        bStack = false;
                        CountBill();
                    }
                    else if (bStack)
                    {
                        if (iPassNewState == 2 || iPassNewState == 3 || iPassNewState == 5 || iPassNewState == 7)
                        {

                            CountBill();
                        }
                        else
                        {
                            bReturn = true;
                            ReturnBill();
                        }
                        bStack = false;
                    }
                    else if (bReturn && (iPassNewState == 2 || iPassNewState == 3))
                    {
                        Debug.WriteLine("BRETURN IS TRUE - " + iPassNewState);

                        bReturn = false;
                        ShowCashBalance(22, 60);
                    }
                }

                if (iPassNewState == 13)
                {
                    DB.PullCassette(iPIC);
                }

                iPrevious = iStatus_CAS;
                iStatus_CAS = iPassNewState;
                if (iPassNewState == 0 || iPassNewState == 11 || iPassNewState == 12 || iPassNewState == 13 || (iPassNewState == 1 && (iPrevious == 0 || iPrevious == 11 || iPrevious == 12 || iPrevious == 13)))
                {
                    UpdateOkStatus();
                }
            }
        }

        void StackBill()
        {
            iStackAttempt++;
            sRequest = sRequest + Convert.ToString((char)0x04) + Convert.ToString((char)0x02);
        }

        void ReturnBill()
        {
            iReturnAttempt++;
            sRequest = sRequest + Convert.ToString((char)0x04) + Convert.ToString((char)0x03);
        }

        void CountBill()
        {
            Debug.WriteLine("COUNT BILL " + iBillValue);

            DB.AddToCassette(iPIC, iBillValue);

            if (iTranState == 2)
            {
                iTranState = 3;
            }
            iBillCount++;
            iDeposit = iDeposit + iBillValue;
            ShowCashBalance(22, 60);
        }

        void SetString(int iPassIndex, string sPassMsg)
        {
            sRequest = sRequest + Convert.ToString((char)0x10) + sPassMsg + Convert.ToString((char)0xF0);
        }

        void ShowMessage(int iPassIndex, int iPassWait)
        {
            sRequest = sRequest + Convert.ToString((char)0x20) + Convert.ToString((char)iPassIndex);
            dtNewScreen = DateTime.Now;
            tsScreenLimit = TimeSpan.FromMilliseconds(iPassWait * 1000);
        }

        void GetStatusUpdate(int iPassComponent)
        {
            sRequest = sRequest + Convert.ToString((char)0x03) + Convert.ToString((char)iPassComponent);
        }

        void ShowPaymentPrompt()//Cash Only
        {
            if (iTranState == 1)
            {
                iTranState = 2;
            }
            sRequest = sRequest + Convert.ToString((char)0x10) + Convert.ToString((char)15) + "Insert Cash" + Convert.ToString((char)0xF0);
            sRequest = sRequest + Convert.ToString((char)0x13) + Convert.ToString((char)15) + Convert.ToString((char)0x01);
            dtNewScreen = DateTime.Now;
            tsScreenLimit = TimeSpan.FromMilliseconds(30 * 1000);//set for 30-60 seconds
        }

        void ShowCashBalance(int iPassIndex, int iPassWait)
        {
            sRequest = sRequest + Convert.ToString((char)0x14) + Convert.ToString((char)iPassIndex) + Convert.ToString((char)iDeposit);
            dtNewScreen = DateTime.Now;
            tsScreenLimit = TimeSpan.FromMilliseconds(iPassWait * 1000);
        }

        void ShowCancelConfirmation(int iPassWait)
        {
            sRequest = sRequest + Convert.ToString((char)0x18) + Convert.ToString((char)44) + Convert.ToString((char)2) + Convert.ToString((char)1) + Convert.ToString((char)51) + Convert.ToString((char)2) + Convert.ToString((char)52);
            dtNewScreen = DateTime.Now;
            tsScreenLimit = TimeSpan.FromMilliseconds(iPassWait * 1000);
        }

        void UpdateScreenTimer(int iPassWait)
        {
            dtNewScreen = DateTime.Now;
            tsScreenLimit = TimeSpan.FromMilliseconds(iPassWait * 1000);
        }

        void ProcessKey(int iPassKey)
        {
            if (bCancel)
            {
                if (iPassKey == 13 || iPassKey == 1)
                {
                    Debug.WriteLine("3333");
                    CancelTransaction();
                }
                else if (iPassKey == 2)//only keys 1 and 2 are enabled
                {
                    Debug.WriteLine("AAAAA" + iPassKey);
                    bCancel = false;
                    ShowCashBalance(22, 60);
                }
            }
            else
            {
                if (iPassKey == 13)
                {
                    Debug.WriteLine("4444");
                    CancelTransaction();
                }
                else if (iPassKey == 15)
                {
                    Debug.WriteLine("BBBBB");
                    AuthorizationRequest(iDeposit);
                }
            }
        }

        void CancelTransaction()
        {
            Debug.WriteLine("CANCEL TRANSACTION");
            Debug.WriteLine(iTranState + "-" + iStatus_CAS);

            if (iTranState == 3)
            {
                if (bCancel == false)
                {
                    if (iStatus_CAS == 2 || iStatus_CAS == 1)//PIC DISABLES
                    {
                        Debug.WriteLine("CONFIRM");
                        bCancel = true;
                        ShowCancelConfirmation(30);
                    }
                    else
                    {
                        Debug.WriteLine("CA NOT IDLE");
                        //wait for CA to return to idle
                    }
                }
                else
                {
                    Debug.WriteLine("GO TO FINAL - CANCEL");
                    GotoFinal(false);
                    bCancel = false;
                }
            }
            else
            {
                if (iTranState > 1)
                {
                    if (iTranState < 4)
                    {
                        TRAN_MGR.Clear(PUMP_MGR.PUMPs[iPump - 1].sTranId);
                    }
                    PUMP_MGR.PUMPs[iPump - 1].Init();
                    Init();
                }
                GotoMain();
            }
        }

        void StartTransaction(int iPassNum, int iPassType)
        {
            iPump = iPassNum;
            iTranType = iPassType;

            if (iTranType == 1)
            {
                PUMP_MGR.PUMPs[iPump - 1].iPIC = iPIC;
                Reserve();
                
            }
            else if (iTranType == 2)
            {
                sReceipt = TRAN_MGR.LookupReceipt(iPump);
                //PIC_MGR.bLock = true;
                ShowMessage(70, 0);
                PrintReceipt();
            }

        }

        void Reserve()
        {
            if (PUMP_MGR.PUMPs[iPump - 1].Reserve())
            {
                Debug.WriteLine("PUMP " + iPump + " RESERVED");
                
                ShowPaymentPrompt();
            }
            else
            {
                Debug.WriteLine("PUMP " + iPump + " NOT RESERVED");
                ShowMessage(76, 5);
            }
        }

        //void ReservationRequest()
        //{
        //    if (PUMP_MGR.PUMPs[iPump - 1].ReservationRequest())
        //    {
        //        Debug.WriteLine("RESERVE PUMP ATTEMPT");
        //        PUMP_MGR.PUMPs[iPump - 1].iPIC = iPIC;
        //    }
        //    else
        //    {
        //        Debug.WriteLine("PUMP ALREADY RESERVED");
        //        ShowMessage(77, 5);
        //    }
        //}

        //public void ReservationResponse()
        //{
        //    //if (iPump == iNum)
        //    //{
        //        if (PUMP_MGR.PUMPs[iPump - 1].bReserved)
        //        {
        //            Debug.WriteLine("PUMP " + iPump + " RESERVED");
        //            //PICs[iCurrentPIC - 1].iState = 0;
        //            ShowPaymentPrompt();

        //            //sTranTime = Convert.ToString(DateTime.Now);
        //            //TRAN_MGR.Create(iPIC, iPump, sTranTime);
        //        }
        //        else
        //        {
        //            Debug.WriteLine("PUMP " + iPump + " NOT RESERVED");
        //            //PICs[iCurrentPIC - 1].iState = 0;
        //            ShowMessage(76, 5);
        //        }
        //    //}
        //    //else
        //    //{
        //    //    Debug.WriteLine("ERROR ");
        //    //}
        //}

        public void AuthorizationRequest(int iPassAmount)
        {
            //if (PICs[iCurrentPIC - 1].iPump == iNum)
            //{
                PUMP_MGR.PUMPs[iPump - 1].AuthorizationRequest(iPassAmount);
            //}
            //else
            //{
            //    Debug.WriteLine("ERROR ");
            //}
        }

        public void AuthorizationResponse()
        {
            //if (PICs[iCurrentPIC - 1].iPump == iNum)
            //{
                if (PUMP_MGR.PUMPs[iPump - 1].bAuthorized)
                {
                    Debug.WriteLine("PUMP " + iPump + " AUTHORIZED");
                    GotoFinal(true);
                }
                else
                {
                    Debug.WriteLine("PUMP " + iPump + " NOT AUTHORIZED");
                    GotoFinal(false);
                }
            //}
            //else
            //{
            //    Debug.WriteLine("ERROR ");
            //}
        }

    }

    public static class PUMP_MGR
    {
        public static List<PUMP> PUMPs = new List<PUMP>();
        public static List<int> lActivePumps = new List<int>();
        public static List<int> lAuthorize = new List<int>();
        public static List<int> lGetSalesReport = new List<int>();
        public static List<int> lComplete = new List<int>();
        
        static int iPump = 0;
        static int iIndex = 0;

        public static bool bLock = false;

        static DateTime dtLastRequest;
        static TimeSpan tsWait;
        static TimeSpan tsLimit = TimeSpan.FromMilliseconds(500);

        static TimerCallback timerDelegate = new TimerCallback(Run);
        static System.Threading.Timer PumpTimer = new System.Threading.Timer(timerDelegate, null, 100, 100);

        public static void Init()
        {
            int i;
            PUMP PUMP_Obj;

            for (i = 1; i <= CenCom.iPumpCount; i++)
            {
                PUMP_Obj = new PUMP(i);
                PUMPs.Add(PUMP_Obj);
            }

            dtLastRequest = DateTime.Now;
        }

        static void Run(Object stateInfo)
        {
            //GotoNextPump();

            //if (iCurrentPump > 0)
            //{
            //    PUMPs[iCurrentPump - 1].SendRequest();
            //}
            //if (CenCom.bStartup == false)
            //{
                tsWait = DateTime.Now - dtLastRequest;
                Debug.WriteLine("TIME: " + tsWait);

                //if (tsWait > tsLimit)
                //{
                //    bLock = false;
                //}

                if (tsWait > tsLimit)//don't go too fast...
                {
                    //if (SFC.iStatus == 0 && bLock) { bLock = false; };
                    //Debug.WriteLine("HI" + bLock);
                    if (!bLock || SFC.iStatus == 0)
                    {
                        bLock = true;
                        dtLastRequest = DateTime.Now;

                        if (lAuthorize.Count == 0 && lGetSalesReport.Count == 0 && lComplete.Count == 0)
                        {
                            SFC.GetState(0);
                        }
                        else if (lAuthorize.Count > 0)
                        {
                            iPump = lAuthorize[0];
                            SFC.Authorize(iPump, PUMP_MGR.PUMPs[iPump - 1].iPurchased);
                        }
                        else if (lGetSalesReport.Count > 0)
                        {
                            iPump = lGetSalesReport[0];
                            SFC.GetSalesReport(iPump);
                        }
                        else if (lComplete.Count > 0)
                        {
                            iPump = lComplete[0];
                            SFC.CompleteSale(iPump);
                        }
                    }
                }
            }
        //}
    }

    public class PUMP
    {
        public int iPIC;
        public int iState;
        public int iPurchased;
        public int iDispensed;
        public int iGallons;
        public bool bReserved;
        public bool bAuthorized;
        public bool bCompleted;
        //public string sTranTime;
        //public DateTime dtReserved;
        //public DateTime dtAuthorized;
        public string sTranId;
        public string sReservedTime;
        public string sAuthorizedTime;

        int iPump;
        string sRequest = "";

        DateTime dtLastRequest;
        TimeSpan tsWait;
        TimeSpan tsLimit = TimeSpan.FromMilliseconds(500);

        public PUMP(int iNum)
        {
            this.iPump = iNum;
            iPIC = 0;
            iState = 0;
            iPurchased = 0;
            iDispensed = 0;
            iGallons = 0;
            bReserved = false;
            bAuthorized = false;
            dtLastRequest = DateTime.Now;
            //sTranTime = "";
            sTranId = "";
            sRequest = "";
        }

        public void Init()
        {
            if (bAuthorized == false || bCompleted)//pic cancel after final screen but pump should be monitored if authorized
            {
                Debug.WriteLine("ACTIVE PUMP COUNT: " + PUMP_MGR.lActivePumps.Count);
                if (PUMP_MGR.lAuthorize.Contains(iPump))
                {
                    PUMP_MGR.lAuthorize.Remove(iPump);
                }
                if (PUMP_MGR.lGetSalesReport.Contains(iPump))
                {
                    PUMP_MGR.lGetSalesReport.Remove(iPump);
                }
                if (PUMP_MGR.lComplete.Contains(iPump))
                {
                    PUMP_MGR.lComplete.Remove(iPump);
                }
                Debug.WriteLine("ACTIVE PUMP COUNT: " + PUMP_MGR.lActivePumps.Count);

                iPIC = 0;
                iState = 0;
                iPurchased = 0;
                iDispensed = 0;
                iGallons = 0;
                bReserved = false;
                bAuthorized = false;
                bCompleted = false;
                dtLastRequest = DateTime.Now;
                //sTranTime = "";
                sTranId = "";
                sRequest = "";
            }
        }

        public void SendRequest()
        {
            tsWait = DateTime.Now - dtLastRequest;
            Debug.WriteLine("TIME: " + tsWait);

            if (tsWait > tsLimit)
            {
                dtLastRequest = DateTime.Now;

                //if (sRequest == "" && !bCompleted)//so don't repeat sales report
                //{
                //    SFC.GetState(iPump);
                //    //sRequest = "K" + CenCom.Format(iPump, 2);
                //}
                //else if (sRequest == "A")
                //{
                //    SFC.Authorize(iPump, iPurchased);
                //}
                //else if (sRequest == "S")
                //{
                //    SFC.GetSalesReport(iPump);
                //}
                //else if (bCompleted)
                //{
                //    SFC.CompleteSale(iPump);
                //    //sRequest = "";
                //    Init();
                //}

                //SFC.Send(sRequest);
                Debug.WriteLine("SEND PUMP REQUEST: " + sRequest);

                sRequest = "";
            }
            else
            {
                Debug.WriteLine("PUMP REQUEST BLOCKED: " + tsWait);
            }
        }

        public bool Reserve()
        {
            if (bReserved == false)
            {
                bReserved = true;

                //sTranTime = Convert.ToString(DateTime.Now);
                //DateTime dtNow = DateTime.Now;
                string sNow = DateTime.Now.ToString("yyMMddHHmmss");
                string sId = sNow + CenCom.Format(iPIC, 2) + CenCom.Format(iPump, 2);

                PUMP_MGR.PUMPs[iPump - 1].sReservedTime = sNow;
                PUMP_MGR.PUMPs[iPump - 1].sTranId = sId;

                TRAN_MGR.Create(iPIC, iPump, sNow, sId);

                //PIC_MGR.PICs[iPIC - 1].ReservationResponse();

                return true;
            }
            else
            {
                return false;
            }
        }

        public void GetStateResponse(int iPassStatus)
        {
            iState = iPassStatus;

            //if (bReserved == false)
            //{
            //    if (iState == 1)//Testing
            //    {
            //        bReserved = true;
            //        //sTranTime = Convert.ToString(DateTime.Now);
            //        DateTime dtNow= DateTime.Now;
                    
            //        PUMP_MGR.PUMPs[iPump - 1].dtReserved = dtNow;
            //        TRAN_MGR.Create(iPIC, iPump, dtNow);

            //        PIC_MGR.PICs[iPIC - 1].ReservationResponse();
            //    }
            //    else
            //    {
            //        PIC_MGR.PICs[iPIC - 1].ReservationResponse();
            //        Init();
            //    }
            //}
            //else if (bAuthorized == true)
            //{
            //    if (iState == 3)
            //    {
            //        if (sRequest == "")//so don't repeat
            //        {
            //            sRequest = "S";
            //            Debug.WriteLine("GET SALES REPORT!!");
            //        }
            //    }
            //    else
            //    {
            //        Debug.WriteLine("WAITING FOR SALES REPORT - " + iState);
            //    }
            //}

            if (bAuthorized == true)
            {
                if (iState == 3)
                {
                    if (!PUMP_MGR.lGetSalesReport.Contains(iPump))
                    {
                        PUMP_MGR.lGetSalesReport.Add(iPump);
                    }
                }
            }

            if (iPump == CenCom.iPumpCount)//wait for last pump with K00
            {
                PUMP_MGR.bLock = false;
            }
            
        }

        //public void ReservationResponse(int iState)
        //{
        //    if (iState == 1)
        //    {
        //        bReserved = true;
        //    }
        //    PIC_MGR.PICs[iPIC - 1].ReservationResponse();
        //}

        public bool AuthorizationRequest(int iPassAmount)
        {
            if (iPassAmount > 0)
            {
                iPurchased = iPassAmount;

                PUMP_MGR.lAuthorize.Add(iPump);

                return true;
            }
            else
            {
                return false;
            }
        }

        public void AuthorizationResponse(bool bPassACK)
        {
            Debug.WriteLine("PUMP AUTHORIZATION RESPONSE - bACK = " + bPassACK);

            if (PUMP_MGR.lAuthorize.Contains(iPump))
            {
                PUMP_MGR.lAuthorize.Remove(iPump);
            }

            if (bPassACK)//testing
            //if (true)
            {
                bAuthorized = true;
                //dtAuthorized = DateTime.Now;
                sAuthorizedTime = DateTime.Now.ToString("yyMMddHHmmss");
                TRAN_MGR.AuthorizeTransaction(sTranId, iPurchased, sAuthorizedTime);
                //TRAN_MGR.AuthorizeTransaction(iPump, Convert.ToString(DateTime.Now), iPurchased);
            }
            PIC_MGR.PICs[iPIC - 1].AuthorizationResponse();

            PUMP_MGR.bLock = false;
        }

        public void GetReportResponse(string sPassGrade, string sPassVolume, string sPassBought, string sPassPrice)
        {
            Debug.WriteLine("PUMP REPORT RESPONSE - PUMP = " + iPump + " REPORT LIST COUNT = " + PUMP_MGR.lGetSalesReport.Count + " INDEX 0 PUMP = " + PUMP_MGR.lGetSalesReport[0]);
            
            if (PUMP_MGR.lGetSalesReport.Contains(iPump))
            {
                PUMP_MGR.lGetSalesReport.Remove(iPump);
            }
            if (!PUMP_MGR.lComplete.Contains(iPump))
            {
                PUMP_MGR.lComplete.Add(iPump);
            }

            TRAN_MGR.GetSalesReport(sTranId, sPassGrade, sPassVolume, sPassBought, sPassPrice);

            PUMP_MGR.bLock = false;
        }

        public void CompleteResponse(bool bPassACK)
        {
            Debug.WriteLine("PUMP COMPLETE RESPONSE");

            //DONE IT INIT
            //if (PUMP_MGR.lComplete.Contains(iPump))
            //{
            //    PUMP_MGR.lComplete.Remove(iPump);
            //}
            bCompleted = true;
            Init();
            PUMP_MGR.bLock = false;
        }

    }

    public static class TRAN_MGR
    {
        public static List<TRAN> TRANs = new List<TRAN>();

        static TimeSpan tsWait;
        static TimeSpan tsLimit = TimeSpan.FromSeconds(300);

        static int iTranIndex = 0;

        static TimerCallback timerDelegate = new TimerCallback(Run);
        static System.Threading.Timer TranTimer = new System.Threading.Timer(timerDelegate, null, 1000, 1000);

        static void Run(Object stateInfo)
        {
            if (TRANs.Count > 0)
            {
                if (iTranIndex < (TRANs.Count - 1))
                {
                    iTranIndex++;
                }
                else
                {
                    iTranIndex = 0;
                }

                Debug.WriteLine("AUTO CLEAR TRAN " + iTranIndex);

                if (TRANs[iTranIndex].bCompleted)
                {
                    Debug.WriteLine("AUTO CLEAR ENABLED");
                    tsWait = DateTime.Now - TRANs[iTranIndex].dtCompleted;
                    Debug.WriteLine("TIME: " + tsWait);
                    Debug.WriteLine(DateTime.Now);
                    Debug.WriteLine(TRANs[iTranIndex].dtCompleted);
                    if (tsWait > tsLimit)
                    {
                        Clear(TRANs[iTranIndex].sTranId);
                    }
                }
            }
        }

        public static void Create(int iPassPicNum, int iPassPumpNum, string sPassTime, string sPassID)
        {
            TRAN TRAN_Obj;

            TRAN_Obj = new TRAN(TRANs.Count, iPassPicNum, iPassPumpNum, sPassTime, sPassID);
            TRANs.Add(TRAN_Obj);

            //DB.CreateTransaction(sID, sTime, iPicNum, iPumpNum);
        }

        public static void Clear(string sPassId)
        {
            Debug.WriteLine("CLEAR TRANSACTION");

            int i;
            if (TRANs.Count > 0)
            {
                for (i = 0; i < TRANs.Count; i++)
                {
                    //Debug.WriteLine("INSIDE LOOP: PIC " + TRANs[i].iPIC + " Pump " + TRANs[i].iPump + " RESERVED " + TRANs[i].dtReserved);
                    if (TRANs[i].sTranId == sPassId)
                    {
                        TRANs.Remove(TRANs[i]);
                        AdjustPositions();
                        Display.UpdateButtonList();
                    }
                }
            }
        }

        public static void AdjustPositions()
        {
            Debug.WriteLine("ADJUST POSITIONS");

            int i;
            if (TRANs.Count > 0)
            {
                for (i = 0; i < TRANs.Count; i++)
                {
                    TRANs[i].iPosition = i + 1;
                }
            }
        }

        public static bool Active(int iPassPumpNum)
        {
            Debug.WriteLine("IS PUMP " + iPassPumpNum + "STILL ACTIVE TRANSACTION");

            //CHECK COMPLETED TIME TO ALLOW MULTIPLE TRANSACTIONS

            bool bTrue = false;
            int i;
            if (TRANs.Count > 0)
            {
                for (i = 0; i < TRANs.Count; i++)
                {
                    Debug.WriteLine("INSIDE LOOP: PUMP " + TRANs[i].iPump);
                    if (!bTrue && TRANs[i].iPump == iPassPumpNum && !TRANs[i].bCompleted)
                    {
                        bTrue = true;
                        break;
                    }
                }
            }
            return bTrue;
        }

        public static bool AuthorizeTransaction(string sPassId, int iPassAmount, string sPassTime)
        {
            int i;

            Debug.WriteLine("TRANSACTION AUTHORIZATION - TRANID = " + sPassId);

            //DB.AuthorizeTransaction(sId, iAmount, sTime);

            if (TRANs.Count > 0)
            {
                for (i = 0; i < TRANs.Count; i++)
                {
                    //Debug.WriteLine("TRANSACTION AUTHORIZATION - PIC = " + TRANs[i].iPIC + " PUMP = " + TRANs[i].iPump + "RESERVED TIME = " + TRANs[i].dtReserved);

                    if (TRANs[i].sTranId == sPassId)
                    {
                        TRANs[i].Authorize(sPassTime, iPassAmount);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool GetSalesReport(string sPassId, string sPassGrade, string sPassVolume, string sPassBought, string sPassPrice)
        {
            int i;

            Debug.WriteLine("TRAN REPORT RESPONSE " + TRANs.Count);
            Debug.WriteLine("TRAN ID = " + sPassId);
                    
            if (TRANs.Count > 0)
            {
                for (i = 0; i < TRANs.Count; i++)
                {
                    //Debug.WriteLine("INSIDE LOOP: PIC " + TRANs[i].iPIC + " Pump " + TRANs[i].iPump + " RESERVED " + TRANs[i].dtReserved);
                    if (TRANs[i].sTranId == sPassId)
                    {
                        TRANs[i].SalesReport(sPassGrade, sPassVolume, sPassBought, sPassPrice);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool CancelSale(string sPassId, int iPassDeposit)
        {
            int i;

            Debug.WriteLine("CANCEL SALE " + TRANs.Count);
            Debug.WriteLine("TRAN ID = " + sPassId);

            if (TRANs.Count > 0)
            {
                for (i = 0; i < TRANs.Count; i++)
                {
                    //Debug.WriteLine("INSIDE LOOP: PIC " + TRANs[i].iPIC + " Pump " + TRANs[i].iPump + " RESERVED " + TRANs[i].dtReserved);
                    if (TRANs[i].sTranId == sPassId)
                    {
                        TRANs[i].CancelSale(DateTime.Now.ToString("yyMMddHHmmss"), iPassDeposit);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string LookupReceipt(int iPassPumpNum)
        {
            string sReturn;
            bool bTrue;

            Debug.WriteLine("RECEIPT LOOKUP");

            sReturn = "";
            bTrue = false;

            int i;
            if (TRANs.Count > 0)
            {
                for (i = 0; i < TRANs.Count; i++)
                {
                    if (!bTrue && TRANs[i].iPump == iPassPumpNum)
                    {
                        //if (!TRANs[i].bChange)
                        if (TRANs[i].sChange == "0.00")
                        {
                            Debug.WriteLine("HALLA");
                            sReturn = TRANs[i].PicReceipt();
                            bTrue = true;
                            break;
                        }
                    }
                }
            }
            //sReturn = ";lajdf;ajdf;ajdf;ajdf;adjf;adfja;dfj";
            //sReturn = sReturn + ";lajdf;ajdf;ajdf;ajdf;adjf;adfja;dfj";
            //sReturn = sReturn + ";lajdf;ajdf;ajdf;ajdf;adjf;adfja;dfj";
            //sReturn = sReturn + ";lajdf;ajdf;ajdf;ajdf;adjf;adfja;dfj";
            //sReturn = sReturn + ";lajdf;ajdf;ajdf;ajdf;adjf;adfja;dfj";
            //sReturn = sReturn + ";lajdf;ajdf;ajdf;ajdf;adjf;adfja;dfj";
            //sReturn = sReturn + ";lajdf;ajdf;ajdf;ajdf;adjf;adfja;dfj";
            //sReturn = sReturn + ";lajdf;ajdf;ajdf;ajdf;adjf;adfja;dfj";
            //sReturn = sReturn + ";lajdf;ajdf;ajdf;ajdf;adjf;adfja;dfj";
            //sReturn = sReturn + ";lajdf;ajdf;ajdf;ajdf;adjf;adfja;dfj";
            //sReturn = sReturn + ";lajdf;ajdf;ajdf;ajdf;adjf;adfja;dfj";
            //sReturn = sReturn + ";lajdf;ajdf;ajdf;ajdf;adjf;adfja;dfj\n\n\n\n\n\n\n";

            Debug.WriteLine(sReturn);
            return sReturn;
        }

    }

    public class TRAN
    {
        public int iPIC;
        public int iPump;
        public string sGrade;
        public int iDeposited;
        public string sPurchased;
        public string sVolume;
        public string sPrice;
        public string sChange;
        double dPurchased;
        double dVolume;
        double dPrice;

        public bool bAuthorized;
        public bool bCompleted;
        public bool bChange;
        public DateTime dtCompleted;

        public int iPosition;

        //public DateTime dtReserved;
        //public DateTime dtAuthorized;
        //public DateTime dtCompleted;
        public string sTranId;
        public string sReservedTime;
        public string sAuthorizedTime;
        public string sCompletedTime;
        public string sShowTime;

        public TRAN(int iPassIndex, int iPassPicNum, int iPassPumpNum, string sPassTime, string sPassID)
        {
            this.iPosition = iPassIndex + 1;
            this.iPIC = iPassPicNum;
            this.iPump = iPassPumpNum;
            this.sReservedTime = sPassTime;
            this.sTranId = sPassID;
            sGrade = "";
            iDeposited = 0;
            sVolume = "";
            sPurchased = "";
            sPrice = "";
            sChange = "";
            this.bAuthorized = false;
            this.bCompleted = false;
            this.bChange = false;
            //sAuthorizedTime = "";
            //sCompletedTime = "";

            //DB.CreateTransaction(sPassID, sPassTime, iPassPicNum, iPassPumpNum);
            //Display.UpdateButton(iPosition, "Reserved: " + PUMP_MGR.PUMPs[iPumpNum - 1].sReservedTime + " PIC: " + iPicNum + " Pump: " + iPumpNum);
            //Display.UpdateButtonText(iPosition, "ID: " + sTranId + " PIC: " + iPIC + " Pump: " + iPump);
            if (Display.iView == 1)
            {
                Display.UpdateButtonText(iPosition, "PUMP: " + iPump + " RESERVED");
            }
        }

        public void Authorize(string sPassTime, int iPassAmount)
        {
            this.sAuthorizedTime = sPassTime;
            this.iDeposited = iPassAmount;
            this.bAuthorized = true;

            DB.CreateTransaction(sTranId, sReservedTime, iPIC, iPump);
            DB.AuthorizeTransaction(sTranId, Convert.ToString(iPassAmount), sPassTime);
            //Display.UpdateButtonText(iPosition, "ID: " + sTranId + "Authorized: " + sAuthorizedTime + " PIC: " + iPIC + " Pump: " + iPump + " Deposit: $" + iDeposited);
            if (Display.iView == 1)
            {
                Display.UpdateButtonText(iPosition, "PUMP: " + iPump + " DEPOSIT: " + iDeposited);
            }
        }

        //public void SalesReport(int iGradeNum, int iVolumeNum, int iDispensedNum, int iPpuNum)
        public void SalesReport(string sPassGrade, string sPassVolume, string sPassBought, string sPassPrice)
        {
            this.sGrade = sPassGrade;
            this.sVolume = sPassVolume;
            this.sPurchased = sPassBought;
            this.sPrice = sPassPrice;
            this.dVolume = Convert.ToDouble(sVolume);
            this.dPurchased = Convert.ToDouble(sPurchased);
            this.dPrice = Convert.ToDouble(sPrice);
            this.bCompleted = true;
            this.dtCompleted = DateTime.Now;
            this.sCompletedTime = dtCompleted.ToString("yyMMddHHmmss");
            this.sShowTime = dtCompleted.ToString();

            dVolume = dVolume / 1000;
            dPurchased = dPurchased / 1000;
            dPrice = dPrice / 1000;

            sVolume = Convert.ToString(dVolume);
            //sPurchased = Convert.ToString(dPurchased);
            sPurchased = String.Format("{0:0.00}", dPurchased);
            dPurchased = Convert.ToDouble(sPurchased);
            sPrice = Convert.ToString(dPrice);
            sChange = String.Format("{0:0.00}", iDeposited - dPurchased);
            //if (Convert.ToInt16(iDeposited - dPurchased) != 0)
            if (sChange != "0.00")
            {
                this.bChange = true;
            }
            Debug.WriteLine("CHANGE = " + sChange + " - " + bChange + " - " + Convert.ToInt16(iDeposited - dPurchased));
            Debug.WriteLine(bChange);
            Debug.WriteLine(Convert.ToInt16(iDeposited - dPurchased));
            Debug.WriteLine("CHANGE = " + sChange);

            DB.CompleteTransaction(sTranId, sPurchased, sPrice, sVolume, sGrade, sChange, sCompletedTime, sShowTime);

            if (Display.iView == 1)
            {
                Display.UpdateButtonText(iPosition, "PUMP: " + iPump + " @ " + sShowTime + "\nPAID: $" + iDeposited + "  CHANGE: $" + sChange);
                Display.UpdateButtonColor(iPosition, 1);
            }
        }

        public void CancelSale(string sPassTime, int iPassDeposit)
        {
            this.sCompletedTime = sPassTime;
            this.bCompleted = true;
            this.iDeposited = iPassDeposit;
            this.dtCompleted = DateTime.Now;
            this.sCompletedTime = dtCompleted.ToString("yyMMddHHmmss");
            this.sShowTime = dtCompleted.ToString();
            this.sGrade = "CANCELLED";

            sChange = String.Format("{0:0.00}", iDeposited);

            Debug.WriteLine("CHANGE = " + sChange);

            DB.CreateTransaction(sTranId, sReservedTime, iPIC, iPump);
            DB.AuthorizeTransaction(sTranId, Convert.ToString(iPassDeposit), "");
            DB.CompleteTransaction(sTranId, "0", "0", "0", sGrade, sChange, sCompletedTime, sShowTime);
            
            if (Display.iView == 1)
            {
                Display.UpdateButtonText(iPosition, "PUMP: " + iPump + " @ " + sShowTime + "\nPAID: $" + Convert.ToString(iPassDeposit) + "  CHANGE: $" + sChange);
                Display.UpdateButtonColor(iPosition, 1);
            }
        }

        public void Print()
        {
            string sReceipt = "";

            if (Printer.iStatus == 1)
            {
                //**update DB to show receipt printed
                //**show DUPLICATE if already printed

                sReceipt = sReceipt + Printer.sHeader;
                sReceipt = sReceipt + "\n\n\n";
                
                sReceipt = sReceipt + "Pump: " + iPump + "\n";
                sReceipt = sReceipt + "Terminal: " + iPIC + "\n\n";

                sReceipt = sReceipt + "Descr.           qty               amount" + "\n";
                sReceipt = sReceipt + "------           ---               ------" + "\n";
                sReceipt = sReceipt + sGrade + "               " + sVolume + "               " + sPurchased + "\n";
                sReceipt = sReceipt + "                @ " + sPrice + "/G" + "\n";
                sReceipt = sReceipt + "                Prepay Fuel        -" + iDeposited + ".00" + "\n";
                sReceipt = sReceipt + "                                 --------" + "\n";
                sReceipt = sReceipt + "                    Subtotal        -" + sChange + "\n";
                sReceipt = sReceipt + "                         Tax         0.00" + "\n";
                sReceipt = sReceipt + "                       TOTAL        -" + sChange + "\n";
                sReceipt = sReceipt + "                        CASH        -" + sChange + "\n\n";

                sReceipt = sReceipt + "TRAN# " + sTranId + "\n"; 
                sReceipt = sReceipt + sShowTime + "\n\n";
                sReceipt = sReceipt + Printer.sFooter + "\n\n\n\n\n\n\n\n\n\n\n\n\n";

                Printer.sReceipt = sReceipt;
                CenCom.bPrintRequest = true;

                TRAN_MGR.Clear(sTranId);
            }
            else
            {
                Display.ShowMessageBox("Printer Offline", 3);
            }
        }

        public string PicReceipt()
        {
            string sReceipt = "";

            Debug.WriteLine("GET RECEIPT");

            //**update DB to show receipt printed
            //**show DUPLICATE if already printed

            sReceipt = sReceipt + Printer.sHeader;
            sReceipt = sReceipt + "\n\n\n";
            
            sReceipt = sReceipt + " " + "Pump: " + iPump + "\n";
            sReceipt = sReceipt + " " + "Terminal: " + iPIC + "\n\n";

            sReceipt = sReceipt + String.Format(" {0,-8}{1,8}{2,8}\n", "Descr.", "qty", "amount");
            sReceipt = sReceipt + String.Format(" {0,-8}{1,8}{2,8}\n", "------", "---", "------");
            sReceipt = sReceipt + String.Format(" {0,-8}{1,8}{2,8}\n", sGrade, sVolume, sPurchased);
            sReceipt = sReceipt + String.Format(" {0,-8}{1,8}{2,8}\n\n", "", "@" + sPrice + "/G", "");
            sReceipt = sReceipt + String.Format(" {0,-4}{1,12}{2,8}\n", "", "Prepay Fuel", "-" + iDeposited + ".00");
            sReceipt = sReceipt + String.Format(" {0,-8}{1,8}{2,8}\n", "", "", "--------");
            sReceipt = sReceipt + String.Format(" {0,-8}{1,8}{2,8}\n", "", "Subtotal", "-" + sChange);
            sReceipt = sReceipt + String.Format(" {0,-8}{1,8}{2,8}\n", "", "Tax", "0.00");
            sReceipt = sReceipt + String.Format(" {0,-8}{1,8}{2,8}\n", "", "TOTAL", "-" + sChange);
            sReceipt = sReceipt + String.Format(" {0,-8}{1,8}{2,8}\n\n", "", "CASH", "-" + sChange);

            sReceipt = sReceipt + " TRAN# " + sTranId + "\n"; 
            sReceipt = sReceipt + " " + sShowTime + "\n\n";
            sReceipt = sReceipt + Printer.sFooter + "\n\n\n\n\n\n\n\n\n\n\n\n\n";

            TRAN_MGR.Clear(sTranId);

            return sReceipt;
        }

    }

    public static class DB
    {
        public static int iPage = 1;

        static List<EodStruct> lEodReports = new List<EodStruct>();
        static List<CashStruct> lCashReports = new List<CashStruct>();
        public static List<TransStruct> lCompletedTrans = new List<TransStruct>();

        struct EodStruct
        {
            public string sReportTime;
            public string sFromTime;
            public string sShowTimeStart;
            public string sShowTimeEnd;
        }

        struct CashStruct
        {
            public string sReportTime;
            public string sFromTime;
            public string sShowTimeStart;
            public string sShowTimeEnd;
        }

        public struct TransStruct
        {
            public string sTranId;
            public string sShowTime;
            public string sPIC;
            public string sPump;
            public string sDeposit;
            public string sPurchase;
            public string sPrice;
            public string sChange;
            public string sGrade;
            public string sVolume;
        }

        static OleDbConnection Conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:/DB/MainDB.mdb;Mode=Read|Write");

        public static OleDbConnection OpenConn()
        {
            if (Conn.State == System.Data.ConnectionState.Closed)
            {
                Conn.Open();
            }
            return Conn;
        }

        public static void CloseConn()
        {
            if (Conn.State == System.Data.ConnectionState.Open)
            {
                Conn.Close();
            }
        }

        public static void ExecuteNonQuery(string sPassQuery)
        {
            try
            {
                OleDbCommand cmd = new OleDbCommand(sPassQuery, OpenConn());
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                //ErrorLogEvents(ex.ToString());
            }
            //CloseConn();
        }

        public static void CreateTransaction(string sPassID, string sPassTime, int iPassPicNum, int iPassPumpNum)
        {
            string sQuery = @"INSERT INTO TRANSACTIONS (TRAN_ID, PIC, PUMP, RESERVED_TIME) Values('" + sPassID + "', " + iPassPicNum + ", " + iPassPumpNum + ", '" + sPassTime + "')";
            Debug.WriteLine(sQuery); 
            ExecuteNonQuery(sQuery);
        }

        public static void AuthorizeTransaction(string sPassID, string sPassAmount, string sPassTime)
        {
            string sQuery = @"UPDATE TRANSACTIONS SET AUTHORIZED_TIME = '" + sPassTime + "', DEPOSIT = '" + sPassAmount + "' WHERE TRAN_ID LIKE '" + sPassID + "'";
            Debug.WriteLine(sQuery);
            ExecuteNonQuery(sQuery);
        }

        public static void CompleteTransaction(string sPassID, string sPassPurchased, string sPassPrice, string sPassVolume, string sPassGrade, string sPassChange, string sPassCompletedTime, string sPassShowTime)
        {
            string sQuery = @"UPDATE TRANSACTIONS SET PURCHASE = " + sPassPurchased + ", PRICE = '" + sPassPrice + "', VOLUME = " + sPassVolume + ", GRADE = '" + sPassGrade + "', CHANGE = '" + sPassChange + "', COMPLETED_TIME = '" + sPassCompletedTime + "', SHOW_TIME = '" + sPassShowTime + "' WHERE TRAN_ID LIKE '" + sPassID + "'";
            Debug.WriteLine(sQuery);
            ExecuteNonQuery(sQuery);
        }

        public static void NewCassette(int iPassPicNum)
        {
            string sQuery = @"INSERT INTO CASSETTES (PIC, BILLS, TOTAL) Values(" + iPassPicNum + ", 0, 0)";
            Debug.WriteLine(sQuery); 
            ExecuteNonQuery(sQuery);
        }

        public static void AddToCassette(int iPassPicNum, int iPassBillValue)
        {
            string sQuery = @"UPDATE CASSETTES SET BILLS = BILLS + " + 1 + ", TOTAL = TOTAL + " + iPassBillValue + " WHERE PIC = " + iPassPicNum + " AND PULLED = NO";
            Debug.WriteLine(sQuery);
            ExecuteNonQuery(sQuery);
        }

        public static void PullCassette(int iPassPicNum)
        {
            DateTime dtNow = DateTime.Now;
            string sQuery = @"UPDATE CASSETTES SET PULLED_TIME = '" + dtNow.ToString("yyMMddHHmmss") + "', SHOW_TIME = '" + dtNow.ToString() + "', PULLED = YES WHERE PIC = " + iPassPicNum + " AND PULLED = NO";
            Debug.WriteLine(sQuery); 
            ExecuteNonQuery(sQuery);

            NewCassette(iPassPicNum);
        }

        public static void GetCassette(int iPassPicNum)
        {
            string sQuery;
            OleDbCommand dbCmd;
            OleDbDataReader drRecordSet;

            sQuery = "SELECT PIC FROM CASSETTES WHERE PIC = " + iPassPicNum + " AND PULLED = NO";
            Debug.WriteLine("CHECK FOR AVAILABLE CASSETTE");

            dbCmd = new OleDbCommand(sQuery, OpenConn());
            drRecordSet = dbCmd.ExecuteReader();

            Debug.WriteLine(sQuery);

            if (drRecordSet.HasRows)
            {
                Debug.Write("THERE IS A CASSETTE!!!");
            }
            else
            {
                NewCassette(iPassPicNum);
            }
            dbCmd.Dispose();
            drRecordSet.Dispose();
            //CloseConn();
        }

        public static void CheckForAvailableCassette(int iPassPicNum)
        {
            string sQuery;
            OleDbCommand dbCmd;
            OleDbDataReader drRecordSet;

            sQuery = "SELECT PIC FROM CASSETTES WHERE PIC = " + iPassPicNum + " AND PULLED = NO";
            Debug.WriteLine("CHECK FOR AVAILABLE CASSETTE");

            dbCmd = new OleDbCommand(sQuery, OpenConn());
            drRecordSet = dbCmd.ExecuteReader();

            Debug.WriteLine(sQuery);

            if (drRecordSet.HasRows)
            {
                Debug.Write("THERE IS A CASSETTE!!!");
            }
            else
            {
                Debug.Write("NEED NEW CASSETTE!!!");
                NewCassette(iPassPicNum);
            }
            dbCmd.Dispose();
            drRecordSet.Dispose();
            //CloseConn();
        }

        public static void UpdateEodView()
        {
            int iIndex, iCount;
            string sQuery;
            OleDbCommand dbCmd;
            OleDbDataReader drRecordSet;
            EodStruct myEodStruct;

            Debug.WriteLine("UPDATE EOD VIEW");

            sQuery = "SELECT REPORT_TIME, FROM_TIME, SHOW_TIME_START, SHOW_TIME_END FROM TRANSACTIONS_REPORT ORDER BY REPORT_TIME DESC";
            dbCmd = new OleDbCommand(sQuery, OpenConn());
            drRecordSet = dbCmd.ExecuteReader();

            Debug.WriteLine(sQuery);
            Debug.WriteLine(drRecordSet.HasRows);

            iCount = 0;
            lEodReports.Clear();
            while (drRecordSet.Read())
            {
                myEodStruct.sReportTime = drRecordSet[0].ToString();
                myEodStruct.sFromTime = drRecordSet[1].ToString();
                myEodStruct.sShowTimeStart = drRecordSet[2].ToString();
                myEodStruct.sShowTimeEnd = drRecordSet[3].ToString();

                lEodReports.Add(myEodStruct);
                Debug.WriteLine(lEodReports[iCount].sFromTime + "-" + lEodReports[iCount].sReportTime);
                iCount++;
            }

            for (iIndex = 0; iIndex <= 10; iIndex++)
            {
                if (iIndex < iCount)
                {
                    Display.UpdateButtonText(iIndex + 1, "REPORT TIME: " + lEodReports[iIndex].sShowTimeEnd);
                }
            }
            dbCmd.Dispose();
            drRecordSet.Dispose();
            //CloseConn();
        }

        public static void UpdateCashView()
        {
            int iIndex, iCount;
            string sQuery;
            OleDbCommand dbCmd;
            OleDbDataReader drRecordSet;
            CashStruct myCashStruct;

            Debug.WriteLine("UPDATE CASH VIEW");

            sQuery = "SELECT REPORT_TIME, FROM_TIME, SHOW_TIME_START, SHOW_TIME_END FROM CASSETTES_REPORT ORDER BY REPORT_TIME DESC";
            dbCmd = new OleDbCommand(sQuery, OpenConn());
            drRecordSet = dbCmd.ExecuteReader();

            Debug.WriteLine(sQuery);
            Debug.WriteLine(drRecordSet.HasRows);

            iCount = 0;
            lCashReports.Clear();
            while (drRecordSet.Read())
            {
                myCashStruct.sReportTime = drRecordSet[0].ToString();
                myCashStruct.sFromTime = drRecordSet[1].ToString();
                myCashStruct.sShowTimeStart = drRecordSet[2].ToString();
                myCashStruct.sShowTimeEnd = drRecordSet[3].ToString();

                lCashReports.Add(myCashStruct);
                Debug.WriteLine(lCashReports[iCount].sFromTime + "-" + lCashReports[iCount].sReportTime);
                iCount++;
            }

            for (iIndex = 0; iIndex <= 10; iIndex++)
            {
                if (iIndex < iCount)
                {
                    Display.UpdateButtonText(iIndex + 1, "REPORT TIME: " + lCashReports[iIndex].sShowTimeEnd);
                }
            }
            dbCmd.Dispose();
            drRecordSet.Dispose();
            //CloseConn();
        }

        public static void UpdateCompletedTransView()
        {
            int iIndex, iCount;
            string sQuery;
            OleDbCommand dbCmd;
            OleDbDataReader drRecordSet;
            TransStruct myTransStruct;

            Debug.WriteLine("UPDATE COMPLETE TRANSACTIONS VIEW");

            sQuery = "SELECT COMPLETED_TIME, PIC, PUMP, DEPOSIT, PURCHASE, PRICE, CHANGE, GRADE, VOLUME, SHOW_TIME, TRAN_ID FROM TRANSACTIONS ORDER BY COMPLETED_TIME DESC";
            dbCmd = new OleDbCommand(sQuery, OpenConn());
            drRecordSet = dbCmd.ExecuteReader();

            Debug.WriteLine(sQuery);
            Debug.WriteLine(drRecordSet.HasRows);

            iCount = 0;
            lCompletedTrans.Clear();
            while (drRecordSet.Read())
            {
                myTransStruct.sPIC = drRecordSet[1].ToString();
                myTransStruct.sPump = drRecordSet[2].ToString();
                myTransStruct.sDeposit = drRecordSet[3].ToString();
                myTransStruct.sPurchase = drRecordSet[4].ToString();
                myTransStruct.sPrice = drRecordSet[5].ToString();
                myTransStruct.sChange = drRecordSet[6].ToString();
                myTransStruct.sGrade = drRecordSet[7].ToString();
                myTransStruct.sVolume = drRecordSet[8].ToString();
                myTransStruct.sShowTime = drRecordSet[9].ToString();
                myTransStruct.sTranId = drRecordSet[10].ToString();

                lCompletedTrans.Add(myTransStruct);
                iCount++;
            }

            for (iIndex = 0; iIndex <= 10; iIndex++)
            {
                if (iIndex < iCount)
                {
                    Display.UpdateButtonText(iIndex + 1, "PUMP: " + lCompletedTrans[iIndex].sPump + " @ " + lCompletedTrans[iIndex].sShowTime + "\nPAID: $" + lCompletedTrans[iIndex].sDeposit + "  CHANGE: $" + lCompletedTrans[iIndex].sChange);
                }
            }
            dbCmd.Dispose();
            drRecordSet.Dispose();
            CloseConn();
        }

        public static void GenerateEodReport()
        {
            string sQuery;
            string sReportTime;
            string sFromTime;
            string sShowTimeStart;
            string sShowTimeEnd;
            EodStruct myEodStruct;
            DateTime dtNow;

            Debug.WriteLine("GENERATE EOD REPORT");

            dtNow = DateTime.Now;
            sReportTime = dtNow.ToString("yyMMddHHmmss");
            sShowTimeEnd = dtNow.ToString();
            myEodStruct = GetEodStartTime();
            sFromTime = myEodStruct.sFromTime;
            sShowTimeStart = myEodStruct.sShowTimeStart;

            sQuery = @"INSERT INTO TRANSACTIONS_REPORT (REPORT_TIME, FROM_TIME, SHOW_TIME_START, SHOW_TIME_END) VALUES('" + sReportTime + "', '" + sFromTime + "', '" + sShowTimeStart + "', '" + sShowTimeEnd + "')";
            Debug.WriteLine(sQuery); 
            ExecuteNonQuery(sQuery);

            UpdateEodView();

            //DELETE OLD DATA TRANSACTIONS + TRANSACTION REPORT DATES
        }

        public static void PrintEodReport(int iPassIndex)
        {
            int iGrade;
            double dGradeTotal = 0;
            double dPurchaseTotal = 0;
            string sQuery;
            string sReport;
            string sReportTime;
            string sFromTime;
            string sStartTime;
            string sEndTime;
            OleDbCommand dbCmd;
            OleDbDataReader drRecordSet;

            Debug.WriteLine("TRYING TO PRINT... REPORT + " + Printer.iStatus);

            if (Printer.iStatus == 1)
            {
                sReport = "";
                sReportTime = lEodReports[iPassIndex].sReportTime;
                sFromTime = lEodReports[iPassIndex].sFromTime;
                sStartTime = lEodReports[iPassIndex].sShowTimeStart;
                sEndTime = lEodReports[iPassIndex].sShowTimeEnd;

                Display.ShowMessageBox("Processing...", 3);

                try
                {
                    sReport = sReport + Printer.sHeader;
                    sReport = sReport + "\n\n";
                    sReport = sReport + "                PIC EOD REPORT\n\n";
                    sReport = sReport + String.Format("{0,-5} {1,-24}\n", "From:", sStartTime);
                    sReport = sReport + String.Format("{0,-5} {1,-22}\n\n", "To:", sEndTime);
                    sReport = sReport + (String.Format("{0,5}     {1,6}     {2,9}\n\n", "GRADE", "VOLUME", "PURCHASED"));

                    for (iGrade = 1; iGrade <= 4; iGrade++)
                    {
                        sQuery = @"SELECT SUM(VOLUME) AS VOLUME_TOTAL, SUM(PURCHASE) AS PURCHASE_TOTAL FROM TRANSACTIONS WHERE GRADE LIKE '" + iGrade + "' AND COMPLETED_TIME BETWEEN '" + sReportTime + "' AND '" + sFromTime + "'";
                        
                        dbCmd = new OleDbCommand(sQuery, OpenConn());
                        drRecordSet = dbCmd.ExecuteReader();

                        if (drRecordSet.HasRows)
                        {
                            Debug.WriteLine("HAS ROWS");
                            drRecordSet.Read();

                            if (drRecordSet[0].ToString() != "")
                            {
                                dGradeTotal = dGradeTotal + Convert.ToDouble(drRecordSet[0].ToString());
                                dPurchaseTotal = dPurchaseTotal + Convert.ToDouble(drRecordSet[1].ToString());

                                sReport = sReport + (String.Format("{0,5}     {1,6:0.000}     {2,9:0.00}\n", iGrade, Convert.ToDouble(drRecordSet[0].ToString()), Convert.ToDouble(drRecordSet[1].ToString())));
                            }
                            else
                            {
                                sReport = sReport + (String.Format("{0,10} {1,10:0.000} {2,10:0.00}", iGrade, 0, 0));
                            }
                        }
                        else
                        {
                            //Just for this Grade...
                            //Display.ShowMessageBox("No Transactions Found", 3);
                        }
                        //dbCmd.Dispose();
                        //drRecordSet.Dispose();
                    }
                    sReport = sReport + (String.Format("\n{0,6}    {1,6:0.000}     {2,9:0.00}", "TOTAL:", dGradeTotal, dPurchaseTotal));
                    sReport = sReport + "\n\n";

                    sQuery = @"SELECT SUM(DEPOSIT) AS DEPOSIT_TOTAL, SUM(CHANGE) AS CHANGE_TOTAL FROM TRANSACTIONS WHERE COMPLETED_TIME BETWEEN '" + sReportTime + "' AND '" + sFromTime + "'";

                    dbCmd = new OleDbCommand(sQuery, OpenConn());
                    drRecordSet = dbCmd.ExecuteReader();

                    if (drRecordSet.HasRows)
                    {
                        drRecordSet.Read();

                        if (drRecordSet[0].ToString() != "")
                        {
                            sReport = sReport + (String.Format("TOTAL PIC DEPOSIT: {0,6:0.00}\n", Convert.ToDouble(drRecordSet[0].ToString())));
                            sReport = sReport + (String.Format("TOTAL PIC CHANGE:  {0,6:0.00}\n", Convert.ToDouble(drRecordSet[1].ToString())));
                        }
                        else
                        {
                            //should never happen
                            //sReport = sReport + (String.Format("{0,10} {1,10:0.000} {2,10:0.00}", iGrade, 0, 0));
                        }
                    }
                    sReport = sReport + "\n\n\n\n\n\n\n\n";

                    Printer.sReceipt = sReport;
                    CenCom.bPrintRequest = true;

                    dbCmd.Dispose();
                    drRecordSet.Dispose();
                }
                catch (Exception ex)
                {
                    //ErrorLogEvents(ex.ToString());
                }

                CloseConn();
                Debug.WriteLine(sReport);
            }
            else
            {
                Display.ShowMessageBox("Printer Offline", 3);
            }
        }

        public static void GenerateCashReport()
        {
            string sQuery;
            string sReportTime;
            string sFromTime;
            string sShowTimeStart;
            string sShowTimeEnd;
            CashStruct myCashStruct;
            DateTime dtNow;

            Debug.WriteLine("GENERATE CASH REPORT");

            dtNow = DateTime.Now;
            sReportTime = dtNow.ToString("yyMMddHHmmss");
            sShowTimeEnd = dtNow.ToString();
            myCashStruct = GetCashStartTime();
            sFromTime = myCashStruct.sFromTime;
            sShowTimeStart = myCashStruct.sShowTimeStart;

            sQuery = @"INSERT INTO CASSETTES_REPORT (REPORT_TIME, FROM_TIME, SHOW_TIME_START, SHOW_TIME_END) VALUES('" + sReportTime + "', '" + sFromTime + "', '" + sShowTimeStart + "', '" + sShowTimeEnd + "')";
            Debug.WriteLine(sQuery); 
            ExecuteNonQuery(sQuery);

            UpdateCashView();

            //DELETE OLD DATA IN BOTH CASSETTES AND CASSSETTES_REPORT
        }

        public static void PrintCashReport(int iPassIndex)
        {
            int iBillTotal = 0;
            int iCashTotal = 0;
            string sQuery;
            string sReport;
            string sReportTime;
            string sFromTime;
            string sStartTime;
            string sEndTime;
            OleDbCommand dbCmd;
            OleDbDataReader drRecordSet;

            Debug.WriteLine("TRYING TO PRINT... REPORT + " + Printer.iStatus);

            if (Printer.iStatus == 1)
            {
                sReport = "";
                sReportTime = lCashReports[iPassIndex].sReportTime;
                sFromTime = lCashReports[iPassIndex].sFromTime;
                sStartTime = lCashReports[iPassIndex].sShowTimeStart;
                sEndTime = lCashReports[iPassIndex].sShowTimeEnd;

                Display.ShowMessageBox("Processing...", 3);

                try
                {
                    sQuery = @"SELECT PIC, BILLS, TOTAL, SHOW_TIME FROM CASSETTES WHERE PULLED_TIME BETWEEN '" + sReportTime + "' AND '" + sFromTime + "'";
                    Debug.WriteLine(sQuery);
                    dbCmd = new OleDbCommand(sQuery, OpenConn());
                    drRecordSet = dbCmd.ExecuteReader();

                    if (drRecordSet.HasRows)
                    {

                        Debug.WriteLine("HAS ROWS");

                        sReport = sReport + Printer.sHeader;
                        sReport = sReport + "\n\n";
                        sReport = sReport + "                PIC CASH REPORT\n\n";
                        sReport = sReport + String.Format("{0,-5} {1,-24}\n", "From:", sStartTime);
                        sReport = sReport + String.Format("{0,-5} {1,-22}\n\n", "To:", sEndTime);
                        sReport = sReport + String.Format("{0,2}{1,8}{2,8}  {3,-22}\n\n", "ID", "BILLS", "AMOUNT", "PULLED");

                        while (drRecordSet.Read())
                        {
                            Debug.WriteLine("hi - 4");
                            if (drRecordSet[0].ToString() != "")
                            {
                                iBillTotal = iBillTotal + Convert.ToInt32(drRecordSet[1].ToString());
                                iCashTotal = iCashTotal + Convert.ToInt32(drRecordSet[2].ToString());

                                sReport = sReport + (String.Format("{0,2}{1,8}{2,8}  {3,-22}\n", drRecordSet[0].ToString(), drRecordSet[1].ToString(), drRecordSet[2].ToString(), drRecordSet[3].ToString()));
                            }
                            else
                            {
                                //should never happen...
                            }
                        }
                        dbCmd.Dispose();
                        drRecordSet.Dispose();

                        sReport = sReport + (String.Format("\nTOTAL:{0,3}{1,8}", iBillTotal, iCashTotal));
                        sReport = sReport + "\n\n\n\n\n\n\n\n";

                        Printer.sReceipt = sReport;
                        CenCom.bPrintRequest = true;
                    }
                    else
                    {
                        Display.ShowMessageBox("No Cassettes Pulled", 3);
                    }
                }
                catch (Exception ex)
                {
                    //ErrorLogEvents(ex.ToString());
                }

                //CloseConn();
                Debug.WriteLine(sReport);
            }
            else
            {
                Display.ShowMessageBox("Printer Offline", 3);
            }
        }

        static CashStruct GetCashStartTime()
        {
            string sQuery;
            string sStartTime;
            OleDbCommand dbCmd;
            OleDbDataReader drRecordSet;
            CashStruct myCashStruct = new CashStruct();

            try
            {
                sQuery = "SELECT * FROM CASSETTES_REPORT";
                dbCmd = new OleDbCommand(sQuery, OpenConn());
                drRecordSet = dbCmd.ExecuteReader();

                if (drRecordSet.HasRows)
                {
                    Debug.WriteLine("HAS ROWS");

                    sQuery = "SELECT REPORT_TIME, SHOW_TIME_END FROM CASSETTES_REPORT ORDER BY REPORT_TIME DESC";
                    dbCmd = new OleDbCommand(sQuery, OpenConn());
                    drRecordSet = dbCmd.ExecuteReader();

                    Debug.WriteLine(drRecordSet.HasRows);

                    drRecordSet.Read();
                    Debug.WriteLine(drRecordSet[0].ToString());
                    while (drRecordSet[0].ToString() == "")
                    {
                        drRecordSet.Read();
                    }
                    Debug.WriteLine(drRecordSet[0].ToString());
                    myCashStruct.sFromTime = drRecordSet.GetString(0);
                    myCashStruct.sShowTimeStart = drRecordSet.GetString(1);

                    //dbCmd.Dispose();
                    //drRecordSet.Dispose();
                }
                else
                {
                    Debug.WriteLine("NO ROWS");

                    sQuery = "SELECT PULLED_TIME, SHOW_TIME FROM CASSETTES ORDER BY PULLED_TIME";
                    dbCmd = new OleDbCommand(sQuery, OpenConn());
                    drRecordSet = dbCmd.ExecuteReader();

                    Debug.WriteLine(drRecordSet.HasRows);

                    if (drRecordSet.HasRows)
                    {
                        drRecordSet.Read();
                        Debug.WriteLine(drRecordSet[0].ToString());
                        while (drRecordSet[0].ToString() == "")
                        {
                            drRecordSet.Read();
                        }
                        Debug.WriteLine(drRecordSet[0].ToString());
                        sStartTime = drRecordSet.GetString(0);

                        myCashStruct.sFromTime = drRecordSet.GetString(0);
                        myCashStruct.sShowTimeStart = drRecordSet.GetString(1);
                    }
                    else
                    {
                        myCashStruct.sFromTime = DateTime.Now.ToString("yyMMddHHmmss");
                        myCashStruct.sShowTimeStart = DateTime.Now.ToString();
                    }
                    //dbCmd.Dispose();
                    //drRecordSet.Dispose();
                }
                dbCmd.Dispose();
                drRecordSet.Dispose();
            }
            catch (Exception ex)
            {
                myCashStruct.sFromTime = DateTime.Now.ToString("yyMMddHHmmss");
                myCashStruct.sShowTimeStart = DateTime.Now.ToString();
                //ErrorLogEvents(ex.ToString());
            }
            
            //CloseConn();

            myCashStruct.sReportTime = "";
            myCashStruct.sShowTimeEnd = "";
            return myCashStruct;
        }

        static EodStruct GetEodStartTime()
        {
            string sQuery;
            string sFromTime;
            OleDbCommand dbCmd;
            OleDbDataReader drRecordSet;
            EodStruct myEodStruct = new EodStruct();

            try
            {
                sQuery = "SELECT * FROM TRANSACTIONS_REPORT";
                dbCmd = new OleDbCommand(sQuery, OpenConn());
                drRecordSet = dbCmd.ExecuteReader();

                if (drRecordSet.HasRows)
                {
                    Debug.WriteLine("HAS ROWS");

                    sQuery = "SELECT REPORT_TIME, SHOW_TIME_END FROM TRANSACTIONS_REPORT ORDER BY REPORT_TIME DESC";
                    dbCmd = new OleDbCommand(sQuery, OpenConn());
                    drRecordSet = dbCmd.ExecuteReader();

                    Debug.WriteLine(drRecordSet.HasRows);

                    drRecordSet.Read();
                    Debug.WriteLine(drRecordSet[0].ToString());
                    while (drRecordSet[0].ToString() == "")
                    {
                        drRecordSet.Read();
                    }
                }
                else
                {
                    Debug.WriteLine("NO ROWS");

                    sQuery = "SELECT COMPLETED_TIME, SHOW_TIME FROM TRANSACTIONS ORDER BY COMPLETED_TIME";
                    dbCmd = new OleDbCommand(sQuery, OpenConn());
                    drRecordSet = dbCmd.ExecuteReader();

                    Debug.WriteLine(drRecordSet.HasRows);


                    drRecordSet.Read();
                    Debug.WriteLine(drRecordSet[0].ToString());
                    while (drRecordSet[0].ToString() == "")
                    {
                        drRecordSet.Read();
                    }
                }
                Debug.WriteLine(drRecordSet[0].ToString());
                myEodStruct.sFromTime = drRecordSet.GetString(0);
                myEodStruct.sShowTimeStart = drRecordSet.GetString(1);

                dbCmd.Dispose();
                drRecordSet.Dispose();
            }
            catch (Exception ex)
            {
                //return "";
                //ErrorLogEvents(ex.ToString());
            }
            
            //CloseConn();

            myEodStruct.sReportTime = "";
            myEodStruct.sShowTimeEnd = "";
            return myEodStruct;
        }

        public static void PrintReceipt(int iPassIndex)
        {
            Debug.WriteLine("TRYING TO PRINT... STATUS + " + Printer.iStatus);

            if (Printer.iStatus == 1)
            {
                string sReceipt = "";
                string sPIC = lCompletedTrans[iPassIndex].sPIC;
                string sPump = lCompletedTrans[iPassIndex].sPump;
                string sGrade = lCompletedTrans[iPassIndex].sGrade;
                string sVolume = lCompletedTrans[iPassIndex].sVolume;
                string sPurchase = lCompletedTrans[iPassIndex].sPurchase;
                string sPrice = lCompletedTrans[iPassIndex].sPrice;
                string sDeposit = lCompletedTrans[iPassIndex].sDeposit;
                string sChange = lCompletedTrans[iPassIndex].sChange;
                string sShowTime = lCompletedTrans[iPassIndex].sShowTime;
                string sTranId = lCompletedTrans[iPassIndex].sTranId;

                sReceipt = sReceipt + Printer.sHeader;
                sReceipt = sReceipt + "\n\n\n";

                sReceipt = sReceipt + "Pump: " + sPump + "\n";
                sReceipt = sReceipt + "Terminal: " + sPIC + "\n\n";

                sReceipt = sReceipt + "Descr.           qty               amount" + "\n";
                sReceipt = sReceipt + "------           ---               ------" + "\n";
                sReceipt = sReceipt + sGrade + "               " + sVolume + "               " + sPurchase + "\n";
                sReceipt = sReceipt + "                @ " + sPrice + "/G" + "\n";
                sReceipt = sReceipt + "                Prepay Fuel        -" + sDeposit + ".00" + "\n";
                sReceipt = sReceipt + "                                 --------" + "\n";
                sReceipt = sReceipt + "                    Subtotal        -" + sChange + "\n";
                sReceipt = sReceipt + "                         Tax         0.00" + "\n";
                sReceipt = sReceipt + "                       TOTAL        -" + sChange + "\n";
                sReceipt = sReceipt + "                        CASH        -" + sChange + "\n\n";

                sReceipt = sReceipt + "TRAN# " + sTranId + "\n";
                sReceipt = sReceipt + sShowTime + "\n\n";
                sReceipt = sReceipt + Printer.sFooter + "\n\n\n\n\n\n\n\n\n\n\n\n\n";

                Printer.sReceipt = sReceipt;
                CenCom.bPrintRequest = true;

                Debug.WriteLine(sReceipt);
            }
            else
            {
                Display.ShowMessageBox("Printer Offline", 3);
            }
        }
    }

    public static class Crc
    {
        const ushort polynomial = 0xA001;
        static ushort[] table = new ushort[256];

        public static void InitTable()
        {
            ushort value;
            ushort temp;
            for (ushort i = 0; i < table.Length; ++i)
            {
                value = 0;
                temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                table[i] = value;
            }
        }

        public static string ComputeChecksum(string sBytes)
        {
            ushort crc = 0x4f4f;
            int iCheckVal1, iCheckVal2;

            for (int i = 0; i < sBytes.Length; ++i)
            {
                byte index = (byte)(crc ^ (char)sBytes[i]);
                crc = (ushort)((crc >> 8) ^ table[index]);
            }

            iCheckVal1 = (byte)(crc >> 8);
            iCheckVal2 = (byte)(crc & 0x00ff);

            return String.Format("{0:X2}", (iCheckVal1)) + String.Format("{0:X2}", (iCheckVal2));
        }

        public static string ComputeChecksum2(string sBytes)
        {
            ushort crc = 0xffff;
            int iCheckVal1, iCheckVal2;

            for (int i = 0; i < sBytes.Length; ++i)
            {
                byte index = (byte)(crc ^ (char)sBytes[i]);
                crc = (ushort)((crc >> 8) ^ table[index]);
            }

            iCheckVal1 = (byte)(crc >> 8);
            iCheckVal2 = (byte)(crc & 0x00ff);

            return Convert.ToString((char)iCheckVal2) + Convert.ToString((char)iCheckVal1);
        }
    }//CRC

}