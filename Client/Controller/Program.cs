using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Authentication;

namespace Controller
{
    internal class Program
    {
        static IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.99"), 9000);
        static TcpClient client = new TcpClient();

        static void Main(string[] args)
        {
            Autorization("abobus","123321");
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
            //if (sslPolicyErrors == SslPolicyErrors.None)
            //    return true;

            //Console.WriteLine($"Certificate error: {sslPolicyErrors}");
            //return false;
        }

        static public void Autorization(string login, string password)
        {
            //X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            //store.Open(OpenFlags.ReadOnly);
            //X509CertificateCollection cert = store.Certificates.Find(X509FindType.FindBySubjectName, "CloudDiskCer", false);

            try
            {
                client.Connect(endPoint);
                using (SslStream sslStream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null))
                {
                    sslStream.AuthenticateAsClient("CloudDiskCer");

                    JsonToSend send = new JsonToSend()
                    {
                        Command = "Autorization",
                        Login = login,
                        Password = password
                    };
                    byte[] messsage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(send));
                    sslStream.Write(messsage);
                    sslStream.Flush();
                }             
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //public bool Registration(string login, string password)
        //{

        //}
    }

    class JsonToSend
    {
        public string Command { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
