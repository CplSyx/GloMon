using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreengrabSave
{
    public partial class Form1 : Form
    {
        /***
         * On click of the windows form button, the application captures the primary screen and saves it to the same folder as the application under the filename "test.bmp". 
         * Also displays a preview (not from the file but from the image data).
         * If slow, this is due to the save event and disk activity; commenting out the save significantly increases performance
         ***/
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
                bmpScreenCapture.Save("test.bmp");
            }

        }
    }
}
