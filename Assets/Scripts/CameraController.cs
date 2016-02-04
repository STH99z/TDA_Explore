using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform trans;
	public Transform focus;

	public int iFrameRate;

    void LateUpdate()
    {
		trans.position = new Vector3(focus.position.x, focus.position.y, -10f);
	}

	void Awake()
	{
		//Application.targetFrameRate = iFrameRate;
	}
    
}
