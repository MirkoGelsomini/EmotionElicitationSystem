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

            if (topic == "Bitalino: StateUpdateRequest"){               
                mc.Publish("Bitalino: StateUpdateAnswer","OK");
            }else if(topic == "Bitalino: StartSampling"){
               Console.WriteLine("Bitalino: StartSampling Start Sampling");
                //mc.Publish("Bitalino: StartSampling", "Start Sampling");
            }else if(topic == "Bitalino: RestartSampling"){
                Console.WriteLine("Bitalino: RestartSampling Restarting Sampling");
                //mc.Publish("Bitalino: RestartSampling", "Restarting Sampling");
            }else if(topic == "Bitalino: StopSampling"){
                Console.WriteLine("Bitalino: StopSampling Stop Sampling");
                //mc.Publish("Bitalino: StopSampling", "Stop Sampling");
            }else if(topic == "Bitalino: FinishSampling"){
                Console.WriteLine("Bitalino: FinishSampling Finishing Sampling");
                //mc.Publish("Bitalino: FinishSampling", "Finishing Sampling");
            }else if(topic == "Bitalino: NewSampling"){
                Console.WriteLine("Bitalino: NewSampling Starting new Sampling");
                //mc.Publish("Bitalino: NewSampling", "Starting new Sampling");
            }else if(topic == "Bitalino: SaveSampling"){
                Console.WriteLine("Saving Sampling "+content);
            }else if(topic == "Bitalino: DeleteSampling"){
                Console.WriteLine("Deleting Sampling");
            }else if(topic == "Bitalino: StateUpdateAnswer"){
            }else{
                Console.WriteLine("Error");
            }      
        }

        private static void OnAuthorized(bool authorized)
        {
            Console.WriteLine("Authorized!!!");

            mc.SubscribeTo("Bitalino: StartSampling");
            mc.SubscribeTo("Bitalino: StopSampling");
            mc.SubscribeTo("Bitalino: RestartSampling");
            mc.SubscribeTo("Bitalino: NewSampling");
            mc.SubscribeTo("Bitalino: FinishSampling");
            mc.SubscribeTo("Bitalino: StateUpdateRequest");
            mc.SubscribeTo("Bitalino: StateUpdateAnswer");
            mc.SubscribeTo("Bitalino: SaveSampling");
            mc.SubscribeTo("Bitalino: DeleteSampling");

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