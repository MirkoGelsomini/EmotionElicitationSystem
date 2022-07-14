using Newtonsoft.Json;
using System;

namespace Message
{
    public class MessageType
    {
        //public string id { get; set; }
        public string action { get; set; }
        public string topic { get; set; }
        public dynamic content { get; set; }

        public string isValidMessage()
        {
            string result = null;
            try
            {
                result = JsonConvert.SerializeObject(this, Formatting.None);
            }
            catch (Exception)
            {
                return result;
            }

            return result;

            //old checker
            /*if (content.GetType() != null && (content.GetType() == typeof(int) || content.GetType() == typeof(bool) || content.GetType() == typeof(double) || content.GetType()=="JObject"))
            {
                return true;
            }

            try
            {
                JContainer.Parse(content);
            }
            catch (Exception E)
            {
                if(content.GetType() != null && content.GetType() == typeof(String))
                {
                    return true;
                }
                return false;
            }
                                
            //}

            return true;*/
        }

        public string isValidMessage(dynamic msg)
        {
            string result = null;
            if (!msg.ContainsKey("action"))
            {
                return result;
            }
            if (!msg.ContainsKey("topic"))
            {
                return result;
            }
            if (!msg.ContainsKey("content"))
            {
                return result;
            }

            try
            {
                result = JsonConvert.SerializeObject(this, Formatting.None);
            }
            catch (Exception)
            {
                return result;
            }

            return result;

            //old checker
            /*if (msg.content.GetType() == typeof(int) || msg.content.GetType() == typeof(bool) || msg.content.GetType() == typeof(double))
            {
                return true;
            }

            if(msg.content.GetType() == typeof(JValue))
            {
                return true;
            }

            try
            {
                JContainer.Parse(msg.content);
            }
            catch (Exception E)
            {
                return false;
            }

            return true;*/
        }
    }
}
