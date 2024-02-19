using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool ContainsSquare()
    {
        // Здесь вы можете реализовать логику для проверки наличия квадрата на клетке
        // Например, можно проверить наличие дочерних объектов
        return transform.childCount > 0;
    }
}