using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider slider;

    public void ChangeMaxHealth(float max){
        slider.maxValue = max;
    }

    public void ChangeActualHealth(float health){
        slider.value = health;
    }

    public void InitializeHealthBar(float health){
        slider = GetComponent<Slider>();
        ChangeMaxHealth(health);
        ChangeActualHealth(health);
    }
}
