using System;
using VCockpit.BitalinoLibrary;
using VCockpit.BitalinoLibrary.util;
using System.Threading;

namespace VC.BitalinoLibrary
{
    class VCBitalinoLibraryTest
    {
        static void Main(string[] args)
        {
            VCBitalino bitalino = new VCBitalino();
            // NOTE: it could be more than one
            VCData bitalinoData = new VCData(100, 100); // frames, sampling rate

            try
            {
                // Altough the bitalino is connected and it will samples, this methods doesn't return the connected devices
                // List<string> devices = bitalino.ListBluetoothDevices();

                /***
                 * Create device(s)
                 **/
                Console.WriteLine("Connecting to device...");
                // Device 1
                VCBitalinoConfig bitalinoConnection = VCBitalinoConfig.FromBluetooth("20:19:07:00:80:C2");
                Bitalino dev = bitalino.Instance(bitalinoConnection.ConnectionData);
                // Device 2, ...
                // VCBitalinoConfig bitalinoConnection2 = VCBitalinoConfig.FromBluetooth("20:19:07:00:80:ZZ");
                // Bitalino dev2 = bitalino.Instance(bitalinoConnection.ConnectionData);

                /***
                 * set up device(s)
                 **/
                string ver = dev.version();    // get device version string
                Console.WriteLine("BITalino version: {0}", ver);
                dev.battery(10);  // set battery threshold (optional)

                // NOTE: it could be skipped
                // try a start and stop for early problem detection before real sampling
                bitalino.StartDeviceSampling(dev, bitalinoData.samplingRate);
                bitalino.StopDeviceSampling(dev);

                /***
                 * sampling
                 *      The main program control the execution of the others
                 *          each device 
                 *              - has its own thread for sampling
                 *              - should have its own thread for validation
                 **/
                // one for each device
                VCSampler sampler_dev = VCSamplerInit(bitalino, dev, bitalinoData);
                bitalino.StartDeviceSampling(dev, bitalinoData.samplingRate);

                // start the thread
                sampler_dev.SetStartState();
                bitalinoData.timestamp = VCSampler.GetTimestampMilliseconds();
                sampler_dev.StartSampling();

                Console.WriteLine("[main thread] The thread is starting. It follows the commands to control it:");
                Console.WriteLine("0 - Restart sampling");
                Console.WriteLine("1 - Pause sampling");
                Console.WriteLine("2 - Stop sampling");

                // Do while main control loop
                while (sampler_dev.IsActive())
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
                            long timestampAction = VCSampler.GetTimestampMilliseconds();
                            string log = timestampAction.ToString();
                            switch (choice)
                            {
                                case 0:
                                    Console.WriteLine("[main thread] Re-start samplings ...");
                                    log = log + ", 0";
                                    // sampling_log_actions.Add(log);
                                    sampler_dev.RestartSampling();
                                    break;
                                case 1:
                                    Console.WriteLine("[main thread] Pause samplings ...");
                                    log = log + ", 1";
                                    // sampling_log_actions.Add(log);
                                    sampler_dev.PauseSampling();
                                    break;
                                case 2:
                                    Console.WriteLine("[main thread] Stops samplings ...");
                                    log = log + ", 2";
                                    // sampling_log_actions.Add(log);
                                    sampler_dev.StopSampling();
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
                sampler_dev.FinishSampling();
                bitalino.StopDeviceSampling(dev);
                sampler_dev.SaveResults(); // results saved in bin\x86\Release    
                bitalino.DisposeDevice(dev);
            }
            catch (Bitalino.Exception e)
            {
                Console.WriteLine("BITalino exception: {0}", e.Message);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("BITalino exception: {0}", e.Message);
            }
        }

        private static VCSampler VCSamplerInit(VCBitalino bitalino, Bitalino dev, VCData bitalinoData)
        {
            return new VCSampler(bitalino, dev, bitalinoData);
        }
    }
}
