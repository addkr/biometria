using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Biometria
{
    public partial class FormBiometria : Form
    {
        string range = "";
        double[] sourceFFT;
        double[] sourceFFT1;
        double[] newFFT;
        WaveIn sourceStream;
        WaveFileWriter waveWriter;
        readonly String FilePath;
        public WaveIn wi;
        public BufferedWaveProvider bwp;
        public Int32 envelopeMax;
        private int RATE = 44100; 
        private int BUFFERSIZE = (int)Math.Pow(2, 11);

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string command, StringBuilder retstring, int returnLength, IntPtr callback);

        public FormBiometria()
        {
            InitializeComponent();
            FilePath = "..\\..\\Sounds\\";
        }

      
        public void startRecording(object sender, EventArgs e)
        {
            newFFT = null;
            sourceStream = new WaveIn
            {

                WaveFormat =
                    new WaveFormat(RATE, 2)
            };

            sourceStream.DataAvailable += this.SourceStreamDataAvailable;

            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }            
            waveWriter = new WaveFileWriter(@"..\\..\\Sounds\\check.wav", sourceStream.WaveFormat);
            sourceStream.StartRecording();
        }

        public void SourceStreamDataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveWriter == null) return;
            waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            waveWriter.Flush();
        }

        private void stopRecording(object sender, EventArgs e)
        {
            if (sourceStream != null)
            {
                sourceStream.StopRecording();
                sourceStream.Dispose();
                sourceStream = null;
            }
            if (this.waveWriter == null)
            {
                return;
            }
            this.waveWriter.Dispose();
            this.waveWriter = null;
            readWavStream("..\\..\\Sounds\\check.wav", false, "");
        }

        private void randomNumbers(object sender, EventArgs e)
        {
            Random rnd = new Random();           
            int number = rnd.Next(0, 9);
            textBox.Text = number.ToString();
        }

    
        public void readWavStream(string filename, bool isCompraing, string access)
        {
            try
            {
                using (FileStream fs = File.Open(filename, FileMode.Open))
                {
                    BinaryReader reader = new BinaryReader(fs);

                    int bytes = reader.ReadInt32();
                    byte[] byteArray = reader.ReadBytes(bytes);
                    //newFFT = new double[byteArray.Length];
                    int count = byteArray.Length / BUFFERSIZE;
                    for (int j = 0; j < count; j++)
                    {
                        int frameSize = BUFFERSIZE;
                        var frames = new byte[frameSize];
                        Array.Copy(byteArray, j * frameSize, frames, 0, frameSize);
                        //reader.Read(frames, j*frameSize, j*frameSize + frameSize);
                        //for(int k = 0; k < frameSize; k++)
                        //{
                        //    frames[k] = byteArray[j * frameSize + k];
                        //}
                        if (frames.Length == 0) return;

                        int SAMPLE_RESOLUTION = 16;
                        int BYTES_PER_POINT = SAMPLE_RESOLUTION / 8;
                        Int32[] vals = new Int32[frames.Length / BYTES_PER_POINT];
                        double[] Ys = new double[frames.Length / BYTES_PER_POINT];
                        for (int i = 0; i < vals.Length; i++)
                        {
                            byte hByte = frames[i * 2 + 1];
                            byte lByte = frames[i * 2 + 0];
                            vals[i] = (int)(short)((hByte << 8) | lByte);
                            Ys[i] = vals[i];
                        }
                        if (isCompraing == true)
                        {
                            
                            if (access == "Ada")
                            {
                                if (sourceFFT == null)
                                {
                                    sourceFFT = FFT(Ys);
                                }
                                else
                                {
                                    sourceFFT.Concat(FFT(Ys));
                                }

                            }
                            else if(access == "Kasia")
                            {
                                if (sourceFFT1 == null)
                                {
                                    sourceFFT1 = FFT(Ys);
                                }
                                else
                                {
                                    sourceFFT1.Concat(FFT(Ys));
                                }

                            }                           
                            
                        }
                        else if (isCompraing == false)
                        {

                            if (newFFT == null)
                            {
                                newFFT = FFT(Ys);
                            }
                            else
                            {
                                newFFT.Concat(FFT(Ys));
                            }
                        }
                    }

                }
            }
            catch(Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
        }
        
        public double[] FFT(double[] data)
        {
            double[] fft = new double[data.Length];
            System.Numerics.Complex[] fftComplex = new System.Numerics.Complex[data.Length];
            for (int i = 0; i < data.Length; i++)
                fftComplex[i] = new System.Numerics.Complex(data[i], 0.0);
            Accord.Math.FourierTransform.FFT(fftComplex, Accord.Math.FourierTransform.Direction.Forward);
            for (int i = 0; i < data.Length; i++)
            {
                fft[i] = fftComplex[i].Magnitude;
            }                
            return fft;
        }

        private void range1_CheckedChanged(object sender, EventArgs e)
        {
            range = "44100Hz";
        }

        private void range2_CheckedChanged(object sender, EventArgs e)
        {
            range = "48000Hz";
        }      

    
        private void button1_Click(object sender, EventArgs e)
        {
            List<double> resultsList = new List<double>();
            List<double> resultsList1 = new List<double>();
            for (int i = 1; i < 4; i++)
            {
                sourceFFT = null;
                string fileName = "..\\..\\Sounds\\Ada\\" + range + "\\" + i + "\\" + textBox.Text + ".wav";
                readWavStream(fileName, true, "Ada");
                resultsList.Add(ComputeCoeff(sourceFFT, newFFT));
            }
            for (int j = 1; j < 4; j++)
            {
                sourceFFT1 = null;
                string fileName = "..\\..\\Sounds\\Kasia\\" + range + "\\" + j + "\\" + textBox.Text + ".wav";
                readWavStream(fileName, true, "Kasia");
                resultsList1.Add(ComputeCoeff(sourceFFT1, newFFT));
            }
            checkResult(resultsList, resultsList1);
        }

       
        public double ComputeCoeff(double[] values1, double[] values2)
        {
            var avg1 = values1.Average();
            var avg2 = values2.Average();

            var sum1 = values1.Zip(values2, (x1, y1) => (x1 - avg1) * (y1 - avg2)).Sum();

            var sumSqr1 = values1.Sum(x => Math.Pow((x - avg1), 2.0));
            var sumSqr2 = values2.Sum(y => Math.Pow((y - avg2), 2.0));

            var result = sum1 / Math.Sqrt(sumSqr1 * sumSqr2);            
            return result;
        }

        public void checkResult(List<double> list, List<double> list1)
        {
           
            int a = 0;
            double sum = 0;
            double sum1 = 0;
            for(int i=0; i < list.Count; i++)
            {
                sum += list[i];
                sum1 += list1[i];
            }
            sum = sum / 3;
            sum1 = sum1 / 3;
            string result = "Ada: " + sum + " Kasia: " + sum1;
            
            richTextBox1.Text = result;
        }

       
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
