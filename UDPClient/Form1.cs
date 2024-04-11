using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace udpClient
{
    public partial class Form1 : Form
    {
        private static IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.220.242"), 35488);
        private static UdpClient udpClient = new UdpClient(34285);
        private static string ipFrom = "192.168.220.242";
        private static string ipTo = "";
        private System.Threading.Timer _timer = null;
        public Form1()
        {
            String strHostName = string.Empty;
            strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;

            InitializeComponent();

            _timer = new System.Threading.Timer(TimerCallback, null, 0, 10000);
        }

        private void button1_Click(object sender, EventArgs e)
        {



            Message message = new Message(Command.Text, Text.Text, ipTo, ipFrom);
            string messageJson = message.ToJSON();
            byte[] messageData = Encoding.UTF8.GetBytes(messageJson);
            udpClient.Send(messageData, ep);
            Updater();

            //var curDir = Directory.GetCurrentDirectory();
            //curDir = curDir.Replace("\\", "/");
            //string filePath = $"{curDir}/history/{ipFrom}.txt";
            //string content = File.ReadAllText(filePath);
            //textBox1.Text = content;

        }

        private static void TimerCallback(Object o)
        {
            if (ipTo != "")
            {
                Message message = new Message("Update", "", ipTo, ipFrom);
                string messageJson = message.ToJSON();

                byte[] messageData = Encoding.UTF8.GetBytes(messageJson);
                udpClient.Send(messageData, ep);
            }

        }

        private async void Updater()
        {
            var curDir = Directory.GetCurrentDirectory();
            curDir = curDir.Replace("\\", "/");
            string newFileName = String.Format($"{curDir}/history/{ipFrom + " = " + ipTo}.txt");
            if (File.Exists(newFileName))
            {
                string fileText = File.ReadAllText(newFileName);
                textBox1.Text = fileText;
            }
        }

        public static async Task ClientListner()
        {
            while (true)
            {
                var curDir = Directory.GetCurrentDirectory();
                curDir = curDir.Replace("\\", "/");
                string newFileName = String.Format($"{curDir}/history/{ipFrom + "=" + ipTo}.txt");
                var receiveResult = await udpClient.ReceiveAsync();
                byte[] answerData = receiveResult.Buffer;
                File.WriteAllBytes(newFileName, answerData);


            }

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            ipTo = IpToBox.Text;
            await ClientListner();
            Updater();

        }

        public static string ModIP(string adres)
        {
            adres = adres.Replace(".", "_");
            string modIp = "";

            for (int i = 0; i < adres.Length; i++)
            {
                if (adres[i] == ':')
                {
                    modIp = adres.Substring(0, i);
                }
            }
            if (modIp == "")
            {
                modIp = adres;
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
