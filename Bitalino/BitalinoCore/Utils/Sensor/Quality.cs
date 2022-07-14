using System;
using System.Collections;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace BitalinoCore.Utils.Sensor
{
   
    public class Quality
    {
        /***
         * Quality control sample respiration (pzt) signal
         **/
        public static SYSTEM_STATE qualityContolPzt(List<double> sampling_resp, bool showStatistic)
        {
            // PZT
            double min_resp = sampling_resp.Min();
            double max_resp = sampling_resp.Max();
            double range_resp = max_resp - min_resp;
            double mean_resp = sampling_resp.Average();
            double median_resp = median(sampling_resp);
            double std_resp = stdDev(sampling_resp);

            /***
             * Sensor respiration not plugged to bitalino
             * 
             * PZT: -49.90234375[%]
             * PZT: -50[%]
             * PZT: -50[%]
             * PZT: -50[%]
             * PZT: -50[%]
             * PZT: -50[%]
             * 
             * min: -50
             * max: -49.90234375
             * range: 0.09765625
             * mean: -49.9899088541667
             * std dev: 0.0297309077955863
             * median: -50
             **/
            if ((min_resp < -48) & (max_resp < -48) & (std_resp < 0.1))
            {
                Console.WriteLine("[ERROR] sensor PZT (respiration band) is not connected to bitalino");
                return SYSTEM_STATE.RESP_SENSOR_NOT_WORKING_CORRECTLY;
            }

            /***
             * Sensor respiration plugged to bitalino, but not mouted on the human
             *
             * PZT: -1.85546875 [%]
             * PZT: -1.7578125 [%]
             * PZT: -1.7578125 [%]
             * PZT: -1.7578125 [%]
             * PZT: -1.7578125 [%]
             * PZT: -1.7578125 [%]
             * PZT: -1.7578125 [%]
             * PZT: -1.7578125 [%]
             * PZT: -1.7578125 [%]
             * PZT: -1.7578125 [%]
             * 
             * case 1: flat band
             * min: -1.85546875
             * max: -1.66015625
             * range: 0.1953125
             * mean: -1.76012369791667
             * std dev: 0.0154762595697741
             * median: -1.7578125
             * 
             * case 2: sligthy curved band
             * min: -13.18359375
             * max: 12.40234375
             * range: 25.5859375
             * mean: 8.3735546875
             * std dev: 5.31595106178522
             * median: 10.44921875
             **/
            if (std_resp < 0.5)
            {
                Console.WriteLine("[ERROR] sensor PZT (respiration band) is not sampling corectly. Control the correct set up of the band.");
                return SYSTEM_STATE.RESP_SENSOR_NOT_WORKING_CORRECTLY;
            }

            /***
             * Sensor respiration plugged to bitalino, mouted on the human, 
             *  but it's not sampling correctly due to band position in the chest
             *  
             *  Values
             *      <sulla gabbia toracica, sotto il sotto pettorale, dove l'apertura è maggiore>
             *      min: -28.125
             *      max: 43.65234375
             *      range: 71.77734375
             *      mean: -0.6675
             *      std dev: 18.3715662003886
             *      -> qui la banda è stata posizionata troppo bassa
             *  
             *      <subito sotto il pettorale - come da immagine sul datasheet>
             *      min: -11.1328125
             *      max: 10.546875
             *      range: 21.6796875
             *      mean: 2.9002734375
             *      std dev: 6.24090226474204 
             *      median: 5.56640625
             *      
             *      < in mezzo al pettorale>
             *      min: -4.58984375
             *      max: 14.2578125
             *      range: 18.84765625
             *      mean: 3.99826171875
             *      std dev: 6.58486252231947
             *      median: 3.02734375
             *  
             **/

            if ((range_resp > 50) & (std_resp > 15))
            {
                Console.WriteLine("[ERROR] sensor PZT (respiration band) is not corectly positioned");
                return SYSTEM_STATE.RESP_SENSOR_NOT_WORKING_CORRECTLY;
            }

            /***
             * Sensor respiration plugged to bitalino, mouted on the human, 
             *  but it's sampling strange values out of the expected range
             *  Expected range [-50, +50]
             **/
            if (min_resp < -55.0)
            {
                Console.WriteLine("[ERROR] sensor PZT (respiration band) is sampling out of the expected range of values");
                return SYSTEM_STATE.RESP_SENSOR_NOT_WORKING_CORRECTLY;
            }
            if (max_resp > +55.0)
            {
                Console.WriteLine("[ERROR] sensor PZT (respiration band) is sampling out of the expected range of values");
                return SYSTEM_STATE.RESP_SENSOR_NOT_WORKING_CORRECTLY;
            }

            /***
             * Summary for the human operator
             **/
            if (showStatistic)
            {
                Console.WriteLine("> Respiration sensor (pzt) seems to work properly.");
                Console.WriteLine("min: {0}", min_resp);
                Console.WriteLine("max: {0}", max_resp);
                Console.WriteLine("range: {0}", range_resp);
                Console.WriteLine("mean: {0}", mean_resp);
                Console.WriteLine("std dev: {0}", std_resp);
                Console.WriteLine("median: {0}", median_resp);
            }
            return SYSTEM_STATE.OK;
        }

        public static SYSTEM_STATE qualityContolEcg(List<double> sampling_ecg, bool showStatistic)
        {
            // ECG
            double min_ecg = sampling_ecg.Min();
            double max_ecg = sampling_ecg.Max();
            double range_ecg = max_ecg - min_ecg;
            double mean_ecg = sampling_ecg.Average();
            double median_ecg = median(sampling_ecg);
            double std_ecg = stdDev(sampling_ecg);

            /***
             * Sensor ECG not plugged to bitalino
             * 
             * ECG: -1.5[mV]
             * ECG: -1.5[mV]
             * ECG: -1.5[mV]
             * ECG: -1.5[mV]
             * ECG: -1.4970703125[mV]
             * ECG: -1.5[mV]
             * ECG: -1.5[mV]
             * ECG: -1.5[mV]
             * ECG: -1.5[mV]
             * 
             * min: -1.5
             * max: -1.4970703125
             * range: 0.00292968749999978
             * mean: -1.49966484375
             * std dev: 0.000932602441408165
             * median: -1.5
             **/
            if ((min_ecg < -1.4) & (max_ecg < -1.4) & (std_ecg < 0.1))
            {
                Console.WriteLine("[ERROR] sensor ECG is not connected to bitalino");
                return SYSTEM_STATE.ECG_SENSOR_NOT_WORKING_CORRECTLY;
            }

            /***
             * Sensor ECG plugged to bitalino, but not mouted on the human
             *
             * [NOT PLUGGED]
             * 
             * ECG: -0.71484375[mV]
             * ECG: 1.494140625[mV]
             * ECG: 1.4970703125[mV]
             * ECG: 1.4970703125[mV]
             * ECG: 1.494140625[mV]
             * ECG: 1.494140625[mV]
             * ECG: 1.494140625[mV]
             * ECG: 1.494140625[mV]
             * ECG: 1.4970703125[mV]
             * ECG: 1.4970703125[mV]
             * ECG: 0.392578125[mV]
             * ECG: -1.5[mV]
             * ECG: -1.5[mV]
             * ECG: -1.5[mV]
             * ECG: -1.5[mV]
             * ECG: -1.5[mV]
             * ECG: -1.5[mV]
             * ECG: -1.5[mV]
             * ECG: -1.5[mV]
             * ECG: -1.5[mV]
             * ECG: -0.7001953125[mV]
             * ECG: 1.494140625[mV]
             * ECG: 1.494140625[mV]
             * ECG: 1.4970703125[mV]
             * ECG: 1.4970703125[mV]
             * ECG: 1.494140625[mV]
             * 
             * min: -1.5
             * max: 1.4970703125
             * range: 2.9970703125
             * mean: -0.0174146484375
             * std dev: 1.4337043705376
             * median: -0.1435546875
             * 
             * [PLUGGED]
             * ECG: -0.01171875 [mV]
             * ECG: -0.01171875 [mV]
             * ECG: -0.0234375 [mV]
             * ECG: -0.029296875 [mV]
             * ECG: -0.0234375 [mV]
             * ECG: -0.0205078125 [mV]
             * ECG: -0.0146484375 [mV]
             * ECG: -0.005859375 [mV]
             * ECG: -0.005859375 [mV]
             * ECG: 0.0029296875 [mV]
             * ECG: 0.01171875 [mV]
             * ECG: 0.01171875 [mV]
             * ECG: 0 [mV]
             * ECG: -0.0234375 [mV]
             * ECG: -0.041015625 [mV]
             * ECG: -0.0498046875 [mV]
             * ECG: -0.0439453125 [mV]
             * ECG: -0.0322265625 [mV]
             * ECG: -0.0205078125 [mV]
             * 
             * min: -0.111328125
             * max: 0.1728515625
             * range: 0.2841796875
             * mean: -0.006516796875
             * std dev: 0.03489689752127
             * median: -0.01171875
             **/
            if ((min_ecg < -1.4) & (max_ecg > 1.4) & (range_ecg > 2))
            {
                Console.WriteLine("[ERROR] sensor ECG is not sampling corectly. Control the correct set up of the electodes.");
                return SYSTEM_STATE.ECG_SENSOR_NOT_WORKING_CORRECTLY;
            }

            /***
             * Sensor ECG plugged to bitalino, mouted on the human, 
             *  but it's sampling strange values out of the expected range
             *  Expected range [-1.5, 1.5]
             **/
            if (min_ecg < -1.6)
            {
                Console.WriteLine("[ERROR] sensor EDA is not expected to have values lower than -1.5 [mV]");
                return SYSTEM_STATE.ECG_SENSOR_NOT_WORKING_CORRECTLY;
            }
            if (max_ecg > 1.6)
            {
                Console.WriteLine("[ERROR] sensor EDA is not expected to have values upper than 1.5 [mV]");
                return SYSTEM_STATE.ECG_SENSOR_NOT_WORKING_CORRECTLY;
            }

            /***
             * Summary for the human operator
             **/
            if (showStatistic)
            {
                Console.WriteLine("> ECG sensor seems to work properly.");
                Console.WriteLine("min: {0}", min_ecg);
                Console.WriteLine("max: {0}", max_ecg);
                Console.WriteLine("range: {0}", range_ecg);
                Console.WriteLine("mean: {0}", mean_ecg);
                Console.WriteLine("std dev: {0}", std_ecg);
                Console.WriteLine("median: {0}", median_ecg);
            }
            return SYSTEM_STATE.OK;
        }
        
        public static SYSTEM_STATE qualityContolEda(List<double> sampling_eda, bool showStatistic)
        {
            double min_eda = sampling_eda.Min();
            double max_eda = sampling_eda.Max();
            double range_eda = max_eda - min_eda;
            double mean_eda = sampling_eda.Average();
            double median_eda = median(sampling_eda);
            double std_eda = stdDev(sampling_eda);

            /***
            * Sensor EDA not plugged to bitalino
            * 
            * EDA: 0[uS]
            * EDA: 0[uS]
            * EDA: 0[uS]
            * EDA: 0.0732421875[uS]
            * EDA: 0.0732421875[uS]
            * EDA: 0.0732421875[uS]
            * EDA: 0.0732421875[uS]
            * EDA: 0.0732421875[uS]
            * EDA: 0.0732421875[uS]
            * EDA: 0.048828125[uS]
            * EDA: 0[uS] 
            * EDA: 0[uS]
            * EDA: 0[uS]
            * EDA: 0[uS]
            * 
            * min: 0
            * max: 0.09765625
            * range: 0.09765625
            * mean: 0.0329638671875
            * std dev: 0.035264397625123
            * median: 0
            * 
            * 
            * Sensor EDA plugged to bitalino, but not mouted on the human
            * 
            * EDA: 0 [uS]
            * EDA: 0 [uS]
            * EDA: 0 [uS]
            * EDA: 0 [uS]
            * EDA: 0 [uS]
            * EDA: 0 [uS]
            * EDA: 0 [uS]
            * EDA: 0.0244140625 [uS]
            * EDA: 0.0244140625 [uS]
            * EDA: 0.0244140625 [uS]
            * EDA: 0.0244140625 [uS]
            * EDA: 0.0244140625 [uS]
            * EDA: 0 [uS]
            * EDA: 0 [uS]
            * EDA: 0 [uS]
            * EDA: 0.0244140625 [uS]
            * EDA: 0.0244140625 [uS]
            * EDA: 0.0244140625 [uS]
            * EDA: 0.0244140625 [uS]
            * EDA: 0.0244140625 [uS]
            * EDA: 0 [uS]
            * EDA: 0 [uS]
            * 
            * min: 0
            * max: 0.1220703125
            * range: 0.1220703125
            * mean: 0.0124853515625
            * std dev: 0.0174892752777501
            * median: 0
            * 
            * 
            * Sensor Plugged correctly (the value depend by the hand, but they are far from 0 potential)
            * 
            * EDA: 6.201171875 [uS]
            * EDA: 6.201171875 [uS]
            * EDA: 6.1767578125 [uS]
            * EDA: 6.201171875 [uS]
            * EDA: 6.1767578125 [uS]
            * EDA: 6.201171875 [uS]
            * EDA: 6.201171875 [uS]
            * EDA: 6.2255859375 [uS]
            * EDA: 6.2255859375 [uS]
            * EDA: 6.25 [uS]
            * EDA: 6.2744140625 [uS]
            * EDA: 6.2744140625 [uS]
            * 
            * min: 6.1767578125
            * max: 6.3720703125
            * range: 0.1953125
            * mean: 6.2704296875
            * std dev: 0.0432700534398853
            * median: 6.2744140625
            * 
            **/
            if ((median_eda < 0.1) & (std_eda < 0.2))
            {
                Console.WriteLine("[ERROR] sensor EDA is not connected to bitalino OR not connected to a human. Control the correct set up of the electodes.");
                return SYSTEM_STATE.EDA_SENSOR_NOT_WORKING_CORRECTLY;
            }

            /***
             * Sensor EDA plugged to bitalino, mouted on the human, 
             *  but it's sampling strange values out of the expected range
             *  Expected range [0, 25]
             **/
            if (min_eda < 0.0)
            {
                Console.WriteLine("[ERROR] sensor EDA is not expected to have values lower than 0");
                return SYSTEM_STATE.EDA_SENSOR_NOT_WORKING_CORRECTLY;
            }
            if (min_eda > 26.0)
            {
                Console.WriteLine("[ERROR] sensor EDA is not expected to have values upper than 25");
                return SYSTEM_STATE.EDA_SENSOR_NOT_WORKING_CORRECTLY;
            }

            /***
             * Summary for the human operator
             **/
            if (showStatistic)
            {
                Console.WriteLine("> EDA sensor seems to work properly.");
                Console.WriteLine("min: {0}", min_eda);
                Console.WriteLine("max: {0}", max_eda);
                Console.WriteLine("range: {0}", range_eda);
                Console.WriteLine("mean: {0}", mean_eda);
                Console.WriteLine("std dev: {0}", std_eda);
                Console.WriteLine("median: {0}", median_eda);
            }
            return SYSTEM_STATE.OK;
        }

        public static double median(List<double> numbers)
        {
            if (numbers.Count == 0)
                return 0;
            numbers = numbers.OrderBy(n => n).ToList();
            var halfIndex = numbers.Count() / 2;
            if (numbers.Count() % 2 == 0)
                return (numbers[halfIndex] + numbers[halfIndex - 1]) / 2.0;
            return numbers[halfIndex];
        }

        private static double stdDev(List<double> values)
        {
            if (values.Count == 0)
                return 0;
            var avg = values.Average();
            var sum = values.Sum(d => Math.Pow(d - avg, 2));
            return Math.Sqrt(sum / (values.Count() - 1));
        }

    }
}
