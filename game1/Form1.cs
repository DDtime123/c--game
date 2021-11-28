using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using NAudio.Wave;

namespace game1
{
    public partial class Form1 : Form
    {
        string host = "10.1.230.74";
        int port = 3900;
        TcpClient tcpClient;
        NetworkStream ns;
        bool _stopEvent;
        bool _musicStopEvent;
        Byte[] cmd;
        Bitmap[] bgimages = { Properties.Resources.bg1, Properties.Resources.bg2, Properties.Resources.bg3, Properties.Resources.bg4 };
        int curbgIdx;
        WaveOut waveOut;

        public Form1()
        {
            InitializeComponent();
            
        }
        /************************游戏背景音乐*************************/
        void startPlayMusic()
        {
            Thread musicProcess = new Thread(playMusic);
            musicProcess.IsBackground = true;
            musicProcess.Start(this);
        }
        void playMusic(object sender)
        {
            //System.Media.SoundPlayer player = new System.Media.SoundPlayer(Properties.Resources.bg);
            //player.Play();
            Stream stream = new MemoryStream(Properties.Resources.bg);
            var reader = new Mp3FileReader(stream);
            waveOut = new WaveOut();
            waveOut.Init(reader);
            waveOut.Play();
            waveOut.Dispose();
            _musicStopEvent = false;
            while (true)
            {
                if (_musicStopEvent)
                {
                    waveOut.Stop();
                    waveOut.Dispose();

                    reader.Close();
                    reader.Dispose();
                    return;
                }
            }
        }
        /*************************************************************/
        /************************游戏背景图片*************************/
        void startBackgroundProcess()
        {
            curbgIdx = 0;
            Thread bgprocess = new Thread(BackgroundProcess);
            bgprocess.IsBackground = true;
            bgprocess.Start(this);
        }
        void BackgroundProcess(object sender)
        {
            Form1 form = (Form1)sender;
            while (true)
            {
                if (_stopEvent)
                {
                    return;
                }

                form.pictureBox1.Invoke((MethodInvoker)delegate {
                    // Running on the UI thread
                    form.pictureBox1.Image = bgimages[curbgIdx];
                });
                Thread.Sleep(1000 * 10);
                curbgIdx = (curbgIdx + 1) % bgimages.Length;
            }
        }

        /*************************Telnet客户端************************/
        void createTelnetClient(object sender)
        {
            _stopEvent = false;
            Form1 tt = (Form1)sender;
            
            tt.tcpClient = new TcpClient(host, port);
            tt.ns = tt.tcpClient.GetStream();


            tt.connectHost(tt);
            tt.sendCommand();

            tt.tcpClient.Close();
        }
        void stopconnect()
        {
            _stopEvent = true;
        }
        void SendToTelnet()
        {
            cmd = System.Text.Encoding.GetEncoding("gb18030").GetBytes(textBox1.Text + "\r\n");
            ns.Write(cmd, 0, cmd.Length);
            Thread.Sleep(1000);
        }

        public void connectHost(Form1 tt)
        {
            _stopEvent = false;
            bool i = true;
            /***********************开始连接****************************/
            tt.listBox1.Invoke((MethodInvoker)delegate {
                // Running on the UI thread
                tt.listBox1.Items.Add("Connecting......");
            });
            Byte[] output = new byte[1024];
            String responseoutput = String.Empty;
            /**
            
            ns.Write(cmd, 0, cmd.Length);
            */
            Thread.Sleep(1000);
            Int32 bytes = ns.Read(output, 0, output.Length);
            responseoutput = System.Text.Encoding.GetEncoding("gb18030").GetString(output, 0, bytes);
            MessageBox.Show("Responseoutput: " + responseoutput);
            tt.listBox1.Invoke((MethodInvoker)delegate {
                // Running on the UI thread
                tt.listBox1.Items.Add("Connected!");
                var responseoutputarray = responseoutput.Split('\n');
                tt.listBox1.Items.Add(
                    responseoutputarray[responseoutputarray.Length - 3] +
                    responseoutputarray[responseoutputarray.Length - 2] +
                    responseoutputarray[responseoutputarray.Length - 1]);
            });
            /***********************************************************/
            while (i)
            {
                if (_stopEvent)
                {
                    return;
                }
                Thread.Sleep(1000);
                bytes = ns.Read(output, 0, output.Length);
                responseoutput = System.Text.Encoding.GetEncoding("gb18030").GetString(output, 0, bytes);
                tt.listBox1.Invoke((MethodInvoker)delegate {
                    // Running on the UI thread
                    tt.listBox1.Items.Add(responseoutput);
                });

                /**
                Thread.Sleep(1000);
                bytes = ns.Read(output, 0, output.Length);
                responseoutput = System.Text.Encoding.GetEncoding("gb18030").GetString(output, 0, bytes);
                tt.listBox1.Invoke((MethodInvoker)delegate {
                    // Running on the UI thread
                    tt.listBox1.Items.Add(responseoutput);
                });*/
                /**
                objToMatch = new Regex("Password");
                if (objToMatch.IsMatch(responseoutput))
                {
                    cmd = System.Text.Encoding.ASCII.GetBytes(passwd + "\r");
                    ns.Write(cmd, 0, cmd.Length);
                }*/
            }
            MessageBox.Show("Just works");
        }
        public void sendCommand()
        {

        }
        /****************************************************************/
        /************************************************************/

        /************************************************************/
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendToTelnet();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {

        }
        /*************************************************************/
        /**
         * 点击“进入游戏”事件
         */
        private void button10_Click(object sender, EventArgs e)
        {
            stopconnect();
            Thread telnet = new Thread(createTelnetClient);
            telnet.IsBackground = true;
            telnet.Start(this);

            startBackgroundProcess();
            startPlayMusic();
        }
        /**
         * 点击“退出游戏”事件
         */
        private void button11_Click(object sender, EventArgs e)
        {
            _musicStopEvent = true;
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.listBox1.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                MessageBox.Show(listBox1.Items[index].ToString());
            }
        }
        /*************************************************************/
        /************************窗口关闭事件****************************/
        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _musicStopEvent = true;
        }
        /*************************************************************/
    }
}
