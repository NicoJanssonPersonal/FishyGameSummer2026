using UnityEngine;

public class DockColliderChecker : MonoBehaviour
{
    public static bool isNearDock = false;
    public static bool enterdDockArea = false;
    void OnTriggerStay(Collider other)
    {
        //Debug.Log(other);
        isNearDock = true;
    }
    void OnTriggerExit(Collider other)
    {
        //Debug.Log(other);
        isNearDock = false;
    }
    void OnTriggerEnter(Collider other)
    {
        enterdDockArea = true;
    }
}
