using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ObligatoriskOpgave1;

namespace ObligatoriskOpgave5
{
    internal class Worker
    {
        private List<Bog> books = new List<Bog>()
        {
            new Bog("God bog", "Alexander", 150, "1234567890123"),
            new Bog("Mindre god bog", "Carsten", 100, "0123456789123"),
            new Bog("Bedste bog i verden", "Henrik", 350, "1123456789123")
        };

        public void Start()
        {
            TcpListener server = new TcpListener(IPAddress.Loopback, 4646);

            server.Start();

            while (true)
            {
                TcpClient socket = server.AcceptTcpClient();
                Task.Run(() =>
                {
                    TcpClient tmpSocket = socket;
                    DoClient(tmpSocket);
                });
            }
        }

        private void DoClient(TcpClient socket)
        {
            using (StreamReader sr = new StreamReader(socket.GetStream()))
            using (StreamWriter sw = new StreamWriter(socket.GetStream()))
            {
                bool active = true;

                while (active)
                {
                    string input = sr.ReadLine();

                    switch (input)
                    {
                        case "HentAlle":
                            sw.WriteLine(JsonConvert.SerializeObject(books));
                            break;
                        case "Hent":
                            string isbn13 = sr.ReadLine();
                            Bog bog = books.Find(c => c.Isbn13 == isbn13);
                            if(bog != null) sw.WriteLine(JsonConvert.SerializeObject(bog));
                            else sw.WriteLine("Bogen findes ikke!");
                            break;
                        case "Gem":
                            string data = sr.ReadLine();
                            Bog gemBog = JsonConvert.DeserializeObject<Bog>(data);

                            books.Add(gemBog);
                            break;
                        case "STOP":
                            socket.Close();
                            active = false;
                            return;
                    }

                    sw.Flush();
                }
            }
        }
    }
}