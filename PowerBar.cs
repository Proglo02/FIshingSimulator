using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerBar : MonoBehaviour
{
    public Slider powerBar;

    public void SetPower(float power)
    {
        powerBar.value = power;
    }
}
