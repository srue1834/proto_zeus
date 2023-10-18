using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    private List<ParallaxImage> images;

    private void Start()
    {
        InitController();
        
    }

    private void InitController()
    {
        InitList(); // list is usable
        ScanForImages();

        foreach (var item in images)
        {
            item.InitImage();
        }
    }



    private void InitList()
    {
        if (images == null) images = new List<ParallaxImage>();
        else
        {
            foreach (var item in images)
            {
                item.CleanUpImage();
            }
            images.Clear();
        }

    }

    private void ScanForImages()
    {
        ParallaxImage pi;
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                pi = child.GetComponent<ParallaxImage>();

                if (pi != null) images.Add(pi);
            }
        }
    }


}
