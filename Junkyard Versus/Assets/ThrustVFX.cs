using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrustVFX : MonoBehaviour
{
    Animator _animator;
    public GameObject _MeshThrustVFX;

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
    }
}
