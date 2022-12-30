using System.Collections.Generic;
using UnityEngine;

public class CollisionUtils : MonoBehaviour
{
    public static RaycastHit2D Raycast(Vector3 origin, Vector2 direction, float distance, LayerMask layerMask, Vector3 end)
    {
        RaycastHit2D hit2D = Physics2D.Raycast(origin, direction, distance, layerMask);
        Debug.DrawLine(origin, end, Color.red);
        return hit2D;
    }

    public static RaycastHit2D Raycast(Vector3 origin, Vector2 direction, float distance, LayerMask layerMask)
    {
        Vector3 end = new(origin.x + direction.x * distance, origin.y + direction.y * distance, origin.z);
        return Raycast(origin, direction, distance, layerMask, end);
    }

    public static RaycastHit2D[] RaycastArc(int hits, float angle, Vector3 origin, float originOffset, Vector2 direction, float distance, LayerMask layerMask)
    {
        if (hits % 2 == 0)
        {
            hits += 1;
        }

        RaycastHit2D[] hit2Ds = new RaycastHit2D[hits];

        Vector3 rayOrigin = new(origin.x + direction.x * originOffset, origin.y + direction.y * originOffset, origin.z);
        float rayAngle = angle / 2 / ((hits - 1) / 2);

        hit2Ds[0] = Raycast(rayOrigin, direction, distance, layerMask);

        int anglePos = 1;
        for (int i = 1; i < hits; i += 2)
        {
            Vector3 rayDirectionR = VectorUtils.RotateVector(direction, -rayAngle * anglePos).normalized;
            Vector3 rayOriginR = originOffset != 0 ? origin + rayDirectionR * originOffset : rayOrigin;
            hit2Ds[i] = Raycast(rayOriginR, rayDirectionR, distance, layerMask);

            Vector3 rayDirectionL = VectorUtils.RotateVector(direction, rayAngle * anglePos).normalized;
            Vector3 rayOriginL = originOffset != 0 ? origin + rayDirectionL * originOffset : rayOrigin;
            hit2Ds[i + 1] = Raycast(rayOriginL, rayDirectionL, distance, layerMask);

            anglePos++;
        }

        return hit2Ds;
    }

    public static RaycastHit2D FindFirst(RaycastHit2D[] hits, string name)
    {
        RaycastHit2D firstHit = new();

        foreach (RaycastHit2D rc in hits)
        {
            Collider2D collider = rc.collider;
            if (collider && collider.name == name)
            {
                firstHit = rc;
                break;
            }
        }

        return firstHit;
    }

    public static int Count(RaycastHit2D[] hits, string tag)
    {
        int collisions = 0;

        foreach (RaycastHit2D rc in hits)
        {
            Collider2D collider = rc.collider;
            if (collider)
            {
                collisions++;
            }
        }

        return collisions;
    }
}
