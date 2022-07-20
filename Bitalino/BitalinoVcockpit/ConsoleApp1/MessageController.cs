using SuperSocket.ClientEngine;
using System;
using System.IO;
using System.Text;
using System.Timers;
using WebSocket4Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MessageClient
{
    public class MessageController
    {

        WebSocket _websocket;
        string _ModuleName;
        bool _unique;
        dynamic _message_actions;
        dynamic _config;
        dynamic _authorized;

        System.Timers.Timer _timer;
        int _timer_milliseconds;

        public event Action<string, string> OnMessage;
        public event Action<bool> OnAuthorized;

        public MessageController()
        {
            Console.WriteLine("- " + this.GetType().Name + " started.");

            _authorized = false;

            getConfiguration();

            _timer_milliseconds = (int)_config["reconnect"] | 3000;
        }

        public void Setup(string ModuleName, bool unique)
        {
            _ModuleName = ModuleName;
            _unique = unique;

            string protocol = "ws";
            if ((bool)_config["secure"]) protocol = "wss";

            //no remoteAddress connection for now (all modules work locally)
            _websocket = new WebSocket(protocol + "://" + _config["host"] + ":" + _config["port"] + "/" + _config["context"]);

            _websocket.Opened += Websocket_Opened;
            _websocket.Error += Websocket_Error;
            _websocket.Closed += Websocket_Closed;
            _websocket.MessageReceived += Websocket_MessageReceived;
        }

        public void Connect()
        {
            _websocket.Open();
        }

        public bool Publish(string topic, dynamic content)
        {
            return _send("PUBLISH", topic, content);
        }

        public void SubscribeTo(String topic)
        {
            this._send("SUBSCRIBE", topic, "");
        }

        public void UnsubscribeFrom(String topic)
        {
            this._send("UNSUBSCRIBE", topic, "");
        }

        private bool _send(string action, string topic, dynamic content)
        {
            MessageType msg = new MessageType();
            msg.action = Convert.ToString(action);
            msg.topic = Convert.ToString(topic);
            msg.content = content;

            string result = msg.isValidMessage();
            if (result != null)
            {
                if (_websocket.State == WebSocketState.Open)
                {
                    _websocket.Send(result);
                    return true;
                }
            }

            return false;
        }

        private void Websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {

            dynamic mex = JObject.Parse(e.Message.ToString());

            if (new MessageType().isValidMessage(mex) != null)
            {
                if (mex.action.Value == "AUTHORIZE")
                {
                    if (mex.content.Value == "true")
                    {
                        _authorized = true;
                        Console.WriteLine("Module " + _ModuleName + " authorized");
                        if (OnAuthorized != null) OnAuthorized(true);
                    }
                }
                else
                {
                    if (OnMessage != null)
                    {
                        var cnt = "";
                        try
                        {
                            cnt = JsonConvert.SerializeObject(mex.content.Value, Formatting.None);
                            if (cnt == "null")
                            {
                                throw new Exception();
                            }
                        }
                        catch (Exception)
                        {
                            cnt = JsonConvert.SerializeObject(mex.content, Formatting.None);
                        }
                        OnMessage(mex.topic.Value, cnt);

                    }
                }
            }
        }

        private void Websocket_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Closed connection", "Alert");
            Reconnect();
        }

        private void Websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine("Warn: " + e.Exception.Message);
        }

        private void Websocket_Opened(object sender, EventArgs e)
        {

            Console.WriteLine("Module " + _ModuleName + " connected with MessageBroker");

            Console.WriteLine("- Requesting Authorization...");
            this.Authorize();
        }

        private void Reconnect()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }
            // Set up the timer for _timer_milliseconds milliseconds.
            _timer = new System.Timers.Timer(_timer_milliseconds);
            // To add the elapsed event handler:            
            _timer.Elapsed += _timer_Elapsed;

            _timer.Enabled = true;
            Console.WriteLine("- Reconnecting to MessageBroker...");
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Connect();
            _timer.Enabled = false;
        }

        private void Authorize()
        {
            MessageType msg = new MessageType();
            msg.action = "AUTHORIZE";
            msg.topic = "";
            msg.content = "";

            dynamic content = new JObject();
            content["name"] = _ModuleName;
            content["token"] = "vcockpit";
            content["unique"] = _unique;

            msg.content = content;

            _websocket.Send(JsonConvert.SerializeObject(msg, Formatting.None));
        }

        private void getConfiguration()
        {
            string path = @"./settings.json"; ;

            try {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    _config = JsonConvert.DeserializeObject(json);
                    Console.WriteLine("- Configuration loaded.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error:" + e.Message);
                //Environment.Exit(0);
            }
        }


    }


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
        }
    }
}
