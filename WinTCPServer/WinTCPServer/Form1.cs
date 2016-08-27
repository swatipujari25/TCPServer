using System;
using System.Threading;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinTCPServer
{
    public partial class Form1 : Form
    {
        public delegate void DisplayDelegate();
        DisplayDelegate delg = null;
        string val = string.Empty;


        public static Hashtable clientsList = new Hashtable(); 
        public Form1()
        {
            InitializeComponent();
        }

        public void BindToRichText()
        {
            richTextBox1.AppendText(val);
        }

        private void InvokeTextBox()
        {
            richTextBox1.Invoke(delg);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            TcpListener serverSocket = new TcpListener(8000);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            serverSocket.Start();
            timer1.Enabled = true;
            val = "Chat Server Started ....";
          //  richTextBox1.AppendText("Chat Server Started ....");
            counter = 0;
            while ((true))
            {
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();

                byte[] bytesFrom = new byte[10025];
                string dataFromClient = null;

                NetworkStream networkStream = clientSocket.GetStream();
                networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
               // dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                clientsList.Add(dataFromClient, clientSocket);

                broadcast(dataFromClient + " Joined ", dataFromClient, false);

            //    InvokeTextBox();
                //richTextBox1.AppendText(dataFromClient + " Joined chat room ");
                //handleClinet client = new handleClinet();
                //client.startClient(clientSocket, dataFromClient, clientsList);
            }

            clientSocket.Close();
            serverSocket.Stop();
            richTextBox1.AppendText("exit");
            
        }

        public  void broadcast(string msg, string uName, bool flag)
        {
            foreach (DictionaryEntry Item in clientsList)
            {
                TcpClient broadcastSocket;
                broadcastSocket = (TcpClient)Item.Value;
                NetworkStream broadcastStream = broadcastSocket.GetStream();
                Byte[] broadcastBytes = null;

                if (flag == true)
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(uName + " says : " + msg);
                }
                else
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(msg);
                }

                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();
            }
        } 


        private void Form1_Load(object sender, EventArgs e)
        {
            delg = new DisplayDelegate(BindToRichText);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            NewMethod(true);
        }

        private static void NewMethod(bool flag)
        {
            string msg = string.Empty;
            TcpClient broadcastSocket;
            broadcastSocket = new TcpClient();// (TcpClient)Item.Value;
            NetworkStream broadcastStream = broadcastSocket.GetStream();
            Byte[] broadcastBytes = null;

            if (flag == true)
            {
                broadcastBytes = Encoding.ASCII.GetBytes("Server says : " + msg);
            }
            else
            {
                broadcastBytes = Encoding.ASCII.GetBytes(msg);
            }

            broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
            broadcastStream.Flush();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(val))
            {
                InvokeTextBox();
              
                //timer1.Enabled = false;
            }
        }
    }


    //public class handleClinet
    //{
    //    TcpClient clientSocket;
    //    string clNo;
    //    Hashtable clientsList;

    //    public void startClient(TcpClient inClientSocket, string clineNo, Hashtable cList)
    //    {
    //        this.clientSocket = inClientSocket;
    //        this.clNo = clineNo;
    //        this.clientsList = cList;
    //        Thread ctThread = new Thread(doChat);
    //        ctThread.Start();
    //    }

    //    private void doChat()
    //    {
    //        int requestCount = 0;
    //        byte[] bytesFrom = new byte[10025];
    //        string dataFromClient = null;
    //        Byte[] sendBytes = null;
    //        string serverResponse = null;
    //        string rCount = null;
    //        requestCount = 0;

    //        while ((true))
    //        {
    //            try
    //            {
    //                requestCount = requestCount + 1;
    //                NetworkStream networkStream = clientSocket.GetStream();
    //                networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
    //                dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
    //                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
    //                Console.WriteLine("From client - " + clNo + " : " + dataFromClient);
    //                rCount = Convert.ToString(requestCount);

    //                Form1 obj = new Form1();
    //                obj.broadcast(dataFromClient, clNo, true);
    //            }
    //            catch (Exception ex)
    //            {
    //                Console.WriteLine(ex.ToString());
    //            }
    //        }//end while
    //    }//end doChat
    //}
}
