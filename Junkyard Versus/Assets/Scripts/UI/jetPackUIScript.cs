using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class jetPackUIScript : MonoBehaviour
{
    // Start is called before the first frame update

    Slider slider;
    [SerializeField]
    PlayerController playerController;
    void Start()
    {
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = playerController.jetPackGas;
    }
}
