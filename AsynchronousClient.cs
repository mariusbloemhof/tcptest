using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace TCPTest
{
    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 512;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    public static class AsynchronousClient
    {
        // The port number for the remote device.  
        //private const string hostName = "102.182.216.90";
        //private const string hostName = "197.234.175.66";
        private const string hostName = "102.140.164.98";
        private const int port = 4567;

        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        // The response from the remote device.  
        private static String response = String.Empty;

        private static string JSON = @"{""EVENT"": {""SEQ"": ""2"",""DEVICENO"": ""12345678"",""EVENTCODE"": ""E120"",""EVENTTIME"": ""18 Jan 2017 09:10:11.000"",""ZONE"": ""1"",""ZONEDESC"": ""GARAGE"",""USERID"": ""1"",""PARTITION"": ""1"",""RECEIVER"": ""1"",""RSSI"": ""9"",""FORMAT"": ""PID"",""LONGITUDE"": ""-26.022695"",""LATITUDE"": ""28.001557"",""OBNUMBER"": [],""REFNO"": ""REF12345"",""MESSAGE"": ""THIS IS FREE TEXT"",""CUSTID"": []} }";

        //private static byte[] ByteData = Encoding.ASCII.GetBytes(JSON);
        private static byte[] ByteData = Encoding.ASCII.GetBytes(JSON + "\r\n");

        private static EventData SeedData()
        {
            var eventData = new EventData
            {
                Event = new EventContent {
                CustId = 828213175,
                Seq = 2,
                DeviceNo = Guid.NewGuid().ToString(),
                EventCode = "E120",
                Latitude = 28.001557,
                Longitude = -26.022695,
                EventTime = DateTime.Now,
                Zone = 0,
                RefNo = "0828213175",
                Message = "Panic event triggered"}
        };

            return eventData;

        }

        private static byte[] GetEventByteData()
        {
            var thisEvent = SeedData();
            var settings = new JsonSerializerSettings {DefaultValueHandling = DefaultValueHandling.Ignore};
            var json = JsonConvert.SerializeObject(thisEvent, settings);

            var byteData = Encoding.ASCII.GetBytes(json + "\r\n");

            return byteData;
        }


        public static void StartClient()
        {
            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // The name of the   
                // remote device is "host.contoso.com".  

                //RunClient(hostName, port);

                IPAddress ipAddress = IPAddress.Parse(hostName);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.  
                Socket client = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();

                // Send test data to the remote device.  
                Send(client, GetEventByteData());
                sendDone.WaitOne();

                client.Shutdown(SocketShutdown.Send);

                // Receive the response from the remote device.  
                Receive(client);
                receiveDone.WaitOne();

                // Write the response to the console.  
                Console.WriteLine("Response received : {0}", response);

                // Release the socket.  
                client.Shutdown(SocketShutdown.Both);
                client.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());
                Console.WriteLine();

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;

                Console.WriteLine($"Waiting for response from: {hostName}: {port}....");

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);

                    response = state.sb.ToString();
                    Console.WriteLine("Response received : {0}", response);
                    Console.WriteLine();
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.  
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Send(Socket client, byte[] data)
        {
            // Begin sending the data to the remote device.  
            client.BeginSend(data, 0, data.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                var data = GetEventByteData();
                Console.WriteLine("data send : {0}", Encoding.ASCII.GetString(data, 0, data.Length));
                Console.WriteLine();

                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
