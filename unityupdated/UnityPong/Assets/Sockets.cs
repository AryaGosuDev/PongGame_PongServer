using UnityEngine;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System;
using System.Diagnostics;
using System.Threading;

public class Sockets : MonoBehaviour {
	
	public TcpClient client; 
	
	public Stream s;
	public NetworkStream nws;
	public StreamReader sr ;
	public StreamWriter sw ;
	public static int clientNumber;
	public static bool startGame ;
	public bool connected;
	
	public static float paddleY = 23;
	public float veloX;
	public float veloY;
	public float possX;
	public float possY;
	
	public static int point0 = 0;
	public static int point1 = 0;
	
	public static DateTime dt;
	public static DateTime beforeLatCheck;
	public static int latency ;
	public static Stopwatch uniClock;
	public static Thread t = null; 
	public static Thread t1 = null; 
		
	protected static DateTime T1 ; 
	protected static bool threadState = false;

	protected static double  timeel ;
	protected static double  timee2 ;

	
	public Queue sendStack;
	
	public static Ball ball ;
	
	public Sockets()
	{
		uniClock = new Stopwatch();
		connected = false;
		sendStack = new Queue();
		
		ball = GameObject.Find("Ball").GetComponent<Ball>();
	}
	
	// Use this for initialization
	void Start () {
		
	}
	// Update is called once per frame
	void Update () {
		
	}
	
	public bool Connect ()
	{
		try
		{
			ball = GameObject.Find("Ball").GetComponent<Ball>();
			//client  = new TcpClient("68.4.62.156" , 2345 );
			//client  = new TcpClient("128.195.11.120" , 2345 );
			client  = new TcpClient("128.195.11.136" , 2345 );
			//clientUDP = new UdpClient("68.4.62.156" , 2345 );
			//s = client.GetStream();
			nws = client.GetStream();
			//sr = new StreamReader(s);
			//sw = new StreamWriter(s);
			sr = new StreamReader(nws);
			sw = new StreamWriter(nws);
			sw.AutoFlush = true;
		}
		catch ( Exception ex )
		{
			print ( ex.Message);
			
		}
		finally
		{
			if ( client.Connected )
			{
				connected = true;
				ThreadSock tSock = new ThreadSock(sr, sw );
				//Thread t = new Thread ( new ThreadStart(tSock.Service));
				t = new Thread ( new ThreadStart(tSock.Service));

				//ThreadSendSock tSockSend = new ThreadSendSock(sr, sw, ref sendStack  );
				//t1 = new Thread ( new ThreadStart(tSockSend.Service));

				//t.IsBackground = true;
				t.Start();
				//t1.IsBackground = true;
				//t1.Start();
				threadState = true;	
			}
			
		}
		return client.Connected;
	}
	
	public bool Disconnect ()
	{
		try
		{
			sr.Close();
			sw.Close();
			s.Close();
			client.Close();
		}
		catch ( Exception ex )
		{
			print ( ex.Message);
			
		}
		return true;
	}
	
	public float returnPaddle(){
		return paddleY;
	}
	
	public int returnPoint0(){
		return point0;
	}
	
	public int returnPoint1(){
		return point1;
	}
	
	public int returnLatency(){
		return latency;
	}
	
	public int returnClientNumber(){
		return clientNumber;
	}
	
	public void endThread(){
		threadState = false;
	}

	public void setGameNTPTime ()
	{
		dt = getNTPTime ();


	}
	
	public void testThread(){
		if ( t!= null && !threadState  )
		{
			print ( "thread aborted");
			t.Abort();
			t1.Abort ();
			threadState = !threadState;	
		}
	}
	
	
	public void SendTCPPacket ( string toSend )
	{
		try
		{
			//sw.Flush();
			//print ( "fdgdfg");
			//yield return new WaitForSeconds(5);
			//Thread.Sleep ( 200);
			//print ( "55555");
			timeel = uniClock.ElapsedMilliseconds;
			sw.WriteLine ( toSend);	
		}
		catch ( Exception ex )
		{
			print ( ex.Message );
		}	
	}
	
	public void measureLatency ()
	{
		//T1 = DateTime.UtcNow;
		//beforeLatCheck = dt.Add(new TimeSpan(0, 0, uniClock.Elapsed.Minutes, uniClock.Elapsed.Seconds, uniClock.Elapsed.Milliseconds));
		//string latString = TCPCommunicate ( "lat");
		SendTCPPacket ( "lat"); 	
		//DateTime T4 = DateTime.UtcNow;
		//TimeSpan tsOffset = T4.Subtract(T1);
		//print (tsOffset);
	}
	
