using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Test_DB_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(IPAddress.Parse("192.168.0.103"), 8888);
                JsonUser dataJson = new JsonUser
                {
                    Request = "Registration",
                    Login = "kek",
                    Password = "12345",
                    Gmail = "gamejudicator@gmail.com"
                };
                var a = JsonSerializer.Serialize<JsonUser>(dataJson);
                byte[] data = Encoding.UTF8.GetBytes(a);
                using (NetworkStream clientStream = client.GetStream())
                {
                    clientStream.Write(data, 0, data.Length);
                    byte[] dataRecieve = new byte[256];
                    int read = clientStream.Read(dataRecieve, 0, dataRecieve.Length);
                    string result = Encoding.UTF8.GetString(dataRecieve, 0, read);
                    Console.WriteLine(result);
                }
            }
        }
    }
    public class JsonUser
    {
        public string Request { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Gmail { get; set; }
    }
}
