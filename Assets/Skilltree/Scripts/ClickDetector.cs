using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class ClickDetector : MonoBehaviour
{
    public UnityEvent onClickAction;
    public bool clickedOnce = false;
    public string Titel;
    public string Description;
    public int SkillpointCost = 1;
    [Header("Line Settings")]
    public GameObject lineRendererPrefab;
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    public Color unlockableColor = Color.white;
    public Color purchasedColor = Color.green;

    [Header("Connections")]
    public GameObject[] nextNodes;

    public bool buyable = false;

    private Dictionary<GameObject, LineRenderer> nodeLines = new Dictionary<GameObject, LineRenderer>();

    void Start()
    {
        DrawPaths();  
    }

    private void OnMouseDown()
    {
        if (onClickAction != null && !clickedOnce && buyable && GlobalStats.skillpoints >= SkillpointCost)
        {
            clickedOnce = true;
            GlobalStats.skillpoints = GlobalStats.skillpoints - SkillpointCost;
            GlobalStats.nodesUnlocked = GlobalStats.nodesUnlocked + 1;
            Debug.Log(gameObject.name + " was purchased!");

            foreach (var kvp in nodeLines)
            {
                GameObject targetNode = kvp.Key;
                LineRenderer line = kvp.Value;

                if (line != null && targetNode != null)
                {
                    ClickDetector targetScript = targetNode.GetComponent<ClickDetector>();
                    
                    if (targetScript != null && targetScript.clickedOnce)
                    {
                        line.startColor = purchasedColor;
                        line.endColor = purchasedColor;
                    }
                    else
                    {
                        line.startColor = unlockableColor;
                        line.endColor = unlockableColor;
                    }
                }
            }

            NotifyIncomingConnections();

            foreach (var node in nextNodes)
            {
                if (node != null)
                {
                    ClickDetector nextNodeScript = node.GetComponent<ClickDetector>();
                    if (nextNodeScript != null)
                    {
                        nextNodeScript.buyable = true;
                    }
                }
            }

            onClickAction.Invoke();
        }
    }
    void OnMouseEnter()
    {
        Debug.Log(Titel + " " + Description);
    }
    void OnMouseExit()
    {
        
    }

    private void NotifyIncomingConnections()
    {
        ClickDetector[] allNodes = FindObjectsByType<ClickDetector>(FindObjectsSortMode.None);
        foreach (var node in allNodes)
        {
            if (node.nodeLines.ContainsKey(gameObject))
            {
                LineRenderer incomingLine = node.nodeLines[gameObject];
                if (incomingLine != null)
                {
                    incomingLine.startColor = purchasedColor;
                    incomingLine.endColor = purchasedColor;
                }
            }
        }
    }

    void DrawPaths()
    {
        if (lineRendererPrefab == null) return;

        foreach (var node in nextNodes)
        {
            if (node == null) continue;

            GameObject lineInstance = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity, transform);
            lineInstance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            LineRenderer lr = lineInstance.GetComponent<LineRenderer>();

            if (lr != null)
            {
                lr.useWorldSpace = true;
                lr.positionCount = 2;

                Vector3 startPos = new Vector3(transform.position.x, transform.position.y - 0.01f, transform.position.z);
                Vector3 endPos = new Vector3(node.transform.position.x, node.transform.position.y - 0.01f, node.transform.position.z);

                lr.SetPosition(0, startPos);
                lr.SetPosition(1, endPos);

                lr.startColor = lockedColor;
                lr.endColor = lockedColor;

                nodeLines.Add(node, lr);
            }
        }
    }
}