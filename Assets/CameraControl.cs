using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float cameraSize;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0.5f * Main.S.width, 0.5f * Main.S.height+5f,-10f);
        cameraSize = Mathf.Max( 0.5f*Main.S.height, 0.5f * Main.S.width);
        GetComponent<Camera>().orthographicSize = cameraSize;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
