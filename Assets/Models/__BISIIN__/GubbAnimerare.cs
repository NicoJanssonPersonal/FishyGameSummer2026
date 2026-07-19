using UnityEngine;

public class GubbAnimerare : MonoBehaviour
{
    [Header("References")]
    public Transform rodTip;
    public LineRenderer fishingLine;
    public Animator animator;

    [Header("Settings")]
    public float waterHeight = 0f;

    private Camera cam;
    private Vector3 targetCastPosition;
    private bool isLineDrawn = false;

    private void Start()
    {
        cam = Camera.main;
        
        if (fishingLine != null)
        {
            fishingLine.positionCount = 0;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryCast();
        }

        if (isLineDrawn && fishingLine != null && rodTip != null)
        {
            fishingLine.SetPosition(0, rodTip.position);
            fishingLine.SetPosition(1, targetCastPosition);
        }
    }

    private void TryCast()
    {
        Plane waterPlane = new Plane(Vector3.up, new Vector3(0, waterHeight, 0));
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (waterPlane.Raycast(ray, out float enter))
        {
            targetCastPosition = ray.GetPoint(enter);

            if (animator != null)
            {
                animator.SetTrigger("cast");
            }

            DrawLine();
        }
    }

    private void DrawLine()
    {
        if (fishingLine == null || rodTip == null) return;

        fishingLine.positionCount = 2;
        fishingLine.SetPosition(0, rodTip.position);
        fishingLine.SetPosition(1, targetCastPosition);
        isLineDrawn = true;
    }

    public void ClearLine()
    {
        isLineDrawn = false;
        if (fishingLine != null)
        {
            fishingLine.positionCount = 0;
        }
    }
}
