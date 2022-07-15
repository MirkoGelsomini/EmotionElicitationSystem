using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Fleck;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebServer;

namespace MessageBroker
{
    class Program
    {       

        IWebSocketServer WebSocket_server;

        Connections connections;

        static LogManager.Logger Logger;

        static ConfigurationManager.Configuration cfmShared;

        static bool debug;

        static string ProjectName;

        static int IntervalCheckAuthorized;

        private static ManualResetEvent mre = new ManualResetEvent(false);

        private string ModulePongEnabled = "ProcessManager";

        /*--------------------------------------------------------------------------------------------*/
        static void Main(string[] args)
        {
            ProjectName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;  
            //bool logger_enabled = cfm.Find("logger", true).Value;
            //Logger = new LogManager.Logger(logger_enabled);
            Logger = new LogManager.Logger();
            Logger.Log("Starting Module " + ProjectName + "...");
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            cfmShared = new ConfigurationManager.Configuration("shared");

            checkDebug();

            if (debug)
            {
                FleckLog.Level = LogLevel.Debug;
            }            

            Program mb = new Program();            

            IntervalCheckAuthorized = (int)cfmShared.Find("checkAuthorized") | 3000;

            bool secure = (bool)cfmShared.Find("secure");
            string host = (string)cfmShared.Find("host");
            string port = (string)cfmShared.Find("port");
            string context = (string)cfmShared.Find("context");

            mb.connections = new Connections();

            mb.InitializeWebSockets(secure, host, port, context);

            Logger.Log("Module " + ProjectName + " ready.");
            Logger.Log("----------------------------------------------------------------------");

            mre.WaitOne();
        }

        /*--------------------------------------------------------------------------------------------*/

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Log("-------------- UnhandledExceptionTrapper: " + e.ExceptionObject.ToString(),"Error");
            
            mre.WaitOne();
        }

        private static void checkDebug()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                debug = true;
            }
            else
            {
                debug = false;
            }
        }

        private static bool isEmptyStringArray(String[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != null)
                {
                    return false;
                }
            }
            return true;
        }

        private void InitializeWebSockets(bool secure, string host, string port, string context)
        {
            string protocol = "ws";
            if (secure) protocol = "wss";

            var prf = new ProcessFinder();
            //prf.KillByPort(Convert.ToInt32(port));

            WebSocket_server = new WebSocketServer(protocol + "://" + host + ":" + port + "/" + context);
            
            //WebSocket_server.ListenerSocket.NoDelay = true; //true = Disable Nagle's Algorithm
            //WebSocket_server.RestartAfterListenError = true; //Auto Restart After Listen Error

            WebSocket_server.Start(socket =>
            {
                socket.OnOpen = () =>
                {                  
                    
                    ModuleConnection mc = new ModuleConnection(socket);
                    connections.Add(mc);

                    Logger.Log("Module " + mc.getInfo() + " connected", "Success");

                };

                socket.OnClose = () =>
                {
                    //socket.ConnectionInfo.Host + socket.ConnectionInfo.Path ");                    

                    ModuleConnection module = connections.Find(socket.ConnectionInfo.Id);
                    if (module != null)
                    {
                        connections.Remove(module);
                        Logger.Log("Module " + module.getInfo() + " disconnected", "Alert");
                    }
                    else
                    {
                        Logger.Log("Module " + socket.ConnectionInfo.Id + " disconnected", "Alert");
                    }
                };

                socket.OnMessage = message =>
                {
                    ModuleConnection module = connections.Find(socket.ConnectionInfo.Id);
                    bool result;
                    dynamic mex = JObject.Parse(message);

                    #if DEBUG
                    Logger.Log("Message received from: "+ socket.ConnectionInfo.Id + " - message: "+ message);
                    #endif

                    if (mex.ContainsKey("action")){

                        string action = mex.action.Value;
                        action = action.ToUpper();

                        if (action == "PUBLISH")
                        {
                            result = Publish(module, mex);
                        }
                        /*------------------------------------------------------------------------------------*/
                        else if (action == "AUTHORIZE") 
                        {       
                            if (module != null)
                            {
                                Logger.Log("Module " + module.getInfo() + " requesting authorization", "Info");

                                if (module.Authorize(mex, "content", connections.GetAuthorizedModules()))
                                {                                
                                    Reply(module, "AUTHORIZE", "true");
                                    Logger.Log("Module " + module.getInfo() + " authorized", "Success");
                                }
                                else
                                {
                                    Reply(module, "AUTHORIZE", "false");
                                    Logger.Log("Module " + module.getInfo() + " not authorized", "Alert");                                    
                                }
                            }
                        }
                        /*------------------------------------------------------------------------------------*/
                        else if (action == "SUBSCRIBE")
                        {
                            result = module.AddSubscription(mex, "topic");
                            if (result) Logger.Log("Module " + module.getInfo() + " has subscribed to " + mex.topic, "Info");
                        }
                        /*------------------------------------------------------------------------------------*/
                        else if (action == "UNSUBSCRIBE")
                        {
                            result = module.RemoveSubscription(mex, "topic");
                            if (result) Logger.Log("Module " + module.getInfo() + " has unsubscribed to " + mex.topic , "Info");
                        }
                        /*------------------------------------------------------------------------------------*/                        
                        else if (action == "STATE?")
                        {
                            string ModuleRequested = mex.content;
                            Logger.Log("Module " + module.getInfo() + " wants to know the STATE of " + ModuleRequested, "Info");
                        }
                        /*------------------------------------------------------------------------------------*/
                        else if (action == "STATE!")
                        {
                            string ModuleState = mex.content;
                            string ModuleSender = mex.topic;
                            Logger.Log("Module " + module.getInfo() + " - " + ModuleSender + " sent its STATE: "+ ModuleState, "Info");
                        }                        
                        /*------------------------------------------------------------------------------------*/
                        else
                        {
                            Logger.Log("Module " + module.getInfo() + " sent an uninterpreted message", "Warn");
                            Logger.Log("Message: " + JsonConvert.SerializeObject(mex, Formatting.None));
                        }
                        /*------------------------------------------------------------------------------------*/


                    }
                };
            });

            connections.CheckAuthorizedOrDestroy(IntervalCheckAuthorized);
            
        }

        private bool Publish(ModuleConnection module, dynamic message)
        {
            if (module!=null && module.isAuthorized())
            {
                if ((message.ContainsKey("topic") && message.topic != "" && message.ContainsKey("content")))
                {
                    //sessionQueue.add(message);

                    foreach (ModuleConnection item in connections.all)
                    {
                        if(item.subscribed_to.Contains(message.topic.Value) && item.is_connection_available && item.isAuthorized())
                        {
                            item.Send(message);
                        }
                    }
                }
                return true;
            }
            return false;                
        }

        private bool Reply(ModuleConnection module, string action, string content)
        {
            return Reply(module, action, "", content);
        }

        private bool Reply(ModuleConnection module, string action, string topic, string content)
        {
            Message.MessageType msg = new Message.MessageType();
            msg.action = action;
            msg.topic = topic;
            msg.content = content;

            module.Send(JsonConvert.SerializeObject(msg, Formatting.None));

            return true;
        }

        private string getUnixTime()
        {
            return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMinutes.ToString();
        }
    }
}