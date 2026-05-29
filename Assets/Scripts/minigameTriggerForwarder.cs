using System;
using UnityEngine;

public class minigameTriggerForwarder : MonoBehaviour
{
    // Actions passing: (This GameObject, The other Collider, Is it entering?)
    public event Action<GameObject, Collider2D, bool> OnForwardTriggerChanged;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // true means "Entered"
        OnForwardTriggerChanged?.Invoke(gameObject, other, true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // false means "Exited"
        OnForwardTriggerChanged?.Invoke(gameObject, other, false);
    }
}