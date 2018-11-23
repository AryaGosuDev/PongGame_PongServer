using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace ServerPongEmpty
{
    

    class Server
    {
        protected struct PaddleStruct
        {
            public int threadNumber;
            public float location;

            public PaddleStruct(int inThread, float inLoc)
            {
                threadNumber = inThread;
                location = inLoc;
            }
        }

        protected struct HitStruct
        {
            public string velocityX;
            public string velocityY;
            public string posX;
            public string posY;
            public string timeStamp;


            public HitStruct(string inVelX, string inVelY, string inPosX, string inPosY, string inTime)
            {
                velocityX = inVelX;
                velocityY = inVelY;
                posX = inPosX;
                posY = inPosY;
                timeStamp = inTime;
            }
        }

        protected struct StopPointandGoStruct
        {
            public int threadNumber ;
            public int point;
            public bool visited;

            public StopPointandGoStruct(int inThread, int inPoint, bool inVisited)
            {
                threadNumber = inThread;
                point = inPoint;
                visited = inVisited;

            }
        }

        static TcpListener listener;
        const int numberOfThreads = 2;
        protected static string inData;
        protected static int speedX;
        protected static int speedY;
        private Random rnd1;
        protected static int numOfPlayers;
        protected static bool gameStart = false;
        protected static bool hitWall = false;
        //protected static Stack sendStack;
        //protected static Stack stopStack;

        protected static int stopSection;
        protected static int goSection;
        
        protected static float paddlePlayer1Y;
        protected static float paddlePlayer2Y;
        protected static float paddleX;

        
        //protected static Stack paddleStack0;
        //protected static Stack paddleStack1;
        protected static Queue paddleStack0;
        protected static Queue paddleStack1;
        protected static Stack hitStack0;
        protected static Stack hitStack1;
        //protected static List<StopPointandGoStruct> stopAndGoList;
        protected static StopPointandGoStruct[] stopAndGoList;

        protected static double ballLocX;
        protected static double ballLocY;

        protected static char debugLoc ;
        protected static int curreSpeedX;
        protected static int curreSpeedY;

        protected static Stopwatch uniClock;
        protected static DateTime dt;
        protected static TimeSpan ts;




        

        public Server()
        {
          
            numOfPlayers = 0;
            paddlePlayer1Y = 0;
            paddlePlayer2Y = 0;
            ballLocX = 0;
            ballLocY = 21;
            goSection = 0;
            stopSection = 2;


            //stopAndGoList = new List<StopPointandGoStruct>();
            stopAndGoList = new StopPointandGoStruct[4];
            //sendStack = new Stack();
            //stopStack = new Stack();
            //paddleStack1 = new Stack();
            //paddleStack0 = new Stack();
            paddleStack1 = new Queue();
            paddleStack0 = new Queue();
            hitStack0 = new Stack();
            hitStack1 = new Stack();

            uniClock = new Stopwatch();
            dt = new DateTime();
            ts = new TimeSpan();
            


            ServerFunction();
        }

        public void ServerFunction()
        {
            rnd1 = new Random();
            //speedX = rnd1.Next(-20, 20);
            //speedY = rnd1.Next(-20, 20);

            speedX = 10;
            speedY = 3;
            curreSpeedX = speedX;
            curreSpeedY = speedY;
            //stopAndGoList.Add(new StopPointandGoStruct(0, 0, false));
            //stopAndGoList.Add(new StopPointandGoStruct(1, 0, false));
            //stopAndGoList.Add(new StopPointandGoStruct(0, 0, true));
            //stopAndGoList.Add(new StopPointandGoStruct(1, 0, true));
            stopAndGoList[goSection] = new StopPointandGoStruct(0, 0, false);
            stopAndGoList[goSection+1] = new StopPointandGoStruct(0, 0, false);
            stopAndGoList[stopSection] = new StopPointandGoStruct(0, 0, true);
            stopAndGoList[stopSection+1] = new StopPointandGoStruct(0, 0, true);

            try
            {
                dt = getNTPTime();
               
                listener = new TcpListener(2345);

                listener.Start();
               

                for (int i = 0; i < numberOfThreads; i++)
                {
                    ThreadSock tSock = new ThreadSock(i);
                    //Thread t = new Thread(new ThreadStart(Service));
                    Thread t = new Thread(new ThreadStart(tSock.Service));
                    t.Name = "thread" + i;
                    t.Start();
                    Console.WriteLine("Thread Started");
                }
               
                ThreadFixedUpdate tFxedUp = new ThreadFixedUpdate();
                Thread tTime = new Thread(new ThreadStart(tFxedUp.Service));
                tTime.Start();
               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " Debug Loc : " + debugLoc);
            }
        }

        private DateTime getNTPTime()
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //IPAddress serverAddr = IPAddress.Parse("time.windows.com");

            IPAddress serverAddr = IPAddress.Parse("64.147.116.229"); 

            IPEndPoint endPoint = new IPEndPoint(serverAddr, 123);

            byte[] ntpData = new byte[48];

            ntpData[0] = 0x1B;
            ulong currSeconds = (ulong)((DateTime.UtcNow - new DateTime(1900, 1, 1)).TotalSeconds); 
            ulong tempSeconds;
            ntpData[40] = (byte)(currSeconds >> 24);
            tempSeconds = currSeconds << 40;
            ntpData[41] = (byte)(tempSeconds >> 56);
            tempSeconds = currSeconds << 48;
            ntpData[42] = (byte)(tempSeconds >> 56);
            tempSeconds = currSeconds << 56;
            ntpData[43] = (byte)(tempSeconds >> 56);

            UInt32 transTime1 = (UInt32)(ntpData[40] << 24) | (UInt32)(ntpData[41] << 16) | (UInt32)(ntpData[42] << 8) | (UInt32)(ntpData[43]);

            Console.WriteLine("Current Seconds " + currSeconds);
            Console.WriteLine("Trans Time Sending " + transTime1);



            sock.Connect(endPoint);
            sock.Send(ntpData);
            sock.Receive(ntpData);
            sock.Close();
            /*
            Console.WriteLine("LI " + (ntpData[0] >> 6));
            int temp = ntpData[0] << 2;
            temp = temp >> 5;
            Console.WriteLine("VN " + temp);
            temp = (byte)(ntpData[0] << 5);
            temp = temp >> 5;
            Console.WriteLine("Mode " + temp);
            Console.WriteLine("Stratum " + ntpData[1]);
            Console.WriteLine("Poll Interval " + ntpData[2]);
            Console.WriteLine("Precision " + ntpData[3]);

           

            UInt32 refTime = (UInt32)(ntpData[16] << 24) | (UInt32)(ntpData[17] << 16) | (UInt32)(ntpData[18] << 8) | (UInt32)(ntpData[19]);
            UInt32 origTime = (UInt32)(ntpData[24] << 24) | (UInt32)(ntpData[25] << 16) | (UInt32)(ntpData[26] << 8) | (UInt32)(ntpData[27]);
            UInt32 recTime = (UInt32)(ntpData[32] << 24) | (UInt32)(ntpData[33] << 16) | (UInt32)(ntpData[34] << 8) | (UInt32)(ntpData[35]);
            */
            UInt32 transTime = (UInt32)(ntpData[40] << 24) | (UInt32)(ntpData[41] << 16) | (UInt32)(ntpData[42] << 8) | (UInt32)(ntpData[43]);
            UInt32 transTimeMicro = (UInt32)(ntpData[44] << 24) | (UInt32)(ntpData[45] << 16) | (UInt32)(ntpData[46] << 8) | (UInt32)(ntpData[47]);
            Int32 milliseconds = (Int32)(((Double)transTimeMicro / UInt32.MaxValue) * 1000);




            //Console.WriteLine(refTime);
            //Console.WriteLine(origTime);
            //Console.WriteLine(recTime);
            //Console.WriteLine(transTime);

            //DateTime start = new DateTime(1900, 1, 1);
            //DateTime dt = start.AddSeconds(refTime);
            DateTime BaseDate = new DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime dt = BaseDate.AddSeconds(transTime).AddMilliseconds(milliseconds);
            
            uniClock.Start();
            //Console.WriteLine(dt.ToString());
            //Console.WriteLine(dt.ToUniversalTime().ToString());

            return dt;

            





            //temp = 0;




        }

        class ThreadSock
        {
            private int threadNumber;
            private Object thisLock = new Object();
            private StopPointandGoStruct temp;
            
            public ThreadSock(int number)
            {
                threadNumber = number;

            }
            public void Service()
            {
                while (true)
                {

                    
                    
                    Socket soc = listener.AcceptSocket();

                    Console.WriteLine("Connected: {0}",
                                             soc.RemoteEndPoint);
                    
                    try
                    {
                        //Stream s = new NetworkStream(soc);
                        NetworkStream nws = new NetworkStream(soc);
                        //StreamReader sr = new StreamReader(s);
                        //StreamWriter sw = new StreamWriter(s);
                        StreamReader sr = new StreamReader(nws);
                        StreamWriter sw = new StreamWriter(nws);
                        sw.AutoFlush = true; // enable automatic flushing
                        //stopWatch.Start();
                        
                        

                        

                        
                        while (true)
                        {
                            //dt = dt.Add(new TimeSpan(0, 0, 0, uniClock.Elapsed.Seconds, uniClock.Elapsed.Milliseconds));
                            //Console.WriteLine(dt.Add(new TimeSpan(0, 0, 0, uniClock.Elapsed.Seconds, uniClock.Elapsed.Milliseconds)).Second);
                            //if (sr.Peek() != -1)
                            debugLoc = '/';
                            if (nws.DataAvailable)
                            {
                                inData = sr.ReadLine();
                                //inData = sr.re.ReadLine();


                                //Console.WriteLine(inData);

                                debugLoc = '4';
                                if (inData.StartsWith("hit"))
                                {
                                    //sr.DiscardBufferedData();
                                    char[] stringSeparators = { '\\' };
                                    string[] hitStr = inData.Split(stringSeparators);

                                   // Console.WriteLine(hitStr.Length);
                                    //Console.WriteLine(inData);


                                    if (hitStr.Length ==  7)
                                    {
                                        //Console.WriteLine("in");
                                        if (hitStr[1] == "1")
                                        {
                                            hitStack0.Push(new HitStruct(hitStr[2], hitStr[3], hitStr[4], hitStr[5], hitStr[6] ));
                                        }
                                        else
                                        {
                                            hitStack1.Push(new HitStruct(hitStr[2], hitStr[3], hitStr[4], hitStr[5], hitStr[6]));
                                        }
                                    }

                                   
                                }
                                else if (inData.StartsWith("paddle"))
                                {
                                    //sr.DiscardBufferedData();

                                    char[] stringSeparators = { '\\' };
                                    string[] paddleStr = inData.Split(stringSeparators);

                                    
                                    if (paddleStr[1] == "1")
                                    {
                                        
                                        ///paddleStack0.Push(new PaddleStruct(0, (float.Parse(paddleStr[2]))));
                                        //paddleStack0.Enqueue(new PaddleStruct(0, (float.Parse(paddleStr[2]))));
                                        paddleStack0.Enqueue(inData);
                                        paddlePlayer2Y = float.Parse ( paddleStr[2] );
                                        
                                    }
                                    else
                                    {
                                        
                                        //paddleStack1.Push(new PaddleStruct(1, (float.Parse(paddleStr[2]))));
                                        //paddleStack1.Enqueue(new PaddleStruct(1, (float.Parse(paddleStr[2]))));
                                        paddleStack1.Enqueue(inData);
                                        paddlePlayer1Y = float.Parse ( paddleStr[2] );
                                        
                                    }
                                    
                                    //paddleStack0.Enqueue(inData);
                                    //paddleStack1.Enqueue(inData);

                                    
                                    //paddleY = (float.Parse(paddleStr[2]));
                                   
                                }
                                else if (inData.StartsWith("lat"))
                                {
                                    //char[] stringSeparators = { '\\' };
                                    //string[] latString = inData.Split(stringSeparators);
                                    sw.WriteLine("lat\\" + (dt.Add(new TimeSpan(0, 0, 0, uniClock.Elapsed.Seconds, uniClock.Elapsed.Milliseconds))).Ticks);



                                }
                                else if (inData.StartsWith("point"))
                                {
                                    gameStart = false;
                                    sr.DiscardBufferedData();

                                    //numOfPlayers = 2;

                                    //stopStack.Push(0);
                                    //stopStack.Push(1);
                                    //((StopPointandGoStruct)stopAndGoList[stopSection+0])
                                    //(stopAndGoList[stopSection+0])


                                }
                                else if (inData == "client")
                                {
                                    Console.WriteLine(inData + threadNumber);
                                    sr.DiscardBufferedData();

                                    sw.WriteLine(threadNumber);

                                }
                                else if (inData == "start game")
                                {
                                    Console.WriteLine(inData + threadNumber);
                                    sr.DiscardBufferedData();
                                    sw.Flush();
                                    numOfPlayers++;
                                    sw.WriteLine("x\\" + speedX + "\\y\\" + speedY);
                                    if (numOfPlayers == 2)
                                    {
                                        //sendStack.Push(0);
                                        //sendStack.Push(1);
                                        stopAndGoList[goSection].visited = false;
                                        stopAndGoList[goSection + 1].visited = false;


                                    }
                                    else if (numOfPlayers > 2)
                                    {
                                        numOfPlayers = 0;
                                    }


                                }
                            }

                           if (threadNumber == 1)
                           {
                                int dfgdfgfd = 1;
                           }
                           if (threadNumber == 0)
                           {
                                int dfgdfgfd = 1;
                           }


                           debugLoc = 'a';
                           
                            if (threadNumber == 0)
                            {
                                debugLoc = 'f';
                                if (paddleStack0.Count > 0)
                                {
                                    debugLoc = 'g';
                                    sw.WriteLine(paddleStack0.Peek());
                                    paddleStack0.Dequeue();
                                }
                                else if (hitStack0.Count > 0)
                                {
                                    debugLoc = 'h';

                                    HitStruct temp = (HitStruct)hitStack0.Pop();

                                    sw.WriteLine("Hit\\" + temp.velocityX + "\\" + temp.velocityY + "\\" + temp.posX + "\\" + temp.posY + "\\" + temp.timeStamp);

                                }
                                else if (!gameStart && numOfPlayers == 2)
                                {
                                    
                                    if (stopAndGoList[goSection].visited == false)
                                    {
                                        
                                        sw.WriteLine("go");
                                        stopAndGoList[goSection].visited = true;
                                    }
                                    else
                                    {
                                        
                                        if (stopAndGoList[goSection+1].visited == true)
                                        {
                                            gameStart = !gameStart;
                                            numOfPlayers = 0;
                                        }
                                    }
                                }
                                else if (!gameStart && numOfPlayers == 0) 
                                {
                                    
                                    if (stopAndGoList[stopSection].visited == false)
                                    {
                                        sw.WriteLine("stop\\" + stopAndGoList[stopSection].point);
                                        stopAndGoList[stopSection].visited = true;
                                        sw.Flush();

                                    }
                                    debugLoc = 'i';
                                }
                            }
                            else
                            {
                                debugLoc = 'j';
                                if (paddleStack1.Count > 0)
                                {
                                    debugLoc = 'k';
                                    sw.WriteLine(paddleStack1.Peek());

                                    debugLoc = 'l';
                                    paddleStack1.Dequeue();
                                    debugLoc = 'm';
                                }
                                else if (hitStack1.Count > 0)
                                {
                                    debugLoc = 'n';
                                    HitStruct temp = (HitStruct)hitStack1.Pop();
                                    debugLoc = 'o';
                                    sw.WriteLine("Hit\\" + temp.velocityX + "\\" + temp.velocityY + "\\" + temp.posX + "\\" + temp.posY + "\\" + temp.timeStamp);
                                    debugLoc = 'p';
                                }
                                else if (!gameStart && numOfPlayers == 2)
                                {
                                    
                                    if (stopAndGoList[goSection + 1].visited == false)
                                    {
                                        sw.WriteLine("go");
                                        stopAndGoList[goSection + 1].visited = true;
                                    }
                                }
                                else if (!gameStart && numOfPlayers == 0) 
                                {
                                    
                                    if (stopAndGoList[stopSection + 1].visited == false)
                                    {
                                        sw.WriteLine("stop\\" + stopAndGoList[stopSection + 1].point);
                                        stopAndGoList[stopSection + 1].visited = true;

                                    }
                                    debugLoc = 'i';
                                }
                            }
                           
                            debugLoc = 'q';

                            
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message + " loc : " + debugLoc + " thread : " + threadNumber );
                    }

                    Console.WriteLine("Disconnected: {0}",
                                            soc.RemoteEndPoint);

                    soc.Close();
                }
            }
        }

        class ThreadFixedUpdate
        {
            private Stopwatch stopWatch ;
            StopPointandGoStruct temp;

            public ThreadFixedUpdate()
            {
                stopWatch = new Stopwatch();
            }

            public void Service()
            {
                try
                {
                    while (true)
                    {
                        if (gameStart)
                        {
                            stopWatch.Start();
                        }

                        if (stopWatch.Elapsed.Milliseconds >= 20)
                        {
                            stopWatch.Restart();
                            if (ballLocX >= 23.0)
                            {
                                if ((ballLocY <= (paddlePlayer2Y + 3) && ballLocY >= (paddlePlayer2Y - 3)))
                                {
                                    Console.WriteLine("Hit 1");
                                    Console.WriteLine("X : " + ballLocX + " Y : " + ballLocY + " paddle position 2 : " + paddlePlayer2Y + " paddle position 1 : " + paddlePlayer1Y);
                                    curreSpeedX = curreSpeedX * -1;
                                    ballLocX = 22.5;
                                }
                                else
                                {
                                    Console.WriteLine("Miss 1");
                                    Console.WriteLine("X : " + ballLocX + " Y : " + ballLocY + " paddle position 2 : " + paddlePlayer2Y + " paddle position 1 : " + paddlePlayer1Y);
                                    
                                    stopWatch.Stop();
                                    //stopStack.Push(new PointStruct(0, 0));
                                    //stopStack.Push(new PointStruct(1, 0));
                                    
                                    stopAndGoList[stopSection].visited = false;
                                    stopAndGoList[stopSection].point = 0;
                                    
                                    stopAndGoList[stopSection + 1].visited = false;
                                    stopAndGoList[stopSection + 1].point = 0;
                                    gameStart = !gameStart;
                                    ballLocX = 0.0;
                                    ballLocY = 21.0;
                                    curreSpeedX = speedX;
                                    curreSpeedY = speedY;
                                }
                            }
                            else if (ballLocX <= -23.0)
                            {
                                if ((ballLocY <= (paddlePlayer1Y + 3) && ballLocY >= (paddlePlayer1Y - 3)))
                                {
                                    Console.WriteLine("Hit 0");
                                    Console.WriteLine("X : " + ballLocX + " Y : " + ballLocY + " paddle position 2 : " + paddlePlayer2Y + " paddle position 1 : " + paddlePlayer1Y);
                                    curreSpeedX = curreSpeedX * -1;
                                    ballLocX = -22.5;

                                }
                                else
                                {
                                    Console.WriteLine("Miss 0");
                                    Console.WriteLine("X : " + ballLocX + " Y : " + ballLocY + " paddle position 2 : " + paddlePlayer2Y + " paddle position 1 : " + paddlePlayer1Y);
                                    
                                    stopWatch.Stop();
                                   
                                    stopAndGoList[stopSection].visited = false;
                                    stopAndGoList[stopSection].point = 1;
                                    
                                    stopAndGoList[stopSection + 1].visited = false;
                                    stopAndGoList[stopSection + 1].point = 1;
                                    gameStart = !gameStart;
                                    ballLocX = 0.0;
                                    ballLocY = 21.0;
                                    curreSpeedX = speedX;
                                    curreSpeedY = speedY;
                                }
                            }

                            if (ballLocY >= 44 || ballLocY <= 2)
                            {
                                curreSpeedY = curreSpeedY * -1;
                            }

                            ballLocX += ((float)curreSpeedX * 0.02);
                            ballLocY += ((float)curreSpeedY * 0.02);
                        }

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + " Timer loop ");
                }

            }

        }

    }
}
