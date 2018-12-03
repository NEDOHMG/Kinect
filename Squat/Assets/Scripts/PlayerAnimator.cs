using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour {

    public Animator animator;

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    void Update() {

        if (Input.GetKeyDown(KeyCode.A))
        {
            animator.SetBool("isActive", true);
            animator.SetBool("isNotActive", true);     
        } else if (Input.GetKeyDown(KeyCode.S))
        {
            animator.SetBool("isActive", false);
            animator.SetBool("isNotActive", false);
        }
    }
}
