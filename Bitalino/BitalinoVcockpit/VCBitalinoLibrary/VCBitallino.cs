using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace VCockpit.BitalinoLibrary
{

    public class VCData
    {
        public enum SensorType
        {
            RESP,
            ECG,
            EDA,
            PULSE,
            EMG,
            EEG,
            ACC,
        }

        /**
         * Timestamp of data
         */
        public long timestamp;

        /**
         * Number of frames per sensor
         */
        public readonly int nFrames;

        /**
         * Sampling rate
         */
        public readonly int samplingRate;

        // TODO to be used
        // private Dictionary<SensorType, List<double>> samples;
        // public Dictionary<SensorType, List<double>> Samples
        //{
        //  get { return samples; }
        //}

        // temporal fix
        private List<List<double>> samples;
        public List<List<double>> Samples
        {
          get { return samples; }
        }

        // cyclic sequence number
        private List<int> samples_sequence;
        public List<int> Samples_sequence
        {
            get { return samples_sequence; }
        }

        public VCData(int nFrames, int samplingRate)
        {
            this.nFrames = nFrames;
            this.samplingRate = samplingRate;
            samples = new List<List<double>>();
            samples.Add(new List<double>()); // pzt
            samples.Add(new List<double>()); // ecg
            samples.Add(new List<double>()); // eda
            samples.Add(new List<double>()); // ppg
            // cyclic sequence number (0 ... 15)
            samples_sequence = new List<int>();
        }
    }

    public class VCBitalinoConfig
    {
        private string connectionData;
        public string ConnectionData
        {
            get { return connectionData; }
        }

        VCBitalinoConfig(string connectionData)
        {
            this.connectionData = connectionData;
        }

        public static VCBitalinoConfig FromBluetooth(string data) 
        {
            // Validate
            Regex macAddressMatcher = new Regex("^(?:[0-9a-fA-F]{2}:){5}[0-9a-fA-F]{2}|(?:[0-9a-fA-F]{2}-){5}[0-9a-fA-F]{2}|(?:[0-9a-fA-F]{2}){5}[0-9a-fA-F]{2}$");
            if (!macAddressMatcher.IsMatch(data)) 
            {
                throw new ArgumentException("Invalid bluetooth mac address");
            }
            return new VCBitalinoConfig(data);
        }

        public static VCBitalinoConfig FromSerialPort(string data)
        {
            // Validate
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports) 
            {
                if (port.Equals(data)) 
                {
                    return new VCBitalinoConfig(data);
                }
            }
            throw new ArgumentException("Invalid serial port specified");
        }
    }

    public class VCBitalino
    {
        // NOTE: the enum integer is related to the expected pin position on bitalino
        enum SENSORS
        {
            RESP, // EMG,
            ECG,
            EDA,
            PULSE,
            // EEG,
            // ACC,
        }

        /***
         * 
         * Examples for bitalino device connection
         *      new Bitalino("20:19:07:00:80:C2");  // device MAC address
         *      new Bitalino("COM7");               // Bluetooth virtual COM port or USB-UART COM port
         **/
        public Bitalino Instance(string macAddress)
        {
            Bitalino dev = new Bitalino(macAddress);  // device MAC address
            return dev;
        }
        
        public List<string> ListBluetoothDevices()
        {
            Console.WriteLine("The following bluetooth devices were found:");
            Bitalino.DevInfo[] devs = Bitalino.find();
            List<string> devices = new List<string>();
            foreach (Bitalino.DevInfo d in devs)
            {
                devices.Add(string.Format("{0} - {1}", d.macAddr, d.name));
            }
            return devices;
        }
        public List<string> ListSerialPortDevices()
        {
            Console.WriteLine("The following serial ports were found:");
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                Console.WriteLine(port); // Display each port name to the console.
            }
            return new List<string>(ports);
        }

        public void StartDeviceSampling(Bitalino dev, int samplingRate)
        {
            dev.start(samplingRate, new int[] { 0, 1, 2, 3, 4, 5 });   // start acquisition of all channels at 1000 Hz
        }


        //samples.Add(SensorType.RESP, new List<double>());
        //samples.Add(SensorType.ECG, new List<double>());
        //samples.Add(SensorType.EDA, new List<double>());

        public void SamplingFrames(Bitalino.Frame[] frames, VCData data)
        {
            for (int i = 0; i < frames.Length; i++)
            {
                Bitalino.Frame f = frames[i];  // get a reference to the first frame of each n frames block
                /*Console.WriteLine("{0} : {1} {2} {3} {4} ; {5} {6} {7} {8} {9} {10}",   // dump the first frame
                          f.seq,
                          f.digital[0], f.digital[1], f.digital[2], f.digital[3],
                          f.analog[0], f.analog[1], f.analog[2], f.analog[3], f.analog[4], f.analog[5]);*/
                //Console.WriteLine("{0} : {2}", i, f.seq, f.analog[4]/25);
                // 10 bits sampling
                data.Samples_sequence.Add((int)f.seq);
                int nBitsChannel = 10;
                double pzt = VCTransferFunctions.convertPzt((double)f.analog[(int)SENSORS.RESP], nBitsChannel);
                double ecg = VCTransferFunctions.convertEcg((double)f.analog[(int)SENSORS.ECG], nBitsChannel);
                double eda = VCTransferFunctions.convertEda((double)f.analog[(int)SENSORS.EDA], nBitsChannel);
                double ppg = VCTransferFunctions.convertPulse((double)f.analog[(int)SENSORS.PULSE]);
                // Console.WriteLine("PZT: " + pzt + " [%]");
                // Console.WriteLine("ECG: " + ecg + " [mV]");
                // Console.WriteLine("EDA: " + eda + " [uS]");

                // TODO dictionary to be used
                data.Samples[(int)SENSORS.RESP].Add(pzt);
                data.Samples[(int)SENSORS.ECG].Add(ecg);
                data.Samples[(int)SENSORS.EDA].Add(eda);
                data.Samples[(int)SENSORS.PULSE].Add(ppg);
            }
        }

        public List<double> GetRespSamples(VCData data)
        {
            return data.Samples[(int)SENSORS.RESP];
        }

        public List<double> GetEcgSamples(VCData data)
        {
            return data.Samples[(int)SENSORS.ECG];
        }

        public List<double> GetEdaSamples(VCData data)
        {
            return data.Samples[(int)SENSORS.EDA];
        }

        public List<double> GetPpgSamples(VCData data)
        {
            return data.Samples[(int)SENSORS.PULSE];
        }
        

        public List<int> GetSequencesSamples(VCData data)
        {
            return data.Samples_sequence;
        }

        public void StopDeviceSampling(Bitalino dev)
        {
            dev.stop();
        }

        public void DisposeDevice(Bitalino dev)
        {
            dev.Dispose(); // disconnect from device
        }
    }


    class VCTransferFunctions
    {

        /***
         * 
         * Conversion EMG to real value
         * 
         * Input
         *      double value
         * Output
         *      double value (milliVolt)
         * Details
         *      Formula
         *          EMG(V) = [(ADC/2**n - 0.5)*VCC]/Gemg
         *          EMG(mV) = EMG(V) * 1000
         *          Constants
         *              VCC = 3.3
         *              Gecg = 1009
         *              n is the number of bits of the channel
         *                  The number of bits for each channel depends on the resolution of the Analog-to-Digital Converter (ADC); 
         *                  in BITalino the first four channels are sampled using 10-bit resolution (𝑛=10), 
         *                  while the last two may be sampled using 6-bit (𝑛=6).
         *          Parameters
         *              EMG(V):  ECG value in Volt (V)
         *              EMG(mV): ECG value in milliVolt (mV)
         *              ADC:     Value sampled from the channel
         */
        static public double convertEmg(double value, int nBitsChannel)
        {
            const double VCC = 3.3;
            const double gEMG = 1009.0;
            double emg = value / Math.Pow(2, nBitsChannel);
            emg -= -0.5;
            emg *= VCC;
            emg /= gEMG;
            // return millivolt
            emg *= 1000.0;
            return emg;
        }

        /***
         * 
         * Conversion ECG to real value
         * 
         * Input
         *      double value
         * Output
         *      double value (milliVolt)
         * Details
         *      Formula
         *          ECG(V) = [(ADC/2**n - 0.5)*VCC]/Gecg
         *          ECG(mV) = ECG(V) * 1000
         *          Constants
         *              VCC = 3.3
         *              Gecg = 1100
         *              n is the number of bits of the channel
         *                  The number of bits for each channel depends on the resolution of the Analog-to-Digital Converter (ADC); 
         *                  in BITalino the first four channels are sampled using 10-bit resolution (𝑛=10), 
         *                  while the last two may be sampled using 6-bit (𝑛=6).
         *          Parameters
         *              ECG(V): ECG value in Volt (V)
         *              ADC:    Value sampled from the channel
         */
        static public double convertEcg(double value, int nBitsChannel)
        {
            const double VCC = 3.3;
            const double gECG = 1100.0;
            double ecg = value / Math.Pow(2, nBitsChannel);
            ecg -= 0.5;
            ecg *= VCC;
            ecg /= gECG;
            // return millivolt
            ecg *= 1000.0;
            return ecg;
        }

        /***
         * 
         * Conversion EDA to real value
         * 
         * Input
         *      double value
         * Output
         *      double value (microSiemens)
         * Details
         *      Formula
         *          EDA(uS) = [(ADC/2**n)*VCC]/0.132
         *          Constants
         *              VCC = 3.3
         *              n is the number of bits of the channel
         *                  The number of bits for each channel depends on the resolution of the Analog-to-Digital Converter (ADC); 
         *                  in BITalino the first four channels are sampled using 10-bit resolution (𝑛=10), 
         *                  while the last two may be sampled using 6-bit (𝑛=6).
         *          Parameters
         *              EDA(uS): EDA value in micro-Siemens (uS)
         *              ADC:     Value sampled from the channel
         */
        static public double convertEda(double value, int nBitsChannel)
        {
            const double VCC = 3.3;
            double eda = value / Math.Pow(2, nBitsChannel);
            eda *= VCC;
            // return microsiemesens
            eda /= 0.132;
            return eda;
        }

        /***
         * 
         * Conversion PZT (resp) to real value
         * 
         * Input
         *      double value
         * Output
         *      double value (percentual) [-50%, 50%]
         * Details
         *      Formula
         *          PZT(%) = [(ADC/2**n) - 0.5] * 100
         *          Constants
         *              n is the number of bits of the channel
         *                  The number of bits for each channel depends on the resolution of the Analog-to-Digital Converter (ADC); 
         *                  in BITalino the first four channels are sampled using 10-bit resolution (𝑛=10), 
         *                  while the last two may be sampled using 6-bit (𝑛=6).
         *          Parameters
         *              PZT(%):  Displacement value in percentage (%) of full scale
         *              ADC:     Value sampled from the channel
         */
        static public double convertPzt(double value, int nBitsChannel)
        {
            double pzt = value / Math.Pow(2, nBitsChannel);
            pzt -= 0.5;
            // return percentual
            pzt *= 100;
            return pzt;
        }

        /***
         * 
         * Conversion EEG to real value
         * 
         * Input
         *      double value
         * Output
         *      double value (microVolt)
         * Details
         *      Formula
         *          EEG(V) = {[(ADC/2**n) - 0.5]*VCC}/Geeg
         *          EEG(uV) = EEG(V)*1_000_000
         *          Constants
         *              VCC = 3.3
         *              Geeg = 41782
         *              n is the number of bits of the channel
         *                  The number of bits for each channel depends on the resolution of the Analog-to-Digital Converter (ADC); 
         *                  in BITalino the first four channels are sampled using 10-bit resolution (𝑛=10), 
         *                  while the last two may be sampled using 6-bit (𝑛=6).
         *          Parameters
         *              EEG(uV): EEG value in micro-Volt (uV)
         *              ADC:     Value sampled from the channel
         */
        static public double convertEeg(double value, int nBitsChannel)
        {
            const double VCC = 3.3;
            const double gEEG = 41782.0;
            double eeg = value / Math.Pow(2, nBitsChannel);
            eeg -= 0.5;
            eeg *= VCC;
            eeg /= gEEG;
            // return microVolt
            eeg *= 1000000.0;
            return eeg;
        }

        /***
         * 
         * Conversion Acc to real value
         * 
         * Input
         *      double value
         * Output
         *      double value
         * Details
         *      Formula
         *          ACC(g) = [(ADC - Cmin)/(Cmax - Cmin)] * 2 - 1
         *          
         *          Parameters
         *              ACC(g): ACC value in g-force (g)
         *              ADC:    Value sampled from the channel
         *              Cmin:   Minimum calibration value
         *              Cmax:   Maximum calibration value
         */
        static public double convertAcc(double value)
        {
            // NOTE: that's value should be setted
            double cmin = 0.0;
            double cmax = 25.0;
            //  double acc = (value/25.0) - 1.0;
            double acc = (value - cmin) / (cmax - cmin);
            acc = acc * 2.0 - 1.0;
            return acc;
        }

        static public double convertPulse(double value)
        {
            // no transfer function
            return value;
        }
    }
}
