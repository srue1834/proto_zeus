using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ParallaxImage : MonoBehaviour
{
    public float speedX = 0;
    public float speedY = 0;
    public int spawnCount = 2;

    private Transform[] c_transforms;
    private float image_width;
    private float minLeftx;
    private float maxRightx;

    private FloatRef speedMult;
    private const int roundFactor = 10000;

    private HorizontalDir hDir;
    private bool followTransform;
    private VerticalDir vDir;

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

    public void InitImage(FloatRef speedMult, HorizontalDir hDir, VerticalDir vDir, bool followTransform)

    {
        this.speedMult = speedMult;
        this.hDir = hDir;
        this.followTransform = followTransform;
        this.vDir = vDir;

        int arraySize = spawnCount;
        if (followTransform) arraySize *= 2;
        arraySize += 1;

        c_transforms = new Transform[arraySize];
        c_transforms[0] = transform;

        image_width = GetComponent<SpriteRenderer>().bounds.size.x;

        if (followTransform)
        {
            minLeftx = transform.position.x - image_width * (spawnCount + 1) - 0.5f; // smallest left most position
            maxRightx = transform.position.x + image_width * (spawnCount + 1) + 0.5f;


        }
        else
        {
            if (hDir == HorizontalDir.Left)
            {
                minLeftx = transform.position.x - image_width - 0.5f; // smallest left most position
                maxRightx = float.PositiveInfinity;
            }
            else if (hDir == HorizontalDir.Right)
            {
                maxRightx = transform.position.x + image_width + 0.5f;
                minLeftx = float.NegativeInfinity;

            }
            else if (hDir == HorizontalDir.Fix)
            {
                minLeftx = float.NegativeInfinity;
                maxRightx = float.PositiveInfinity;

            }
        }



        float posx;
        for (int i = 1; i < c_transforms.Length; i++)
        {
            if (hDir == HorizontalDir.Right || !followTransform) // Changed this condition
            {
                posx = transform.position.x - image_width * i;
            }
            else
            {
                posx = transform.position.x + image_width * i;
            }

            c_transforms[i] = PCopyAt(posx);
        }
    }

    private Transform PCopyAt(float posX)
    {
        GameObject go = Instantiate(gameObject, new Vector3(posX, transform.position.y, transform.position.z), Quaternion.identity, transform.parent);

        // Check if the clone has the ParallaxImage script attached. If not, add it.
        ParallaxImage parallaxImage = go.GetComponent<ParallaxImage>();
        if (!parallaxImage)
        {
            parallaxImage = go.AddComponent<ParallaxImage>();
        }

        // Initialize the clone's ParallaxImage script with the same properties as the original
        parallaxImage.speedX = this.speedX;
        parallaxImage.speedY = this.speedY;
        parallaxImage.spawnCount = this.spawnCount;

        // Ensure we don't have nested ParallaxImage scripts
        foreach (ParallaxImage childImage in go.GetComponentsInChildren<ParallaxImage>())
        {
            if (childImage != parallaxImage)
            {
                Destroy(childImage);
            }
        }

        return go.transform;
    }


    public void MoveX(float moveBy)
    {
        moveBy *= speedX * speedMult.value;

        if (hDir == HorizontalDir.Right) moveBy *= -1;

        moveBy = Mathf.Round(moveBy * roundFactor) / roundFactor;


        for (int i = 0; i < c_transforms.Length; i++)
        {
            Vector3 newPos = c_transforms[i].position;
            newPos.x -= moveBy; // to the left

            newPos.x = Mathf.Round(newPos.x * roundFactor) / roundFactor;
            c_transforms[i].position = newPos;
        }

        Reposition();
    }

    public void MoveY(float moveBy)
    {
        moveBy *= speedY * speedMult.value;

        if (vDir == VerticalDir.Down) moveBy *= -1;

        for (int i = 0; i < c_transforms.Length; i++)
        {
            Vector3 newPos = c_transforms[i].position;
            newPos.y -= moveBy;
            c_transforms[i].position = newPos;
        }
    }

    private void Reposition()
    {
        if (hDir == HorizontalDir.Left || followTransform)
        {
            for (int i = 0; i < c_transforms.Length; i++)
            {
                if (c_transforms[i].position.x < minLeftx)
                {
                    Vector3 newPos = c_transforms[i].position;
                    newPos.x = GetRightmostTransform().position.x + image_width;
                    c_transforms[i].position = newPos;
                }
            }
        }

        if (hDir == HorizontalDir.Right || followTransform)
        {
            for (int i = 0; i < c_transforms.Length; i++)
            {
                if (c_transforms[i].position.x > maxRightx)
                {
                    Vector3 newPos = c_transforms[i].position;
                    newPos.x = GetLeftmostTransform().position.x - image_width;
                    c_transforms[i].position = newPos;
                }
            }
        }

        if (!followTransform) // New condition for Move Over Time
        {
            for (int i = 0; i < c_transforms.Length; i++)
            {
                if (hDir == HorizontalDir.Left && c_transforms[i].position.x < minLeftx)
                {
                    Vector3 newPos = c_transforms[i].position;
                    newPos.x = GetRightmostTransform().position.x + image_width;
                    c_transforms[i].position = newPos;
                }
                else if (hDir == HorizontalDir.Right && c_transforms[i].position.x > maxRightx)
                {
                    Vector3 newPos = c_transforms[i].position;
                    newPos.x = GetLeftmostTransform().position.x - image_width;
                    c_transforms[i].position = newPos;
                }
            }
        }
    }

    private Transform GetRightmostTransform()
    {
        return GetExtremeTransform(true);
    }

    private Transform GetLeftmostTransform()
    {
        return GetExtremeTransform(false);
    }

    private Transform GetExtremeTransform(bool getRightmost)
    {
        float extremeX = getRightmost ? float.NegativeInfinity : float.PositiveInfinity;
        Transform extremeT = null;

        for (int i = 0; i < c_transforms.Length; i++)
        {
            if (getRightmost && extremeX < c_transforms[i].position.x)
            {
                extremeX = c_transforms[i].position.x;
                extremeT = c_transforms[i];
            }
            else if (!getRightmost && extremeX > c_transforms[i].position.x)
            {
                extremeX = c_transforms[i].position.x;
                extremeT = c_transforms[i];
            }
        }

        return extremeT;
    }





}


