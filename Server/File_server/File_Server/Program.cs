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
        static TcpListener file_Server = new TcpListener(IPAddress.Any, 8888);
        static string pathMainDirectory = Directory.GetCurrentDirectory() + @"\test\";
        static async Task Main(string[] args)
        {

            file_Server.Start();
            await Console.Out.WriteLineAsync("Started");
            var t = Task.Run(() => RequestsClientsAsync());
            t.Wait();
        }

        static async Task RequestsClientsAsync()
        {
            while (true)
            {
                await Task.Yield();
                TcpClient client = await file_Server.AcceptTcpClientAsync();
                await Console.Out.WriteLineAsync("Connected");
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

                await Console.Out.WriteLineAsync("userRequest.Request - " + userRequest.Request);
                await Console.Out.WriteLineAsync("userRequest.Key - " + userRequest.Key);
                await Console.Out.WriteLineAsync("userRequest.Path - " + userRequest.Path);

                // +
                if (userRequest.Request == "Info")
                {
                    await Console.Out.WriteLineAsync("Info");
                    await FileAndDirectoryInfoAsync(ns, userRequest.Key);
                }

                // +
                else if (userRequest.Request == "CreateDirectory")
                {
                    await Console.Out.WriteLineAsync("CreateDirectory");
                    await CreateDirectoryAsync(ns, userRequest.Path, userRequest.Key);
                }
                    
                // +
                else if (userRequest.Request == "DeleteFile")
                {
                    await Console.Out.WriteLineAsync("DeleteFile");
                    await FileDeleteAsync(ns, userRequest.Path, userRequest.Key);
                }
                    
                // +
                else if (userRequest.Request == "DeleteDirectory")
                {
                    await Console.Out.WriteLineAsync("DeleteDirectory");
                    await DeleteDirectoryAsync(ns, userRequest.Path, userRequest.Key);
                }

                // +
                else if (userRequest.Request == "Upload")
                {
                    await Console.Out.WriteLineAsync("Upload");
                    await UploadFileAsync(ns, userRequest.Path, userRequest.Key);
                }
                    
                // проработать манифест для скачивания директории
                else if (userRequest.Request == "Download")
                {
                    await Console.Out.WriteLineAsync("Download");
                    await DownloadFileAsync(ns, userRequest.Path);
                }

                // +
                else if (userRequest.Request == "Registration")
                {
                    await Console.Out.WriteLineAsync("Registration");
                    await CreateMainDirectoryAsync(ns, userRequest.Key);
                }
            }
        }

        static async Task UploadFileAsync(NetworkStream ns, string path, string key)
        {
            byte[] answer = Encoding.UTF8.GetBytes("ok");
            await ns.WriteAsync(answer, 0, answer.Length);

            byte[] bytes = new byte[4096];
            using (FileStream fs = new FileStream(pathMainDirectory + path, FileMode.OpenOrCreate))
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
            await FileAndDirectoryInfoAsync(ns, key);
        }

        static async Task DownloadFileAsync(NetworkStream ns, string path)
        {
            byte[] file = File.ReadAllBytes(pathMainDirectory + path);
            await ns.WriteAsync(file, 0, file.Length);
        }

        // изменить манифест
        static async Task FileAndDirectoryInfoAsync(NetworkStream ns, string key)
        {
            IEnumerable<string> allFiles = Directory.EnumerateFiles(pathMainDirectory + key, "*.*", SearchOption.TopDirectoryOnly);//Заменил,для поиска в текущей директории
            StringBuilder fileInfo = new StringBuilder();
            foreach (string filename in allFiles)
            {
                int snipFilePath = filename.LastIndexOf(key);
                fileInfo.Append(filename.Substring(snipFilePath) + ";" + new FileInfo(filename).Length + ";");
            }

            IEnumerable<string> allDirectory = Directory.EnumerateDirectories(pathMainDirectory + key, "*.*", SearchOption.TopDirectoryOnly);//Заменил,для поиска в текущей директории
            StringBuilder directoryInfo = new StringBuilder();
            foreach (string filename in allDirectory)
            {
                int snipDirectoryPath = filename.LastIndexOf(key);
                directoryInfo.Append(filename.Substring(snipDirectoryPath) + ";");
            }

            byte[] data = Encoding.UTF8.GetBytes(fileInfo.ToString() + directoryInfo.ToString());
            await ns.WriteAsync(data, 0, data.Length);
            Console.WriteLine("Файл предан");
        }

        static async Task CreateDirectoryAsync(NetworkStream ns, string path, string key)
        {
            Directory.CreateDirectory(pathMainDirectory + path);
            await Console.Out.WriteLineAsync("CreateDirectoryAsync");
            await FileAndDirectoryInfoAsync(ns, key);
        }

        static async Task CreateMainDirectoryAsync(NetworkStream ns, string key)
        {
            Directory.CreateDirectory(pathMainDirectory + key);
            await Console.Out.WriteLineAsync("CreateMainDirectoryAsync");
            await FileAndDirectoryInfoAsync(ns, key);
        }

        static async Task FileDeleteAsync(NetworkStream ns, string path, string key)
        {
            File.Delete(pathMainDirectory + path);
            await Console.Out.WriteLineAsync("FileDeleteAsync");
            await FileAndDirectoryInfoAsync(ns, key);
        }

        static async Task DeleteDirectoryAsync(NetworkStream ns, string path, string key)
        {
            Directory.Delete(pathMainDirectory + path);
            await Console.Out.WriteLineAsync("DeleteDirectoryAsync");
            await FileAndDirectoryInfoAsync(ns, key);
        }
    }

    public class JsonRequest
    {
        public string Request { get; set; }
        public string Key { get; set; }
        public string Path { get; set; }
    }
}