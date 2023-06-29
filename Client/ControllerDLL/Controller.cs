using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ControllerDLL
{
    public class Controller
    {
        // Сервер БД
        static IPEndPoint endPointDB = new IPEndPoint(IPAddress.Parse("192.168.0.99"), 9000);
        // Файловый сервер
        static IPEndPoint endPointFile = new IPEndPoint(IPAddress.Parse("192.168.0.99"), 8888);

        static TcpClient client;

        // Для хранения мэйн директории пользователся
        static JsonToRecieve toRecieve = null;

        // заглушка
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
            //if (sslPolicyErrors == SslPolicyErrors.None)
            //    return true;

            //Console.WriteLine($"Certificate error: {sslPolicyErrors}");
            //return false;
        }

        static public async Task<bool> AutorizationAsync(string email, string password)
        {
            #region SSL
            //X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            //store.Open(OpenFlags.ReadOnly);
            //X509CertificateCollection cert = store.Certificates.Find(X509FindType.FindBySubjectName, "CloudDiskCer", false);
            #endregion
            client = new TcpClient();
            JsonToRecieve toRecieve = null;
            try
            {
                client.Connect(endPointDB);
                using (SslStream sslStream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null))
                //using (NetworkStream sslStream = client.GetStream())
                {
                    sslStream.AuthenticateAsClient("CloudDiskCer");

                    JsonToSend send = new JsonToSend()
                    {
                        Command = "Autorization",
                        Email = email,
                        Password = password
                    };
                    byte[] messsage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(send));
                    await sslStream.WriteAsync(messsage, 0, messsage.Length);

                    messsage = new byte[1024];
                    int recievedMessageSize = await sslStream.ReadAsync(messsage, 0, messsage.Length);
                    string recievedMessage = Encoding.UTF8.GetString(messsage, 0, recievedMessageSize);
                    toRecieve = JsonSerializer.Deserialize<JsonToRecieve>(recievedMessage);

                    sslStream.Flush();
                }
                client.Close();
                client.Dispose();
            }
            catch (Exception ex){}

            if (toRecieve.Directory == "NaN")
                return false;
            else
                return true;
        }

        public static async Task<bool> RegistrationAsync(string email, string password)
        {
            client = new TcpClient();
            try
            {
                client.Connect(endPointDB);
                using (SslStream sslStream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null))
                //using (NetworkStream sslStream = client.GetStream())
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
                    toRecieve = JsonSerializer.Deserialize<JsonToRecieve>(recievedMessage);

                    sslStream.Flush();
                }
                client.Close();
                client.Dispose();
            }
            catch (Exception ex) { }

            if (toRecieve.Directory == "NaN")
                return false;
            else
                return true;
        }

        // отображение всего диска
        public static async Task<string> ShowAllFileInfoAsync(string path)
        {
            client = new TcpClient();
            try
            {
                client.Connect(endPointFile);
                using (SslStream sslStream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null))
                {
                    sslStream.AuthenticateAsClient("CloudDiskCer");

                    //

                    sslStream.Flush();
                }
                client.Close();
                client.Dispose();
            }
            catch (Exception ex) { }

            return "";
        }


    }

    // Для отправки на сервер БД
    public class JsonToSend
    {
        public string Command { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // Для получения инфы с БД отправки файловуму серверу
    public class JsonToRecieve
    {
        public string Command { get; set; }
        public string Directory { get; set; }
    }
}
