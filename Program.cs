using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace TCPTest
{
    class Program
    {
        static void Main(string[] args)
        {
            AsynchronousClient.StartClient();
            //SendTCPCommand();
            Console.ReadLine();
        }

        private static void SendTCPCommand()
        {
            TcpClient tcpclnt = new TcpClient();
            Console.WriteLine("Connecting.....");

            tcpclnt.Connect("102.182.216.90", 15009);
            // use the ipaddress as in the server program

            Console.WriteLine("Connected");
            Console.Write("Enter the string to be transmitted : ");

            String str = Console.ReadLine();
            Stream stm = tcpclnt.GetStream();

            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(str);
            Console.WriteLine("Transmitting.....");

            stm.Write(ba, 0, ba.Length);

            byte[] bb = new byte[100];
            int k = stm.Read(bb, 0, 100);

            for (int i = 0; i < k; i++)
                Console.Write(Convert.ToChar(bb[i]));

            tcpclnt.Close();
        }
    }
}
