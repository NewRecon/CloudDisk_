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

        static async Task Main(string[] args)
        {
            //await AutorizationAsync("abobus", "123321");
        }

        // заглушка
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
            //if (sslPolicyErrors == SslPolicyErrors.None)
            //    return true;

            //Console.WriteLine($"Certificate error: {sslPolicyErrors}");
            //return false;
        }

        static public async Task AutorizationAsync(string email, string password)
        {
            #region SSL
            //X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            //store.Open(OpenFlags.ReadOnly);
            //X509CertificateCollection cert = store.Certificates.Find(X509FindType.FindBySubjectName, "CloudDiskCer", false);
            #endregion

            try
            {
                client.Connect(endPoint);
                using (SslStream sslStream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null))
                {
                    sslStream.AuthenticateAsClient("CloudDiskCer");

                    JsonToSend send = new JsonToSend()
                    {
                        Command = "Autorization",
                        Email = email,
                        Password = password
                    };
                    byte[] messsage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(send));
                    await sslStream.WriteAsync(messsage,0,messsage.Length);

                    messsage = new byte[1024];
                    int recievedMessageSize = await sslStream.ReadAsync(messsage, 0, messsage.Length);
                    string recievedMessage = Encoding.UTF8.GetString(messsage,0,recievedMessageSize);
                    JsonToRecieve toRecieve = JsonSerializer.Deserialize<JsonToRecieve>(recievedMessage);

                    sslStream.Flush();
                }             
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task RegistrationAsync(string email, string password)
        {
            try
            {
                client.Connect(endPoint);
                using (SslStream sslStream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null))
                {
                    sslStream.AuthenticateAsClient("CloudDiskCer");

                    JsonToSend send = new JsonToSend()
                    {
                        Command = "Registration",
                        Email = email,
                        Password = password
                    };
                    byte[] messsage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(send));
                    await sslStream.WriteAsync(messsage, 0, messsage.Length);

                    messsage = new byte[1024];
                    int recievedMessageSize = await sslStream.ReadAsync(messsage, 0, messsage.Length);
                    string recievedMessage = Encoding.UTF8.GetString(messsage, 0, recievedMessageSize);
                    JsonToRecieve toRecieve = JsonSerializer.Deserialize<JsonToRecieve>(recievedMessage);

                    sslStream.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class JsonToSend
    {
        public string Command { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class JsonToRecieve
    {
        public string Directory { get; set; }
    }
}
