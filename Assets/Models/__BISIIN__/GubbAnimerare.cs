using UnityEngine;

public class GubbAnimerare : MonoBehaviour
{
    public Animator animator;

    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("cast");
        }

    }
}
