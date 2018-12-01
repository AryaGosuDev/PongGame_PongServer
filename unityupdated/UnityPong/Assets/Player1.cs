using UnityEngine;
using System.Collections;
using System;

public class Player1 : MonoBehaviour {
	
	public KeyCode key_up;
	public KeyCode key_down;
	
	private float paddlePosition;
	private float currPlayer1Pos;
	private float posPlayer2;
	private Vector3 pos ;
	private int prediction;
	
	
	Gui guis;
	
	Player2 player22;
	
	Ball ball ; 
	
	private Sockets socks;
	
	// Use this for initialization
	void Start () {
		
		guis = GameObject.Find("Main Camera").GetComponent<Gui>();
		
		player22 = GameObject.Find("Player2").GetComponent<Player2>();
		
		ball = GameObject.Find("Ball").GetComponent<Ball>();
		
		paddlePosition = this.transform.position.y;
		
		pos = transform.position;
		
		socks = guis.returnSocket ();
		
		prediction = 0;
	}
	
	// Update is called once per frame
	void Update () {

			

	
	}
	
	void FixedUpdate()
	{
		pos = transform.position;
		posPlayer2 = player22.transform.position.y;
		if ( socks.returnClientNumber() == 0 ) 
		{
			if ( Input.GetKey(key_up))
			{
				if ( this.transform.position.y <= 42 ) transform.position = new Vector3( pos.x, pos.y + 0.5f,pos.z );
			}
			
			if ( Input.GetKey(key_down))
			{
				if ( this.transform.position.y >= 4 ) transform.position = new Vector3( pos.x, pos.y - 0.5f, pos.z );
			}
			
			if ( Mathf.Abs( paddlePosition - this.transform.position.y ) >= 0.3 )
			{
				//if ( ball.isMoving ) guis.socks.SendTCPPacket("paddle\\0\\" + this.transform.position.y + "\\0");
				//if ( ball.isMoving )
				//{
					//guis.socks.SendTCPPacket("paddle\\0\\" + this.transform.position.y + "\\0");
					//guis.socks.SendTCPPacket("paddle\\0\\" + paddlePosition );
					//guis.sendStack.Enqueue("paddle\\0\\" + this.transform.position.y + "\\0");

					guis.socks.sendStack.Enqueue("paddle\\0\\" + this.transform.position.y);

				//}
				paddlePosition = this.transform.position.y;
			}
			
			//if ( Math.Abs(posPlayer2 - guis.socks.paddleY) > 0.5f )
			if ( Math.Abs(posPlayer2 - socks.returnPaddle()) > 0.5f )
			{
				
				if ( posPlayer2 < socks.returnPaddle() )
				{
					prediction++;
					player22.transform.position = new Vector3(23, posPlayer2 +  0.5f, 0 );
					if ( prediction >= ball.paddlePredThreshold )
					{
						print ( "predict ahead ");
						prediction = 0 ;
						player22.transform.position = new Vector3(23, posPlayer2 +  0.5f, 0 );
					}
				}
				else
				{
					prediction--;
					player22.transform.position = new Vector3(23, posPlayer2 -  0.5f, 0 );
					if ( prediction <= -1 * ball.paddlePredThreshold )
					{
						print ( "predict back" );
						prediction = 0 ;
						player22.transform.position = new Vector3(23, posPlayer2 -  0.5f, 0 );
					}
				}
			}
		}
	}
}
