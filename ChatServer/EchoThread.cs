using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;

namespace ChatServerPrototype1.ChatServer
{
    using ChatServerPrototype1.Utilities;
    public class EchoThread
    {
        private TcpClient handler;
        private ChatServer server;
        private NetworkStream networkStream = null;
        private DataContractSerializer serializer = null;
        public EchoThread(ChatServer server, TcpClient handler)
        {
            this.handler = handler;
            this.server = server;
        }

        public void Run()
        {
            // Prepare to serialize incoming data from the client
            networkStream = handler.GetStream();
            serializer = new DataContractSerializer(typeof(Message));
                
            while (handler.Connected)
            {
                if (networkStream.CanRead)
                {
                    Message msg = receiveIncomingData();
                    Console.WriteLine("Text received: {0}", msg.TheMessage);
                    serializeOutgoingData(msg);
                }
            }

            networkStream.Close();
            handler.Close();
        }

        private Message receiveIncomingData()
        {
            Message newMessage = null;
            var memStream = new MemoryStream();
            try {
                readBytesInto(ref memStream);
                newMessage = deserializeFromFirstXMLelement(ref memStream);
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Exception message:\n{0}",e.Message);
                Console.WriteLine("Stack trace:\n{0}", e.StackTrace);

            }
            finally 
            {
                memStream?.Dispose();
            }
            return newMessage;
            
        }

        private void readBytesInto(ref MemoryStream memStream)
        {
            // read in chunks of 2kb
            int buffSize = 2048;
            byte[] buffer1 = new byte[buffSize];
            int bytesRead;
            int offset = 0;
            
            do {
                bytesRead = networkStream.Read(buffer1, 0, buffSize);                    
                memStream.Write(buffer1, offset, bytesRead);
                System.Array.Clear(buffer1, 0, buffSize);
                offset += bytesRead;
            } 
            while (networkStream.DataAvailable);
        }
        private Message deserializeFromFirstXMLelement(ref MemoryStream memStream)
        {
            Message msg = null;
            byte[] bMessage = memStream.ToArray();

            XmlDictionaryReader xmlDictReader = XmlDictionaryReader.CreateTextReader(bMessage, 0, bMessage.Length, new XmlDictionaryReaderQuotas());
            while (xmlDictReader.Read())
            {
                switch (xmlDictReader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (serializer.IsStartObject(xmlDictReader))
                        {
                            Console.WriteLine("Found start element");
                            msg = (Message)serializer.ReadObject(xmlDictReader);
                        }
                        Console.WriteLine(xmlDictReader.Name);
                        break;
                }
            }
            return msg;            
        }
        private void serializeOutgoingData(Message msg)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                serializer.WriteObject(memStream, msg); 
                string xmlOutput = toXML(msg); 
                Console.WriteLine(xmlOutput);                 
                byte[] bMessage = memStream.ToArray();
                networkStream.Write(bMessage, 0, bMessage.Length);
            }       
        }

        private string toXML(Message message)
        {
            using (var output = new StringWriter())
            using (var writer = new XmlTextWriter(output) {Formatting = Formatting.Indented})
            {
                serializer.WriteObject(writer, message);
                return output.GetStringBuilder().ToString();
            }
        }        

    }

}
