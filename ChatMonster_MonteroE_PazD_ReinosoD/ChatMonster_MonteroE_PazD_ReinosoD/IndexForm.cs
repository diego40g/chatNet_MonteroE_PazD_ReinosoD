using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;

namespace ChatMonster_MonteroE_PazD_ReinosoD
{
    public partial class IndexForm : Form
    {
        private string nick = "unknown";
        private NetworkStream streamC;
        private StreamWriter streamw;
        private StreamReader streamr;
        private TcpClient clientChat = new TcpClient();
        private delegate void DAddItem(string s);
        private bool bandera = false;
        public IndexForm()
        {
            InitializeComponent();
        }
        private string IP = "127.0.0.1";
        TcpListener listener;
        TcpClient client;
        Socket socketForClient;
        private Thread serverThread;
        private Thread findPC;
        private Thread notification;
        int flag = 0;
        string fileName = "";
        private bool serverRunning = false;
        private bool isConnected = false;
        int x = 9;
        int y = 308;
        int fileReceived = 0;
        string savePath;
        string senderIP;
        string senderMachineName;
        string targetIP;
        string targetName;
        NotificacionForm f2;
        private void IndexForm_Load(object sender, EventArgs e)
        {
            notificationLabel.ForeColor = Color.Red;
            notificationLabel.Text = "Application is offline";
        }

        void startServer()
        {
            try
            {
                serverRunning = true;
                listener = new TcpListener(IPAddress.Parse(IP), 11000);
                listener.Start();
                serverThread = new Thread(new ThreadStart(serverTasks));
                serverThread.Start();
                while (!serverThread.IsAlive) ;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void serverTasks()
        {
            try
            {
                while (true)
                {
                    if (fileReceived == 1)
                    {
                        if (MessageBox.Show("Save File?", "File received", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        {
                            File.Delete(savePath);
                            fileReceived = 0;
                        }
                        else
                        {
                            fileReceived = 0;
                        }
                    }

                    client = listener.AcceptTcpClient();
                    Invoke((MethodInvoker)delegate
                    {
                        notificationPanel.Visible = true;
                        notificationTempLabel.Text = "File coming..." + "\n" + fileName + "\n" + "From: " + senderIP + " " + senderMachineName;
                        fileNotificationLabel.Text = "File Coming from " + senderIP + " " + senderMachineName;
                    });
                    isConnected = true;
                    NetworkStream stream = client.GetStream();
                    if (flag == 1 && isConnected)
                    {
                        savePath = savePathLabel.Text + "\\" + fileName;
                        using (var output = File.Create(savePath))
                        {
                            var buffer = new byte[1024];
                            int bytesRead;
                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                output.Write(buffer, 0, bytesRead);
                            }
                            flag = 0;
                            client.Close();
                            isConnected = false;
                            fileName = "";
                            Invoke((MethodInvoker)delegate
                            {
                                notificationTempLabel.Text = "";
                                notificationPanel.Visible = false;
                                fileNotificationLabel.Text = "";
                            });
                            fileReceived = 1;
                        }
                    }
                    else if (flag == 0 && isConnected)
                    {
                        Byte[] bytes = new Byte[256];
                        String data = null;
                        int i;
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        }
                        string[] msg = data.Split('@');
                        fileName = msg[0];
                        senderIP = msg[1];
                        senderMachineName = msg[2];
                        client.Close();
                        isConnected = false;
                        flag = 1;
                    }
                }
            }
            catch (Exception)
            {
                flag = 0;
                isConnected = false;
                if (client != null)
                    client.Close();
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            ipBox.Text = "";
            notificationLabel.ForeColor = Color.Green;
            notificationLabel.Text = "Finding...";
            try
            {
                findPC = new Thread(new ThreadStart(searchPC));
                findPC.Start();
                while (!findPC.IsAlive) ;
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }

        void searchPC()
        {
            bool isNetworkUp = NetworkInterface.GetIsNetworkAvailable();
            if (isNetworkUp)
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        this.IP = ip.ToString();
                    }
                }
                Invoke((MethodInvoker)delegate
                {
                    infoLabel.Text = "This Computer: " + this.IP;
                });
                string[] ipRange = IP.Split('.');
                for (int i = 100; i < 255; i++)
                {
                    Ping ping = new Ping();
                    string testIP = ipRange[0] + '.' + ipRange[1] + '.' + ipRange[2] + '.' + i.ToString();
                    if (testIP != this.IP)
                    {
                        ping.PingCompleted += new PingCompletedEventHandler(pingCompletedEvent);
                        ping.SendAsync(testIP, 100, testIP);
                    }
                }

                Invoke((MethodInvoker)delegate
                {
                    notificationLabel.ForeColor = Color.Green;
                    notificationLabel.Text = "Application is Online";
                });
                if (!serverRunning)
                    startServer();
            }
            else
            {
                Invoke((MethodInvoker)delegate
                {
                    notificationLabel.ForeColor = Color.Red;
                    notificationLabel.Text = "Application is Offline";
                });
                MessageBox.Show("Not connected to LAN");
            }
        }
        
        void pingCompletedEvent(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            if (e.Reply.Status == IPStatus.Success)
            {
                string name;
                try
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                    name = hostEntry.HostName;
                }
                catch (SocketException ex)
                {
                    name = ex.Message;
                }
                Invoke((MethodInvoker)delegate
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = ip;
                    item.SubItems.Add(name);
                });
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "All Files|*.*";
            openFileDialog1.Title = "Select a File";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fileNameLabel.Text = openFileDialog1.FileName;
                fileNameLabel.Tag = openFileDialog1.SafeFileName;
            }
            timer1.Start();
        }

