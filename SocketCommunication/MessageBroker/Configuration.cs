using Newtonsoft.Json;
using System;
using System.IO;

namespace ConfigurationManager
{
    public class Configuration
    {
        private dynamic _settings;
        private string _localFolder;
        private LogManager.Logger Logger;

        public Configuration(string ProjectName = null)
        {
            Logger = new LogManager.Logger();
            string path = System.Reflection.Assembly.GetEntryAssembly().Location.Replace("MessageBroker.dll","");

            _localFolder = path + @"settings.json";

            _settings = null;

            try
            {
                using (StreamReader r = new StreamReader(_localFolder))
                {
                    string json = r.ReadToEnd();                    
                    _settings = JsonConvert.DeserializeObject(json);
                    Logger.Log("- Configuration loaded.", "Success");
                }
            }
            catch (JsonReaderException jre)
            {
                Logger.Log(jre.Message, "Warn");
                //Environment.Exit(0);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, "Warn");
                //Environment.Exit(0);
            }
        }

        public void editConfigurationField(dynamic newsettings)
        {
            //TODO: update _settings with newsettings and not just overwrite _settings
            newsettings = JsonConvert.SerializeObject(newsettings, Formatting.Indented);
            newsettings = "configuration(" + newsettings + ")";
            System.IO.File.WriteAllText(this._localFolder, newsettings);
        }

        public dynamic settings()
        {
            return _settings;
        }

        public dynamic Find(string key, bool killifnull)
        {
            if (_settings != null && _settings.ContainsKey(key))
            {
                return _settings[key];
            }

            if (killifnull)
            {
                Logger.Log(key + " in conf settings does not exist.", "Error");
                Environment.Exit(0);
            }

            return null;
        }

        public dynamic Find(string key)
        {
            if (_settings != null && _settings.ContainsKey(key))
            {
                return _settings[key];
            }

            Logger.Log(key + " in conf settings does not exist.", "Error");
            Environment.Exit(0);
            return null;
        }

        public dynamic Find(dynamic item, string key)
        {
            if (item != null && item.ContainsKey(key))
            {
                var type = item[key].GetType();
                if (type.Name == "JValue")
                {
                    return item[key].Value;
                }
                else //probably jobject
                {
                    return item[key];
                }
            }

            Logger.Log(key + " in " + item + " does not exist.", "Error");
            Environment.Exit(0);
            return null;

        }
    }
}
