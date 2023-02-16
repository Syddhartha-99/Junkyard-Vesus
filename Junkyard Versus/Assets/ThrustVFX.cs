using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;


public class ThrustVFX : MonoBehaviour
{
    //delete script when replacement is made for enabling vfx element
    //just enableing an gameobject when dashing and disabling when not.
    //script also has aiming rigging for gun, delete when making new one


    Animator _animator;
    public GameObject _MeshThrustVFX;
    public PlayerStateMachine _PlayerSM;
    public Rig playerRig;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(_animator.GetBool("isDashing") == true)
        {
            Debug.Log("Thrusting");
            _MeshThrustVFX.SetActive(true);
        }
        else
        {
            _MeshThrustVFX.SetActive(false);
        }


        //temp aiming 
        if(_PlayerSM.IsAimPressed == true)
        {
            playerRig.weight = 1;
        }
        else
        {
            playerRig.weight = 0;
        }
    }
}
