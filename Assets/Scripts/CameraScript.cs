using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] private GameObject player;

    // Update is called once per frame
    void Update()
    {
        if (player != null){
            Vector3 position = transform.position;
            position.x = player.transform.position.x;
            transform.position = position;
        }
        
    }
}