	public DateTime getNTPTime()
	{
		Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		
		//IPAddress serverAddr = IPAddress.Parse("nist1-la.ustiming.org ");
		
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
		
		
		sock.Connect(endPoint);
		DateTime T1 = DateTime.UtcNow;
		Console.WriteLine("T1 : = " + T1 + " " + T1.Millisecond);
		sock.Send(ntpData);
		while (sock.Receive(ntpData) < 44) { Console.WriteLine("getting NTP"); }
		DateTime T4 = DateTime.UtcNow;
		Console.WriteLine("T4 : = " + T4 + " " + T4.Millisecond);
		//UInt32 destTime = (UInt32)(ntpData[16] << 24) | (UInt32)(ntpData[17] << 16) | (UInt32)(ntpData[18] << 8) | (UInt32)(ntpData[19]);
		sock.Close();
		
		Console.WriteLine("LI (lead indicator) : " + (ntpData[0] >> 6));
		int temp = ntpData[0] << 2;
		temp = temp >> 5;
		Console.WriteLine("VN (version number) : " + temp);
		temp = (byte)(ntpData[0] << 5);
		temp = temp >> 5;
		Console.WriteLine("Mode : " + temp);
		Console.WriteLine("Stratum Level : " + ntpData[1]);
		Console.WriteLine("Poll Interval :  " + ntpData[2]);
		Console.WriteLine("Precision : " + ntpData[3]);
		
		
		TimeSpan calculationTime;
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
		
		
		DateTime BaseDateExample = new DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		Console.WriteLine("Reference time stamp : " + BaseDateExample.AddSeconds(refTime).AddMilliseconds(refTimemilliseconds));
		Console.WriteLine("Originate time stamp : " + BaseDateExample.AddSeconds(origTime).AddMilliseconds(origTimemilliseconds));
		
		
		Console.WriteLine("Receive time stamp   : " + BaseDateExample.AddSeconds(recTime).AddMilliseconds(recTimemilliseconds) +" " + (BaseDateExample.AddSeconds(recTime).AddMilliseconds(recTimemilliseconds)).Millisecond);
		Console.WriteLine("Transmit time stamp  : " + BaseDateExample.AddSeconds(transTime).AddMilliseconds(milliseconds) +" " + (BaseDateExample.AddSeconds(transTime).AddMilliseconds(milliseconds)).Millisecond);
		
		DateTime BaseDate = new DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		DateTime dt = BaseDate.AddSeconds(transTime).AddMilliseconds(milliseconds);
		
		
		DateTime BaseDate1 = new DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		DateTime T2 = BaseDate1.AddSeconds(recTime).AddMilliseconds(recTimemilliseconds);
		
		calculationTime = dt.Subtract(T2);
		
		
		TimeSpan delay = T4.Subtract(T1);
		double networkDelay = delay.Milliseconds / 2.0;
		
		//Console.WriteLine( "Network Latency to NTP Server : " + (delay / 2.0)) ;
		
		
		//tsOffset = tsOffset.Add(dt.Subtract(T4));
		
		dt = dt.AddMilliseconds(networkDelay);

		print (dt);
		
		
		uniClock.Start();
		
		return dt;
	}
	
	public void printUniTime ()
	{
		//dt = dt.Add(new TimeSpan(0, 0, 0, uniClock.Elapsed.Seconds, uniClock.Elapsed.Milliseconds));
		print ((dt.AddMinutes(uniClock.Elapsed.Minutes).AddSeconds(uniClock.Elapsed.Seconds).AddMilliseconds(uniClock.Elapsed.Milliseconds)).Second);
	}
	
	private class ThreadSock
	{
		private StreamReader sr;
		private StreamWriter sw;
		private string inData;
		private string[] inStr;
		
		public ThreadSock (StreamReader insr, StreamWriter insw)
		{
			sr = insr;
			sw = insw;
			inData = "";
		}
		
