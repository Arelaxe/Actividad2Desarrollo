using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour
{
    private Slider slider;

    private void Start(){
        slider = GetComponent<Slider>();
    }

    public void CambiaVidaMax(float vidaMax){
        slider.maxValue = vidaMax;
    }

    public void CambiaVidaActual(float vida){
        slider.value = vida;
    }

    public void InicializarBarra(float vida){
        CambiaVidaMax(vida);
        CambiaVidaActual(vida);
    }
}
