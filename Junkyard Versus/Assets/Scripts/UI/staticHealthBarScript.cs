using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class staticHealthBarScript : MonoBehaviour
{
    Slider slider;
    [SerializeField]
    PlayerStateMachine playerStateMachine;
    void Start()
    {
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = playerStateMachine.Health/playerStateMachine.MaxHealth;
    }
}
