using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ParallaxBackground : MonoBehaviour
{
    private GameObject cam;

    [SerializeField] private float parallaxeffect;

    private float xposition;
    private float length;
    void Start()
    {
        cam = GameObject.Find("Main Camera");


        length = GetComponent<SpriteRenderer>().bounds.size.x;
        xposition = transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        float distancetomove = cam.transform.position.x * parallaxeffect;
        float distancemoved = cam.transform.position.x * (1 - parallaxeffect);

        transform.position = new Vector3(xposition + distancetomove, transform.position.y);

        if (distancemoved > length + xposition)
        {
            xposition += length;

        }

        else if(distancemoved < xposition - length) 
        {
            xposition -= length;
        }
    }

}
