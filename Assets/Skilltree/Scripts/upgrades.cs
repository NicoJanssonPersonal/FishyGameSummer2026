using UnityEngine;

public class upgrades : MonoBehaviour
{
    public void boosh()
    {
        Debug.Log("booshed :)"); 
    }
    public void buyCannon()
    {
        GlobalStats.cannon = true;
    }
}
