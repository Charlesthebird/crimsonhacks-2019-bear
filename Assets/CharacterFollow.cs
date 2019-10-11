using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFollow : MonoBehaviour {
    public Rigidbody toFollow;
    public bool useWorldUp = true;
    public bool faceCharacterVelocity = true;

    Vector3 offset;
    Vector3 lastForwardDirection;
    PlayerController pc;
	void Awake()
    {
        offset = this.transform.position - toFollow.transform.position;
        lastForwardDirection = transform.forward;
        pc = toFollow.GetComponent<PlayerController>();
	}
	void Update () {
        this.transform.position = toFollow.transform.position + offset;


        if (useWorldUp)
        {
            transform.up = Vector3.up;
        }
        if (faceCharacterVelocity)
        {
            if (pc != null && pc.inputRequest.magnitude > .1f)
            {
                transform.forward = Vector3.Lerp(
                    lastForwardDirection,
                    new Vector3(pc.inputRequest.x, transform.forward.y, pc.inputRequest.z), .8f);
            }
            /*
            if (Mathf.Abs(toFollow.velocity.x) + Mathf.Abs(toFollow.velocity.z) > .1f)
            {
                transform.forward = Vector3.Lerp(
                    lastForwardDirection,
                    new Vector3(toFollow.velocity.x, transform.forward.y, toFollow.velocity.z), .3f);
            }*/
            lastForwardDirection = transform.forward;
        }
    }
}
