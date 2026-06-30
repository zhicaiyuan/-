using UnityEngine;

public static class NearestPlatformFinder
{
    public struct Settings
    {
        public LayerMask groundLayer;
        public CapsuleCollider2D bodyCollider;
        public Collider2D[] excludeColliders;
        public Collider2D partialTrapVolume;
        public float standGap;
        public float searchRadius;
        public float horizontalStep;
        public float probeHeight;
        public float maxRayDistance;
        public float verticalSearchBoost;
        public float upwardPenaltyWeight;
        public float maxUpwardFromOrigin;
    }

    public static bool TryFind(Vector3 from, in Settings settings, out Vector3 standPosition)
    {
        standPosition = from;
        float bestScore = float.MaxValue;
        bool found = false;

        float pivotToBottom = GetPivotToBottom(settings.bodyCollider);
        float step = Mathf.Max(0.2f, settings.horizontalStep);
        float radius = Mathf.Max(step, settings.searchRadius);

        for (float offsetX = -radius; offsetX <= radius + 0.001f; offsetX += step)
        {
            if (!TryFindBestOnColumn(from, offsetX, pivotToBottom, in settings, out Vector3 candidate, out float score))
                continue;

            if (score >= bestScore)
                continue;

            bestScore = score;
            standPosition = candidate;
            found = true;
        }

        return found;
    }

    private static bool TryFindBestOnColumn(
        Vector3 from,
        float offsetX,
        float pivotToBottom,
        in Settings settings,
        out Vector3 standPosition,
        out float score)
    {
        standPosition = default;
        score = float.MaxValue;
        bool found = false;

        float[] probeHeights =
        {
            settings.probeHeight,
            settings.probeHeight + settings.verticalSearchBoost,
            settings.probeHeight + settings.verticalSearchBoost * 2f
        };

        foreach (float probeHeight in probeHeights)
        {
            Vector2 origin = new Vector2(from.x + offsetX, from.y + probeHeight);
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, settings.maxRayDistance, settings.groundLayer);

            if (hits == null || hits.Length == 0)
                continue;

            System.Array.Sort(hits, (a, b) => b.point.y.CompareTo(a.point.y));

            foreach (RaycastHit2D hit in hits)
            {
                if (!IsValidPlatformHit(hit))
                    continue;

                float platformTop = GetPlatformSurfaceY(hit);
                Vector3 candidate = new Vector3(from.x + offsetX, platformTop + settings.standGap + pivotToBottom, from.z);

                if (IsBlockedByExcludedAreas(candidate, pivotToBottom, hit, settings.excludeColliders, settings.partialTrapVolume))
                    continue;

                if (!HasClearBodySpace(candidate, settings.bodyCollider, settings.groundLayer))
                    continue;

                float candidateScore = Vector2.Distance(new Vector2(from.x, from.y), new Vector2(candidate.x, candidate.y));
                float verticalDelta = candidate.y - from.y;

                if (settings.maxUpwardFromOrigin >= 0f && verticalDelta > settings.maxUpwardFromOrigin)
                    continue;

                float upwardWeight = settings.upwardPenaltyWeight > 0f ? settings.upwardPenaltyWeight : 0.35f;
                float upwardPenalty = Mathf.Max(0f, verticalDelta) * upwardWeight;
                float downwardBonus = Mathf.Max(0f, -verticalDelta) * 0.2f;
                candidateScore += upwardPenalty - downwardBonus;

                if (candidateScore >= score)
                    continue;

                score = candidateScore;
                standPosition = candidate;
                found = true;
            }
        }

