using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.IO;

namespace DB_server
{
    internal class Program
    {
        static TcpClient client;
        static TcpListener listener = new TcpListener(IPAddress.Any, 9000);
        static X509Certificate2 serverCertificate = null;

        static async Task Main(string[] args)
        {
            serverCertificate = new X509Certificate2(@"C:\Users\99max\Desktop\CloudDisk\Server\CloudDisk.pfx", "123321", X509KeyStorageFlags.PersistKeySet);

            //X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            //store.Open(OpenFlags.ReadOnly);
            //X509CertificateCollection cert = store.Certificates.Find(X509FindType.FindBySubjectName, "localhost", false);
            //serverCertificate = cert[0];


            listener.Start();
            while (true)
            {
                await Task.Yield();
                client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => RecieveAsync());
            }
        }

        static async Task RecieveAsync()
        {
            

            await Console.Out.WriteLineAsync("Client connected");
            try
            {
                using (SslStream sslStream = new SslStream(client.GetStream(), false))
                {
                    sslStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);
                    sslStream.ReadTimeout = 5000;
                    sslStream.WriteTimeout = 5000;
                    JsonToRecieve recieve = JsonSerializer.Deserialize<JsonToRecieve>(await ReadMessageAsync(sslStream));
                    if (recieve.Command == "Autorization")
                    {
                        await Console.Out.WriteLineAsync(recieve.Command); // debug
                        await Console.Out.WriteLineAsync(recieve.Login); // debug
                        await Console.Out.WriteLineAsync(recieve.Password); // debug
                    }
                    else if (recieve.Command == "Registration")
                    {
                        await Console.Out.WriteLineAsync(recieve.Command); // debug
                    }
                }
            }
            catch(Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }

        static async Task<string> ReadMessageAsync(SslStream sslStream)
        {
            byte[] recieve = new byte[1024];
            int recieveSize = await sslStream.ReadAsync(recieve, 0, recieve.Length);
            return Encoding.UTF8.GetString(recieve, 0, recieveSize);
        }
    }

    class JsonToRecieve
    {
        public string Command { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
    class JsonToSend
    {
        public string Directory { get; set; }
    }

    class Entity
    {

    }
}
