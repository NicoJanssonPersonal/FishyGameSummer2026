using UnityEngine;

public class FishingLine : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform rodTip;

    [Header("Line Settings")]
    public int segmentCount = 20;
    public float sagAmount = 0.5f;

    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
    }

    public void DrawLine(GameObject bobber)
    {
        if (bobber == null || rodTip == null || lineRenderer == null) return;

        lineRenderer.enabled = true;
        lineRenderer.positionCount = segmentCount;

        Vector3 start = rodTip.position;
        Vector3 end = bobber.transform.position;
        float distance = Vector3.Distance(start, end);

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);

            Vector3 point = Vector3.Lerp(start, end, t);

            float sag = Mathf.Sin(t * Mathf.PI) * sagAmount * (distance * 0.1f);
            point.y -= sag;

            lineRenderer.SetPosition(i, point);
        }
    }

    // Call this when reeling in to hide the line
    public void ClearLine()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }
    }
    public float fishingLineLength()
    {
        if (lineRenderer == null || lineRenderer.positionCount < 2) return 0f;

        float totalLength = 0f;

        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            Vector3 currentPoint = lineRenderer.GetPosition(i);
            Vector3 nextPoint = lineRenderer.GetPosition(i + 1);

            totalLength += Vector3.Distance(currentPoint, nextPoint);
        }

        return totalLength;
    }
}