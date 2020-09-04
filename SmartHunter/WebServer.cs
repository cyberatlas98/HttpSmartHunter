using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using SmartHunter.Game.Data;
using System.Runtime.Serialization.Json;
using SmartHunter.Core;
using System.Windows.Media;

namespace SmartHunter.StartServer
{
    class WebServer
    {
        public static HttpListener listener;
        //If you want to see the data on smartphone it is recommended to change this to "http://*:8080/", which would need to be run as admin
        //I might add manifest to force admin in the future
        public static string url = "http://*:8080/";
        public static int requestCount = 0;
        public static string pageData;

        public static bool showPlayerDamage = false;
        public static bool showPartsData = true;

        public static string AllBuffs = "";
        public static string AllPlayerDamage = "Dame dane";

        public static string[] monsterNames = { "No Monster Data", "No Monster Data", "No Monster Data" };
        public static string[] monsterSizes = { "null", "null", "null" };
        public static string[] monsterCurrentHealths = { "null", "null", "null" };
        public static string[] monsterMaxHealths = { "null", "null", "null" };
        public static string[] monsterPartsAndStatuses = { "null", "null", "null" };

        public static List<Player> allPlayers = new List<Player>();
        public static List<ulong> selectedMonsterAddresses = new List<ulong>();

        public static async Task HandleIncomingConnections()
        {
            while (true)
            {
                //Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();
                //HttpListenerContext ctx = listener.GetContext();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;
                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/MonsterDataPack"))
                {
                    //Console.WriteLine("MonsterData1 requested");
                    resp.StatusCode = 200; // HttpStatusCode.OK;
                    resp.ContentType = "text/plain";
                    byte[] data = Encoding.UTF8.GetBytes(JSONify());
                    resp.ContentLength64 = data.Length;
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                }


                //When Getting give the entire html file to it.
                if (req.HttpMethod == "GET")
                {
                    byte[] data = Encoding.UTF8.GetBytes(pageData);
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;

                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                }
            }
        }
        public static void RunServer()
        {
            //Read from PAGE.html as string
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("PAGE.html"));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                pageData = reader.ReadToEnd();
            }

