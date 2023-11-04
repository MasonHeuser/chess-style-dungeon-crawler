using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class FlickerEffect : MonoBehaviour
{
    public Light2D lightSource;
    float timer;

    void Update()
    {   
        if (timer <= 0f) {
            lightSource.intensity = Random.Range(1f, 1.25f);
            timer = 0.12f;
        } else {
            timer -= Time.deltaTime;
        }
    }
}
