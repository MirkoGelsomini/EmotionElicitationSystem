using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitalinoCore.Utils.Sensor
{
    class TransferFunctions
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
            double acc = (value - cmin)/(cmax - cmin);
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
