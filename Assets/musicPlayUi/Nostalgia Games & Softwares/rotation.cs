using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotation : MonoBehaviour
{
    int spinSpeed;
    public
    void Update()
    {
        transform.Rotate(0, 0, -spinSpeed * Time.deltaTime);
    }

    public void stop() { spinSpeed = 0; }
    public void play() { spinSpeed = 150; }
}
