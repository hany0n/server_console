using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace server_t2
{
    internal class Program
    {

        static List<TcpClient> clients = new List<TcpClient>();
        static object lockObj = new object();

        static void Main(string[] args)
        {
            var listener = new TcpListener(IPAddress.Any, 7000);
            listener.Start();
            Console.WriteLine("Сервер включен... (Порт: 7000)");

            while (true)
            {
                var client = listener.AcceptTcpClient();
                lock (lockObj) clients.Add(client);

                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        }


        static void HandleClient(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                byte[] buffer = new byte[4096];

                while (true)
                {
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    if (bytes == 0) break;

                    string msg = Encoding.UTF8.GetString(buffer, 0, bytes);
                    Console.WriteLine($"Получено: {msg}");
                    SendToAll(msg, client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                lock (lockObj) clients.Remove(client);
                client.Close();
            }
        }

        static void SendToAll(string message, TcpClient sender)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            lock (lockObj)
            {
                foreach (var client in clients)
                {
                    if (client != sender)
                    {
                        try
                        {
                            client.GetStream().Write(data, 0, data.Length);
                        }
                        catch
                        {
                            
                        }
                    }
                }
            }
        }
    }
}
