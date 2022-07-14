using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace MessageBroker
{
    class Connections
    {
        List<ModuleConnection> _connections;
        Timer _timer;
        int _timer_milliseconds;
        static LogManager.Logger Logger;

        internal List<ModuleConnection> all { get => _connections; }

        public Connections()
        {
            _connections = new List<ModuleConnection>();

            Logger = new LogManager.Logger();
        }

        public void Add(ModuleConnection item)
        {
            _connections.Add(item);
        }

        public ModuleConnection Find(Guid id)
        {
            if (_connections != null) { 
                foreach (ModuleConnection item in _connections)
                {
                    if (item.id == id)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        public ModuleConnection Find(string ModuleName)
        {
            if (_connections != null)
            {
                foreach (ModuleConnection item in _connections)
                {
                    if (item.name == ModuleName)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        public void Remove(Guid id)
        {
            if (_connections != null)
            {
                foreach (ModuleConnection item in _connections)
                {
                    if (item.id == id)
                    {
                        _connections.Remove(item);
                        item.Disconnect();
                    }
                }
            }
        }

        public void Remove(ModuleConnection item)
        {
            if (_connections != null)
            {
                if (_connections.Contains(item)) _connections.Remove(item);
                item.Disconnect();
            }            
        }

        public List<ModuleConnection> GetAuthorizedModules()
        {
            List<ModuleConnection> _authorizedModules = new List<ModuleConnection>();

            if (_connections != null) { 
                foreach (ModuleConnection item in _connections)
                {
                    if (item.isAuthorized()) {
                        _authorizedModules.Add(item);
                    }
                }
            }

            return _authorizedModules;
        }

        public void CheckAuthorizedOrDestroy(int milliseconds)
        {
            if (milliseconds>0)
            {
                _timer_milliseconds = milliseconds;
                // Set up the timer for 3 seconds.
                _timer = new Timer(milliseconds);
                // To add the elapsed event handler:            
                _timer.Elapsed += _timer_Elapsed;

                _timer.Enabled = true;
            }            
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_connections != null)
            {
                foreach (ModuleConnection item in _connections)
                {
                    //ENABLE FOR AUTHORIZATION
                    /*if (item.isAuthorized() == false && (DateTime.Now - item.connected_on).TotalSeconds * 1000 > _timer_milliseconds)
                    {
                        this.Remove(item);
                        Logger.Log("Module " + item.id + " not authorized. Disconnecting...", "Alert");
                    }*/

                    if (item.is_connection_available == false && (DateTime.Now - item.connected_on).TotalSeconds * 1000 > _timer_milliseconds)
                    {
                        this.Remove(item);
                        item.Disconnect();
                        Logger.Log("Module " + item.id + " disconnected without notice. Removing it from the list...", "Alert");
                    }
                }
            }
        }
    }
}