		public void Service ()
		{
			try
			{
				while ( true )
				{

					if ((inData = sr.ReadLine())  != null)
					{
						
						if ( inData.StartsWith("paddle") )
						{
							char[] stringSeparators = {'\\'};
							inStr = inData.Split(stringSeparators);
							
							paddleY = float.Parse(inStr[2]);
							print ( paddleY );
							
						}
						else if ( inData.StartsWith("hit") )
						{
							DateTime recTime = dt.Add(new TimeSpan(0, 0, uniClock.Elapsed.Minutes, uniClock.Elapsed.Seconds, uniClock.Elapsed.Milliseconds));
							char[] stringSeparators =  {'\\'};
							inStr = inData.Split(stringSeparators);
							
							print ( "hit : " + inData);
							
							long elapsed = recTime.Ticks - Int64.Parse(inStr[5].ToString());
							
							TimeSpan elapsedSpan = new TimeSpan( (long)(recTime.Ticks - Int64.Parse(inStr[5])) );
							//ball.updateOnHit ( (int)elapsedSpan.TotalMilliseconds , float.Parse(inStr[1]), float.Parse(inStr[2]), float.Parse(inStr[3]), float.Parse(inStr[4]));
							ball.updateOnHit = true;
							ball.posUpdateX = float.Parse(inStr[1]);
							ball.posUpdateY = float.Parse(inStr[2]);
							ball.speedX = int.Parse ( inStr[3]);
							ball.speedY = int.Parse ( inStr[4]);
							ball.elapsed = (int)elapsedSpan.TotalMilliseconds ;
							//print ( elapsedSpan.TotalMilliseconds );
							
						}
						else if ( inData == "go")
						{
							ball.startBall = true;
						}
						else if ( inData.StartsWith("stop"))
						{
							print ( ball.ballLocBeforeStop );
							char[] stringSeparators =  {'\\'};
							inStr = inData.Split(stringSeparators);
							ball.startBall = false;
							ball.isMoving = false;
							
							ball.reset = true;
							
							
							print ( "stop game" + inData );
							
							if ( inStr[1] == "0" ) point0++;
							else point1++;
							
							sr.DiscardBufferedData();
							
						}
						else if ( inData.StartsWith("lat"))
						{
							//DateTime T2 = DateTime.UtcNow;

							timee2 = uniClock.ElapsedMilliseconds ;

							print (  timee2 - timeel );
							
							//print ( (T2.Subtract (T1 )).Milliseconds);
							/*
							DateTime recLat = dt.Add(new TimeSpan(0, 0, uniClock.Elapsed.Minutes, uniClock.Elapsed.Seconds, uniClock.Elapsed.Milliseconds));
							char[] stringSeparators =  {'\\'};
							string[] latStringDir = inData.Split(stringSeparators);
			
							long elapsed = long.Parse(latStringDir[1]) - beforeLatCheck.Ticks ;
							TimeSpan elapsedSpan = new TimeSpan(elapsed);
		

		
							latency = ((int)(elapsedSpan.Seconds + elapsedSpan.Milliseconds)) / 2;
							
							elapsedSpan = new TimeSpan( (recLat.Subtract(beforeLatCheck)).Ticks );
							print ( elapsedSpan.TotalMilliseconds/2 );
							
							elapsed = recLat.Ticks - beforeLatCheck.Ticks;
							elapsedSpan = new TimeSpan( elapsed );
							
							latency = ((int)(elapsedSpan.Seconds + elapsedSpan.Milliseconds)) / 2;
							*/

						}
						else if ( inData.StartsWith("start game"))
						{
							char[] stringSeparators =  {'\\'};
							inStr = inData.Split(stringSeparators);
							
							print ( "start game" + int.Parse(inStr[2]) + " "  + int.Parse(inStr[4]));
							
							ball.speedX = int.Parse(inStr[2]);
							ball.speedY = int.Parse(inStr[4]);
							startGame = true;
						}
						else if ( inData.StartsWith("client"))
						{
							DateTime recTime = dt.AddTicks(uniClock.ElapsedTicks);
							char[] stringSeparators =  {'\\'};
							inStr = inData.Split(stringSeparators);
							
							clientNumber = int.Parse(inStr[1]) ;
						
						}	
					}	
				}
			}
			catch ( Exception ex )
			{
				print ( ex.Message + " Thread loop " );
				threadState = false;
				t.Join();
					
				
			}
			finally 
			{
					
			}	
		}

	}
	private class ThreadSendSock
	{
		private StreamReader sr;
		private StreamWriter sw;
		private string inData;
		private string[] inStr;
		private Queue sendStack;
		
		public ThreadSendSock (StreamReader insr, StreamWriter insw, ref Queue inQueue)
		{
			sr = insr;
			sw = insw;
			sendStack = inQueue;
			inData = "";
		}
		
		public void Service ()
		{
			try
			{
				while ( true )
				{

						//if ( sendStack.Count > 0 )
						//{
							//Thread.Sleep ( 50 );
							//sw.WriteLine((string)sendStack.Dequeue());
						////}

				}
			}
			catch ( Exception ex )
			{
				print ( ex.Message + " Thread loop " );
				threadState = false;
				t1.Join();
				
				
			}
			finally 
			{
				
			}	
		}
		
	}	
}
