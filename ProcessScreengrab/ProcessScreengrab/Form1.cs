using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessScreengrab
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (Bitmap bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                            Screen.PrimaryScreen.Bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bmpScreenCapture))
                {
                    g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                     Screen.PrimaryScreen.Bounds.Y,
                                     0, 0,
                                     bmpScreenCapture.Size,
                                     CopyPixelOperation.SourceCopy);
                }

                pictureBox1.Image = new Bitmap(bmpScreenCapture, new Size(pictureBox1.Width, pictureBox1.Height));
                pictureBox2.BackColor = ProcessImage(bmpScreenCapture);

            }

        }

        private Color ProcessImage(Bitmap b)
        {
            int total = 0;
            long[] colors = new long[] { 0, 0, 0 };
            int skip = b.Width * b.Height / 50000;
            for (int x = 0; x < b.Width; x += skip)
            {
                for (int y = 0; y < b.Height; y += skip)
                {
                    Color pixel = b.GetPixel(x, y);
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
            return col;

        }
    }
}
