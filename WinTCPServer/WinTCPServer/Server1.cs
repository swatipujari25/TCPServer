using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace WinTCPServer
{
    

    public partial class Server1 : Form
    {
       // public static Dictionary<int, string> clientsList = new Dictionary<int, string>();
        public static Hashtable clientsList = new Hashtable();
        public static Hashtable SelectedclientsList = new Hashtable();
        public delegate void AppendDataDelegate();
        AppendDataDelegate dataDelegate = null;

        public delegate void AppendCheckboxWithIPAddress();
        AppendCheckboxWithIPAddress chkIPDelegate = null;

        Thread serverStartThread = null;

        public Server1()
        {
            InitializeComponent();
        }

        private void BindDataToTextBox()
        {
            richTextBox1.AppendText(StaticVar.receivedData);
            StaticVar.receivedData = string.Empty;
        }

        private void InvokeTextBox()
        {
            richTextBox1.Invoke(dataDelegate);
        }

        private void BindDataToCheckbox()
        {           
           
            chkList.Items.Add(StaticVar.ClientIPAddress);
            StaticVar.ClientIPAddress = string.Empty;
        }

        private void InvokeCheckBox()
        {
            chkList.Invoke(chkIPDelegate);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
             serverStartThread = new Thread(StartServer);
            serverStartThread.Start();
            //StartServer();
        }

        private void StartServer()
        {
            TcpListener serverSocket = new TcpListener(8000);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            serverSocket.Start();
            StaticVar.receivedData = "Chat Server Started ....";
            counter = 0;

            while ((true))
            {
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();

                byte[] bytesFrom = new byte[10025];
                string dataFromClient = null;

                NetworkStream networkStream = clientSocket.GetStream();
              int numberofBytes=  networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
              Array.Resize(ref bytesFrom, numberofBytes);
                dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);

                clientsList.Add(clientsList.Count + 1, clientSocket);

               
                 broadcast(dataFromClient + " Joined ",ref dataFromClient, false);

                 StaticVar.receivedData = dataFromClient + " Joined chat room ";
                handleClinet client = new handleClinet();
                client.startClient(clientSocket, dataFromClient, clientsList);
                StaticVar.ClientIPAddress=dataFromClient + "-" + clientSocket.Client.RemoteEndPoint.ToString();
               // SelectedclientsList.Add(dataFromClient + "-" + clientSocket.Client.RemoteEndPoint.ToString(), false);
                //chkList.Items.Add(dataFromClient + "-" + clientSocket.Client.RemoteEndPoint.ToString());
                InvokeCheckBox();
            }

            clientSocket.Close();
            serverSocket.Stop();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (chkList.SelectedItems.Count > 0)
            {
                foreach(string itme in chkList.CheckedItems)
                {
                    string uNames = string.Empty;
                    string[] split = itme.Split('-');
                    if (split.Length == 2)
                    {
                        uNames = split[0].ToString().Trim();
                    }
                    broadcast(textBox1.Text, ref uNames, false);
                }
            }
        }

        private void Server1_Load(object sender, EventArgs e)
        {
            dataDelegate = new AppendDataDelegate(BindDataToTextBox);
            chkIPDelegate = new AppendCheckboxWithIPAddress(BindDataToCheckbox);
        }

        public void broadcast(string msg, ref string uName, bool flag)
        {
          if(SelectedclientsList.Count>0)
          {
              foreach (DictionaryEntry item in clientsList)
              {
                  Thread.Sleep(100);
                  TcpClient broadcastSocket;

                  broadcastSocket = (TcpClient)item.Value;

                  if (SelectedclientsList.Contains(uName + "-" + broadcastSocket.Client.RemoteEndPoint.ToString()))
                  {
                      NetworkStream broadcastStream = broadcastSocket.GetStream();
                      Byte[] broadcastBytes = null;

                      if (flag == true)
                      {
                          broadcastBytes = Encoding.ASCII.GetBytes( msg);
                          StaticVar.receivedData = StaticVar.receivedData +  msg;
                      }
                      else
                      {
                          broadcastBytes = Encoding.ASCII.GetBytes(msg);
                          StaticVar.receivedData = StaticVar.receivedData + msg;
                      }
                  }
              }
            }
        }

        public void broadcastForCommands(string msg,  bool flag)
        {
            if (SelectedclientsList.Count > 0)
            {
                foreach (DictionaryEntry item in clientsList)
                {
                    TcpClient broadcastSocket;

                    broadcastSocket = (TcpClient)item.Value;

                    if (SelectedclientsList.Count > 0)
                    {
                        foreach (string selUser in chkList.CheckedItems)
                        {
                            string uNames = string.Empty;
                            string[] split = selUser.Split('-');
                            if (split.Length == 2)
                            {
                                uNames = split[0].ToString().Trim();
                            }
                            //broadcast(textBox1.Text, uNames, false);

                            if (SelectedclientsList.Contains(uNames + "-" + broadcastSocket.Client.RemoteEndPoint.ToString()))
                            {
                                NetworkStream broadcastStream = broadcastSocket.GetStream();
                                Byte[] broadcastBytes = null;

                                if (flag == true)
                                {
                                    broadcastBytes = Encoding.ASCII.GetBytes( msg);
                                    StaticVar.receivedData =  msg;
                                }
                                else
                                {
                                    broadcastBytes = Encoding.ASCII.GetBytes(msg);
                                  StaticVar.receivedData = msg;
                                }

                                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                                broadcastStream.Flush();
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(StaticVar.receivedData))
            {
                InvokeTextBox();
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (SelectedclientsList.Count > 0)
            {
                //foreach (string itme in chkList.CheckedItems)
                //{
                //    string uNames = string.Empty;
                //    string[] split = itme.Split('-');
                //    if (split.Length == 2)
                //    {
                //        uNames = split[0].ToString().Trim();
                //    }
                    broadcastForCommands("select route 12736",  false);
               // }
            }
        }

        private void btnDeselect_Click(object sender, EventArgs e)
        {
            if (chkList.SelectedItems.Count > 0)
            {
                //foreach (string itme in chkList.CheckedItems)
                //{
                //    string uNames = string.Empty;
                //    string[] split = itme.Split('-');
                //    if (split.Length == 2)
                //    {
                //        uNames = split[0].ToString().Trim();
                //    }
                    broadcastForCommands("deselect 12736",  false);
               // }
            }
        }

        private void btnStatus_Click(object sender, EventArgs e)
        {
            if (chkList.SelectedItems.Count > 0)
            {
                //foreach (string itme in chkList.CheckedItems)
                //{
                //    string uNames = string.Empty;
                //    string[] split = itme.Split('-');
                //    if (split.Length == 2)
                //    {
                //        uNames = split[0].ToString().Trim();
                //    }
                    broadcastForCommands("status",  false);
                //}
            }
        }

        private void chkList_SelectedValueChanged(object sender, EventArgs e)
        {
            SelectedclientsList.Clear();
            if (chkList.CheckedItems.Count > 0)
            {
                foreach (string itme in chkList.CheckedItems)
                {                   

                    SelectedclientsList.Add(itme, true);
                }
            }
            else
            {
                SelectedclientsList.Clear();
            }
        //  if(  SelectedclientsList.Add(clientSocket.Client.RemoteEndPoint.ToString(), false);
        }

        private void chkList_SelectedIndexChanged(object sender, EventArgs e)
        {

        } 
    }


    public class handleClinet
    {
        TcpClient clientSocket;
        string clNo;
       Hashtable clientsList;

        public void startClient(TcpClient inClientSocket, string clineNo, Hashtable cList)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            this.clientsList = cList;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }

        private void doChat()
        {
            int requestCount = 0;
            byte[] bytesFrom = new byte[10025];
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
            string rCount = null;
            requestCount = 0;

            while ((true))
            {
                try
                {
                    Thread.Sleep(300);
                    bytesFrom = new byte[10025];
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                   networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                 // Array.Resize(ref bytesFrom, noOfBytes);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);

                   // dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    //Console.WriteLine("From client - " + clNo + " : " + dataFromClient);
                    rCount = Convert.ToString(requestCount);

                    Server1 obj = new Server1();
                    obj.broadcast(dataFromClient, ref clNo, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }//end while
        }//end doChat
    }
}
