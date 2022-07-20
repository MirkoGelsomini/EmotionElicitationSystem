using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Linq;

namespace VCockpit.BitalinoLibrary.util
{
    class VCSampler
    {
        SAMPLER_THREAD_STATE state = SAMPLER_THREAD_STATE.INIT;
        VCBitalino bitalino = null;
        Bitalino dev = null;
        VCData bitalinoData = null;
        Thread t = null;

        public enum SAMPLER_THREAD_STATE
        {
            INIT,
            START,
            RUNNING,
            PAUSE,
            STOP,
        }

        public VCSampler(VCBitalino bitalino, Bitalino dev, VCData bitalinoData)
        {
            this.bitalino = bitalino;
            this.dev = dev;
            this.bitalinoData = bitalinoData;
        }

        public void SetStartState()
        {
            state = SAMPLER_THREAD_STATE.START;
        }

        public bool IsActive()
        {
            return state != SAMPLER_THREAD_STATE.STOP;
        }

        public void StartSampling()
        {
            t = new Thread(new ThreadStart(sampling_background));
            // Thread t_validator = new Thread(new ThreadStart(validation_background));
            t.Start();
            // t_validator.Start();
        }

        public void FinishSampling()
        {
            t.Join();
            // t_validator.Join();
        }

        private void sampling_background()
        {
            state = SAMPLER_THREAD_STATE.RUNNING;
            try
            {
                Console.WriteLine("[sampling thread] Start samplings in background thread ...");
                Bitalino.Frame[] frames = InitializationFrames();
                bool ledState = false;
                Console.WriteLine("[sampling thread] Read Frames");
                do
                {
                    Console.Write(".");
                    ledState = !ledState;   // toggle LED state
                    dev.trigger(new bool[] { false, false, ledState, false });
                    // sampling_timestamp_chunk.Add(getTimestampMilliseconds());
                    dev.read(frames); // get n frames from device
                    bitalino.SamplingFrames(frames, bitalinoData);
                    while (state == SAMPLER_THREAD_STATE.PAUSE)
                    {
                        bitalino.StopDeviceSampling(dev);
                        while (state == SAMPLER_THREAD_STATE.PAUSE)
                            Thread.Sleep(50);
                        bitalino.StartDeviceSampling(dev, bitalinoData.samplingRate);
                    }
                } while (state != SAMPLER_THREAD_STATE.STOP); //sampling_enabled
                Console.WriteLine("[sampling thread] Sampling thread finish the work");
            }
            catch (Bitalino.Exception e)
            {
                Console.WriteLine("[sampling thread] BITalino exception: {0}", e.Message);
            }
        }
        public void RestartSampling()
        {
            state = SAMPLER_THREAD_STATE.RUNNING;
        }

        public void PauseSampling()
        {
            state = SAMPLER_THREAD_STATE.PAUSE;
        }

        public void StopSampling()
        {
            state = SAMPLER_THREAD_STATE.STOP;
        }

        private Bitalino.Frame[] InitializationFrames()
        {
            // first sample timestamp
            Bitalino.Frame[] frames = new Bitalino.Frame[bitalinoData.nFrames];
            for (int i = 0; i < frames.Length; i++)
                frames[i] = new Bitalino.Frame();   // must initialize all elements in the array
            return frames;
        }

        public static long GetTimestampMilliseconds()
        {
            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            return milliseconds;
        }

        public void SaveResults(String nameFile)
        {
            List<double> sample_resp = bitalino.GetRespSamples(bitalinoData);
            List<double> sample_ecg = bitalino.GetEcgSamples(bitalinoData);
            List<double> sample_eda = bitalino.GetEdaSamples(bitalinoData);
            List<double> sample_ppg = bitalino.GetPpgSamples(bitalinoData);
            List<int> sample_sequences = bitalino.GetSequencesSamples(bitalinoData);

            Console.WriteLine("Exporting Results ...");
            SaveOnFile("sampling_pzt_" + bitalinoData.timestamp + "_" + bitalinoData.samplingRate +"_"+ nameFile +".csv", sample_resp);
            SaveOnFile("sampling_ecg_" + bitalinoData.timestamp + "_" + bitalinoData.samplingRate +"_"+ nameFile +".csv", sample_ecg);
            SaveOnFile("sampling_eda_" + bitalinoData.timestamp + "_" + bitalinoData.samplingRate +"_"+ nameFile +".csv", sample_eda);
            SaveOnFile("sampling_ppg_" + bitalinoData.timestamp + "_" + bitalinoData.samplingRate +"_"+ nameFile +".csv", sample_ppg);

            // it will be used analytics service to perform data quality analysis and timestamp inferring
            SaveOnFileInt("sampling_sequences_" + bitalinoData.timestamp + "_" + bitalinoData.samplingRate + "_" + nameFile +".csv", sample_sequences);
        }

        private void SaveOnFile(string filename, List<double> samples)
        {
            TextWriter tw = new StreamWriter(filename);

            for (int i = 0; i < samples.Count; i++)
            {
                string line = samples[i].ToString();
                tw.WriteLine(line);
            }
            tw.Close();
        }

        private void SaveOnFileInt(string filename, List<int> samples)
        {
            TextWriter tw = new StreamWriter(filename);

            for (int i = 0; i < samples.Count; i++)
            {
                string line = samples[i].ToString();
                tw.WriteLine(line);
            }
            tw.Close();
        }

        //Clear Buffer to delete Buffered Data
        public void ClearSamples()
        {
            bitalino.GetRespSamples(bitalinoData);
            bitalino.GetEcgSamples(bitalinoData);
            bitalino.GetEdaSamples(bitalinoData);
            bitalino.GetPpgSamples(bitalinoData);
            bitalino.GetSequencesSamples(bitalinoData);
        }
    }
}
