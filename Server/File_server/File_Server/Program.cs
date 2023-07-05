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
<<<<<<< HEAD
        static string pathMainDirectory = $@"{Environment.CurrentDirectory}\Files\";
=======
        static string pathMainDirectory = Directory.GetCurrentDirectory() + @"\test\";
>>>>>>> acf963cdb2762bf436e6c90b0c56e65cf431126a
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
<<<<<<< HEAD
                    await FileAndDirectoryInfoAsync(ns, pathMainDirectory + userRequest.Key + @"\" + userRequest.Path);
=======
                {
                    await Console.Out.WriteLineAsync("Info");
                    await FileAndDirectoryInfoAsync(ns, userRequest.Key);
                }
>>>>>>> acf963cdb2762bf436e6c90b0c56e65cf431126a

                // +
                else if (userRequest.Request == "CreateDirectory")
<<<<<<< HEAD
                    await CreateDirectory(ns, pathMainDirectory + userRequest.Key + @"\" + userRequest.Path);

                else if (userRequest.Request == "DeleteFile")
                    await FileDelete(ns,  pathMainDirectory + userRequest.Key + @"\" + userRequest.Path);

                else if (userRequest.Request == "DeleteDirectory")
                    await DeleteDirectory(ns, pathMainDirectory + userRequest.Key + @"\" + userRequest.Path);
=======
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
>>>>>>> acf963cdb2762bf436e6c90b0c56e65cf431126a

                // +
                else if (userRequest.Request == "Upload")
<<<<<<< HEAD
                    await UploadFileAsync(ns, pathMainDirectory + userRequest.Key + @"\" + userRequest.Path);

                else if (userRequest.Request == "DownloadFile")
                    await DownloadFileAsync(ns, pathMainDirectory + userRequest.Key + @"\" + userRequest.Path);

                else if (userRequest.Request == "DownloadDirectory")
                    await DownloadDirectoryAsync(ns, pathMainDirectory + userRequest.Key + @"\" + userRequest.Path);
=======
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
>>>>>>> acf963cdb2762bf436e6c90b0c56e65cf431126a
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
            byte[] file = File.ReadAllBytes(path);
            await ns.WriteAsync(file, 0, file.Length);
        }

<<<<<<< HEAD
        static async Task DownloadDirectoryAsync(NetworkStream ns, string path)
        {
            ZipFile.CreateFromDirectory(path, path + ".zip");
            byte[] file = File.ReadAllBytes(path + ".zip");
            await ns.WriteAsync(file, 0, file.Length);
            await FileDelete(ns, path + ".zip");
        }

        static async Task FileAndDirectoryInfoAsync(NetworkStream ns, string path)
        {
=======
        // изменить манифест
        static async Task FileAndDirectoryInfoAsync(NetworkStream ns, string key)
        {
            IEnumerable<string> allFiles = Directory.EnumerateFiles(pathMainDirectory + key, "*.*", SearchOption.TopDirectoryOnly);//Заменил,для поиска в текущей директории
>>>>>>> acf963cdb2762bf436e6c90b0c56e65cf431126a
            StringBuilder fileInfo = new StringBuilder();
            IEnumerable<string> allFiles = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly);
            foreach (string filename in allFiles)
            {
                fileInfo.Append(filename.Substring(filename.LastIndexOf(@"\") + 1) + ";" + new FileInfo(filename).Length + ";");
            }

<<<<<<< HEAD
            IEnumerable<string> allDirectory = Directory.EnumerateDirectories(path, "*.*", SearchOption.TopDirectoryOnly);
=======
            IEnumerable<string> allDirectory = Directory.EnumerateDirectories(pathMainDirectory + key, "*.*", SearchOption.TopDirectoryOnly);//Заменил,для поиска в текущей директории
            StringBuilder directoryInfo = new StringBuilder();
>>>>>>> acf963cdb2762bf436e6c90b0c56e65cf431126a
            foreach (string filename in allDirectory)
            {
                fileInfo.Append(filename.Substring(filename.LastIndexOf(@"\") + 1) + ";");
            }

<<<<<<< HEAD
            byte[] data = Encoding.UTF8.GetBytes(fileInfo.ToString());
            await ns.WriteAsync(data, 0, data.Length);
=======
            byte[] data = Encoding.UTF8.GetBytes(fileInfo.ToString() + directoryInfo.ToString());
            await ns.WriteAsync(data, 0, data.Length);
            Console.WriteLine("Файл предан");
>>>>>>> acf963cdb2762bf436e6c90b0c56e65cf431126a
        }

        static async Task CreateDirectory(NetworkStream ns, string path)
        {
<<<<<<< HEAD
            Directory.CreateDirectory(path);
            await FileAndDirectoryInfoAsync(ns, path.Substring(0, path.LastIndexOf(@"\")));
=======
            Directory.CreateDirectory(pathMainDirectory + path);
            await Console.Out.WriteLineAsync("CreateDirectoryAsync");
            await FileAndDirectoryInfoAsync(ns, key);
        }

        static async Task CreateMainDirectoryAsync(NetworkStream ns, string key)
        {
            Directory.CreateDirectory(pathMainDirectory + key);
            await Console.Out.WriteLineAsync("CreateMainDirectoryAsync");
            await FileAndDirectoryInfoAsync(ns, key);
>>>>>>> acf963cdb2762bf436e6c90b0c56e65cf431126a
        }

        static async Task FileDelete(NetworkStream ns, string path)
        {
<<<<<<< HEAD
            File.Delete(path);
            await FileAndDirectoryInfoAsync(ns, path.Substring(0, path.LastIndexOf(@"\")));
=======
            File.Delete(pathMainDirectory + path);
            await Console.Out.WriteLineAsync("FileDeleteAsync");
            await FileAndDirectoryInfoAsync(ns, key);
>>>>>>> acf963cdb2762bf436e6c90b0c56e65cf431126a
        }

        static async Task DeleteDirectory(NetworkStream ns, string path)
        {
<<<<<<< HEAD
            Directory.Delete(path);
            await FileAndDirectoryInfoAsync(ns, path.Substring(0, path.LastIndexOf(@"\")));
=======
            Directory.Delete(pathMainDirectory + path);
            await Console.Out.WriteLineAsync("DeleteDirectoryAsync");
            await FileAndDirectoryInfoAsync(ns, key);
>>>>>>> acf963cdb2762bf436e6c90b0c56e65cf431126a
        }
    }

    public class JsonRequest
    {
        public string Request { get; set; }
        public string Key { get; set; }
        public string Path { get; set; }
    }
<<<<<<< HEAD
}

=======
}
>>>>>>> acf963cdb2762bf436e6c90b0c56e65cf431126a
