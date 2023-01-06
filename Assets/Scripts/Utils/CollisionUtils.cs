using System.Collections.Generic;
using UnityEngine;

public class CollisionUtils : MonoBehaviour
{
    /// <summary>
    /// Creates Physics2D Raycast and draw debug line
    /// </summary>
    public static RaycastHit2D Raycast(Vector3 origin, Vector2 direction, float distance, LayerMask layerMask)
    {
        Vector3 end = new(origin.x + direction.x * distance, origin.y + direction.y * distance, origin.z);
        Debug.DrawLine(origin, end, Color.red);
        return Physics2D.Raycast(origin, direction, distance, layerMask);
    }

    /// <summary>
    /// Creates arc of Physics2D Raycasts.
    /// </summary>
    /// <param name="hits">Number of Raycasts</param>
    /// <param name="angle">Angle of the arc</param>
    public static RaycastHit2D[] RaycastArc(int hits, float angle, Vector3 origin, Vector2 direction, float distance, LayerMask layerMask)
    {
        RaycastHit2D[] hit2Ds = new RaycastHit2D[hits];

        Vector3 rayOrigin = new(origin.x + direction.x, origin.y + direction.y, origin.z);
        float initialRayAngle = angle / 2;
        float rayAngle = angle / (hits - 1);

        for (int i = 0; i < hits; i ++)
        {
            Vector3 rayDirection = VectorUtils.RotateVector(direction, initialRayAngle).normalized;
            hit2Ds[i] = Raycast(rayOrigin, rayDirection, distance, layerMask);

            initialRayAngle -= rayAngle;
        }

        return hit2Ds;
    }

    /// <summary>
    /// Creates group of Physics2D Raycasts covering a Bounds horizontal side
    /// </summary>
    public static RaycastHit2D[] RaycastHorizontal(Bounds bounds, Vector2 direction, int hits, float distance, LayerMask layerMask)
    {
        RaycastHit2D[] hits2D = new RaycastHit2D[hits];

        float aug = (bounds.max.y - bounds.min.y) / (hits - 1);

        for (int i = 0; i < hits; i++)
        {
            if (direction.x > 0)
            {
                Vector3 origin = new(bounds.max.x, bounds.max.y - aug * i, bounds.max.z);
                hits2D[i] = Raycast(origin, Vector2.right, distance, layerMask);
            }
            else if (direction.x < 0)
            {
                Vector3 origin = new(bounds.min.x, bounds.max.y - aug * i, bounds.max.z);
                hits2D[i] = Raycast(origin, Vector2.left, distance, layerMask);
            }
        }

        return hits2D;
    }

    /// <summary>
    /// Creates group of Physics2D Raycasts covering a Bounds vertical side
    /// </summary>
    public static RaycastHit2D[] RaycastVertical(Bounds bounds, Vector2 direction, int hits, float distance, LayerMask layerMask)
    {
        RaycastHit2D[] hits2D = new RaycastHit2D[hits];

        float aug = (bounds.max.x - bounds.min.x) / (hits - 1);


        for (int i = 0; i < hits; i++)
        {
            if (direction.y > 0)
            {
                Vector3 origin = new(bounds.max.x - aug * i, bounds.max.y, bounds.max.z);
                hits2D[i] = Raycast(origin, Vector2.up, distance, layerMask);
            }
            else if (direction.y < 0)
            {
                Vector3 origin = new(bounds.max.x - aug * i, bounds.min.y, bounds.max.z);
                hits2D[i] = Raycast(origin, Vector2.down, distance, layerMask);
            }
        }

        return hits2D;
    }

    /// <summary>
    /// Creates group of Physics2D Raycasts covering a Bounds vertical and horizontal sides
    /// </summary>
    public static RaycastHit2D[] RaycastMovement(Bounds bounds, Vector2 movement, int hits, float distance, LayerMask layerMask)
    {
        RaycastHit2D[] hits2D = new RaycastHit2D[hits * 2];
        RaycastHit2D[] hits2DHorizontal = RaycastHorizontal(bounds, movement, hits, distance, layerMask);
        RaycastHit2D[] hits2DVertical = RaycastVertical(bounds, movement, hits, distance, layerMask);
        hits2DHorizontal.CopyTo(hits2D, 0);
        hits2DVertical.CopyTo(hits2D, hits2DHorizontal.Length);

        return hits2D;
    }

    /// <summary>
    /// Finds first RaycastHit2D which has hit a game object with specific name
    /// </summary>
    public static RaycastHit2D FindFirst(RaycastHit2D[] hits, string name)
    {
        RaycastHit2D firstHit = new();

        foreach (RaycastHit2D rc in hits)
        {
            Collider2D collider = rc.collider;
            if (collider && collider.gameObject.name == name)
            {
                firstHit = rc;
                break;
            }
        }

        return firstHit;
    }

    /// <summary>
    /// Counts RaycastHit2Ds which has hit a game object with specific tag
    /// </summary>
    public static int Count(RaycastHit2D[] hits, string tag)
    {
        int collisions = 0;

        foreach (RaycastHit2D rc in hits)
        {
            Collider2D collider = rc.collider;
            if (collider && collider.CompareTag(tag))
            {
                collisions++;
            }
        }

        return collisions;
    }
}
