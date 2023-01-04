using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private void Start()
    {
        Physics2D.IgnoreLayerCollision(6, 6, true);
        Physics2D.IgnoreLayerCollision(6, 8, true);
        Physics2D.IgnoreLayerCollision(3, 8, true);
    }

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
