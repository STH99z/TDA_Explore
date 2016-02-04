using UnityEngine;
using System.Collections;

public class PlayerSelf : MonoBehaviour {

	void OnCollision2DEnter(Collision2D collision)
	{
		print("Collided with someone C");
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		print("Collided with someone T");
	}
}
