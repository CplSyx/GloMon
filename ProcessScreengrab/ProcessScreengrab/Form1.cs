using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO.Ports;
using Newtonsoft.Json;

namespace ProcessScreengrab
{
    public partial class Form1 : Form
    {
        bool processing = false;
        SerialPort serial;
        String hostUUID = "c32beb5ac8794498a201053da10f0f23";

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            COMCheck(); //TODO
        }

        private void COMCheck()
        {
            //Send an IdentPacketRequest to each COM port.
        }

        private void setupSerial(string port)
        {
            serial = new SerialPort(port, 115200);
            serial.Open();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            processing = true;
            button1.Enabled = false;
            button2.Enabled = true;
            textBox1.Text = "";
            textBox2.Text = "";
            new Thread(CaptureScreen).Start();

        }

        private void ReadSerial()
        {
            while(processing)
            {
                String s;
                try
                {
                    if (serial.BytesToRead > 0)
                    {
                        s = serial.ReadLine();
                        //s = s.TrimEnd(new char[] { '\n' });
                        dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(s);
                        switch ((string)jsonObject.type)
                        {
                            case ("IdentPacket"):
                                textBox2.Text = "Identified " + jsonObject.uuid;
                                break;

                            case ("HeartbeatPacket"):
                                break;

                            case ("Error"):
                                textBox2.Text = jsonObject.errormessage;
                                break;

                            default:
                                textBox2.Text = s;
                                break;
                        }

                    }
                    Thread.Sleep(10);
                } catch (Exception e)
                {
                    ////////////////////////textBox2.Text = e.Message; //We can ignore a number of these errors as they are garbage messages or overlapping incoming serial data; these will fail to deserialize.
                    
                }
            }
        }

        private void sendColourData(Color c)
        {
            RGBData output = new RGBData(c.R, c.G, c.B);
            output.type = "RGBData";
            output.uuid = hostUUID;
            serial.WriteLine(JsonConvert.SerializeObject(output));
            setDetail(JsonConvert.SerializeObject(output));
        }


        private void CaptureScreen()
        {
            setupSerial("COM4");
            new Thread(ReadSerial).Start();

            while (processing)
            {
                using (Bitmap bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmpScreenCapture))
                    {
                        g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                         Screen.PrimaryScreen.Bounds.Y,
                                         0, 0,
                                         bmpScreenCapture.Size,
                                         CopyPixelOperation.SourceCopy);
                    }

                    //Comment out the below line for pictureBox1 to improve performance
                    //pictureBox1.Image = new Bitmap(bmpScreenCapture, new Size(pictureBox1.Width, pictureBox1.Height));


                    pictureBox2.BackColor = ProcessImage(bmpScreenCapture);

                    //setDetail("Bitmap Size:" + bmpScreenCapture.Width + " x " + bmpScreenCapture.Height + "\r\nColour value of screen:" + pictureBox2.BackColor.ToString());
                    sendColourData(pictureBox2.BackColor);
                }
                Thread.Sleep((int)numericUpDown1.Value);
            }

            serial.Close();

        }

        private Color ProcessImage(Bitmap bm)
        {
            FastBitmap fb = new FastBitmap(bm);
            fb.Lock();
            int total = 0;
            long[] colors = new long[] { 0, 0, 0 };
            for (int x = 0; x < fb.Width; x += 50)
            {
                for (int y = 0; y < fb.Height; y += 50)
                {
                    Color pixel = fb.GetPixel(x, y);
                    colors[0] += pixel.R;
                    colors[1] += pixel.G;
                    colors[2] += pixel.B;
                    total += 1;
                }
            }
            byte red, green, blue;
            red = (byte)((double)colors[0] / total);
            green = (byte)((double)colors[1] / total);
            blue = (byte)((double)colors[2] / total);
            Color col = Color.FromArgb(red, green, blue);
            fb.Unlock();
            return col;

        }

        private void setDetail(String s)
        {
            try
            {
                textBox1.Text = s;
            } 
            catch (Exception)
            {
                //Do nothing as this will be a result of the form closing whilst trying to update the text box control.
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            processing = false;
            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            processing = false;
        }

    }

    public class HeartbeatPacket
    {
        public HeartbeatPacket() { }
        public string type;
        public string uuid;
        public HB Heartbeat;

        public HB createHeartbeat()
        {
            return new HB();
        }

        public class HB
        {
            public HB() { }
            public string time;
        }

    }

    public class RGBData
    {
        public RGBData(int r = 0, int g = 0, int b = 0) 
        {
            red = r;
            green = g;
            blue = b;
        }
        public string type;
        public string uuid;
        public int red;
        public int green;
        public int blue;


    }

    public class IdentPacket
    {
        public IdentPacket() { }
        public string type;
        public string uuid;
    
    }

    public class IdentPacketRequest
    {
        public IdentPacketRequest() { }
        public string type;
        public string uuid;

    }

    public class JSONError
    {
        public JSONError() { }
        public string type;
        public string errormessage;

    }

}
