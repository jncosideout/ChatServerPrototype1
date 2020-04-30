using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace ChatServerPrototype1.ChatServer
{
    public class ChatServer
    {

        private static readonly int portNumber = 4444;
        private List<EchoThread> clients;
        public List<EchoThread> getClients()
        {
            return clients;
        }
        public ChatServer(int portNumber)
        {
        }

        public static int Main(string[] args)
        {
            ChatServer server = new ChatServer(portNumber);
            server.StartServer();
            return 0;
        }

        public void StartServer()
        {
            clients = new List<EchoThread>();
            // Get Host IP Address that is used to establish a connection  
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
            // If a host has multiple addresses, you will get a list of addresses 
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, portNumber);

            try 
            {
                // Create a Socket that will use TCP protocol
                TcpListener listener = new TcpListener(localEndPoint);
                // A Socket must be associated with an endpoint using the Bind method
                listener.Start();
                acceptClients(listener);
                
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

            }

            Console.WriteLine("\nPress any key to continue...");
            // Console.ReadLine();
        }
        private void acceptClients(TcpListener listener)
        {
            while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                    try
                    {
                        TcpClient handler = listener.AcceptTcpClient();
                        Console.WriteLine("server accepted: RemoteEndPoint = " + handler.Client.RemoteEndPoint.ToString());
                        EchoThread echoThread = new EchoThread(this, handler);
                        Thread workThread = new Thread(new ThreadStart(echoThread.Run));
                        workThread.Start();
                        clients.Add(echoThread);
                    } 
                    catch (Exception e) 
                    {
                        Console.WriteLine($"Problem in {0}", nameof(acceptClients));
                        throw(e);
                    }
            }


                
        }
    }
}
