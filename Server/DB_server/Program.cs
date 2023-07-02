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
using System.IO;

namespace DB_server
{
    internal class Program
    {
        static TcpListener listener = new TcpListener(IPAddress.Any, 9000);
        static X509Certificate serverCertificate = null;

        static async Task Main(string[] args)
        {
            // запустить от имени администратора
            #region SSL
<<<<<<< HEAD
            serverCertificate = new X509Certificate2(Directory.GetCurrentDirectory()+@"\CloudDisk.pfx", "123321", X509KeyStorageFlags.PersistKeySet);
=======
            serverCertificate = new X509Certificate2($@"{Directory.GetCurrentDirectory()}\CloudDisk.pfx", "123321", X509KeyStorageFlags.PersistKeySet);
>>>>>>> 2fd1ea3c4dccaa236e2b05ee8e098c17578f5391
            //X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            //store.Open(OpenFlags.ReadOnly);
            //X509CertificateCollection cert = store.Certificates.Find(X509FindType.FindBySubjectName, "localhost", false);
            //serverCertificate = cert[0];
            #endregion

            listener.Start();
            await Console.Out.WriteLineAsync("Server started");
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
                //using (NetworkStream sslStream = client.tcpClient.GetStream())
                {
                    sslStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);
                    sslStream.ReadTimeout = 5000;
                    sslStream.WriteTimeout = 5000;
                    client.json = JsonSerializer.Deserialize<JsonToRecieve>(await ReadMessageAsync(sslStream));

                    client.sslStream = sslStream;

                    if (client.json.Command == "Autorization")
                        await AuthorizationAsync(client);
                    else if (client.json.Command == "Registration")
                        await RegistartionAsync(client);
                }
            }
            catch(Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
            await Console.Out.WriteLineAsync("Disconnected");
        }

        static async Task AuthorizationAsync(Client client)
        {
            using (UserContext context = new UserContext())
            {
                User user = context.Users.FirstOrDefault(c => c.Email == client.json.Email && c.Password == client.json.Password);
                byte[] messageToSend;
                if (user != null)
                    messageToSend = Encoding.UTF8.GetBytes(user.Directory);
                else
                    messageToSend = Encoding.UTF8.GetBytes("NaN");

                await client.sslStream.WriteAsync(messageToSend, 0, messageToSend.Length);
            }
            await Console.Out.WriteLineAsync("Autorization");
        }

        static async Task RegistartionAsync(Client client)
        {
            using (UserContext context = new UserContext())
            {
                byte[] messageToSend;
                if (context.Users.FirstOrDefault(c => c.Email == client.json.Email && c.Password == client.json.Password) != null)
                    messageToSend = Encoding.UTF8.GetBytes("NaN");
                else
                {
                    string directory = Guid.NewGuid().ToString();
                    context.Users.Add(new User()
                    {
                        Email = client.json.Email,
                        Password = client.json.Password,
                        Directory = directory
                    });
                    context.SaveChanges();

                    messageToSend = Encoding.UTF8.GetBytes(directory);
                }

                await client.sslStream.WriteAsync(messageToSend, 0, messageToSend.Length);
            }
            await Console.Out.WriteLineAsync("Registration");
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
