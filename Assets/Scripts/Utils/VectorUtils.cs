using UnityEngine;

public class VectorUtils : MonoBehaviour
{
    /// <summary>
    /// Rotates a Vector3 an specific angle
    /// </summary>
    public static Vector3 RotateVector(Vector3 v, float angle)
    {
        float radianAngle = angle * Mathf.Deg2Rad;
        float x = v.x * Mathf.Cos(radianAngle) - v.y * Mathf.Sin(radianAngle);
        float y = v.x * Mathf.Sin(radianAngle) + v.y * Mathf.Cos(radianAngle);
        return new Vector3(x, y, v.z);
    }

}
