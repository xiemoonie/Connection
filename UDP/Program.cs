using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace UDP
{
    public class UDPSocket
    {
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private const int bufSize = 8 * 1024;
        private State state = new State();
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private AsyncCallback recv = null;

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public void Server(string address, int port)
        {
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            Receive();
        }

        public void Client(string address, int port)
        {
            _socket.Connect(IPAddress.Parse(address), port);
            Console.WriteLine($"Some info written here in client: address {address} port: {port}");
        }

        public void Send(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndSend(ar);
                Console.WriteLine("SEND: {0}, {1}", bytes, text);
            }, state);
        }

        private void Receive()
        {
            _socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
                _socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
                Console.WriteLine("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, Encoding.ASCII.GetString(so.buffer, 0, bytes));
            }, state);
        }
    }

    public class TCPGetMethod{

        public static void getMethod(NetworkStream networkStream)
        {

            Console.WriteLine("this is happening in getMethod");
            var message = @"GET / HTTP/1.1
            Accept: text/html, charset=utf-8
            Accept-Language: en-US
            User-Agent: C# program
            Connection: close
            Host: webcode.me" + "\r\n\r\n"; 

            Console.WriteLine(message);

            using var reader = new StreamReader(networkStream, Encoding.UTF8);
            byte[] bytes = Encoding.UTF8.GetBytes(message);

            networkStream.Write(bytes, 0, bytes.Length);
            Console.WriteLine(reader.ReadToEnd());
        }
        public static void postMethod(NetworkStream networkStream)
        {
            Console.WriteLine("this is happening in postMethod");
            var data = "name=John+Doe&occupation=gardener";
            var n = data.Length;

            var message = @$"POST /anything HTTP/1.1
                            Accept: */*
                            Accept-Language: en-US
                            User-Agent: C# program
                            Host: httpbin.org
                            Content-Type: application/x-www-form-urlencoded
                            Content-Length: {n}
                            Connection: close"
            + "\r\n\r\n" + data;

            Console.WriteLine(message);

            using var reader = new StreamReader(networkStream, Encoding.UTF8);
            byte[] bytes = Encoding.UTF8.GetBytes(message);

            networkStream.Write(bytes, 0, bytes.Length);

            Console.WriteLine("---------------------");

            Console.WriteLine(reader.ReadToEnd());
        }

    }


    internal class Program
    {
        public static object ConfigurationSettings { get; private set; }

        static void Main(string[] args)
        {
            //UDPListener.StartListener();

            //if it listens it is the server if it sends data it is the client

            /*
            UDPSocket s = new UDPSocket();
            s.Server("127.0.0.1", 27000);

            UDPSocket c = new UDPSocket();
            c.Client("127.0.0.1", 27000);
            c.Send("TEST!");
            
            */

            
            TcpClient client = new TcpClient();
       
            var hostname = "webcode.me";
            client.Connect(hostname, 80);

            using NetworkStream networkStream = client.GetStream();
            networkStream.ReadTimeout = 2000;
    
           
           var message = "HEAD / HTTP/1.1\r\nHost: webcode.me\r\nUser-Agent: C# program\r\n"
               + "Connection: close\r\nAccept: text/html\r\n\r\n";
           
           Console.WriteLine(message);

            using var reader = new StreamReader(networkStream, Encoding.UTF8);

            byte[] bytes = Encoding.UTF8.GetBytes(message);
            networkStream.Write(bytes, 0, bytes.Length);

            Console.WriteLine(reader.ReadToEnd());

            TCPGetMethod.getMethod(networkStream);
            //TCPGetMethod.postMethod(networkStream);


            Console.ReadKey();
            

        }
    }
}
