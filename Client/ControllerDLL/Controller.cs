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
using System.IO;

namespace ControllerDLL
{
    public class Controller
    {
        // Сервер БД
        static IPEndPoint endPointDB = new IPEndPoint(IPAddress.Parse("192.168.0.113"), 9000);
        // Файловый сервер
        static IPEndPoint endPointFile = new IPEndPoint(IPAddress.Parse("192.168.0.113"), 8888);

        static TcpClient client;

        // Для хранения мэйн директории пользователся
        static JsonToRecieveFromDBAndSentToFileServer toRecieve = new JsonToRecieveFromDBAndSentToFileServer();

        static string currentDirectory = "";

        // заглушка
        static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
            //if (sslPolicyErrors == SslPolicyErrors.None)
            //    return true;

            //Console.WriteLine($"Certificate error: {sslPolicyErrors}");
            //return false;
        }

        public static async Task<bool> AuthorizationAsync(string email, string password)
        {
            #region SSL
            //X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            //store.Open(OpenFlags.ReadOnly);
            //X509CertificateCollection cert = store.Certificates.Find(X509FindType.FindBySubjectName, "CloudDiskCer", false);
            #endregion
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
                        Command = "Autorization",
                        Email = email,
                        Password = password
                    };
                    byte[] messsage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(send));
                    await sslStream.WriteAsync(messsage, 0, messsage.Length);

                    messsage = new byte[1024];
                    int recievedMessageSize = await sslStream.ReadAsync(messsage, 0, messsage.Length);
                    string recievedMessage = Encoding.UTF8.GetString(messsage, 0, recievedMessageSize);
                    toRecieve.Key = recievedMessage;
                    currentDirectory = toRecieve.Key;

                    sslStream.Flush();
                }
                client.Close();
                client.Dispose();
            }
            catch (Exception ex){}

            if (toRecieve.Key == "NaN")
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
                    toRecieve.Key = recievedMessage;
                    currentDirectory = toRecieve.Key;

                    sslStream.Flush();
                }
                client.Close();
                client.Dispose();
            }
            catch (Exception ex) { }

            if (toRecieve.Key == "NaN")
                return false;
            else
                return true;
        }

        // отображение всего диска
        public static async Task<string> ShowAllFileInfoAsync(string path)
        {
            currentDirectory = path;
            client = new TcpClient();
            string recievedMessage = "";
            try
            {
                client.Connect(endPointFile);
                using (NetworkStream ns = client.GetStream())
                {
                    toRecieve.Request = "Info";
                    toRecieve.Path = currentDirectory;

                    byte[] messsage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(toRecieve));
                    await ns.WriteAsync(messsage, 0, messsage.Length);

                    messsage = new byte[1024];
                    int recievedMessageSize = await ns.ReadAsync(messsage, 0, messsage.Length);
                    recievedMessage = Encoding.UTF8.GetString(messsage, 0, recievedMessageSize);

                    ns.Flush();
                }
                client.Close();
                client.Dispose();
            }
            catch (Exception ex) { }

            return recievedMessage;
        }

        // Создание директории при регистрации
        public static async Task<string> CreateMainDirectoryAsync()
        {
            client = new TcpClient();
            string recievedMessage = "";
            try
            {
                client.Connect(endPointFile);
                using (NetworkStream ns = client.GetStream())
                {
                    toRecieve.Request = "Registration";
                    toRecieve.Path = toRecieve.Key;

                    byte[] messsage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(toRecieve));
                    await ns.WriteAsync(messsage, 0, messsage.Length);

                    messsage = new byte[1024];
                    int recievedMessageSize = await ns.ReadAsync(messsage, 0, messsage.Length);
                    recievedMessage = Encoding.UTF8.GetString(messsage, 0, recievedMessageSize);

                    ns.Flush();
                }
                client.Close();
                client.Dispose();
            }
            catch (Exception ex) { }

            return recievedMessage;
        }

        // затестить
        // Создание директории
        public static async Task<string> CreateDirectoryAsync(string path)
        {
            currentDirectory = path;
            client = new TcpClient();
            string recievedMessage = "";
            try
            {
                client.Connect(endPointFile);
                using (NetworkStream ns = client.GetStream())
                {
                    toRecieve.Request = "CreateDirectory";
                    toRecieve.Path = currentDirectory;

                    byte[] messsage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(toRecieve));
                    await ns.WriteAsync(messsage, 0, messsage.Length);

                    messsage = new byte[1024];
                    int recievedMessageSize = await ns.ReadAsync(messsage, 0, messsage.Length);
                    recievedMessage = Encoding.UTF8.GetString(messsage, 0, recievedMessageSize);

                    ns.Flush();
                }
                client.Close();
                client.Dispose();
            }
            catch (Exception ex) { }

            return recievedMessage;
        }

        // затестить
        // Загрузка на диск
        public static async Task<string> UploadFileAsync(string path, string file)
        {
            currentDirectory = path;
            client = new TcpClient();
            string recievedMessage = "";
            try
            {
                client.Connect(endPointFile);
                using (NetworkStream ns = client.GetStream())
                {
                    toRecieve.Request = "Upload";
                    
                    toRecieve.Path = currentDirectory+@"\"+ file.Substring(file.LastIndexOf(@"\") + 1);

                    byte[] messsage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(toRecieve));
                    await ns.WriteAsync(messsage, 0, messsage.Length);

                    await ns.ReadAsync(messsage,0, messsage.Length);

                    byte[] data = File.ReadAllBytes(file);
                    await ns.WriteAsync(data,0,data.Length);

                    messsage = new byte[1024];
                    int recievedMessageSize = await ns.ReadAsync(messsage, 0, messsage.Length);
                    recievedMessage = Encoding.UTF8.GetString(messsage, 0, recievedMessageSize);

                    ns.Flush();
                }
                client.Close();
                client.Dispose();
            }
            catch (Exception ex) { }

            return recievedMessage;
        }

        // затестить
        // Скачивание одного файла
        public static async Task DownloadFileAsync(string path, string file, string curDirectory) // path - путь куда сохраняем, file - имя файла, curDirectory - текущая папка на диске
        {
            currentDirectory = curDirectory;
            client = new TcpClient();
            string recievedMessage = "";
            try
            {
                client.Connect(endPointFile);
                using (NetworkStream ns = client.GetStream())
                {
                    toRecieve.Request = "DownloadFile";
                    toRecieve.Path = currentDirectory + file.Substring(file.LastIndexOf(@"\") + 1);

                    byte[] messsage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(toRecieve));
                    await ns.WriteAsync(messsage, 0, messsage.Length);

                    byte[] bytes = new byte[4096];
                    using (FileStream fs = new FileStream(path + file.Substring(file.LastIndexOf(@"\") + 1), FileMode.OpenOrCreate))
                    {
                        var count = await ns.ReadAsync(bytes, 0, bytes.Length);
                        while (count > 0)
                        {
                            await fs.WriteAsync(bytes, 0, count);
                            if (count < 4096)
                                break;
                            count = await ns.ReadAsync(bytes, 0, bytes.Length);
                        }
                    }

                    ns.Flush();
                }
                client.Close();
                client.Dispose();
            }
            catch (Exception ex) { }
        }

        // затестить
        // Скачивание директории
        public static async Task DownloadDirectoryAsync(string path, string file, string curDirectory)
        {
            currentDirectory = curDirectory;
            client = new TcpClient();
            string recievedMessage = "";
            try
            {
                client.Connect(endPointFile);
                using (NetworkStream ns = client.GetStream())
                {
                    toRecieve.Request = "DownloadDirectory";
                    toRecieve.Path = currentDirectory+ file.Substring(file.LastIndexOf(@"\") + 1);

                    byte[] messsage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(toRecieve));
                    await ns.WriteAsync(messsage, 0, messsage.Length);

                    byte[] bytes = new byte[4096];
                    using (FileStream fs = new FileStream(path + file.Substring(file.LastIndexOf(@"\") + 1) + ".zip", FileMode.OpenOrCreate))
                    {
                        var count = await ns.ReadAsync(bytes, 0, bytes.Length);
                        while (count > 0)
                        {
                            await fs.WriteAsync(bytes, 0, count);
                            if (count < 4096)
                                break;
                            count = await ns.ReadAsync(bytes, 0, bytes.Length);
                        }
                    }

                    ns.Flush();
                }
                client.Close();
                client.Dispose();
            }
            catch (Exception ex) { }
        }

        // затестить
        // Удаление файла
        public static async Task<string> DeleteFileAsync(string path, string file)
        {
            currentDirectory = path;
            client = new TcpClient();
            string recievedMessage = "";
            try
            {
                client.Connect(endPointFile);
                using (NetworkStream ns = client.GetStream())
                {
                    toRecieve.Request = "DeleteFile";
                    toRecieve.Path = currentDirectory + file.Substring(file.LastIndexOf(@"\") + 1);

                    byte[] messsage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(toRecieve));
                    await ns.WriteAsync(messsage, 0, messsage.Length);

                    messsage = new byte[1024];
                    int recievedMessageSize = await ns.ReadAsync(messsage, 0, messsage.Length);
                    recievedMessage = Encoding.UTF8.GetString(messsage, 0, recievedMessageSize);

                    ns.Flush();
                }
                client.Close();
                client.Dispose();
            }
            catch (Exception ex) { }

            return recievedMessage;
        }

        // затестить
        // Удаление директории
        public static async Task<string> DeleteDirectoryAsync(string path)
        {
            currentDirectory = path;
            client = new TcpClient();
            string recievedMessage = "";
            try
            {
                client.Connect(endPointFile);
                using (NetworkStream ns = client.GetStream())
                {
                    toRecieve.Request = "DeleteDirectory";
                    toRecieve.Path = currentDirectory;

                    byte[] messsage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(toRecieve));
                    await ns.WriteAsync(messsage, 0, messsage.Length);

                    messsage = new byte[1024];
                    int recievedMessageSize = await ns.ReadAsync(messsage, 0, messsage.Length);
                    recievedMessage = Encoding.UTF8.GetString(messsage, 0, recievedMessageSize);

                    ns.Flush();
                }
                client.Close();
                client.Dispose();
            }
            catch (Exception ex) { }

            return recievedMessage;
        }
    }

    // Для отправки на сервер БД
    public class JsonToSend
    {
        public string Command { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // Для получения инфы с БД и для отправки файловуму серверу
    public class JsonToRecieveFromDBAndSentToFileServer
    {
        public string Request { get; set; }
        public string Key { get; set; }
        public string Path { get; set; }
    }
}
