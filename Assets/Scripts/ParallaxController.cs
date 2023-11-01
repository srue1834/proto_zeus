using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    public HorizontalDir horizontalDir = HorizontalDir.Fix;
    public VerticalDir verticalDir = VerticalDir.Fix;

    public FloatRef speedMult;
    private List<ParallaxImage> parallaxImages;

    public MoveType moveType = MoveType.OverTime;
    public Transform transformFollow;

    private Vector3 lastPosition;

    private void Start()
    {
        parallaxImages = new List<ParallaxImage>(GetComponentsInChildren<ParallaxImage>());
        lastPosition = transformFollow.position;

        foreach (var image in parallaxImages)
        {
            image.InitImage(speedMult, horizontalDir, verticalDir, moveType == MoveType.FollowTransform);
        }
    }

    private void FixedUpdate()
    {
        if (moveType == MoveType.OverTime)
        {
            MoveOverTime();
        }
        else if (moveType == MoveType.FollowTransform)
        {
            FollowTransform();
        }
    }

    private void MoveOverTime()
    {
        if (horizontalDir == HorizontalDir.Fix) return;

        float distanceMoved = Time.deltaTime * speedMult.value;
        foreach (var image in parallaxImages)
        {
            image.MoveX(distanceMoved);
        }
    }

    private void FollowTransform()
    {
        Vector3 delta = transformFollow.position - lastPosition;

        if (horizontalDir != HorizontalDir.Fix)
        {
            foreach (var image in parallaxImages)
            {
                image.MoveX(delta.x);
            }
        }

        if (verticalDir != VerticalDir.Fix)
        {
            foreach (var image in parallaxImages)
            {
                image.MoveY(delta.y);
            }
        }

        lastPosition = transformFollow.position;
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