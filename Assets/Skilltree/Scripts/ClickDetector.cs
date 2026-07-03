using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;

public class ClickDetector : MonoBehaviour
{
    public UnityEvent onClickAction;
    public bool clickedOnce = false;
    public string Titel;
    public string Description;
    public int SkillpointCost = 1;

    [Header("Line & Node Colors")]
    public GameObject lineRendererPrefab;
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    public Color unlockableColor = Color.white;
    public Color purchasedColor = Color.green;

    [Header("Connections")]
    public GameObject[] nextNodes;

    public bool buyable = false;

    private Dictionary<GameObject, LineRenderer> nodeLines = new Dictionary<GameObject, LineRenderer>();
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    // Cache the renderer to change the node's visual color
    private Renderer nodeRenderer; 
    private SpriteRenderer spriteRenderer; 

    void Start()
    {
        // Cache visual components
        nodeRenderer = GetComponent<Renderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GlobalStats.LoadStats();
        LoadNodeState();

        DrawPaths();

        SyncEntireNetworkVisuals();
        
        if (titleText != null) titleText.text = "";
        if (descriptionText != null) descriptionText.text = "";
    }

    private void OnMouseDown()
    {
        if (onClickAction != null && !clickedOnce && buyable && GlobalStats.skillpoints >= SkillpointCost)
        {
            clickedOnce = true;
            buyable = false; // Once purchased, this specific node is no longer "buyable"
            
            GlobalStats.skillpoints = GlobalStats.skillpoints - SkillpointCost;
            GlobalStats.nodesUnlocked = GlobalStats.nodesUnlocked + 1;
            Debug.Log(gameObject.name + " was purchased!");

            // Updates its own material color immediately
            UpdateNodeSelfColor();

            SaveNodeState();

            foreach (var node in nextNodes)
            {
                if (node != null)
                {
                    ClickDetector nextNodeScript = node.GetComponent<ClickDetector>();
                    if (nextNodeScript != null)
                    {
                        nextNodeScript.buyable = true;
                        nextNodeScript.UpdateNodeSelfColor(); // Refresh the neighbor's color (e.g. from locked to unlockable)
                        nextNodeScript.SaveNodeState();
                    }
                }
            }

            SyncEntireNetworkVisuals();
            GlobalStats.SaveMoneyAndSkillpoints();
            onClickAction.Invoke();
        }
    }

    public void SyncEntireNetworkVisuals()
    {
        ClickDetector[] allNodes = FindObjectsByType<ClickDetector>(FindObjectsSortMode.None);
        foreach (var node in allNodes)
        {
            if (node != null)
            {
                node.UpdateLineColors();
                node.UpdateNodeSelfColor(); // Ensure node materials update dynamically across the network
            }
        }
    }

    /// <summary>
    /// Updates the material color of this specific node depending on its current state.
    /// </summary>
    public void UpdateNodeSelfColor()
    {
        Color targetColor = lockedColor;

        if (clickedOnce)
        {
            targetColor = purchasedColor;
        }
        else if (buyable)
        {
            targetColor = unlockableColor;
        }

        // Apply to 3D Mesh Renderer if it exists
        if (nodeRenderer != null)
        {
            nodeRenderer.material.color = targetColor;
        }
        // Apply to 2D Sprite Renderer if it exists
        if (spriteRenderer != null)
        {
            spriteRenderer.color = targetColor;
        }
    }

    public void UpdateLineColors()
    {
        foreach (var kvp in nodeLines)
        {
            GameObject targetNode = kvp.Key;
            LineRenderer line = kvp.Value;

            if (line != null && targetNode != null)
            {
                ClickDetector targetScript = targetNode.GetComponent<ClickDetector>();

                if (clickedOnce && targetScript != null && targetScript.clickedOnce)
                {
                    line.startColor = purchasedColor;
                    line.endColor = purchasedColor;
                }
                else if (clickedOnce)
                {
                    line.startColor = unlockableColor;
                    line.endColor = unlockableColor;
                }
                else
                {
                    line.startColor = lockedColor;
                    line.endColor = lockedColor;
                }
            }
        }
    }

    public void SaveNodeState()
    {
        if (string.IsNullOrEmpty(Titel))
        {
            Debug.LogWarning($"Node on {gameObject.name} is missing a Title! Cannot save.");
            return;
        }

        PlayerPrefs.SetInt(Titel + "_clickedOnce", clickedOnce ? 1 : 0);
        PlayerPrefs.SetInt(Titel + "_buyable", buyable ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void LoadNodeState()
    {
        if (string.IsNullOrEmpty(Titel)) return;

        if (PlayerPrefs.HasKey(Titel + "_clickedOnce"))
        {
            clickedOnce = PlayerPrefs.GetInt(Titel + "_clickedOnce") == 1;

            bool savedBuyable = PlayerPrefs.GetInt(Titel + "_buyable") == 1;

            if (clickedOnce)
            {
                buyable = false;
            }
            else
            {
                buyable = savedBuyable;
            }
        }

        // Apply loaded states to visual nodes immediately after setup
        UpdateNodeSelfColor(); 
    }

    void OnMouseEnter()
    {
        if (descriptionText != null) descriptionText.text = Description;
        if (titleText != null) titleText.text = Titel;
    }

    void OnMouseExit()
    {
        if (descriptionText != null) descriptionText.text = "";
        if (titleText != null) titleText.text = "";
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