using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxImage : MonoBehaviour
{
    public float speedX = 0;
    public int spawnCount = 2;

    private Transform[] c_transforms;
    private float image_width;

    public void CleanUpImage()
    {
        if (c_transforms != null)
        {
            for (int i = 1; i < c_transforms.Length; i++)
            {
                Destroy(c_transforms[i].gameObject);
            }
        }

    }

    public void InitImage()
    {
        c_transforms = new Transform[spawnCount + 1];
        c_transforms[0] = transform;

        image_width = GetComponent<SpriteRenderer>().bounds.size.x;


        for (int i = 1; i < c_transforms.Length; i++)
        {
            c_transforms[i] = PCopyAt(transform.position.x + image_width * i);
        }
    }

    private Transform PCopyAt(float posX)
    {
        GameObject go = Instantiate(gameObject, new Vector3(posX, transform.position.y, transform.position.z), Quaternion.identity, transform.parent);
        Destroy(go.GetComponent<ParallaxImage>());

        return go.transform;
    }
   
}
