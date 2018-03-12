using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress myIp in ipHostInfo.AddressList)
            {
                Console.WriteLine(string.Format("myIp: {0} myFam: {1}", myIp, myIp.AddressFamily));
            }
            IPAddress ipAddress = ipHostInfo.AddressList[2];
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 1337);
            Console.WriteLine(string.Format("My IP ADDRESS: {0}", localEndPoint));
            TcpListener serverSocket = new TcpListener(localEndPoint);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            serverSocket.Start();
            Console.WriteLine(" >> " + "Server Started");

            counter = 0;
            while (true)
            {
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine(" >> " + "Client No:" + Convert.ToString(counter) + " started!");
                handleClinet client = new handleClinet();
                client.startClient(clientSocket, Convert.ToString(counter));
            }

            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine(" >> " + "exit");
            Console.ReadLine();
        }
    }

    //Class to handle each client request separatly
    public class handleClinet
    {
        TcpClient clientSocket;
        string clNo;
        public void startClient(TcpClient inClientSocket, string clineNo)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            Thread ctThread = new Thread(DoChat);
            ctThread.Start();
        }
        private void DoChat()
        {
            int requestCount = 0;
            Boolean connected = true;
            
            while ((connected))
            {
                try
                {
                    if (clientSocket.Connected)
                    {
                        requestCount = requestCount + 1;
                        NetworkStream networkStream = clientSocket.GetStream();
                        byte[] myReadBuffer = new byte[1024];
                        StringBuilder myCompleteMessage = new StringBuilder();
                        int numberOfBytesRead = 0;
                        do
                        {
                            numberOfBytesRead = networkStream.Read(myReadBuffer, 0, myReadBuffer.Length);
                            myCompleteMessage.AppendFormat("{0}", Encoding.UTF8.GetString(myReadBuffer, 0, numberOfBytesRead));
                        }
                        while (networkStream.DataAvailable);
                        if (numberOfBytesRead > 0)
                        {
                            string temp = myCompleteMessage.ToString();
                            Console.WriteLine(" >> " + "From client-" + temp + " " + DateTime.Now);
                            string[] myCommand = temp.Split(' ');
                            if (myCommand[0].Contains("Connect"))
                        {
                                Console.WriteLine("Client Connected " + clientSocket.Client.RemoteEndPoint + " " +  DateTime.Now);
                                string serverResponse = "404 OK\r\n";
                                byte[] sendBytes = Encoding.UTF8.GetBytes(serverResponse);
                                networkStream.Write(sendBytes, 0, sendBytes.Length);
                            }
                            else if (myCommand[0].Contains("Disconnect"))
                            {
                                connected = false;
                                Console.Write("Client D/C " + clientSocket.Client.RemoteEndPoint + " " + DateTime.Now);
                            }
                            //networkStream.Flush();
                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                }
            }
        }
    }
}