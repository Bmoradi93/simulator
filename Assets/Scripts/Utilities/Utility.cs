/**
 * Copyright (c) 2019 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

namespace Simulator.Utilities
{
    public static class Utility 
    {
        public static Transform FindDeepChild(this Transform parent, string name)
        {
            var result = parent.Find(name);
            if (result != null)
            {
                return result;
            }

            foreach (Transform child in parent)
            {
                result = child.FindDeepChild(name);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public static Type GetCollectionElement(this Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            return type.IsGenericList() ? type.GetGenericArguments()[0] : null;
        }

        public static bool IsCollectionType(this Type type) => (type.IsGenericList() || type.IsArray);

        public static bool IsGenericList(this Type type) => type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>));

        public static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) != null;

        public static object TypeDefaultValue(this Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }

        public static void RemoveAdjacentDuplicates(this IList list)
        {
            if (list.Count < 1)
            {
                return;
            }
            IList results = new List<object>();
            results.Add(list[0]);
            foreach (var e in list)
            {
                if (results[results.Count - 1] != e)
                {
                    results.Add(e);
                }
            }
            list.Clear();
            foreach (var r in results)
            {
                list.Add(r);
            }
        }

        public static bool IsPointCloseToLine(Vector2 p1, Vector2 p2, Vector2 pt, float connectionProximity)
        {
            bool isClose = false;
            Vector2 closestPt = Vector2.zero;
            float dx = p2.x - p1.x;
            float dy = p2.y - p1.y;

            // Calculate the t that minimizes the distance.
            float t = ((pt.x - p1.x) * dx + (pt.y - p1.y) * dy) / (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closestPt = new Vector2(p1.x, p1.y);
                dx = pt.x - p1.x;
                dy = pt.y - p1.y;
            }
            else if (t > 1)
            {
                closestPt = new Vector2(p2.x, p2.y);
                dx = pt.x - p2.x;
                dy = pt.y - p2.y;
            }
            else
            {
                closestPt = new Vector2(p1.x + t * dx, p1.y + t * dy);
                dx = pt.x - closestPt.x;
                dy = pt.y - closestPt.y;
            }

            if (Mathf.Sqrt(dx * dx + dy * dy) < connectionProximity)
            {
                isClose = true;
            }
            return isClose;
        }

        public static float SqrDistanceToSegment(Vector3 p0, Vector3 p1, Vector3 point)
        {
            var t = Vector3.Dot(point - p0, p1 - p0) / Vector3.SqrMagnitude(p1 - p0);

            Vector3 v = t < 0f ? p0 : t > 1f ? p1 : p0 + t * (p1 - p0);

            return Vector3.SqrMagnitude(point - v);
        }


        public static Vector3 ClosetPointOnSegment(Vector3 p0, Vector3 p1, Vector3 point)
        {
            float t = Vector3.Dot(point - p0, p1 - p0) / Vector3.SqrMagnitude(p1 - p0);
            return t < 0f ? p0 : t > 1f ? p1 : p0 + t * (p1 - p0);
        }

        // Calculate the distance between
        // point pt and the segment p1 --> p2.
        public static float FindDistanceToSegment(Vector2 pt, Vector2 p1, Vector2 p2, out Vector2 closest)
        {
            float dx = p2.x - p1.x;
            float dy = p2.y - p1.y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.x - p1.x;
                dy = pt.y - p1.y;
                return (float)Mathf.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            float t = ((pt.x - p1.x) * dx + (pt.y - p1.y) * dy) / (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new Vector2(p1.x, p1.y);
                dx = pt.x - p1.x;
                dy = pt.y - p1.y;
            }
            else if (t > 1)
            {
                closest = new Vector2(p2.x, p2.y);
                dx = pt.x - p2.x;
                dy = pt.y - p2.y;
            }
            else
            {
                closest = new Vector2(p1.x + t * dx, p1.y + t * dy);
                dx = pt.x - closest.x;
                dy = pt.y - closest.y;
            }

            return (float)Mathf.Sqrt(dx * dx + dy * dy);
        }

        // Test whether two line segments intersect. If so, calculate the intersection point.
        public static bool LineSegementsIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection, bool considerCollinearOverlapAsIntersect = false)
        {
            intersection = new Vector2();

            var r = a2 - a1;
            var s = b2 - b1;
            var rxs = Cross(r, s);
            var qpxr = Cross(b1 - a1, r);

            // If r x s = 0 and (b1 - a1) x r = 0, then the two lines are collinear.
            if (Math.Abs(rxs) < 0.0001f && Math.Abs(qpxr) < 0.0001f)
            {
                // 1. If either  0 <= (b1 - a1) * r <= r * r or 0 <= (a1 - b1) * s <= * s
                // then the two lines are overlapping,
                if (considerCollinearOverlapAsIntersect)
                {
                    if ((0 <= Vector2.Dot(b1 - a1, r) && Vector2.Dot(b1 - a1, r) <= Vector2.Dot(r, r)) || (0 <= Vector2.Dot(a1 - b1, s) && Vector2.Dot(a1 - b1, s) <= Vector2.Dot(s, s)))
                    {
                        return true;
                    }
                }

                // 2. If neither 0 <= (b1 - a1) * r = r * r nor 0 <= (a1 - b1) * s <= s * s
                // then the two lines are collinear but disjoint.
                // No need to implement this expression, as it follows from the expression above.
                return false;
            }

            // 3. If r x s = 0 and (b1 - a1) x r != 0, then the two lines are parallel and non-intersecting.
            if (Math.Abs(rxs) < 0.0001f && !(Math.Abs(qpxr) < 0.0001f))
                return false;

            // t = (b1 - a1) x s / (r x s)
            var t = Cross(b1 - a1, s) / rxs;

            // u = (b1 - a1) x r / (r x s)

            var u = Cross(b1 - a1, r) / rxs;

            // 4. If r x s != 0 and 0 <= t <= 1 and 0 <= u <= 1
            // the two line segments meet at the point a1 + t r = b1 + u s.
            if (!(Math.Abs(rxs) < 0.0001f) && (0 <= t && t <= 1) && (0 <= u && u <= 1))
            {
                // We can calculate the intersection point using either t or u.
                intersection = a1 + t * r;

                // An intersection was found.
                return true;
            }

            // 5. Otherwise, the two line segments are not parallel but do not intersect.
            return false;
        }

        public static float Cross(Vector2 v1, Vector2 v2)
        {
            return v1.x * v2.y - v1.y * v2.x;
        }

        public static bool CurveSegmentsIntersect(List<Vector2> a, List<Vector2> b, out List<Vector2> intersections)
        {
            intersections = new List<Vector2>();
            for (int i = 0; i < a.Count - 1; i++)
            {
                for (int j = 0; j < b.Count - 1; j++)
                {
                    Vector2 intersect;
                    if (LineSegementsIntersect(a[i], a[i + 1], b[j], b[j + 1], out intersect))
                    {
                        intersections.Add(intersect);
                    }
                }
            }
            return intersections.Count > 0;
        }

        //compute the s-coordinates of p's nearest point on line segments, p does not have to be on the segment
        public static float GetNearestSCoordinate(Vector2 p, List<Vector2> lineSegments, out float totalLength)
        {
            totalLength = 0;
            if (lineSegments.Count < 2)
            {
                return 0;
            }

            float minDistToSeg = float.MaxValue;
            float sCoord = 0;
            for (int i = 0; i < lineSegments.Count - 1; i++)
            {
                Vector2 closestPt;
                float distToSeg = FindDistanceToSegment(p, lineSegments[i], lineSegments[i + 1], out closestPt);
                float segLeng = (lineSegments[i + 1] - lineSegments[i]).magnitude;
                totalLength += segLeng;
                if (distToSeg < minDistToSeg)
                {
                    minDistToSeg = distToSeg;
                    sCoord = totalLength - segLeng + (closestPt - lineSegments[i]).magnitude;
                }
            }

            return sCoord;
        }

        public static float GetCurveLength(List<Vector2> lineSegments)
        {
            if (lineSegments.Count < 2)
            {
                return 0;
            }
            float totalLength = 0;
            for (int i = 0; i < lineSegments.Count - 1; i++)
            {
                totalLength += (lineSegments[i] - lineSegments[i + 1]).magnitude;
            }
            return totalLength;
        }
    }
}