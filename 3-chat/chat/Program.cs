using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;



namespace chat
{
    class Program
    {

        public static int portL;
        public static int portG;
        public static string NameOfUser;
        public static int AmountOfUsers = 0;
        public struct Users
        {
            public string name;
            public IPAddress UserIp;
        }

        public static Users[] ArrayofUsers = new Users[10];
        public static NetworkStream[] ArrayofOnlineUsers = new NetworkStream[10];
        public static TcpClient[] ArrayofClients = new TcpClient[10];
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Укажите порт отправки");
                portG = Convert.ToInt16(Console.ReadLine());
                Console.WriteLine("Укажите порт прослушки");
                portL = Convert.ToInt16(Console.ReadLine());
                Console.Write("Введите имя:");
                NameOfUser = Console.ReadLine();
                UdpClient message = new UdpClient();
                IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, portG);
                byte[] bytes = Encoding.UTF8.GetBytes(NameOfUser);
                message.Send(bytes, bytes.Length, ip);
                Thread receiveThreadTcp = new Thread(new ThreadStart(ReceiveTcpConnection));
                receiveThreadTcp.Start();
                Thread receiveThread = new Thread(new ThreadStart(ReceiveUdpMessage));
                receiveThread.Start();
                Thread receiveThreadTcpMess = new Thread(new ThreadStart(ReceiveTcpMessages));
                receiveThreadTcpMess.Start();
                string s;
                while (true)
                {
                    s = Console.ReadLine();
                    for (int i = 0; i < AmountOfUsers; i++)
                    {
                        if (s == "exit")
                        {
                            byte[] exitmess = Encoding.UTF8.GetBytes(s);
                            ArrayofOnlineUsers[i].Write(exitmess, 0, exitmess.Length);
                        }
                        else
                        {
                            byte[] buffer = Encoding.UTF8.GetBytes(NameOfUser + ":" + s);
                            ArrayofOnlineUsers[i].Write(buffer, 0, buffer.Length);
                        }
                        
                    }
                    if (s == "exit")
                    {
                            
                            ArrayofClients[0].Client.Shutdown(SocketShutdown.Send);
                            ArrayofOnlineUsers[0].Close();
                            ArrayofClients[0].Close();
                        
                        Environment.Exit(0);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
 
            }
        }

        

       public static void ReceiveTcpMessages()
       {

           try
           {
               while (true)
               {
                   TcpListener ServerSocket = new TcpListener(IPAddress.Any, portG + 4);
                   ServerSocket.Start();
                   TcpClient client = ServerSocket.AcceptTcpClient();
                   while (true)
                   {
                       NetworkStream ns = client.GetStream();
                       int byte_count = 0;
                       byte[] buffer = new byte[1024];
                       try
                       {
                           byte_count = ns.Read(buffer, 0, buffer.Length);
                       }
                       catch (Exception ex)
                       {
                           
                            for (int j = 0; j < AmountOfUsers; j++)
                            {
                                if (ArrayofUsers[j].UserIp.ToString() == ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString())
                                {
                                    string l = ArrayofUsers[j].name;
                                    Console.WriteLine(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString() + " " + l + " " + "exit");
                                    for (int i = j; i < AmountOfUsers; i++)
                                    {
                                        ArrayofOnlineUsers[i] = ArrayofOnlineUsers[i + 1];
                                    }
                                    for (int i = j; i < AmountOfUsers; i++)
                                    {
                                        ArrayofClients[i] = ArrayofClients[i + 1];
                                    }
                                    AmountOfUsers = AmountOfUsers - 1;
                                }
                            }
                           break;
                       }
                       
                       if (byte_count == 0) break;
                       byte[] formated = new Byte[byte_count];
                       Array.Copy(buffer, formated, byte_count);
                       string returnData = Encoding.UTF8.GetString(formated);
                       if (returnData == "exit")
                       {
                           for (int j = 0; j < AmountOfUsers; j++)
                           {
                               if (ArrayofUsers[j].UserIp.ToString() == ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString())
                               {

                                   string l = ArrayofUsers[j].name;
                                   Console.WriteLine(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString() + " " + l + " " + returnData);
                                   for (int i = j; i < AmountOfUsers; i++)
                                   {
                                       ArrayofOnlineUsers[i] = ArrayofOnlineUsers[i + 1];
                                   }
                                   for (int i = j; i < AmountOfUsers; i++)
                                   {
                                       ArrayofClients[i] = ArrayofClients[i + 1];
                                   }
                                   AmountOfUsers = AmountOfUsers - 1;
                               }
                           }

                       }
                       else
                           Console.WriteLine(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString() + " " + returnData);
                   }

                   ServerSocket.Stop();
                   client.Close();


               }

           }
           catch (Exception ex)
           {
               Console.WriteLine(ex.Message);
           }
       }

       public static void ReceiveTcpConnection()
       {

           try
           {
               TcpListener ServerSocket = new TcpListener(IPAddress.Any, portG + 2);
               ServerSocket.Start();
               while (true)
               {
                   TcpClient client = ServerSocket.AcceptTcpClient();
                   NetworkStream ns = client.GetStream();
                   byte[] buffer = new byte[1024];
                   int byte_count = ns.Read(buffer, 0, buffer.Length);
                   byte[] formated = new Byte[byte_count];
                   Array.Copy(buffer, formated, byte_count);
                   string returnData = Encoding.UTF8.GetString(formated);
                   ArrayofUsers[AmountOfUsers].name = returnData;
                   ArrayofUsers[AmountOfUsers].UserIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                   Console.WriteLine(ArrayofUsers[AmountOfUsers].name + " присоединился его ip:" + ArrayofUsers[AmountOfUsers].UserIp.ToString());
                   ArrayofClients[AmountOfUsers] = new TcpClient();
                   ArrayofClients[AmountOfUsers].Connect(ArrayofUsers[AmountOfUsers].UserIp, portL + 4);
                   ArrayofOnlineUsers[AmountOfUsers] = ArrayofClients[AmountOfUsers].GetStream();
                   AmountOfUsers = AmountOfUsers + 1;
                   client.Close();
               } 
           }
           catch (Exception ex)
           {
               Console.WriteLine(ex.Message);
           }
       }

        public static void ReceiveUdpMessage()
        {
            UdpClient NameAndAdress = new UdpClient(portL);
            IPEndPoint ep = null; 
            try
            {
                while (true)
                {
                    byte[] data = NameAndAdress.Receive(ref ep);
                    string backmessage = Encoding.UTF8.GetString(data);
                    ArrayofUsers[AmountOfUsers].name = backmessage;
                    ArrayofUsers[AmountOfUsers].UserIp = ep.Address;
                    Console.WriteLine(ArrayofUsers[AmountOfUsers].name+" присоединился его ip:" + ArrayofUsers[AmountOfUsers].UserIp.ToString());
                    TcpClient tcpClient = new TcpClient();
                    tcpClient.Connect(ep.Address, portL+2);
                    NetworkStream tcpStream = tcpClient.GetStream();
                    byte[] sendBytes = Encoding.UTF8.GetBytes(NameOfUser);
                    tcpStream.Write(sendBytes, 0, sendBytes.Length);
                    ArrayofClients[AmountOfUsers] = new TcpClient();
                    ArrayofClients[AmountOfUsers].Connect(ep.Address, portL + 4);
                    ArrayofOnlineUsers[AmountOfUsers] = ArrayofClients[AmountOfUsers].GetStream();
                    AmountOfUsers = AmountOfUsers + 1;
                    tcpClient.Close();
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                NameAndAdress.Close();
            }
        }

    }        
}