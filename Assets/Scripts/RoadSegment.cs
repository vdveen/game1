using System.Collections.Generic;
using UnityEngine;

public class RoadSegment : MonoBehaviour
{
    public float width = 10f;
    public float thickness = 0.2f;

    public void SetDimensions(Vector3 startPos, Vector3 endPos)
    {
        float length = Vector3.Distance(startPos, endPos);
        transform.localScale = new Vector3(width, thickness, length);

        // Position at midpoint
        float verticalOffset = 0.6f; // Adjust this value to lower/raise the road
        transform.position = (startPos + endPos) / 2f - (Vector3.up * verticalOffset);

        // Rotate to align with direction
        transform.rotation = Quaternion.LookRotation(endPos - startPos);
    }
}