        return found;
    }

    private static bool IsValidPlatformHit(RaycastHit2D hit)
    {
        if (hit.collider == null)
            return false;

        if (DropThroughPlatform.IsDropThroughCollider(hit.collider))
            return true;

        return hit.normal.y > 0.05f;
    }

    private static float GetPlatformSurfaceY(RaycastHit2D hit)
    {
        if (DropThroughPlatform.IsDropThroughCollider(hit.collider))
            return hit.collider.bounds.max.y;

        return hit.point.y;
    }

    private static float GetPivotToBottom(CapsuleCollider2D bodyCollider)
    {
        if (bodyCollider == null)
            return 0.75f;

        return bodyCollider.transform.position.y - bodyCollider.bounds.min.y;
    }

    private static bool HasClearBodySpace(Vector3 pivotPosition, CapsuleCollider2D bodyCollider, LayerMask groundLayer)
    {
        if (bodyCollider == null)
            return true;

        Transform bodyTransform = bodyCollider.transform;
        Vector2 worldOffset = bodyTransform.TransformVector(bodyCollider.offset);
        Vector2 capsuleCenter = (Vector2)pivotPosition + worldOffset;

        Vector2 capsuleSize = bodyCollider.size;
        Vector2 scale = bodyTransform.lossyScale;
        capsuleSize = new Vector2(
            capsuleSize.x * Mathf.Abs(scale.x),
            capsuleSize.y * Mathf.Abs(scale.y));

        Collider2D[] overlaps = Physics2D.OverlapCapsuleAll(
            capsuleCenter,
            capsuleSize,
            bodyCollider.direction,
            bodyTransform.eulerAngles.z,
            groundLayer);

        float pivotToBottom = bodyTransform.position.y - bodyCollider.bounds.min.y;
        float feetY = pivotPosition.y - pivotToBottom;

        foreach (Collider2D overlap in overlaps)
        {
            if (overlap == null || overlap == bodyCollider)
                continue;

            if (DropThroughPlatform.IsDropThroughCollider(overlap) && overlap.bounds.max.y <= feetY + 0.08f)
                continue;

            return false;
        }

        return true;
    }

    private static bool IsBlockedByExcludedAreas(
        Vector3 pivotPosition,
        float pivotToBottom,
        RaycastHit2D platformHit,
        Collider2D[] excludeColliders,
        Collider2D partialTrapVolume)
    {
        if (partialTrapVolume != null && IsBlockedByPartialTrapVolume(pivotPosition, pivotToBottom, platformHit, partialTrapVolume))
            return true;

        if (excludeColliders == null || excludeColliders.Length == 0)
            return false;

        float feetY = pivotPosition.y - pivotToBottom;
        Vector2 feet = new Vector2(pivotPosition.x, feetY);
        float platformTop = GetPlatformSurfaceY(platformHit);
        float bodyHalfWidth = 0.35f;

        Vector2[] samplePoints =
        {
            feet,
            new Vector2(pivotPosition.x, pivotPosition.y),
            platformHit.point,
            new Vector2(platformHit.point.x, platformTop),
            feet + Vector2.left * bodyHalfWidth,
            feet + Vector2.right * bodyHalfWidth,
            new Vector2(feet.x, platformTop)
        };

        foreach (Collider2D exclude in excludeColliders)
        {
            if (exclude == null)
                continue;

            foreach (Vector2 point in samplePoints)
            {
                if (exclude.OverlapPoint(point))
                    return true;
            }

            if (IsStandingOnSurfaceInsideBounds(feet, platformTop, platformHit.collider, exclude))
                return true;
        }

        return false;
    }

    private static bool IsStandingOnSurfaceInsideBounds(
        Vector2 feet,
        float platformTop,
        Collider2D platformCollider,
        Collider2D exclude)
    {
        Bounds excludeBounds = exclude.bounds;

        if (feet.x < excludeBounds.min.x || feet.x > excludeBounds.max.x)
            return false;

        if (platformTop < excludeBounds.min.y - 0.15f || platformTop > excludeBounds.max.y + 0.15f)
            return false;

        if (exclude.OverlapPoint(new Vector2(feet.x, platformTop)))
            return true;

        if (platformCollider != null && excludeBounds.Intersects(platformCollider.bounds))
        {
            Vector2 platformCenter = platformCollider.bounds.center;
            if (exclude.OverlapPoint(platformCenter))
                return true;
        }

        return excludeBounds.Contains(new Vector3(feet.x, platformTop, 0f));
    }

    private static bool IsBlockedByPartialTrapVolume(
        Vector3 pivotPosition,
        float pivotToBottom,
        RaycastHit2D platformHit,
        Collider2D partialTrapVolume)
    {
        float feetY = pivotPosition.y - pivotToBottom;
        float platformTop = GetPlatformSurfaceY(platformHit);
        Vector2 feet = new Vector2(pivotPosition.x, feetY);

        bool insideVolume = partialTrapVolume.OverlapPoint(feet)
            || partialTrapVolume.OverlapPoint(new Vector2(feet.x, platformTop));

        if (!insideVolume)
            return false;

        return platformTop > partialTrapVolume.bounds.center.y;
    }
}
