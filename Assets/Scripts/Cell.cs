using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool ContainsSquare()
    {
        return transform.childCount > 0;
    }
}
