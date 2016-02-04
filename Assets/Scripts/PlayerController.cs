using UnityEngine;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour {
    public GameObject player;
	private Animator anim;
	private Rigidbody2D rb;

	public float fMoveSpeed = 0.04f;

    // Use this for initialization
	void Start ()
    {
		anim = player.GetComponent<Animator>();
		rb = player.GetComponent<Rigidbody2D>();
    }
	// Update is called once per frame
	void Update ()
    {
		float x, y;
		x = Input.GetAxisRaw("Horizontal");
		y = Input.GetAxisRaw("Vertical");
		//方向
		if(x!=0f || y!= 0f)
		{
			anim.SetFloat("X", x);
			anim.SetFloat("Y", y);
		}
		else
		{
			anim.Play("Walk", 0, 0.25f);
		}
		//移动
		rb.velocity = new Vector2(x, y);	
    }
}
