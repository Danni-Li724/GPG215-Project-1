using System.Collections.Generic;
using UnityEngine;

public static class ShootPointPicker
{
    public static List<Vector2> PickPointsInPolygon(
        PolygonCollider2D area,
        int pointCount,
        float minDistance,
        int maxAttempts)
    {
        List<Vector2> points = new List<Vector2>(pointCount);

        if (area == null || pointCount <= 0)
            return points;

        Bounds b = area.bounds;
        float minDistSqr = minDistance * minDistance;

        int attempts = 0;

        while (points.Count < pointCount && attempts < maxAttempts)
        {
            attempts++;

            Vector2 candidate = new Vector2(
                Random.Range(b.min.x, b.max.x),
                Random.Range(b.min.y, b.max.y)
            );

            if (!area.OverlapPoint(candidate))
                continue;

            bool tooClose = false;

            for (int i = 0; i < points.Count; i++)
            {
                if ((candidate - points[i]).sqrMagnitude < minDistSqr)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
                continue;

            points.Add(candidate);
        }

        return points;
    }
}
