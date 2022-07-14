using System;
using System.Collections;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace BitalinoCore.Utils.Sensor
{
    public enum SYSTEM_STATE
    {
        OK, // all working
        PROBLEMS_WITH_BLUETOOTH,
        PROBLEMS_WITH_SERIAL_PORTS,
        BITALINO_DEVICE_NOT_FOUND,
        PROBLEMS_WITH_DEVICE_SET_UP,
        ECG_SENSOR_NOT_WORKING_CORRECTLY,
        EDA_SENSOR_NOT_WORKING_CORRECTLY,
        RESP_SENSOR_NOT_WORKING_CORRECTLY,
        MULTIPLE_SENSORS_NOT_CORRECTLY_WORKING, // more than one is not working
    }

    public enum SENSORS
    {
        RESP, // EMG,
        ECG,
        EDA,
        // EEG,
        // ACC,
        // PULSE,
    }

    public enum SAMPLER_THREAD_STATE
    {
        INIT,
        START,
        RUNNING,
        PAUSE,
        STOP,
    }

    public class Sampler
    {
        // sampling signals
        private volatile List<double> sampling_resp = null;
        private volatile List<double> sampling_ecg = null;
        private volatile List<double> sampling_eda = null;
        // private volatile List<long> sampling_timestamp_chunk = null;
        private volatile List<string> sampling_log_actions = null;
        private Bitalino dev = null;
        private readonly int samplingRate = 1000; // Hz
        private long start_sampling_timestamp = -1;
        private bool sampling_enabled = true;
        // for hierarchical update not limited to the cache for a full visibility in all thread processes
        private volatile SAMPLER_THREAD_STATE sampler_state = SAMPLER_THREAD_STATE.INIT;
        private volatile int validator_counter = 5; // 5 [seconds]

        public Sampler(){
            sampling_resp = new List<double>();
            sampling_ecg = new List<double>();
            sampling_eda = new List<double>();
            // sampling_timestamp_chunk = new List<long>();
            sampling_log_actions = new List<string>();
            start_sampling_timestamp = -1;
        }

        /***
         * Bootstrap
         **/
        public SYSTEM_STATE bootstrap(string device_mac_address)
        {
            /***
             *  Test show bluetooth devices 
             **/
            try
            {
                showBluetoothDevices();
            }
            catch (Bitalino.Exception e)
            {
                Console.WriteLine("[ERROR] BITalino exception: {0}", e.Message);
                return SYSTEM_STATE.PROBLEMS_WITH_BLUETOOTH;
            }

            /***
             *  Test show serial ports
             **/
            try
            {
                showSerialPorts();
            }
            catch (Bitalino.Exception e)
            {
                Console.WriteLine("[ERROR] BITalino exception: {0}", e.Message);
                return SYSTEM_STATE.PROBLEMS_WITH_SERIAL_PORTS;
            }

            /***
             *  Test connection to bitalino
             **/
            try
            {
                connectToBitalino(device_mac_address);
            }
            catch (Bitalino.Exception e)
            {
                Console.WriteLine("[ERROR] BITalino exception: {0}", e.Message);
                return SYSTEM_STATE.BITALINO_DEVICE_NOT_FOUND;
            }
            
            /***
             * Test setup
             **/
            try
            {
                setup();
            }
            catch (Bitalino.Exception e)
            {
                Console.WriteLine("[ERROR] BITalino exception: {0}", e.Message);
                return SYSTEM_STATE.PROBLEMS_WITH_DEVICE_SET_UP;
            }
            return SYSTEM_STATE.OK;
        }

        private void showBluetoothDevices()
        {
            Console.WriteLine("The following bluetooth devices were found:");
            Bitalino.DevInfo[] devs = Bitalino.find();
            foreach (Bitalino.DevInfo d in devs)
            {
                Console.WriteLine("{0} - {1}", d.macAddr, d.name);
            }
        }

        private void showSerialPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            Console.WriteLine("The following serial ports were found:");
            foreach (string port in ports)
            {
                Console.WriteLine(port); // Display each port name to the console.
            }
        }

        private void connectToBitalino(string device_mac_address)
        {
            Console.WriteLine("Connecting to device...");
            // device MAC address
            dev = new Bitalino(device_mac_address);
            // Bluetooth virtual COM port or USB-UART COM port
            //Bitalino dev = new Bitalino("COM7");
            //Bitalino dev = new Bitalino("COM8");
        }

        private void setup()
        {
            Console.WriteLine("Connected to device.");
            string ver = dev.version();    // get device version string
            Console.WriteLine("BITalino version: {0}", ver);
            dev.battery(10);  // set battery threshold (optional)
            // try a start for early problem detection
            dev.start(samplingRate, new int[] { 0, 1, 2, 3, 4, 5 }); // start acquisition of all channels at 1000 Hz
            dev.stop();
        }

        public void clearSampling()
        {
            Console.WriteLine("Clear samplings");
            sampling_resp = new List<double>();
            sampling_ecg = new List<double>();
            sampling_eda = new List<double>();
            // sampling_timestamp_chunk = new List<long>();
            sampling_log_actions = new List<string>();
            start_sampling_timestamp = -1;
        }

        public void startDeviceSampling()
        {
            try
            {
                // restart
                dev.start(samplingRate, new int[] { 0, 1, 2, 3, 4, 5 });   // start acquisition of all channels at 1000 Hz
            }
            catch (Bitalino.Exception e)
            {
                Console.WriteLine("[ERROR] BITalino exception: {0}", e.Message);
            }
        }

        /***
         * sampling
         * 
         * https://stackoverflow.com/questions/45371263/starting-and-stopping-loop-from-thread-c-sharp
         * https://stackoverflow.com/questions/4411639/net-is-there-any-way-to-create-a-non-static-thread-method
         * https://docs.microsoft.com/en-us/dotnet/api/system.threading.thread?view=net-6.0
         * https://docs.microsoft.com/en-us/dotnet/api/system.threading.volatile?view=net-6.0
         **/
        public void sampling(bool background)
        {
            if (background){
                sampler_state = SAMPLER_THREAD_STATE.START;
                Thread t = new Thread(new ThreadStart(sampling_background));
                Thread t_validator = new Thread(new ThreadStart(validation_background));
                // Start ThreadProc.  Note that on a uniprocessor, the new thread does not get any processor time until the main thread
                // is preempted or yields.  Uncomment the Thread.Sleep that follows t.Start() to see the difference.
                Console.WriteLine("[main thread] The thread is starting. It follows the commands to control it:");
                Console.WriteLine("0 - Restart sampling");
                Console.WriteLine("1 - Pause sampling");
                Console.WriteLine("2 - Stop sampling");
                t.Start();
                t_validator.Start();
                // Do while main control loop
                while (sampler_state != SAMPLER_THREAD_STATE.STOP)
                {
                    if (!Console.KeyAvailable)
                    {
                        Thread.Sleep(50);
                    }
                    else
                    {
                        try
                        {
                            string userInput = Console.ReadLine();
                            int choice = int.Parse(userInput);
                            long timestampAction = getTimestampMilliseconds();
                            string log = timestampAction.ToString();
                            switch (choice)
                            {
                                case 0:
                                    Console.WriteLine("[main thread] Re-start samplings ...");
                                    log = log + ", 0";
                                    sampling_log_actions.Add(log);
                                    restarSampling();
                                    break;
                                case 1:
                                    Console.WriteLine("[main thread] Pause samplings ...");
                                    log = log + ", 1";
                                    sampling_log_actions.Add(log);
                                    pauseSampling();
                                    break;
                                case 2:
                                    Console.WriteLine("[main thread] Stops samplings ...");
                                    log = log + ", 2";
                                    sampling_log_actions.Add(log);
                                    stopSampling();
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch 
                        {
                            Console.WriteLine("[main thread] Command not allowed");
                        }
                    }
                }
                Console.WriteLine("[main thread] sampling has finished");
                t.Join();
                t_validator.Join();
                // Console.ReadLine();
            }
            else
            {
                SamplingInForeground(); // blocking main thread sampling
            }
        }

        private void restarSampling()
        {
            sampler_state = SAMPLER_THREAD_STATE.RUNNING;
        }

        private void pauseSampling()
        {
            sampler_state = SAMPLER_THREAD_STATE.PAUSE;
        }

        private void stopSampling()
        {
            sampler_state = SAMPLER_THREAD_STATE.STOP;
        }

        private void sampling_background()
        {
            sampler_state = SAMPLER_THREAD_STATE.RUNNING;
            try
            {
                Console.WriteLine("[sampling thread] Start samplings in background thread ...");
                Bitalino.Frame[] frames = initializationFrames();
                bool ledState = false;
                Console.WriteLine("[sampling thread] Read Frames");
                do
                {
                    Console.Write(".");
                    ledState = !ledState;   // toggle LED state
                    dev.trigger(new bool[] { false, false, ledState, false });
                    // sampling_timestamp_chunk.Add(getTimestampMilliseconds());
                    dev.read(frames); // get 1000 frames from device
                    samplingFrames(frames);
                    while (sampler_state == SAMPLER_THREAD_STATE.PAUSE)
                    {
                        stopDeviceSampling();
                        while (sampler_state == SAMPLER_THREAD_STATE.PAUSE)
                            Thread.Sleep(50);
                        startDeviceSampling();
                    }
                } while (sampler_state != SAMPLER_THREAD_STATE.STOP); //sampling_enabled
                Console.WriteLine("[sampling thread] Sampling thread finish the work");
            }
            catch (Bitalino.Exception e)
            {
                Console.WriteLine("[sampling thread] BITalino exception: {0}", e.Message);
            }
        }

        private void validation_background()
        {
            sampler_state = SAMPLER_THREAD_STATE.RUNNING;
            try
            {
                Console.WriteLine("[validation thread] Start validation in background thread ...");
                do
                {
                    while (sampler_state == SAMPLER_THREAD_STATE.PAUSE)
                        Thread.Sleep(50);
                    if (validator_counter == 0) { 
                        validator_counter = 5;
                        List<double> sampling_resp_sub = Enumerable.Reverse(sampling_resp).Take(4000).Reverse().ToList();
                        List<double> sampling_ecg_sub = Enumerable.Reverse(sampling_ecg).Take(4000).Reverse().ToList();
                        List<double> sampling_eda_sub = Enumerable.Reverse(sampling_eda).Take(4000).Reverse().ToList();
                        SYSTEM_STATE system_state_window = analyzeSubSamples(sampling_resp_sub,
                                                                             sampling_ecg_sub,
                                                                             sampling_eda_sub);
                        if (system_state_window != SYSTEM_STATE.OK)
                            Console.WriteLine("[validation thread] Problem in sampled data window");
                        else
                            Console.WriteLine("[validation thread] Sampled data window seems ok");
                    }
                    validator_counter -= 1;
                    Thread.Sleep(1000);
                } while (sampler_state != SAMPLER_THREAD_STATE.STOP); //sampling_enabled
                Console.WriteLine("[validation thread] Validation thread finish the work");
            }
            catch (Bitalino.Exception e)
            {
                Console.WriteLine("[validation thread] BITalino exception: {0}", e.Message);
            }
        }

        public void SamplingInForegroundTestSensor(int seconds)
        {
            try
            {
                Console.WriteLine("Start samplings for {0} seconds ...", seconds);
                Bitalino.Frame[] frames = initializationFrames();
                bool ledState = false;
                for (int second = 0; second < seconds; second++)
                {
                    ledState = !ledState;   // toggle LED state
                    dev.trigger(new bool[] { false, false, ledState, false });
                    // Console.WriteLine("Read Frames");
                    // sampling_timestamp_chunk.Add(getTimestampMilliseconds());
                    dev.read(frames); // get 1000 frames from device
                    samplingFrames(frames);
                }
            }
            catch (Bitalino.Exception e)
            {
                Console.WriteLine("BITalino exception: {0}", e.Message);
            }
        }

        public void SamplingInForeground()
        {
            try
            {
                Console.WriteLine("Start samplings ...");
                Console.WriteLine("Press something to interrupt the sampling loop ...");
                Bitalino.Frame[] frames = initializationFrames();
                bool ledState = false;
                do
                {
                    ledState = !ledState;   // toggle LED state
                    dev.trigger(new bool[] { false, false, ledState, false });
                    // Console.WriteLine("Read Frames");
                    // sampling_timestamp_chunk.Add(getTimestampMilliseconds());
                    dev.read(frames); // get 1000 frames from device
                    samplingFrames(frames);
                    if (Console.KeyAvailable)
                        sampling_enabled = false;
                } while (sampling_enabled); //sampling_enabled
            }
            catch (Bitalino.Exception e)
            {
                Console.WriteLine("BITalino exception: {0}", e.Message);
            }
        }

        private Bitalino.Frame[] initializationFrames()
        {
            // first sample timestamp
            start_sampling_timestamp = getTimestampMilliseconds(); // ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
            Bitalino.Frame[] frames = new Bitalino.Frame[samplingRate];
            for (int i = 0; i < frames.Length; i++)
                frames[i] = new Bitalino.Frame();   // must initialize all elements in the array
            return frames;
        }

        /***
         *  The number of bits for each channel depends on the resolution of the Analog - to - Digital Converter(ADC);
         *      in BITalino the first four channels are sampled using 10 - bit resolution(𝑛= 10), 
         *       while the last two may be sampled using 6 - bit(𝑛= 6).
         */
        private void samplingFrames(Bitalino.Frame[] frames)
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
                int nBitsChannel = 10;
                double pzt = TransferFunctions.convertPzt((double)f.analog[(int)SENSORS.RESP], nBitsChannel);
                double ecg = TransferFunctions.convertEcg((double)f.analog[(int)SENSORS.ECG], nBitsChannel);
                double eda = TransferFunctions.convertEda((double)f.analog[(int)SENSORS.EDA], nBitsChannel);
                // Console.WriteLine("PZT: " + pzt + " [%]");
                // Console.WriteLine("ECG: " + ecg + " [mV]");
                // Console.WriteLine("EDA: " + eda + " [uS]");
                sampling_resp.Add(pzt);
                sampling_ecg.Add(ecg);
                sampling_eda.Add(eda);
            }
        }

        public SYSTEM_STATE analyzeSamples()
        {
            SYSTEM_STATE system_state = SYSTEM_STATE.OK;
            SYSTEM_STATE system_state_pzt = Quality.qualityContolPzt(sampling_resp, true);
            SYSTEM_STATE system_state_ecg = Quality.qualityContolEcg(sampling_ecg, true);
            SYSTEM_STATE system_state_eda = Quality.qualityContolEda(sampling_eda, true);
            return cascadeCheck(system_state, system_state_pzt, system_state_ecg, system_state_eda);
        }

        public SYSTEM_STATE analyzeSubSamples(List<double> sampling_resp_sub, List<double> sampling_ecg_sub, List<double> sampling_eda_sub)
        {
            Console.WriteLine();
            Console.WriteLine("[validation thread] window control");
            SYSTEM_STATE system_state = SYSTEM_STATE.OK;
            SYSTEM_STATE system_state_pzt = Quality.qualityContolPzt(sampling_resp_sub, false);
            SYSTEM_STATE system_state_ecg = Quality.qualityContolEcg(sampling_ecg_sub, false);
            SYSTEM_STATE system_state_eda = Quality.qualityContolEda(sampling_eda_sub, false);
            Console.WriteLine("[validation thread] window control finished");
            return cascadeCheck(system_state, system_state_pzt, system_state_ecg, system_state_eda);
        }

        private SYSTEM_STATE cascadeCheck(SYSTEM_STATE system_state, 
                                          SYSTEM_STATE system_state_pzt,
                                          SYSTEM_STATE system_state_ecg, 
                                          SYSTEM_STATE system_state_eda)
        {
            /***
             * Cascade check
             **/
            if (system_state_pzt != SYSTEM_STATE.OK)
            {
                if (system_state_ecg != SYSTEM_STATE.OK)
                {
                    system_state = SYSTEM_STATE.MULTIPLE_SENSORS_NOT_CORRECTLY_WORKING;
                }
                else if (system_state_eda != SYSTEM_STATE.OK)
                {
                    system_state = SYSTEM_STATE.MULTIPLE_SENSORS_NOT_CORRECTLY_WORKING;
                }
                else
                {
                    system_state = SYSTEM_STATE.RESP_SENSOR_NOT_WORKING_CORRECTLY;
                }
            }
            else if (system_state_ecg != SYSTEM_STATE.OK)
            {
                if (system_state_eda != SYSTEM_STATE.OK)
                {
                    system_state = SYSTEM_STATE.MULTIPLE_SENSORS_NOT_CORRECTLY_WORKING;
                }
                else
                {
                    system_state = SYSTEM_STATE.ECG_SENSOR_NOT_WORKING_CORRECTLY;
                }
            }
            else if (system_state_eda != SYSTEM_STATE.OK)
            {
                system_state = SYSTEM_STATE.EDA_SENSOR_NOT_WORKING_CORRECTLY;
            }
            return system_state;
        }

        public void stopDeviceSampling()
        {
            try
            {
                dev.stop(); // stop acquisition
            }
            catch (Bitalino.Exception e)
            {
                Console.WriteLine("[ERROR] BITalino exception: {0}", e.Message);
            }
        }
        
        public void disconnectDevice()
        {
            try
            {
                dev.Dispose(); // disconnect from device
            }
            catch (Bitalino.Exception e)
            {
                Console.WriteLine("[ERROR] BITalino exception: {0}", e.Message);
            }
        }
        public void saveResults()
        {
            // TODO linearization timestamp from chunk to sample
            Console.WriteLine("Exporting Results ...");
            Saver.saveOnFile("sampling_pzt_" + start_sampling_timestamp + "_1000.csv", sampling_resp);
            Saver.saveOnFile("sampling_ecg_" + start_sampling_timestamp + "_1000.csv", sampling_ecg);
            Saver.saveOnFile("sampling_eda_" + start_sampling_timestamp + "_1000.csv", sampling_eda);
            // Saver.saveOnFileLong("sampling_timestamp_chunk_" + start_sampling_timestamp + "_1000.csv", sampling_timestamp_chunk);
            Saver.saveOnFileString("sampling_log_actions_" + start_sampling_timestamp + "_1000.csv", sampling_log_actions);
        }

        private long getTimestampMilliseconds()
        {
            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            return milliseconds;
        }
    }
}