        private void sendFileButton_Click(object sender, EventArgs e)
        {
            targetIP = null;
            targetName = null;
            if ((onlinePCList.SelectedIndices.Count > 0 || ipBox.Text != "") && serverRunning && fileNameLabel.Text != ".")
            {
                if (ipBox.Text != "")
                {
                    targetIP = ipBox.Text;
                    targetName = "";
                }
                else
                {
                    targetIP = onlinePCList.SelectedItems[0].Text;
                    targetName = onlinePCList.SelectedItems[0].SubItems[1].Text;
                }
                try
                {
                    Ping p = new Ping();
                    PingReply r;
                    r = p.Send(targetIP);
                    if (!(r.Status == IPStatus.Success))
                    {
                        MessageBox.Show("Target computer is not available.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        notification = new Thread(new ThreadStart(showNotification));
                        notification.Start();
                        fileNotificationLabel.Text = "Please don't do other tasks. File sending to " + targetIP + " " + targetName + "...";
                        listener.Stop();
                        serverThread.Abort();
                        serverThread.Join();
                        serverRunning = false;
                        socketForClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socketForClient.Connect(new IPEndPoint(IPAddress.Parse(targetIP), 11000));
                        string fileName = fileNameLabel.Tag.ToString();
                        byte[] fileNameData = Encoding.Default.GetBytes(fileName + "@" + this.IP + "@" + Environment.MachineName);
                        socketForClient.Send(fileNameData);
                        socketForClient.Shutdown(SocketShutdown.Both);
                        socketForClient.Close();
                        socketForClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socketForClient.Connect(new IPEndPoint(IPAddress.Parse(targetIP), 11000));
                        socketForClient.SendFile(fileNameLabel.Text);
                        socketForClient.Shutdown(SocketShutdown.Both);
                        socketForClient.Close();
                        Invoke((MethodInvoker)delegate
                        {
                            f2.Dispose();
                        });
                        MessageBox.Show("File sent to " + targetIP + " " + targetName, "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    if (socketForClient != null)
                    {
                        socketForClient.Shutdown(SocketShutdown.Both);
                        socketForClient.Close();
                    }
                }
                finally
                {
                    for (int i = 0; i < onlinePCList.SelectedIndices.Count; i++)
                    {
                        onlinePCList.Items[this.onlinePCList.SelectedIndices[i]].Selected = false;
                    }
                    fileNotificationLabel.Text = ".";
                    startServer();
                }
            }
        }
        void showNotification()
        {
            f2 = new NotificacionForm(targetName, targetIP);
            f2.ShowDialog();
        }
        private void IndexForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serverRunning)
            {
                listener.Stop();
                if (serverThread != null)
                {
                    serverThread.Abort();
                    serverThread.Join();
                }

            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            if (serverRunning)
            {
                serverRunning = false;
                onlinePCList.Items.Clear();
                if (listener != null)
                    listener.Stop();
                if (serverThread != null)
                {
                    serverThread.Abort();
                    serverThread.Join();
                }

                notificationLabel.ForeColor = Color.Red;
                notificationLabel.Text = "Application is Offline";
                infoLabel.Text = "";
                fileNameLabel.Text = ".";
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            x = x - 5;
            fileNameLabel.Location = new Point(x, y);
            if (x < (fileNameLabel.Text.Length * (-1)))
                x = 545;
        }
        private void clearButton_Click(object sender, EventArgs e)
        {
            fileNameLabel.Text = ".";
            timer1.Stop();
        }

        private void changeSaveLocButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browse = new FolderBrowserDialog();
            if (browse.ShowDialog() == DialogResult.OK)
            {
                string savePath = browse.SelectedPath;
                savePathLabel.Text = savePath;
            }
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            if (serverRunning)
            {
                if (listener != null)
                    listener.Stop();
                if (serverThread != null)
                {
                    serverThread.Abort();
                    serverThread.Join();
                }
                bandera = false;
            }
            Application.Exit();
        }

        private void AddItem(string s)
        {
            onlinePCList.Items.Add(s);
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            connectButton.Visible = false;
            lblipServer.Visible = false;
            lblName.Visible = false;
            txtName.Visible = false;
            txtServerIp.Visible = false;

            startButton.Visible = true;
            stopButton.Visible = true;
            changeSaveLocButton.Visible = true;
            exitButton.Visible = true;
            browseButton.Visible = true;
            sendFileButton.Visible = true;
            clearButton.Visible = true;
            label1.Visible = true;
            ipBox.Visible = true;
            savePathLabel.Visible = true;
            sendButton.Visible = true;
            textBox1.Visible = true;
            onlinePCList.Visible = true;
            nick = txtName.Text;
            label2.Text = nick;
            label2.Visible = true;

            startButton_Click(sender, e);
            Connect();
        }

        void Listen()
        {
            while (bandera)
            {
                try
                {
                    this.Invoke(new DAddItem(AddItem), streamr.ReadLine());
                }
                catch
                {
                    MessageBox.Show("No se ha podido conectar al servidor");
                    Application.Exit();
                }
            }
        }

        void Connect()
        {
            try
            {
                clientChat.Connect(txtServerIp.Text, 12000);
                if (clientChat.Connected)
                {
                    bandera = true;
                    if (txtServerIp.Text.Equals("127.0.0.1"))
                    {
                        label2.Text = nick + " (Servidor)";
                    }
                    Thread t = new Thread(Listen);
                    streamC = clientChat.GetStream();
                    streamw = new StreamWriter(streamC);
                    streamr = new StreamReader(streamC);

                    streamw.WriteLine(nick);
                    streamw.Flush();

                    t.Start();
                }
                else
                {
                    MessageBox.Show("Servidor no Disponible");
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Servidor no Disponible" + ex);
                Application.Exit();
            }
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            streamw.WriteLine(textBox1.Text);
            streamw.Flush();
            textBox1.Clear();
        }
    }
}
