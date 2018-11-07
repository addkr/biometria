using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Biometria
{
    public partial class FormBiometria : Form
    {

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string command, StringBuilder retstring, int returnLength, IntPtr callback);

        public FormBiometria()
        {
            InitializeComponent();
            
        }

        private void randomNumbers(object sender, EventArgs e)
        {
            string randomSequence = "";
            Random rnd = new Random();
            for(int i=0; i < 5; i++)
            {
                int number = rnd.Next(0, 9);
                randomSequence += number;
            }
            
            textBox.Text = randomSequence;
        }

        private void startRecording(object sender, EventArgs e)
        {
            mciSendString("Open new type waveaudio alias recsound", null, 0, IntPtr.Zero);
            mciSendString("Record recsound", null, 0, IntPtr.Zero);
        }

        private void stopRecording(object sender, EventArgs e)
        {
            string date = DateTime.Now.ToString();
            date = date.Replace(":", "");
            date = date.Replace("/", "");
            date = date.Replace(" ", "");
            date = date.Replace(".", "");
            string path = "Save recsound c:\\biometria\\"+date+".wav";
            //Console.Out.Write(path);
            //"Save recsound c:\\biometria\\sound.wav"
            mciSendString(path, null, 0, IntPtr.Zero);
            mciSendString("Close recsound", null, 0, IntPtr.Zero);
        }

    }
}
