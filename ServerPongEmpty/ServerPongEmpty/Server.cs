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
                //dt = NTPTime.getNTPTime();

                dt = getNTPTime();

                //uniClock.Start();
               
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

        public DateTime getNTPTime()
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //IPAddress serverAddr = IPAddress.Parse("time.windows.com");

            IPAddress serverAddr = IPAddress.Parse("64.147.116.229");

            IPEndPoint endPoint = new IPEndPoint(serverAddr, 123);

            byte[] ntpData = new byte[48];

            ntpData[0] = 0x1B;
            ulong currSeconds = (ulong)((DateTime.UtcNow - new DateTime(1900, 1, 1)).TotalSeconds);
            ulong currMillisecs = (ulong)((DateTime.UtcNow - new DateTime(1900, 1, 1)).TotalMilliseconds);
            ulong tempSeconds;
            ntpData[40] = (byte)(currSeconds >> 24);
            tempSeconds = currSeconds << 40;
            ntpData[41] = (byte)(tempSeconds >> 56);
            tempSeconds = currSeconds << 48;
            ntpData[42] = (byte)(tempSeconds >> 56);
            tempSeconds = currSeconds << 56;
            ntpData[43] = (byte)(tempSeconds >> 56);

            currMillisecs = (ulong)(((UInt32)(currMillisecs / 1000)) * UInt32.MaxValue);

            ntpData[44] = (byte)(currMillisecs >> 24);
            tempSeconds = currMillisecs << 40;
            ntpData[45] = (byte)(tempSeconds >> 56);
            tempSeconds = currMillisecs << 48;
            ntpData[46] = (byte)(tempSeconds >> 56);
            tempSeconds = currMillisecs << 56;
            ntpData[47] = (byte)(tempSeconds >> 56);



            //Console.WriteLine("Current Seconds " + currSeconds);
            //Console.WriteLine("Trans Time Sending " + transTime1);

            // round trip delay = (T4 - T1) - (T2 - T3)



            sock.Connect(endPoint);
            DateTime T1 = DateTime.UtcNow;
            sock.Send(ntpData);
            while (sock.Receive(ntpData) < 44) { Console.WriteLine("getting NTP"); }
            DateTime T4 = DateTime.UtcNow;
            //UInt32 destTime = (UInt32)(ntpData[16] << 24) | (UInt32)(ntpData[17] << 16) | (UInt32)(ntpData[18] << 8) | (UInt32)(ntpData[19]);
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
            */
            TimeSpan delay = T4.Subtract(T1);
            UInt32 refTime = (UInt32)(ntpData[16] << 24) | (UInt32)(ntpData[17] << 16) | (UInt32)(ntpData[18] << 8) | (UInt32)(ntpData[19]);
            UInt32 refTimeMicro = (UInt32)(ntpData[20] << 24) | (UInt32)(ntpData[21] << 16) | (UInt32)(ntpData[22] << 8) | (UInt32)(ntpData[23]);
            UInt32 refTimemilliseconds = (UInt32)(((double)refTimeMicro / UInt32.MaxValue) * 1000);

            UInt32 origTime = (UInt32)(ntpData[24] << 24) | (UInt32)(ntpData[25] << 16) | (UInt32)(ntpData[26] << 8) | (UInt32)(ntpData[27]);
            UInt32 origTimeMicro = (UInt32)(ntpData[28] << 24) | (UInt32)(ntpData[29] << 16) | (UInt32)(ntpData[30] << 8) | (UInt32)(ntpData[31]);
            UInt32 origTimemilliseconds = (UInt32)(((double)origTimeMicro / UInt32.MaxValue) * 1000);

            UInt32 recTime = (UInt32)(ntpData[32] << 24) | (UInt32)(ntpData[33] << 16) | (UInt32)(ntpData[34] << 8) | (UInt32)(ntpData[35]);
            UInt32 recTimeMicro = (UInt32)(ntpData[36] << 24) | (UInt32)(ntpData[37] << 16) | (UInt32)(ntpData[38] << 8) | (UInt32)(ntpData[39]);
            UInt32 recTimemilliseconds = (UInt32)(((double)recTimeMicro / UInt32.MaxValue) * 1000);

            UInt32 transTime = (UInt32)(ntpData[40] << 24) | (UInt32)(ntpData[41] << 16) | (UInt32)(ntpData[42] << 8) | (UInt32)(ntpData[43]);
            UInt32 transTimeMicro = (UInt32)(ntpData[44] << 24) | (UInt32)(ntpData[45] << 16) | (UInt32)(ntpData[46] << 8) | (UInt32)(ntpData[47]);
            UInt32 milliseconds = (UInt32)(((double)transTimeMicro / UInt32.MaxValue) * 1000);

            //Console.WriteLine(refTime);
            //Console.WriteLine(origTime);
            //Console.WriteLine(recTime);
            //Console.WriteLine(transTime);


            DateTime BaseDate = new DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime dt = BaseDate.AddSeconds(transTime).AddMilliseconds(milliseconds);

            //DateTime T2 = BaseDate.AddSeconds(recTime).AddMilliseconds(recTimemilliseconds);

            //delay = delay - (T2.Subtract(dt));

            //TimeSpan tsOffset = T2.Subtract(T1);
            //tsOffset = tsOffset.Add(dt.Subtract(T4));



            uniClock.Start();


            return dt;

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

                        if (threadNumber == 1)
                        {
                            ProcessThreads pThread = new ProcessThreads(1, sr, sw);

                            Thread t = new Thread(new ThreadStart(pThread.Service));

                            t.Start();
                        }
                        else if (threadNumber == 0)
                        {
                            ProcessThreads pThread = new ProcessThreads(0, sr, sw);

                            Thread t = new Thread(new ThreadStart(pThread.Service));

                            t.Start();
                        }

                    

                        
                        while (true)
                        {
                            //dt = dt.Add(new TimeSpan(0, 0, 0, uniClock.Elapsed.Seconds, uniClock.Elapsed.Milliseconds));
                            //Console.WriteLine(dt.Add(new TimeSpan(0, 0, 0, uniClock.Elapsed.Seconds, uniClock.Elapsed.Milliseconds)).Second);
                            //if (sr.Peek() != -1)
                            debugLoc = '/';
                            //if (nws.DataAvailable)
                            //if ( (inData = sr.ReadLine()) != null ) 
                            if ((inData = sr.ReadLine())  != null)
                            {
                               
                                //inData = sr.ReadLine();
                                //inData = sr.re.ReadLine();


                                //Console.WriteLine(inData);

                                
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
                                        Console.WriteLine(paddlePlayer2Y);
                                    }
                                    else
                                    {
                                        
                                        //paddleStack1.Push(new PaddleStruct(1, (float.Parse(paddleStr[2]))));
                                        //paddleStack1.Enqueue(new PaddleStruct(1, (float.Parse(paddleStr[2]))));
                                        paddleStack1.Enqueue(inData);
                                        paddlePlayer1Y = float.Parse ( paddleStr[2] );
                                        //Console.WriteLine(paddlePlayer1Y);

                                        
                                    }
                                    
                                    //paddleStack0.Enqueue(inData);
                                    //paddleStack1.Enqueue(inData);

                                    
                                    //paddleY = (float.Parse(paddleStr[2]));
                                   
                                }
                                else if (inData.StartsWith("lat"))
                                {
                                    DateTime recLat = dt.Add(new TimeSpan(0, 0, uniClock.Elapsed.Minutes, uniClock.Elapsed.Seconds, uniClock.Elapsed.Milliseconds));
                                    
                                    //sw.WriteLine("lat\\" + (dt.Add(new TimeSpan(0, 0, 0, uniClock.Elapsed.Seconds, uniClock.Elapsed.Milliseconds))).Ticks);
                                    sw.WriteLine("lat\\" + ((dt.Add(new TimeSpan(0, 0, uniClock.Elapsed.Minutes, uniClock.Elapsed.Seconds, uniClock.Elapsed.Milliseconds))).Subtract (recLat )).Ticks);
                                    



                                }
                                else if (inData == "client")
                                {
                                    Console.WriteLine(inData + threadNumber);
                                    sr.DiscardBufferedData();

                                    sw.WriteLine("client\\" + threadNumber );

                                }
                                else if (inData == "start game")
                                {
                                    Console.WriteLine(inData + threadNumber);
                                    sr.DiscardBufferedData();
                                    sw.Flush();
                                    numOfPlayers++;
                                    sw.WriteLine("start game" + "\\x\\" + speedX + "\\y\\" + speedY);
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

        class ProcessThreads
        {
            private int threadNumber;
            StreamReader sr;
            StreamWriter sw ;

            public ProcessThreads(int number, StreamReader insr, StreamWriter insw )
            {
                threadNumber = number;
                sr = insr;
                sw = insw;
            }

            public void Service()
            {
                while (true)
                {
                    if (threadNumber == 1)
                    {
                        int dfgdfgfd = 1;
                    }
                    if (threadNumber == 0)
                    {
                        int dfgdfgfd = 1;
                    }
                    try
                    {
                        if (threadNumber == 0)
                        {
                            
                            if (paddleStack0.Count > 0)
                            {
                                
                                sw.WriteLine(paddleStack0.Peek());
                                paddleStack0.Dequeue();
                            }
                            else if (hitStack0.Count > 0)
                            {
                                

                                //HitStruct temp = (HitStruct)hitStack0.Pop();
                                

                                //sw.WriteLine("Hit\\" + temp.velocityX + "\\" + temp.velocityY + "\\" + temp.posX + "\\" + temp.posY + "\\" + temp.timeStamp);
                                sw.WriteLine(hitStack0.Pop() + "\\" + ((dt.AddMinutes(uniClock.Elapsed.Minutes).AddSeconds(uniClock.Elapsed.Seconds).AddMilliseconds(uniClock.Elapsed.Milliseconds)).Ticks));
                                

                            }
                            else if (!gameStart && numOfPlayers == 2)
                            {

                                if (stopAndGoList[goSection].visited == false)
                                {
                                    Console.WriteLine("go " + threadNumber);
                                    sw.WriteLine("go");
                                    stopAndGoList[goSection].visited = true;
                                }
                                else
                                {

                                    if (stopAndGoList[goSection + 1].visited == true)
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
                                
                            }
                        }
                        else
                        {
                            
                            if (paddleStack1.Count > 0)
                            {
                               
                                sw.WriteLine(paddleStack1.Peek());

                               
                                paddleStack1.Dequeue();
                               
                            }
                            else if (hitStack1.Count > 0)
                            {
                               
                                //HitStruct temp = (HitStruct)hitStack1.Pop();
                              
                                //sw.WriteLine("Hit\\" + temp.velocityX + "\\" + temp.velocityY + "\\" + temp.posX + "\\" + temp.posY + "\\" + temp.timeStamp);
                                sw.WriteLine(hitStack1.Pop() + "\\" + (dt.AddMinutes(uniClock.Elapsed.Minutes).AddSeconds(uniClock.Elapsed.Seconds).AddMilliseconds(uniClock.Elapsed.Milliseconds)).Ticks);
                             
                            }
                            else if (!gameStart && numOfPlayers == 2)
                            {

                                if (stopAndGoList[goSection + 1].visited == false)
                                {
                                    Console.WriteLine("go " + threadNumber);
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
                               
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message + " Debug Loc " + debugLoc);
                    }
                }

            }
        }

        class ThreadFixedUpdate
        {
            private Stopwatch stopWatch ;
            StopPointandGoStruct temp;
            private int calcTicksRight;
            private int calcTicksLeft;
            private double pastBallLoc;
            private double pastBallLocx;
            private int calcThreshold = 10;
            private System.IO.StreamWriter file;
            


            public ThreadFixedUpdate()
            {
                stopWatch = new Stopwatch();
                calcTicksRight = 0;
                calcTicksLeft = 0;
                pastBallLoc = 0;
                pastBallLocx = 0;
                file = new System.IO.StreamWriter("C:\\Users\\karrat\\Downloads\\test.txt");
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

                            if (ballLocY >= 43.9 || ballLocY <= 2.1)
                            {
                                curreSpeedY = curreSpeedY * -1;
                            }

                            ballLocX += ((float)curreSpeedX * 0.02);
                            ballLocY += ((float)curreSpeedY * 0.02);

                            //file.WriteLine("X : " + ballLocX + " Y : " + ballLocY);

                            if (ballLocX >= 22)
                            {
                                calcTicksRight = 1;
                                pastBallLoc = ballLocY;
                                pastBallLocx = ballLocX;

                                
                                curreSpeedX = curreSpeedX * -1;
                                ballLocX = 21.88;

                                


                            }
                            else if (ballLocX <= -22)
                            {
                                calcTicksLeft = 1;
                                pastBallLoc = ballLocY;
                                pastBallLocx = ballLocX;
                                curreSpeedX = curreSpeedX * -1;
                                ballLocX = -21.88;

                            }

                            if (calcTicksRight > 0)
                            {
                                ++calcTicksRight;
                                if (calcTicksRight == calcThreshold)
                                {
                                    if (!(pastBallLoc <= (paddlePlayer2Y + 3.5) && pastBallLoc >= (paddlePlayer2Y - 3.5)))
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine("Miss 1");
                                        Console.WriteLine("X : " + pastBallLocx + " Y : " + pastBallLoc + " paddle position 2 : " + paddlePlayer2Y);

                                        stopWatch.Stop();
                                        //stopStack.Push(new PointStruct(0, 0));
                                        //stopStack.Push(new PointStruct(1, 0));

                                        stopAndGoList[stopSection].visited = false;
                                        stopAndGoList[stopSection].point = 0;

                                        stopAndGoList[stopSection + 1].visited = false;
                                        stopAndGoList[stopSection + 1].point = 0;
                                        stopAndGoList[goSection].visited = false;
                                        stopAndGoList[goSection + 1].visited = false;
                                        gameStart = !gameStart;
                                        ballLocX = 0.0;
                                        ballLocY = 21.0;
                                        curreSpeedX = speedX;
                                        curreSpeedY = speedY;
                                        calcTicksRight = 0;
                                    }
                                    else
                                    {
                                        calcTicksRight = 0;
                                        Console.WriteLine();
                                        Console.WriteLine("Hit 1");
                                        Console.WriteLine("X : " + pastBallLocx + " Y : " + pastBallLoc + " paddle position 2 : " + paddlePlayer2Y);
                                        hitStack0.Push("hit\\" + ballLocX + "\\" + ballLocY + "\\" + curreSpeedX + "\\" + curreSpeedY );
                                        hitStack1.Push("hit\\" + ballLocX + "\\" + ballLocY + "\\" + curreSpeedX + "\\" + curreSpeedY);
                                    }
                                }
                                
                            }
                            else if (calcTicksLeft > 0 )
                            {
                                ++calcTicksLeft;
                                if (calcTicksLeft == calcThreshold)
                                {
                                    if (!(pastBallLoc <= (paddlePlayer1Y + 3.5) && pastBallLoc >= (paddlePlayer1Y - 3.5)))
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine("Miss 0");
                                        Console.WriteLine("X : " + pastBallLocx + " Y : " + pastBallLoc + " paddle position 1 : " + paddlePlayer1Y);

                                        stopWatch.Stop();

                                        stopAndGoList[stopSection].visited = false;
                                        stopAndGoList[stopSection].point = 1;

                                        stopAndGoList[stopSection + 1].visited = false;
                                        stopAndGoList[stopSection + 1].point = 1;
                                        stopAndGoList[goSection].visited = false;
                                        stopAndGoList[goSection+1].visited = false;
                                        gameStart = !gameStart;
                                        ballLocX = 0.0;
                                        ballLocY = 21.0;
                                        curreSpeedX = speedX;
                                        curreSpeedY = speedY;
                                        calcTicksLeft = 0;
                                    }
                                    else
                                    {
                                        calcTicksLeft = 0;
                                        Console.WriteLine();
                                        Console.WriteLine("Hit 0");
                                        Console.WriteLine("X : " + pastBallLocx + " Y : " + pastBallLoc + " paddle position 1 : " + paddlePlayer1Y);
                                        hitStack0.Push("hit\\" + ballLocX + "\\" + ballLocY + "\\" + curreSpeedX + "\\" + curreSpeedY);
                                        hitStack1.Push("hit\\" + ballLocX + "\\" + ballLocY + "\\" + curreSpeedX + "\\" + curreSpeedY);
                                    }

                                }
                                
                            }
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
