using UnityEngine;
using System.Collections;
 

public class Gui : MonoBehaviour {

	public Sockets socks;
	bool show = false;
	public GameObject otherGameObject ;
	
	private char[] stringSeparators;
	
	public double delTime;

	Ball ball ;
	
	// Use this for initialization
	void Start () {
		
		socks = new Sockets();

		socks.setGameNTPTime ();
		//socks.getNTPTime();


		
		//print (socks.dt);
		//print ( socks.dt.Second );
		//print ( socks.dt.Millisecond );
		
		ball = GameObject.Find("Ball").GetComponent<Ball>();
		
		//socks.returnPoint0() = 0;
		//socks.returnPoint1() = 0;
		
	}
	// Update is called once per frame
	void Update () 
	{
		//socks.printUniTime ();
		if ( socks.sendStack.Count > 0 )
		{
						
						//print ( socks.sendStack.Peek());
						socks.SendTCPPacket((string)socks.sendStack.Dequeue());
		}

		socks.testThread();
	}
	
	void OnGUI()
	{
		GUI.Box( new Rect(10, 10, 100, 90 ) , "Main Menu");
		
		if ( !show)
		{
			if ( GUI.Button( new Rect( 20, 40, 80, 20), "Connect"))
			{
				//print ( "clicked button");
				if ( socks.Connect() )
				{	
					print ( "connected");
					show = !show;
					
					socks.SendTCPPacket("client");
				}
			}
		}
		
		if ( show ) {
			if ( GUI.Button( new Rect( 20, 40, 80, 20), "Disconnect"))
			{
				print ( "dfgfdgd");
					socks.endThread();
				//socks.Disconnect();
				show = !show;
			}
		}
		
		GUI.Box ( new Rect ( 10, 105, 100, 90 ) , "Comm menu");
		
		if ( GUI.Button ( new Rect ( 20, 135, 80, 20 ), "Send" ))
		{
			socks.SendTCPPacket ( "start game");
		}
		
		GUI.Box ( new Rect ( 10, 200, 100, 90 ) , "Score");
		
		GUI.Label ( new Rect ( 25, 220, 90, 20 ) , "Player1 : " + socks.returnPoint0() );
		
		GUI.Label ( new Rect ( 25, 240, 90, 20 ) , "Player2 : " + socks.returnPoint1() );
		
		GUI.Box ( new Rect ( 10, 295, 100, 90 ) , "Measure Latency");
		
		if ( GUI.Button ( new Rect ( 20, 320, 80, 20 ), "Send" ))
		{
			socks.measureLatency() ;
		}
		
		GUI.Label ( new Rect ( 15, 345, 90, 20 ) , "Latency : " + socks.returnLatency()  );
		
	}
	
	void FixedUpdate()
	{
		//print ( "2");
		//socks.printUniTime ( );
		
	}
	
	public Sockets returnSocket ()
	{
		return socks;
	}
}
