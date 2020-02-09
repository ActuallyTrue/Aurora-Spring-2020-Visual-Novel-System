using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxFillScript : MonoBehaviour
{
    SpriteRenderer sr;

    float alphaValue;
    
    void Start()
    {
        sr = this.GetComponent<SpriteRenderer>();
        alphaValue = sr.color.a;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    float GetAlphaValue() {
        return alphaValue;
    }
    void SetAlphaValue(float a) {
        sr.color = new Color(sr.color.r,sr.color.g,sr.color.b,a);
        alphaValue = a;
    }
}
