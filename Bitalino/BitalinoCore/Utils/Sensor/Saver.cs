using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace BitalinoCore.Utils.Sensor
{
    class Saver
    {
        public static void saveOnFile(string filename, List<double> samples)
        {
            TextWriter tw = new StreamWriter(filename);

            for (int i = 0; i < samples.Count; i++)
            {
                string line = samples[i].ToString();
                tw.WriteLine(line);
            }
            tw.Close();
        }

        public static void saveOnFileString(string filename, List<string> samples)
        {
            TextWriter tw = new StreamWriter(filename);

            for (int i = 0; i < samples.Count; i++)
            {
                string line = samples[i].ToString();
                tw.WriteLine(line);
            }
            tw.Close();
        }

        public static void saveOnFileLong(string filename, List<long> samples)
        {
            TextWriter tw = new StreamWriter(filename);

            for (int i = 0; i < samples.Count; i++)
            {
                string line = samples[i].ToString();
                tw.WriteLine(line);
            }
            tw.Close();
        }

        /*
        public static ArrayList readFromFile(string filename)
        {
            ArrayList samples = new ArrayList();
            string[] lines = File.ReadAllLines(filename);
            foreach (string line in lines)
            {
                double value = Convert.ToDouble(line);
                samples.Add(line);
            }
            return samples;
        }
        */
    }
}
