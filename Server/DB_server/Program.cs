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
using DB_server.DataModel;

namespace DB_server
{
    internal class Program
    {
        static TcpListener listener = new TcpListener(IPAddress.Any, 9000);

        static X509Certificate2 serverCertificate = null;

        static async Task Main(string[] args)
        {
            // запустить от имени администратора
            #region SSL
            serverCertificate = new X509Certificate2(@"C:\Users\99max\Desktop\CloudDisk\Server\CloudDisk.pfx", "123321", X509KeyStorageFlags.PersistKeySet);
            //X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            //store.Open(OpenFlags.ReadOnly);
            //X509CertificateCollection cert = store.Certificates.Find(X509FindType.FindBySubjectName, "localhost", false);
            //serverCertificate = cert[0];
            #endregion

            listener.Start();
            while (true)
            {
                await Task.Yield();
                Client client = new Client(await listener.AcceptTcpClientAsync());
                _ = Task.Run(() => RecieveAsync(client));
            }
        }

        static async Task RecieveAsync(Client client)
        {
            await Console.Out.WriteLineAsync("Client connected");
            try
            {
                using (SslStream sslStream = new SslStream(client.tcpClient.GetStream(), false))
                {
                    sslStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);
                    sslStream.ReadTimeout = 5000;
                    sslStream.WriteTimeout = 5000;
                    client.sslStream = sslStream;
                    client.json = JsonSerializer.Deserialize<JsonToRecieve>(await ReadMessageAsync(sslStream));
                    if (client.json.Command == "Autorization")
                    {
                        await AutorizationAsync(client);
                    }
                    else if (client.json.Command == "Registration")
                    {
                        await RegistartionAsync(client);
                    }
                }
            }
            catch(Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }

        static async Task AutorizationAsync(Client client)
        {
            using (UserContext context = new UserContext())
            {
                User user = context.Users.FirstOrDefault(c => c.Email == client.json.Email && c.Password == client.json.Password);
                byte[] messageToSend;
                if (user != null)
                {
                    messageToSend = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new JsonToSend() {Directory = user.Directory}));
                }
                else
                {
                    messageToSend = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new JsonToSend() { Directory = "NaN" }));
                }

                await client.sslStream.WriteAsync(messageToSend, 0, messageToSend.Length);
            }
        }

        static async Task RegistartionAsync(Client client)
        {
            using (UserContext context = new UserContext())
            {
                byte[] messageToSend;
                if (context.Users.FirstOrDefault(c => c.Email == client.json.Email && c.Password == client.json.Password) != null)
                {
                    messageToSend = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new JsonToSend() { Directory = "NaN" }));
                }
                else
                {
                    string directory = new Guid(client.json.Email).ToString();
                    context.Users.Add(new User()
                    {
                        Email = client.json.Email,
                        Password = client.json.Password,
                        Directory = directory
                    });
                    context.SaveChanges();

                    JsonToSend toSend = new JsonToSend()
                    {
                        Directory = directory
                    };

                    messageToSend = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(toSend));
                }

                await client.sslStream.WriteAsync(messageToSend, 0, messageToSend.Length);
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
        public string Email { get; set; }
        public string Password { get; set; }
    }
    class JsonToSend
    {
        public string Directory { get; set; }
    }

    class Client
    {
        public TcpClient tcpClient;
        public SslStream sslStream;
        public JsonToRecieve json;

        public Client(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
        }
    }
}
