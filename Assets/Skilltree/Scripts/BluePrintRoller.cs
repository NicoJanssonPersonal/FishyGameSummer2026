using UnityEngine;
using System.Collections.Generic;

public class BluePrintRoller : MonoBehaviour
{
    public GameObject nodeManager;
    public GameObject roll;
    public GameObject background;
    [Tooltip("Adjusts how fast the roll spins relative to its movement.")]
    public float rotationSpeedMultiplier = 100f; 

    private MeshRenderer meshRenderer;
    List<GameObject> matchingChildren;

    void Start()
    {
        background.transform.localScale = new Vector3(0, 1, 1);
        meshRenderer = background.GetComponent<MeshRenderer>();
        if (nodeManager == null)
        {
            Debug.LogError("Node Manager is not assigned in the Inspector!", this);
            return;
        }

        matchingChildren = new List<GameObject>();

        Transform[] allChildren = nodeManager.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in allChildren)
        {
            if (child != nodeManager.transform && child.name.ToLower().Contains("node"))
            {
                matchingChildren.Add(child.gameObject);
            }
        }

        Debug.Log($"Found {matchingChildren.Count} nodes under NodeManager.");
    }


    void Update()
    {
        if (meshRenderer != null)
        {
            float rightEdgeX = meshRenderer.bounds.max.x;

            float topEdgeY = meshRenderer.bounds.max.y;
            float bottomEdgeY = meshRenderer.bounds.min.y;

            Vector3 topRightCorner = new Vector3(rightEdgeX, topEdgeY, background.transform.position.z);
            Vector3 bottomRightCorner = new Vector3(rightEdgeX, bottomEdgeY, background.transform.position.z);

            Debug.DrawLine(topRightCorner, bottomRightCorner, Color.red);

            Debug.Log($"Right Edge X: {rightEdgeX}");
            
            float previousX = roll.transform.position.x;
            
            roll.transform.position = new Vector3(rightEdgeX, roll.transform.position.y, roll.transform.position.z);
            
            float deltaX = roll.transform.position.x - previousX;

            roll.transform.Rotate(0, deltaX * rotationSpeedMultiplier, 0);
        }

        foreach (var child in matchingChildren)
        {
            if (roll.transform.position.x >= child.transform.position.x)
            {
                child.SetActive(true);
            }
            else
            {
                child.SetActive(false);
            }
        }
        
       scaleBackground(background, -2f, 1f);
        if (Input.GetKeyDown(KeyCode.R))
        {
            background.transform.localScale = new Vector3(0,1,1);
        }
    }

    public Vector3 GetRightEdgePosition()
    {
        if (meshRenderer != null)
        {
            float rightEdgeX = meshRenderer.bounds.max.x;

            return new Vector3(rightEdgeX, background.transform.position.y, background.transform.position.z);
        }

        return background.transform.position;
    }

    public void scaleBackground(GameObject obj, float targetScaleX, float speed)
    {
        if (obj == null) return;

        Vector3 currentScale = obj.transform.localScale;

        float newScaleX = Mathf.Lerp(currentScale.x, targetScaleX, speed * Time.deltaTime);

        obj.transform.localScale = new Vector3(newScaleX, currentScale.y, currentScale.z);
    }
}