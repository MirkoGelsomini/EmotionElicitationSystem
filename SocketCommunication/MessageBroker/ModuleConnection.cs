using Fleck;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MessageBroker
{
    class ModuleConnection
    {
        private Guid _id;
        private string _name;        
        private IWebSocketConnection _connection;
        private bool _authorized;
        private DateTime? _authorized_on;
        private DateTime _connected_on;
        private bool _unique;
        private dynamic _manifest;
        private List<string> _subscribed_to;

        public Guid id { get => _id; }
        public DateTime? authorized_on { get => _authorized_on; }
        public DateTime connected_on { get => _connected_on; }

        public bool is_connection_available { get => _connection.IsAvailable; }
        public string name { get => _name; set => _name = value; }
        public bool unique { get => _unique; set => _unique = value; }
        public List<string> subscribed_to { get => _subscribed_to; }

        static LogManager.Logger Logger;

        public ModuleConnection(IWebSocketConnection connection)
        {
            this._id = connection.ConnectionInfo.Id;
            this._name = "";
            this._connection = connection;
            this._authorized = false;
            this._authorized_on = null;
            this._connected_on = DateTime.Now;
            this._unique = true;
            this._manifest = null;
            this._subscribed_to = new List<string>();

            Logger = new LogManager.Logger();
            //Logger.Log("- " + this.GetType().Name + " started.", "Success");
        }

        public string getInfo()
        {
            if (!string.IsNullOrEmpty(this._name))
            {
                return _name + " (" + _id + ")";
            }
            else
            {
                return this._id.ToString();
            }
        }

        public bool Authorize(dynamic message, string key, List<ModuleConnection> authorizedModules)
        {
            if (!message.ContainsKey(key))
            {
                Logger.Log("...Content invalid or not sent.","Alert");
                return false;
            }

            if (message[key].ContainsKey("name") && message[key].name.Value != "")
            {
                this.name = message[key].name.Value;
            }
            else
            {
                Logger.Log("...Name invalid or not sent.", "Alert");
                return false;
            }

            if (message[key].ContainsKey("unique") && message[key].unique.Value == false)
            {
                this.unique = false;
            }

            string token = "";

            if (message[key].ContainsKey("token"))
            {
                token = message[key].token.Value;
            }
            else
            {
                Logger.Log("...Token not sent.", "Alert");
                return false;
            }

            if (!this.isAuthorized())
            {
                if (!checkToken(token, this.name))
                {
                    Logger.Log("...Token not valid.", "Alert");
                    return false;
                }

                List <ModuleConnection> ListOfPossibleCandidatesWithTheSameName = authorizedModules.FindAll(item => item.name == message[key].name.Value);
                

                if (_authorized_on!=null && _authorized_on!= DateTime.Now) //check if the client keeps asking to be authorized, if yes it does not authorize the client anymore
                {
                    Logger.Log("...Too many tentatives to be authorized.", "Alert");
                    return false;
                }
                else if (ListOfPossibleCandidatesWithTheSameName.Exists(item => item.unique == true)) //check if there are other clients with the same name that wants to be unique
                {
                    Logger.Log("...A module already exists with the same name and wants to be unique.", "Alert");
                    return false;
                }
                else
                {
                    /*if (this.is_connection_available)
                    {*/
                        _authorized = true;
                        _authorized_on = DateTime.Now;
                        return true;
                    //}                    
                }
                
            }
            
            this.Deauthorize();
            return false;         

        }

        private bool checkToken(string token, string ModuleName)
        {
            return true;            
        }

        public void Deauthorize()
        {
            _authorized = false;
        }

        public void Disconnect()
        {
            this._connection.Close();
        }

        public bool isAuthorized()
        {
            return _authorized;
        }

        public void Send(dynamic message)
        {
            this._connection.Send(Convert.ToString(message));
        }

        public bool SetManifest(dynamic message, string key)
        {
            //TODO check validity of the applied commands
            if (message.ContainsKey(key) && this.isAuthorized())
            {
                _manifest = message.key;
                return true;
            }
            return false;
        }

        public dynamic manifest()
        {
            return _manifest;
        }

        public bool AddSubscription(dynamic message, string key)
        {
            if (message.ContainsKey(key) && this.isAuthorized())
            {
                string topic = message[key];
                if (!_subscribed_to.Contains(topic))
                {
                    _subscribed_to.Add(topic);
                    return true;
                }                
            }

            return false;

        }

        public bool RemoveSubscription(dynamic message, string key)
        {
            if (message.ContainsKey(key) && this.isAuthorized())
            {
                string topic = message[key];
                if (!_subscribed_to.Contains(topic))
                {
                    _subscribed_to.Remove(topic);
                    return true;
                }
            }

            return false;

        }
    }
}
