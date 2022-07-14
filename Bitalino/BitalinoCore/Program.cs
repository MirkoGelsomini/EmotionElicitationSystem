using BitalinoCore.Utils.Sensor;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

/***
* 
*      Prima versione
*          - bootstrap
*              controlla che i sensori siano collegati
*              controlla che il bitalino sia stato trovato
*              ritorna un enum con lo stato del sistema
*          - start
*              fa partire il processo di raccolta dati in un thread
*          - stop
*              ferma la raccolta dati
*          - save data
*              salva i dati su disco
*/

namespace BitalinoCore
{
    class Program
    {
        static void Main(string[] args)
        {
            // that number is provided by the PC, but bitalino should be previouly registered to the laptop's bluetooth
            const string DEVICE_MAC_ADDRESS = "20:19:07:00:80:C2";
            Sampler sampler = new Sampler();
            /***
             * Check Bitalino Set up
             *   - is bitalino device found?
             *   - is bitalino device corrected configured?
             *   - does bitalino has some problem in start and stop sampling?
             *   NOTE: if the sensor is not connected, bitalino working anyway
             */
            // return a value which notifies the set up state
            SYSTEM_STATE system_state = sampler.bootstrap(DEVICE_MAC_ADDRESS);
            if (system_state != SYSTEM_STATE.OK){
                Console.WriteLine("[NOTIFICATION] The program ends for an error during the set up.");
            }
            else
            {
                /***
                 * Check Sensors range (detect problem with sensors in a time window)
                 *   - analyze samples range
                 *   - detect anomalies in the sensors values
                 */
                sampler.clearSampling();
                sampler.startDeviceSampling();
                sampler.SamplingInForegroundTestSensor(5); // blocking main thread sampling
                sampler.stopDeviceSampling();
                system_state = sampler.analyzeSamples();
                sampler.clearSampling();
                if (system_state != SYSTEM_STATE.OK)
                {
                    Console.WriteLine("[NOTIFICATION] The program ends for an error during the sensor sampling set up.");
                }
                else
                {
                    /***
                    * Sampling experience
                    */
                    sampler.startDeviceSampling();
                    sampler.sampling(true);
                    sampler.stopDeviceSampling();
                    // TODO parquet
                    sampler.saveResults(); // results saved in bin\x86\Release    
                }
                sampler.disconnectDevice();
            }
        }
    }
}
