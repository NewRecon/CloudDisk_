using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Runtime.InteropServices.ComTypes;
using System.Reflection;
using System.Runtime.InteropServices;

namespace File_Server
{
    internal class Program
    {
        static TcpListener file_Server = new TcpListener(IPAddress.Parse("192.168.0.103"), 8888);
        static string pathMainDirectory = @"C:\Users\gamej\Desktop\";
        static async Task Main(string[] args)
        {

            file_Server.Start();
            var t = Task.Run(() => RequestsClientsAsync());
            t.Wait();
        }



        static async Task RequestsClientsAsync()
        {
            while (true)
            {
                await Task.Yield();
                TcpClient client = await file_Server.AcceptTcpClientAsync();
                _ = Task.Run(() => GetAndSendRequestAsync(client));
            }
        }

        static async Task GetAndSendRequestAsync(TcpClient client)
        {
            using (NetworkStream ns = client.GetStream())
            {
                byte[] getBytes = new byte[1024];
                int count = await ns.ReadAsync(getBytes, 0, getBytes.Length);
                string result = Encoding.UTF8.GetString(getBytes, 0, count);
                var userRequest = JsonSerializer.Deserialize<JsonRequest>(result);



                if (userRequest.Request == "Info")//протестировано
                    await FileInfo(ns, userRequest.Login);

                else if (userRequest.Request == "CreateDirectory")//протестировано
                    await CreateDirectory(ns, userRequest.Path, userRequest.Login);

                else if (userRequest.Request == "DeleteFile")//протестировано
                    await FileDelete(ns, userRequest.Path, userRequest.Login);

                else if (userRequest.Request == "DeleteDirectory")//протестировано
                    await DeleteDirectory(ns, userRequest.Path, userRequest.Login);

                else if (userRequest.Request == "Upload")//протестировано
                    await UploadFile(ns, userRequest.Path, userRequest.Login);

                else if (userRequest.Request == "Download")//протестировано
                    await DownloadFile(ns, userRequest.Path);
            }
        }

        static async Task UploadFile(NetworkStream ns, string path, string login)
        {
            byte[] bytes = new byte[4096];
            using (FileStream fs = new FileStream(pathMainDirectory + path, FileMode.OpenOrCreate))
            {
                var count = await ns.ReadAsync(bytes, 0, bytes.Length);
                while (count > 0)
                {
                    Console.WriteLine($"вход каунт {count}");
                    await fs.WriteAsync(bytes, 0, count);
                    if (count < 4096)
                        break;
                    count = await ns.ReadAsync(bytes, 0, bytes.Length);
                    Console.WriteLine($"выход каунт {count}");
                }
            }
            Console.WriteLine("файл получен");
            await FileInfo(ns, login);
        }

        static async Task DownloadFile(NetworkStream ns, string path)
        {
            byte[] file = File.ReadAllBytes(pathMainDirectory + path);
            await ns.WriteAsync(file, 0, file.Length);
        }

        static async Task FileInfo(NetworkStream ns, string login)
        {
            IEnumerable<string> allFiles = Directory.EnumerateFiles(pathMainDirectory + login, "*.*", SearchOption.AllDirectories);
            StringBuilder fileInfo = new StringBuilder();
            foreach (string filename in allFiles)
            {
                int snipFilePath = filename.LastIndexOf(login);
                fileInfo.Append(filename.Substring(snipFilePath) + ";" + new FileInfo(filename).Length + ";");
            }

            IEnumerable<string> allDirectory = Directory.EnumerateDirectories(pathMainDirectory + login, "*.*", SearchOption.AllDirectories);
            StringBuilder directoryInfo = new StringBuilder();
            foreach (string filename in allDirectory)
            {
                int snipDirectoryPath = filename.LastIndexOf(login);
                directoryInfo.Append(filename.Substring(snipDirectoryPath) + ";");
            }

            var allInfo = JsonSerializer.Serialize<JsonInfo>(new JsonInfo()
            {
                FileInfo = fileInfo.ToString(),
                DirectoryInfo = directoryInfo.ToString()
            });

            byte[] data = Encoding.UTF8.GetBytes(allInfo);
            await ns.WriteAsync(data, 0, data.Length);

            Console.WriteLine("Файл предан");
        }

        static async Task CreateDirectory(NetworkStream ns, string path, string login)
        {
            Directory.CreateDirectory(pathMainDirectory + path);
            await FileInfo(ns, login);
        }

        static async Task FileDelete(NetworkStream ns, string path, string login)
        {
            File.Delete(pathMainDirectory + path);
            await FileInfo(ns, login);
        }

        static async Task DeleteDirectory(NetworkStream ns, string path, string login)
        {
            Directory.Delete(pathMainDirectory + path);
            await FileInfo(ns, login);
        }
    }

    public class JsonRequest
    {
        public string Request { get; set; }
        public string Login { get; set; }
        public string Path { get; set; }
    }

    public class JsonInfo
    {
        public string FileInfo { get; set; }
        public string DirectoryInfo { get; set; }
    }
}

