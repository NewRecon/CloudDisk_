using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CloudDisk_BD_server
{
    internal class Program
    {
        static TcpListener server_DB = new TcpListener(IPAddress.Parse("192.168.0.103"), 8888);
        static string pathDB = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CloudDisk_DB;Integrated Security=True;Connect Timeout=30;";
        static async Task Main(string[] args)
        {
            //using (UserContext userAdd = new UserContext())
            //{
            //    User addUser = new User
            //    {
            //        Login = "Admin",
            //        Password = "Admin",
            //        Gmail = "Admin@gmail.com"
            //    };
            //    userAdd.Users.Add(addUser);
            //    await userAdd.SaveChangesAsync();
            //}
            //return;
            server_DB.Start();
            var t = Task.Run(() => RequestsClientsAsync());
            t.Wait();
        }

        static async Task RequestsClientsAsync()
        {
            while (true)
            {
                await Task.Yield(); 
                TcpClient client = await server_DB.AcceptTcpClientAsync();
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
                var userRequest = JsonSerializer.Deserialize<JsonUser>(result);
                if (userRequest.Request == "Registration")
                {
                    string str = Registration(userRequest);
                    if(str == "OK")
                        await SendEmailAsync(userRequest);
                    byte[] sendBytes = Encoding.UTF8.GetBytes(str);
                    await ns.WriteAsync(sendBytes, 0, sendBytes.Length);
                }
                else if (userRequest.Request == "Authentication")
                {
                    byte[] sendBytes = Encoding.UTF8.GetBytes(Authentication(userRequest));
                    await ns.WriteAsync(sendBytes, 0, sendBytes.Length);
                }
            }
        }

        static string Registration(JsonUser user)
        {
            using (SqlConnection sqlConnection = new SqlConnection(pathDB))
            {
                sqlConnection.Open();
                SqlCommand command = sqlConnection.CreateCommand();

                try
                {
                    command.CommandText = $"Select * from Users Where Login = '{user.Login}'";
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        return $"Login {reader[1]} busy";
                    }
                } catch (Exception ex)
                {
                    try
                    {
                        command.CommandText = $"Select * from Users Where Gmail = '{user.Gmail}'";
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            reader.Read();
                            return $"Gmail {reader[3]} busy";
                        }
                    }
                    catch (Exception exx)
                    {
                        command.CommandText = $"INSERT INTO Users VALUES('{user.Login}', '{user.Password}', '{user.Gmail}')";
                        command.ExecuteNonQuery();
                        return "OK";
                    }
                }
            }
        }

        static string Authentication(JsonUser user)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(pathDB))
                {
                    sqlConnection.Open();
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandText = $"Select * from Users Where Login = '{user.Login}'";
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        if (user.Password == reader[2].ToString())
                            return "OK";
                        else
                            return "ERROR";
                    }
                }
            }
            catch(Exception ex)
            {
                return "ERROR";
            }
        }
        static async Task SendEmailAsync(JsonUser user)
        {
            MailAddress mailAddress1 = new MailAddress("gamejudicator@gmail.com", "Administration CloudDisk");
            MailAddress mailAddress2 = new MailAddress($"{user.Gmail}");

            MailMessage message = new MailMessage(mailAddress1, mailAddress2);
            message.Subject = "Регистрация";
            message.Body = $"Поздравляем! Вы зарегистрировались!\nВаш логин: {user.Login}\nВаш пароль: {user.Password}";

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new NetworkCredential("gamejudicator@gmail.com", "edewnfoerdymrage");
            smtpClient.EnableSsl = true;
            await smtpClient.SendMailAsync(message);
        }
    }
}
