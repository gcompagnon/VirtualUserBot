using System;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace gc.bot
{
    /// <summary>
    /// keytool -genkey -keyalg RSA -alias selfsigned -keystore c:\Data\keystore.jks -storepass XXXX -validity 360 -keysize  2048
    /// keytool -importkeystore -srckeystore c:\Data\keystore.jks -destkeystore c:\Data\keystore.jks -deststoretype pkcs12
    /// keytool -export -alias selfsigned -storepass masterJ2EE -file c:\Data\server.cer -keystore c:\data\keystore.jks
    /// </summary>
    class Program
    {
        #region Win32
        [DllImport("user32.dll")]
        internal static extern void LockWorkStation();

        #endregion
        /**
         * Arguments 
         * 0: min value for the random duration between 2 mouse moves / default  1 min ( 60 )
         * 1: max value for the random duration between 2 mouse moves / default  6 min (360)
         * 2: begin hour / default 08:10 am
         * 3: end hour / default   06.15 pm
         */
        static void Main(string[] args)
        {
#if DEBUG
            TextWriterTraceListener myWriter = new TextWriterTraceListener(System.Console.Out);
            Debug.Listeners.Add(myWriter);
#endif
            Dictionary<String, Object> arguments = new Dictionary<String, Object>(4);
            Debug.WriteLine("*Arguments\n * 0: min value for the random duration between 2 mouse moves / default  1 min(60)\n * 1: max value for the random duration between 2 mouse moves / default  6 min (360)\n * 2: begin hour / default 08:10 am\n * 3: end hour / default   06.15 pm\n * 3: cursorText text on the mouse cursor on the panel");

            Debug.WriteLine("default arguments: -minDur=60 -maxDur=420 -from=08:00AM -to=06:15PM -cursorText=.");
            Debug.Write("given   arguments:");

            int num;
            DateTime date;
            string text;

            List<String[]> arg = new List<String[]>();
            arg.Add(new String[] { "defaulting", null });

            foreach (string s in args)
            {
                arg.Add(s.Split('='));
            }



            foreach (string[] s in arg)
            {
                if ("defaulting".Equals(s[0]))
                {                   
                    arguments.Add("minDur", 60);
                    arguments.Add("maxDur", 360);
                    arguments.Add("from", DateTime.Parse("08:00AM"));
                    arguments.Add("to", DateTime.Parse("06:15PM"));
                    arguments.Add("cursorText", "");
                    arguments.Add("toFinal", DateTime.Parse("31/12/9999"));
                }
                else if ("-minDur".Equals(s[0]) )
                {
                    if (int.TryParse(s[1], out num))
                    {
                        Debug.Write(" -minDur=" + num);
                        arguments["minDur"] = num;
                    }
                }
                else if ("-maxDur".Equals(s[0]))
                {                    
                    if (int.TryParse(s[1], out num))
                    {
                        Debug.Write(" -maxDur=" + num);
                        arguments["maxDur"] = num;
                    }
                    
                }
                else if ("-from".Equals(s[0]))
                {                  
                    if (DateTime.TryParse(s[1], out date))
                    {
                        Debug.Write(" -from=" + date);
                        arguments["from"] = date;
                    }                                          
                }
                else if ("-to".Equals(s[0]))
                {
                    if (DateTime.TryParse(s[1], out date))
                    {
                        Debug.Write(" -to=" + date);
                        arguments["to"] =  date;
                    }                                            
                }
                else if ("-toFinal".Equals(s[0]))
                {
                    if (DateTime.TryParse(s[1].Replace('_',' '), out date))
                    {
                        Debug.Write(" -toFinal=" + date);
                        arguments["toFinal"] = date;
                    }
                }
                else if ("-cursorText".Equals(s[0]))
                {
                    if (s[1] != null)
                    {
                        text = s[1];
                        arguments["cursorText"] = text;
                    }      
                }
            }
            Debug.WriteLine("");
            // Params testing
            if ( ((DateTime)arguments["to"]).CompareTo((DateTime)arguments["from"])<0 )
            {
                Debug.WriteLine("ERROR  -from > -to");
                return;
            }
            if ((int)arguments["minDur"] > (int)arguments["maxDur"])
            {
                Debug.WriteLine("ERROR  -minDur > -maxDur");
                return;
            }


            Debug.WriteLine("Starting MyBot");
            Debug.WriteLine("End MyBot by pressing 'Q'");
            Random r = new Random();
          
            MouseBot bot = new MouseBot();
#if SCREENLOCK
            //locking
            LockWorkStation();
#else
            bot.drawFullPanels(Color.Black);
#endif

            //bot.drawMouseText((string)arguments["cursorText"], Brushes.LightGoldenrodYellow);       

            double mean = ((int)arguments["maxDur"] - (int)arguments["minDur"]) / 2 + (int)arguments["minDur"];
            double std = 3  * ((int)arguments["maxDur"] - (int)arguments["minDur"])/16;
            double secondsTowait;

            while ( DateTime.Now.CompareTo((DateTime)arguments["toFinal"])<0 )
            {        
                secondsTowait = SampleGaussian(r, mean , stddev: std );
                if ( (DateTime.Now.CompareTo((DateTime)arguments["from"]) > 0) )                  
                {
                    if (DateTime.Now.CompareTo((DateTime)arguments["to"]) < 0)
                    {
                        //ne fonctionne pas avec Skype bot.MoveCursorWPF(500,500);
                       bot.MoveCursorWindowsForms(0, 0);
                    }
                    else // next day
                    {
                        int increment = 1;
                        if (((DateTime)arguments["to"]).DayOfWeek == DayOfWeek.Friday)
                            increment = 3;
                        DateTime fromNewDay = ((DateTime)arguments["from"]).AddDays(increment);                        
                        DateTime toNewDay = ((DateTime)arguments["to"]).AddDays(increment);

                        secondsTowait = (fromNewDay - ((DateTime)arguments["to"])).TotalSeconds;

                        arguments["from"] = fromNewDay;
                        arguments["to"] = toNewDay;                        
                    }
                }                
                Debug.WriteLine("Sleep "+ secondsTowait.ToString()+"s until : "+ DateTime.Now.AddSeconds(secondsTowait).ToString());
                Thread.Sleep((int)secondsTowait * 1000);                
            }
            //locking after the end
            bot.removeFullPanels();
            LockWorkStation();
            // quit

        }

        /// <summary>
        /// Sample double using the Gaussian distribution / Normal Law 
        /// area between mean-2*stddev and mean-3*stddev is 95% of sampling
        /// </summary>
        /// <param name="random"></param>
        /// <param name="mean"></param>
        /// <param name="stddev"></param>
        /// <returns></returns>
        public static double SampleGaussian(Random random, double mean, double stddev)
        {
            // The method requires sampling from a uniform random of (0,1]
            // but Random.NextDouble() returns a sample of [0,1).
            double x1 = 1 - random.NextDouble();
            double x2 = 1 - random.NextDouble();

            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            return y1 * stddev + mean;
        }
    }
}
