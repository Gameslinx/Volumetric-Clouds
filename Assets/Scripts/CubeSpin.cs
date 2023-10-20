using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CubeSpin : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 200;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        transform.rotation = transform.rotation * Quaternion.Euler(0, -1f, 0);
    }
}
