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

    [Header("Line Settings")]
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

    void Start()
    {
        GlobalStats.LoadStats();
        LoadNodeState();

        DrawPaths();

        //Invoke(nameof(SyncEntireNetworkVisuals), 0.02f);
        SyncEntireNetworkVisuals();
        titleText.text = "";
        descriptionText.text = "";
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

            SaveNodeState();

            foreach (var node in nextNodes)
            {
                if (node != null)
                {
                    ClickDetector nextNodeScript = node.GetComponent<ClickDetector>();
                    if (nextNodeScript != null)
                    {
                        nextNodeScript.buyable = true;
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
            }
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
    }

    void OnMouseEnter()
    {
        descriptionText.text = Description;
        titleText.text = Titel;
    }

    void OnMouseExit()
    {
        descriptionText.text = "";
        titleText.text = "";
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