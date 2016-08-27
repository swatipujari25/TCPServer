using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace WinTCPServer
{
    public partial class Server : Form
    {
        public AsyncCallback pfnWorkerCallBack;
        public Socket m_socListener;
        public Socket m_socWorker;
        public delegate void AppendDataDelegate();
        AppendDataDelegate dataDelegate=null;
        string recvMsg = string.Empty;
        string receivedData = string.Empty;

        public Server()
        {
            InitializeComponent();
        }

        private void BindDataToTextBox()
        {
            richTextBox1.AppendText(receivedData);
            receivedData = string.Empty;
        }

        private void InvokeTextBox()
        {
            richTextBox1.Invoke(dataDelegate);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                //create the listening socket...
                m_socListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //IPAddress adr = IPAddress.Parse("127.0.0.1");
                IPAddress adr = IPAddress.Parse("192.168.12.55");

                //				IPEndPoint ipLocal = new IPEndPoint() //( IPAddress.Any ,8221);
                IPEndPoint ipLocal = new IPEndPoint(adr, 8000);
                //bind to local IP Address...
                m_socListener.Bind(ipLocal);
                //start listening...
                m_socListener.Listen(4);
                // create the call back for any client connections...
                m_socListener.BeginAccept(new AsyncCallback(OnClientConnect), null);
                btnStart.Enabled = false;

            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        public void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                m_socWorker = m_socListener.EndAccept(asyn);

                WaitForData(m_socWorker);
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\n OnClientConnection: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }

        }

        public void WaitForData(System.Net.Sockets.Socket soc)
        {
            try
            {
                if (pfnWorkerCallBack == null)
                {
                    pfnWorkerCallBack = new AsyncCallback(OnDataReceived);
                }
                CSocketPacket theSocPkt = new CSocketPacket();
                theSocPkt.thisSocket = soc;
                // now start to listen for any data...
                soc.BeginReceive(theSocPkt.dataBuffer, 0, theSocPkt.dataBuffer.Length, SocketFlags.None, pfnWorkerCallBack, theSocPkt);
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }

        }

        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                CSocketPacket theSockId = (CSocketPacket)asyn.AsyncState;
                //end receive...
                int iRx = 0;
                iRx = theSockId.thisSocket.EndReceive(asyn);
                char[] chars = new char[iRx + 1];
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(theSockId.dataBuffer, 0, iRx, chars, 0);
                System.String szData = new System.String(chars);
                receivedData = receivedData + szData;
                InvokeTextBox();
                recvMsg = szData;
               // dataDelegate();
                //InvokeTextBox(szData);
                //richTextBox1.AppendText( szData);
                WaitForData(m_socWorker);
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                Object objData = textBox1.Text;
                byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString() + "\r\n");
                m_socWorker.Send(byData);
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            timer1.Start();
            dataDelegate = new AppendDataDelegate(BindDataToTextBox);
          
        }      

        private void timer1_Tick(object sender, EventArgs e)
        {
           
            if (!string.IsNullOrEmpty(recvMsg))
            {
                dataDelegate();
                recvMsg = string.Empty;
            }
        }       
        
    }

    public class CSocketPacket
    {
        public System.Net.Sockets.Socket thisSocket;
        public byte[] dataBuffer = new byte[1];
    }
      
}