            // Create a Http server and start listening for incoming connections
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(url);
                listener.Start();
            }
            catch
            {
                Log.WriteLine("");
                Log.WriteLine("ERROR STARTING THE HTTP_LISTENER, MAYBE CHECK ADMIN AND PORT?");
                Log.WriteLine("Starting server in localhost:8080");
                listener = new HttpListener();
                listener.Prefixes.Add("http://*:8080/");
                listener.Start();
            }
            //Console.WriteLine("Listening for connections on {0}", url);

            // Handle requests
            //Task listenTask = HandleIncomingConnections();
            //listenTask.GetAwaiter().GetResult();
            HandleIncomingConnections();
        }
        public static void StartServer(bool OnThread)
        {
            if (OnThread)
            {
                Thread thread1 = new Thread(RunServer);
                thread1.IsBackground = true;
                thread1.Start();
            }
            else
            {
                RunServer();
            }
            // Get the IP  
            string IP = Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString();
            Log.WriteLine("");
            Log.WriteLine("=========================");
            Log.WriteLine("== Your IP address is: ==");
            Log.WriteLine("==   " + IP + "   ==");
            Log.WriteLine("=========================");
            Log.WriteLine("");
            LoadCustomizationSettings();
        }
        public static void EndServer()
        {
            // Close the listener
            listener.Close();
        }
        public static void WebUpdatePlayerData(List<PlayerStatusEffect> playerStatusData)
        {
            AllBuffs = "";
            foreach(PlayerStatusEffect pSE in playerStatusData)
            {
                AllBuffs += pSE.Name + ": ";
                if (pSE.Time != null)
                {
                    AllBuffs += Math.Round(pSE.Time.Current) + "/" + Math.Round(pSE.Time.Max);
                }
                AllBuffs += "<br />";
            }
        }
        public static void WebUpdatePlayerDamageData(Player playerData)
        {
            AllPlayerDamage = playerData.Name +": "+ playerData.Damage + " " + Convert.ToInt32(playerData.DamageFraction*100) + "%";
        }
        public static void WebUpdateTeamData(List<Player> playerData)
        {
            if (!showPlayerDamage){ return; }

            int TotalDamage = 0;
            int playerCount = playerData.Count();
            string DamageData = "";

            for (int i = 0; i < playerCount; i++)
            {
                DamageData += playerData.ElementAt(i).ToString() + "<br />";
                TotalDamage += playerData.ElementAt(i).Damage;
            }
            DamageData += "Total: " + TotalDamage;
            AllPlayerDamage = DamageData;
        }
        public static void WebUpdateMonsterData(List<Monster> monsterData)
        {
            int monsterCount = monsterData.Count();

            for (int i = 0; i < monsterCount; i++)
            {
                Monster monster = monsterData.ElementAt(i);
                if(monster.Name == "Kono Dio da!" || monster.Health.Max > 1999999)
                {
                    continue;
                }
                monsterNames[i] = monster.Name;
                monsterSizes[i] = monster.Crown.ToString() + " Crown, Size: " + monster.Size;

                if (monster.Health.Current < 1)
                    monsterCurrentHealths[i] = "0";
                else
                    monsterCurrentHealths[i] = monster.Health.Current.ToString();
                if (monster.Health.Max < 1)
                    monsterMaxHealths[i] = "0";
                else
                    monsterMaxHealths[i] = monster.Health.Max.ToString();


                int partsCount = monster.Parts.Count();
                int effectsCount = monster.StatusEffects.Count();
                monsterPartsAndStatuses[i] = "";

                for (int j = 0; j < partsCount; j++)
                {
                    MonsterPart monsterPart = monster.Parts.ElementAt(j);
                    monsterPartsAndStatuses[i] += monsterPart.Name + ": " + monsterPart.Health.Current + " / " + monsterPart.Health.Max + "<br />";
                }
                monsterPartsAndStatuses[i] += "<br />";
                for (int j = 0; j < effectsCount; j++)
                {
                    MonsterStatusEffect statusEffect = monster.StatusEffects.ElementAt(j);
                    if (0 < statusEffect.Duration.Fraction && statusEffect.Duration.Fraction < 1000 && statusEffect.Buildup.Fraction > 0)
                    {
                        monsterPartsAndStatuses[i]
                        += statusEffect.Name + " Build: "
                        + Math.Round(statusEffect.Buildup.Fraction) * 100 + "% / Dur "
                        + Math.Round(statusEffect.Duration.Fraction) * 100 + "% <br />";
                    }
                }
            }
        }
        public static void LoadCustomizationSettings()
        {
            try
            {
                string[] textSettings = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "HTTPSH_Settings\\Settings.txt"));
                url = textSettings[1];
                showPlayerDamage = Convert.ToBoolean(textSettings[3]);
                showPartsData = Convert.ToBoolean(textSettings[5]);
                if (Convert.ToBoolean(textSettings[7]))
                {
                    LoadIndexPage();
                }
            }
            catch(Exception e)
            {
                Log.WriteException(e);
                Log.WriteLine("ERROR, Something seems to be wrong when loading customization, using deafult settings instead" +
                    "if you want to use customization, check or create the Settings.txt file under HTTPSH_Settings");
            }
            return;
        }
        public static void LoadIndexPage()
        {
            string indexPage;
            try
            {
                indexPage = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "HTTPSH_Settings\\index.html"));
                pageData = indexPage;
            }
            catch(Exception e)
            {
                Log.WriteException(e);
            }
        }
        public static string JSONify()
        {
            string JSONified = "{ "
                + "\"playerBuffs\": "
                + "\"" + AllBuffs + "\","
                + "\"playerDamage\": "
                + "\"" + AllPlayerDamage + "\","
                + "\"monsters\":[";
            for(int i = 0; i < 3; i ++)
            {
                JSONified +=
                 "{\"monsterName\": "
                + "\"" + monsterNames[i] + "\","
                + "\"monsterSize\": "
                + "\"" + monsterSizes[i] + "\","
                + "\"monsterCurrentHealth\": "
                + monsterCurrentHealths[i] + " , "
                + "\"monsterMaxHealth\": "
                + monsterMaxHealths[i] + " , "
                + "\"monsterStatus\": "
                + "\"" + monsterPartsAndStatuses[i] + "\"}";
                if(i!= 2)
                {
                    JSONified += ",";
                }
            }
            JSONified +="]}";
            return JSONified;
        }
    }
}
