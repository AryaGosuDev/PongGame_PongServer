using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class Ball : MonoBehaviour {

	// Use this for initialization
	public int speedX;
	public int speedY;
	public bool isMoving ;
	public bool startBall;
	public bool reset;
	public bool updateOnHit;
	public float posUpdateX;
	public float posUpdateY;
	public int elapsed;
	public int paddlePredThreshold;

	private Sockets socks;
	public float ballLocBeforeStop ;
	
	private System.IO.StreamWriter file ;
	
	Gui guis;
	 
	void Start () {
		//rigidbody.velocity = Vector3.right * 50.0f;
		//rigidbody.AddForce( 10, 50 , 0);
		//speedX = Random.Range(-20, 20);
		//speedY = Random.Range(-20, 20);
		speedX = 0;
		speedY = 0;
		this.rigidbody.velocity = new Vector3(speedX, speedY, 0);
		
		isMoving = false;
		startBall = false;
		reset = false;
		
		paddlePredThreshold = 7;
		
		//print ( System.DateTime.Now.TimeOfDay.TotalMilliseconds );
		
		guis = GameObject.Find("Main Camera").GetComponent<Gui>();
		
		socks = guis.returnSocket ();
		
		//file = new System.IO.StreamWriter("c:\\Users\\amahini\\Downloads\\test.txt");
	}
	
	// Update is called once per frame
	void Update () {

		if ( reset )
		{
			this.transform.position = new Vector3 ( 0, 21, 0 );
			this.rigidbody.velocity = new Vector3 ( 0 , 0 , 0);
			reset = !reset ;	
		}
		
		if (  !isMoving && startBall &&   speedX != 0  && speedX != 0  )
		{
			this.rigidbody.velocity = new Vector3(speedX, speedY, 0);
			isMoving = !isMoving;
		}
		
		
		if ( this.transform.position.x >= 22.8 )
		{
			print ( "Ball at 23 " + this.transform.position.y );
		}
		else if ( this.transform.position.x <= -22.8 )
		{
			print ( "Ball at -23 " + this.transform.position.y );
		}
		
		if ( updateOnHit )
		{
			updateOnHitFunc();
			updateOnHit = !updateOnHit;
		}
	}
	
	void FixedUpdate()
	{
		//print ( "speedx : " + speedX + " speedy : " + speedY + " locx : " + this.rigidbody.position.x + " locky : " + this.rigidbody.position.y + " time : " + stopWatch.Elapsed.Seconds );
		//print ( " locx : " + this.rigidbody.position.x + " locky : " + this.rigidbody.position.y );	
		//file.WriteLine ( "X : " + this.transform.position.x + " Y : " + this.transform.position.y );
	}
	
	void updateOnHitFunc ()
	{
		print ( "update on hit " +  elapsed );
		
		
		for ( int i = 0 ; i < elapsed / 20 ; ++ i )
		{
			posUpdateX += (float)(speedX * 0.02);
			
			if (posUpdateY >= 43.9 || posUpdateY <= 2.1)
            {
            	speedY = speedY * -1;
				this.rigidbody.velocity = new Vector3( speedX, speedY , 0 );
				
            }
            posUpdateY += (float)(speedY * 0.02);
			
			
			
		}
		
		
		this.transform.position = new Vector3 ( posUpdateX , posUpdateY, 0 );
		this.rigidbody.velocity = new Vector3( speedX, speedY , 0 );
		
	}
	
	void OnCollisionEnter ( Collision collision )
	{
		//rigidbody.velocity = rigidbody.velocity * -1.0f;
		
		  if (collision.collider.name == "BorderTop")
        {
            // change vertical direction
			speedY = speedY * -1;
            this.rigidbody.velocity = new Vector3(speedX , 
                                             speedY, 
                                             0);
        }
		if (collision.collider.name == "BorderBottom")
        {
            // change vertical direction
			speedY = speedY * -1;
            this.rigidbody.velocity = new Vector3(speedX , 
                                             speedY , 
                                             0);
        }
		if (collision.collider.name == "BorderLeft")
        {
            // change vertical direction
			speedX = speedX * -1;
            this.rigidbody.velocity = new Vector3(speedX , 
                                             speedY , 
                                             0 );
			
			
        }
		if (collision.collider.name == "BorderRight")
        {
            // change vertical direction
			speedX = speedX * -1;
            this.rigidbody.velocity = new Vector3(speedX, 
                                             speedY , 
                                             0 );
			
			 
        }
		
		if (collision.collider.name == "Player1")
        {
            // change vertical direction
			ballLocBeforeStop = this.transform.position.y;
			speedX = speedX * -1;
            this.rigidbody.velocity = new Vector3(speedX  , 
                                             speedY , 
                                             rigidbody.velocity.z );
			
			//print ("hit\\0\\" + this.rigidbody.velocity.x + "\\" + this.rigidbody.velocity.y + "\\" + this.rigidbody.position.x + "\\" + this.rigidbody.position.y);
			//guis.sendStack.Enqueue("hit0" );
			if ( socks.returnClientNumber() == 1 ) 
			{
				//if ( isMoving ) guis.socks.SendTCPPacket("hit\\0\\" + this.rigidbody.velocity.x + "\\" + this.rigidbody.velocity.y + "\\" + this.rigidbody.position.x + "\\" + this.rigidbody.position.y  );	
				//guis.sendStack.Enqueue("hit\\0\\" + this.rigidbody.velocity.x + "\\" + this.rigidbody.velocity.y + "\\" + this.rigidbody.position.x + "\\" + this.rigidbody.position.y + "\\" + System.DateTime.Now.TimeOfDay.TotalMilliseconds     );
			}
        }
		if (collision.collider.name == "Player2")
        {
            // change vertical direction
			ballLocBeforeStop = this.transform.position.y;
			speedX = speedX * -1;
            this.rigidbody.velocity = new Vector3( speedX  , 
                                             speedY , 
                                             rigidbody.velocity.z );
			
			//print ("hit\\1\\" + this.rigidbody.velocity.x + "\\" + this.rigidbody.velocity.y + "\\" + this.rigidbody.position.x + "\\" + this.rigidbody.position.y);
			//guis.sendStack.Enqueue("hit1"  );
			if ( socks.returnClientNumber() == 2 ) 
			{
				//if ( isMoving ) guis.socks.SendTCPPacket("hit\\1\\" + this.rigidbody.velocity.x + "\\" + this.rigidbody.velocity.y + "\\" + this.rigidbody.position.x + "\\" + this.rigidbody.position.y  );
				//guis.sendStack.Enqueue("hit\\1\\" + this.rigidbody.velocity.x + "\\" + this.rigidbody.velocity.y + "\\" + this.rigidbody.position.x + "\\" + this.rigidbody.position.y + "\\" + System.DateTime.Now.TimeOfDay.TotalMilliseconds  );
				
			}
        }
	}
}
