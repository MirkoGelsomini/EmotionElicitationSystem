using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MessageClient
{

    class Program
    {
        //START MESSAGE CLIENT VARIABLES
        static string ProjectName;
        static MessageController mc;
        static ManualResetEvent mre = new ManualResetEvent(false);
        //END MESSAGE CLIENT VARIABLES

        #region MODULE

        //write here...
        static System.Timers.Timer aTimer;

        #endregion MODULE

        static void Main(string[] args)
        {
            Init();

            Wait();
        }

        static void Init()
        {
            //init methods
            ProjectName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            Console.WriteLine("Starting Module " + ProjectName + "...");
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            mc = new MessageController();   //send configuration    

            Console.WriteLine("Module " + ProjectName + " ready.");
            Console.WriteLine("----------------------------------------------------------------------");

            //init setup
            mc.Setup(ProjectName, false); //send ModuleName, unique
            mc.OnMessage += OnMessage;
            mc.OnAuthorized += OnAuthorized;
            mc.Connect();

        }

        private static void OnMessage(String topic, String content)
        {
            #if DEBUG
            Console.WriteLine("Message received-> topic: " + topic + ", content: " + content);
            #endif

            if (topic == "TopicName")
            {               

                
            }         

        }

        private static void OnAuthorized(bool authorized)
        {
            Console.WriteLine("Authorized!!!");

            mc.SubscribeTo("TopicName");

            mc.Publish("TopicName", "Content");

        }
        

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            mre.WaitOne();
        }


        static void Wait()
        {            
            mre.WaitOne();
        }
    }
}