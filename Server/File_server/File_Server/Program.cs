using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.IO.Compression;


namespace File_Server
{
    internal class Program
    {
        static TcpListener file_Server = new TcpListener(IPAddress.Any, 8888);
        static string pathMainDirectory = $@"{Environment.CurrentDirectory}\Files\";

        static async Task Main(string[] args)
        {
            file_Server.Start();
            await Console.Out.WriteLineAsync("Started");
            var t = Task.Run(() => RequestsClientsAsync());
            Console.WriteLine("Server start");
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
                    await FileAndDirectoryInfoAsync(ns, pathMainDirectory + userRequest.Key + @"\" + userRequest.Path);

                // +
                else if (userRequest.Request == "CreateDirectory")
                    await CreateDirectory(ns, pathMainDirectory + userRequest.Key + @"\" + userRequest.Path);

                else if (userRequest.Request == "DeleteFile")
                    await FileDelete(ns, pathMainDirectory + userRequest.Key + @"\" + userRequest.Path);

                else if (userRequest.Request == "DeleteDirectory")
                    await DeleteDirectory(ns, pathMainDirectory + userRequest.Key + @"\" + userRequest.Path);

                else if (userRequest.Request == "Upload")
                    await UploadFileAsync(ns, pathMainDirectory + userRequest.Key + @"\" + userRequest.Path);

                else if (userRequest.Request == "DownloadFile")
                    await DownloadFileAsync(ns, pathMainDirectory + userRequest.Key + @"\" + userRequest.Path);

                else if (userRequest.Request == "DownloadDirectory")
                    await DownloadDirectoryAsync(ns, pathMainDirectory + userRequest.Key + @"\" + userRequest.Path);
            }
        }

        static async Task UploadFileAsync(NetworkStream ns, string path)
        {
            byte[] answer = Encoding.UTF8.GetBytes("ok");
            await ns.WriteAsync(answer, 0, answer.Length);

            byte[] bytes = new byte[4096];
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
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
            await FileAndDirectoryInfoAsync(ns, path.Substring(0, path.LastIndexOf(@"\")));
        }

        static async Task DownloadFileAsync(NetworkStream ns, string path)
        {
            await Console.Out.WriteLineAsync(path);
            byte[] file = File.ReadAllBytes(path);
            await ns.WriteAsync(file, 0, file.Length);
        }

        static async Task DownloadDirectoryAsync(NetworkStream ns, string path)
        {
            ZipFile.CreateFromDirectory(path, path + ".zip");
            byte[] file = File.ReadAllBytes(path + ".zip");
            await ns.WriteAsync(file, 0, file.Length);
            await FileDelete(ns, path + ".zip");
        }

        static async Task FileAndDirectoryInfoAsync(NetworkStream ns, string path)
        {
            StringBuilder fileInfo = new StringBuilder();
            IEnumerable<string> allFiles = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly);
            foreach (string filename in allFiles)
            {
                fileInfo.Append(filename.Substring(filename.LastIndexOf(@"\") + 1) + ";" + new FileInfo(filename).Length + ";");
            }

            IEnumerable<string> allDirectory = Directory.EnumerateDirectories(path, "*.*", SearchOption.TopDirectoryOnly);
            foreach (string filename in allDirectory)
            {
                fileInfo.Append(filename.Substring(filename.LastIndexOf(@"\") + 1) + ";");
            }

            byte[] data = Encoding.UTF8.GetBytes(fileInfo.ToString());
            await ns.WriteAsync(data, 0, data.Length);
        }

        static async Task CreateDirectory(NetworkStream ns, string path)
        {
            Directory.CreateDirectory(path);
            await FileAndDirectoryInfoAsync(ns, path.Substring(0, path.LastIndexOf(@"\")));
        }

        static async Task FileDelete(NetworkStream ns, string path)
        {
            File.Delete(path);
            await FileAndDirectoryInfoAsync(ns, path.Substring(0, path.LastIndexOf(@"\")));
        }

        static async Task DeleteDirectory(NetworkStream ns, string path)
        {
            Directory.Delete(path);
            await FileAndDirectoryInfoAsync(ns, path.Substring(0, path.LastIndexOf(@"\")));
        }
    }

    public class JsonRequest
    {
        public string Request { get; set; }
        public string Key { get; set; }
        public string Path { get; set; }
    }
}