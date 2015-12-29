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

namespace ProcessScreengrab
{
    public partial class Form1 : Form
    {
        bool processing = false;

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            processing = true;
            button1.Enabled = false;
            button2.Enabled = true;
            new Thread(CaptureScreen).Start();

        }

        private void CaptureScreen()
        {
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

                    setDetail("Bitmap Size:" + bmpScreenCapture.Width + " x " + bmpScreenCapture.Height + "\r\nColour value of screen:" + pictureBox2.BackColor.ToString());

                }
                Thread.Sleep((int)numericUpDown1.Value);
            }

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

}
