using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Bson;
using MongoDB.Driver;

namespace udpClient
{
    public partial class Form1 : Form
    {
        private static IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.220.237"), 35488);
        private static UdpClient udpClient = new UdpClient(34285);
        private static string ipFrom = "192.168.220.237";
        private static string ipTo = "";
        private System.Threading.Timer _timer = null;

        private static MongoClient mongoClient = new MongoClient("mongodb://localhost:27017");
        private static IMongoDatabase database = mongoClient.GetDatabase("ChatDB");
        private static IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("chats");

        public Form1()
        {
            InitializeComponent();

            _timer = new System.Threading.Timer(TimerCallback, null, 0, 10000);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            Message message = new Message(Command.Text, Text.Text, ipTo, ipFrom);
            string messageJson = message.ToJSON();
            byte[] messageData = Encoding.UTF8.GetBytes(messageJson);
            await DisplayMessage();
            udpClient.Send(messageData, ep);

        }

        public async Task DisplayMessage()
        {
            var filter = Builders<BsonDocument>.Filter.Empty;
            var cursor = await collection.FindAsync(filter);
            cursor.ForEachAsync(document =>
            textBox1.AppendText(document.ToString() + Environment.NewLine));
            _timer = new System.Threading.Timer(TimerCallback, null, 0, 10000);
        }

        private static async void TimerCallback(Object o)
        {
            //Form1 form = (Form1)o;
            //await form.DisplayMessage();       

            if (ipTo != "")
            {
                Message message = new Message("Update", "", ipTo, ipFrom);
                string messageJson = message.ToJSON();
                byte[] messageData = Encoding.UTF8.GetBytes(messageJson);
                udpClient.Send(messageData, ep);
                
            }
        }

        public void UpdateMessage()
        {
            ipTo = IpToBox.Text;
            string modIpTo = ModIP(ipTo);
            string modIpFrom = ModIP(ipFrom);
            string path = GenerateFileName(modIpFrom, modIpTo);
            var curDir = Directory.GetCurrentDirectory();
            curDir = curDir.Replace("\\", "/");
            string newFileName = String.Format($"{curDir}/history/{path}.txt");
        }

        public async Task ClientListener()
        {
            while (true)
            {
                var receiveResult = await udpClient.ReceiveAsync();
                byte[] answerData = receiveResult.Buffer;

                string message = Encoding.UTF8.GetString(answerData);
                textBox1.Invoke((MethodInvoker)delegate
                {
                    textBox1.AppendText(message + Environment.NewLine);
                });
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            ipTo = IpToBox.Text;
            UpdateMessage();

            await ClientListener();
        }

        public static string ModIP(string address)
        {
            address = address.Replace(".", "_");
            string modIp = "";

            for (int i = 0; i < address.Length; i++)
            {
                if (address[i] == ':')
                {
                    modIp = address.Substring(0, i);
                }
            }
            if (modIp == "")
            {
                modIp = address;
            }
            return modIp;
        }

        public static string GenerateFileName(string firstIp, string secondIp)
        {
            string[] arr1 = firstIp.Split('_');
            string[] arr2 = secondIp.Split('_');
            for (int i = 0; i < 4; i++)
            {
                if (int.Parse(arr1[i]) > int.Parse(arr2[i]))
                {
                    return secondIp + "=" + firstIp;
                }
                else if (int.Parse(arr1[i]) < int.Parse(arr2[i]))
                {
                    return firstIp + '=' + secondIp;
                }
            }
            return null;
        }

        
        private void Command_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
