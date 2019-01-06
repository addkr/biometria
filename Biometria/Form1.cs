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
        string access = "";
        double[] sourceFFT;
        double[] newFFT;
        WaveIn sourceStream;
        WaveFileWriter waveWriter;
        readonly String FilePath;
        readonly String FileName;
        readonly int InputDeviceIndex;
        public BufferedWaveProvider bwp;

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string command, StringBuilder retstring, int returnLength, IntPtr callback);

        public FormBiometria()
        {
            InitializeComponent();
            this.FileName = "check.wav";
            this.FilePath = "c:\\biometria\\";
        }



        public void startRecording(object sender, EventArgs e)
        {
            sourceStream = new WaveIn
            {

                WaveFormat =
                    new WaveFormat(44100, 2)
            };

            sourceStream.DataAvailable += this.SourceStreamDataAvailable;

            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }

            waveWriter = new WaveFileWriter(@"c:\\biometria\\check.wav", sourceStream.WaveFormat);
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
            readWav("c:\\biometria\\check.wav", false);
            //recordEndButton.Enabled = false;
            //Application.Exit();
            //Environment.Exit(0);
        }

        private void randomNumbers(object sender, EventArgs e)
        {
            string randomSequence = "";
            Random rnd = new Random();
            //for(int i=0; i < 5; i++)
            //{
            //    int number = rnd.Next(0, 9);
            //    randomSequence += number;
            //}
            int number = rnd.Next(0, 9);
            //textBox.Text = randomSequence;
            textBox.Text = number.ToString();
        }

        //private void startRecording(object sender, EventArgs e)
        //{
        //    mciSendString("Open new type waveaudio alias recsound", null, 0, IntPtr.Zero);
        //    mciSendString("Record recsound", null, 0, IntPtr.Zero);
        //    //dosmth(access,);

        //    //"..\\..\\Sounds\\Ada\\44100Hz\\1\\1.wav"
        //}

        //private void stopRecording(object sender, EventArgs e)
        //{
        //    string date = DateTime.Now.ToString();
        //    date = date.Replace(":", "");
        //    date = date.Replace("/", "");
        //    date = date.Replace(" ", "");
        //    date = date.Replace(".", "");
        //    string path = "Save recsound c:\\biometria\\check.wav";
        //    //Console.Out.Write(path);
        //    //"Save recsound c:\\biometria\\sound.wav"
        //    mciSendString(path, null, 0, IntPtr.Zero);
        //    mciSendString("Close recsound", null, 0, IntPtr.Zero);
        //    readWav("c:\\biometria\\check.wav", false);
        //}

        //public void dosmth(string access, string number, string range, string n)
        //{
        //    using (WaveFileReader reader = new WaveFileReader("..\\..\\Sounds\\"+access+"\\"+range+"\\"+n+"\\"+number+".wav"))
        //    {
        //        //Assert.AreEqual(16, reader.WaveFormat.BitsPerSample, "Only works with 16 bit audio");
        //        byte[] buffer = new byte[reader.Length];
        //        int read = reader.Read(buffer, 0, buffer.Length);
        //        short[] sampleBuffer = new short[read / 2];
        //        Buffer.BlockCopy(buffer, 0, sampleBuffer, 0, read);
        //        Console.Out.WriteLine("buffer: " +buffer);
        //        Console.Out.WriteLine("sampleBuffer: " + sampleBuffer);  
        //        //double fft = FFT()
        //    }
        //}

        bool readWav(string filename, bool isCompraing)
        {
            float[] L, R;
            
            L = R = null;
            //float [] left = new float[1];

            //float [] right;
            try
            {
                using (FileStream fs = File.Open(filename, FileMode.Open))
                {
                    BinaryReader reader = new BinaryReader(fs);

                    // chunk 0
                    int chunkID = reader.ReadInt32();
                    int fileSize = reader.ReadInt32();
                    int riffType = reader.ReadInt32();


                    // chunk 1
                    int fmtID = reader.ReadInt32();
                    int fmtSize = reader.ReadInt32(); // bytes for this chunk
                    int fmtCode = reader.ReadInt16();
                    int channels = reader.ReadInt16();
                    int sampleRate = reader.ReadInt32();
                    int byteRate = reader.ReadInt32();
                    int fmtBlockAlign = reader.ReadInt16();
                    int bitDepth = reader.ReadInt16();

                    if (fmtSize == 18)
                    {
                        // Read any extra values
                        int fmtExtraSize = reader.ReadInt16();
                        reader.ReadBytes(fmtExtraSize);
                    }

                    // chunk 2
                    int dataID = reader.ReadInt32();
                    int bytes = reader.ReadInt32();

                    // DATA!
                    byte[] byteArray = reader.ReadBytes(bytes);

                    int frameSize = 1024; // how much buffer to read at once
                    //var frames = new byte[frameSize]; // init the byte array
                    //bwp.Read(frames, 0, frameSize); // read the buffer
                    int SAMPLE_RESOLUTION = 16; // this is for 16-bit integers in a byte array
                    int BYTES_PER_POINT = SAMPLE_RESOLUTION / 8;
                    Int32[] vals = new Int32[byteArray.Length / BYTES_PER_POINT];
                    for (int i = 0; i < vals.Length; i++)
                    {
                        byte hByte = byteArray[i * 2 + 1];
                        byte lByte = byteArray[i * 2 + 0];
                        vals[i] = (int)(short)((hByte << 8) | lByte);
                    }
                    //int m = 0;
                    //for (int n = 0; n < byteArray.Length; n++)
                    //{
                    //    if(byteArray[n] != 0)
                    //    {                            
                    //        byteArray[m] = byteArray[n];
                    //        //Console.Out.WriteLine(byteArray[m]);
                    //        m++;
                    //    }
                    //}

                    int bytesForSamp = bitDepth / 8;
                    int samps = bytes / bytesForSamp;


                    float[] asFloat = null;
                    double[] db = null;
                    switch (bitDepth)
                    {
                        case 64:
                            double[]
                            asDouble = new double[samps];
                            Buffer.BlockCopy(byteArray, 0, asDouble, 0, bytes);
                            asFloat = Array.ConvertAll(asDouble, e => (float)e);
                            break;
                        case 32:
                            asFloat = new float[samps];
                            Buffer.BlockCopy(byteArray, 0, asFloat, 0, bytes);
                            break;
                        case 16:
                            Int16[]
                            asInt16 = new Int16[samps];
                            asDouble = new double[samps];                           
                            Buffer.BlockCopy(byteArray, 0, asInt16, 0, bytes);
                            asFloat = Array.ConvertAll(asInt16, e => e / (float)Int16.MaxValue);
                            //if (isCompraing == true)
                            //{
                            //    sourceFFT = floatToDouble(asFloat);
                            //}
                            //else if(isCompraing == false)
                            //{
                            //    newFFT = floatToDouble(asFloat);
                            //}
                            //db = floatToDouble(asFloat);
                            break;
                        default:
                            return false;
                    }

                    //switch (channels)
                    //{
                    //    case 1:
                    //        L = asFloat;
                    //        R = null;
                    //        return true;
                    //    case 2:
                    //        L = new float[samps];
                    //        R = new float[samps];
                    //        int s = 0;
                    //        for (int i = 0 ; i < samps; i++)
                    //        {
                    //            L[i] = asFloat[s++];
                    //            R[i] = asFloat[s++];
                    //        }
                    //        //db = floatToDouble(L);
                    //        return true;
                    //    default:
                    //        return false;
                    //}
                }
                
            }
            catch(Exception ex)
            {
                //Debug.Log("...Failed to load note: " + filename);
                Console.Out.WriteLine(ex.Message);
                return false;
                //left = new float[ 1 ]{ 0f };
            }

            return false;
        }

    

        public double[] floatToDouble(float[] ft)
        {
            double[] db = new double[ft.Length+1];
            for(int i = 0; i < ft.Length; i++)
            {
                db[i] = System.Convert.ToDouble(ft[i]);
                //if(i == 30)
                //{
                //    db[i + 1] = 0;
                //}
            }
            double[] fft = FFT(db);
            Console.Out.WriteLine(fft);
            return fft;
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
                Console.Out.WriteLine(fft[i]);
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

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            access = "Kasia";
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            access = "Ada";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string fileName = "..\\..\\Sounds\\" + access + "\\" + range + "\\" + "1" + "\\" + textBox.Text + ".wav";
            readWav("c:\\biometria\\test.wav", true);
            //compareFunction(sourceFFT, newFFT);
        }

        private void compareFunction(double[] source, double[] user)
        {
            for(int i = 0; i < source.Length; i++)
            {
                Console.Out.WriteLine(source[i]);
            }

            for (int j = 0; j < source.Length; j++)
            {
                Console.Out.WriteLine(user[j]);
            }
        }
    }
}
