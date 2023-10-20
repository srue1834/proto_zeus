using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{

    public HorizontalDir horizontalDir;

    public FloatRef speedMult;
    private List<ParallaxImage> images;

    public MoveType moveType;
    [Header("Only for follow transform")]
    public Transform transformFollow;
    public VerticalDir verticalDir;

    private float lasty;
    private float lastx;

    private void Start()
    {
        InitController();
        
    }

    private void FixedUpdate()
    {
        if (images == null) return;

        if (moveType == MoveType.OverTime) MoveOverTime();
        else if (moveType == MoveType.FollowTransform)
        {
            FollowTransformX();
            FollowTransformY();

        }


    }

    private void MoveOverTime()
    {
        if (horizontalDir == HorizontalDir.Fix) return;

        foreach (var item in images)
        {
            item.MoveX(Time.deltaTime);
        }

    }

    private void FollowTransformX()
    {
        if (horizontalDir == HorizontalDir.Fix) return;

        float distance = lastx - transformFollow.position.x;
        if (Mathf.Abs(distance) < 0.001f) return;

        foreach (var item in images)
        {
            item.MoveX(distance);
        }

        lastx = transformFollow.position.x;

    }

    private void FollowTransformY()
    {
        if (verticalDir == VerticalDir.Fix) return;

        float distance = lasty - transformFollow.position.y;
        if (Mathf.Abs(distance) < 0.001f) return;

        foreach (var item in images)
        {
            item.MoveY(distance);
        }

        lasty = transformFollow.position.y;

    }
    private void InitController()
    {
        InitList(); // list is usable
        ScanForImages();

        foreach (var item in images)
        {
            item.InitImage(speedMult, horizontalDir, verticalDir, moveType == MoveType.FollowTransform);
        }
        if (moveType == MoveType.FollowTransform)
        {
            lastx = transformFollow.position.x;
            lasty = transformFollow.position.y;
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

[System.Serializable]
public class FloatRef
{
    [Range(0.01f, 5)]
    public float value = 1;


}

public enum HorizontalDir
{
    Fix,
    Left,
    Right
}

public enum MoveType
{
    OverTime,
    FollowTransform

}

public enum VerticalDir
{
    Fix,
    Up,
    Down
}