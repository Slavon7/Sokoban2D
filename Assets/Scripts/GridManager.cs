using UnityEngine;
using System.Collections;


public class GridManager : MonoBehaviour
{
    public int rows = 4;
    public int columns = 5;
    public float cellSize = 1f; 
    public GameObject[,] grid;
    GameObject[,,] gridObjects;
    public GameObject cellPrefab; 
    public GameObject objectPrefab; 
    public int objectsToPlace = 18; 
    private bool[,] cellOccupied; 

    private Camera _camera;

    void Start()
    {
        _camera = Camera.main;
        gridObjects = new GameObject[rows, columns, 2];
        grid = new GameObject[rows, columns];
        cellOccupied = new bool[rows, columns]; 

        float startX = -(columns * cellSize) / 2f + cellSize / 2f;
        float startY = -(rows * cellSize) / 2f + cellSize / 2f;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject cell = Instantiate(cellPrefab, new Vector3(startX + j * cellSize, startY + i * cellSize, 0), Quaternion.identity);
                grid[i, j] = cell;
            }
        }

        PlaceRandomObjects();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                CheckCellOccupancy(new Vector2Int(i, j)); 
            }
        }
    }

    void PlaceRandomObjects()
    {
        var chosenCells = new System.Collections.Generic.HashSet<Vector2Int>();

        for (int objIndex = 0; objIndex < objectsToPlace; objIndex++)
        {
            Vector2Int randomCell;
            do
            {
                randomCell = new Vector2Int(Random.Range(0, rows), Random.Range(0, columns));
            } while (chosenCells.Contains(randomCell));

            chosenCells.Add(randomCell);

            cellOccupied[randomCell.x, randomCell.y] = true;

            Vector3 cellCenter = grid[randomCell.x, randomCell.y].transform.position;

            GameObject placedObject = Instantiate(objectPrefab, cellCenter, Quaternion.identity);

            gridObjects[randomCell.x, randomCell.y, 0] = grid[randomCell.x, randomCell.y];
            gridObjects[randomCell.x, randomCell.y, 1] = placedObject;
        }
    }

    void CheckCellOccupancy(Vector2Int cellCoordinates)
    {
        bool occupied = cellOccupied[cellCoordinates.x, cellCoordinates.y];

        if (occupied)
        {
            Debug.Log("Клітинка (" + cellCoordinates.x + ", " + cellCoordinates.y + ") зайнята.");
        }
        else
        {
            Debug.Log("клітинка (" + cellCoordinates.x + ", " + cellCoordinates.y + ") вільна.");
        }
    }

    void MoveObjectToNeighborFreeCell(GameObject obj, int rowIndex, int colIndex)
    {
        int[] rowOffsets = { -1, 1, 0, 0 };
        int[] colOffsets = { 0, 0, -1, 1 };

        for (int i = 0; i < rowOffsets.Length; i++)
        {
            int neighborRow = rowIndex + rowOffsets[i];
            int neighborCol = colIndex + colOffsets[i];

            if (IsValidCell(neighborRow, neighborCol))
            {
                if (!cellOccupied[neighborRow, neighborCol])
                {
                    // update the cellOccupied info
                    cellOccupied[rowIndex, colIndex] = false;
                    cellOccupied[neighborRow, neighborCol] = true;
                    
                    // update the gridObjects info
                    gridObjects[rowIndex, colIndex, 1] = null;
                    gridObjects[neighborRow, neighborCol, 1] = obj;

                    // move the object to the new cell
                    Vector3 neighborCellPosition = grid[neighborRow, neighborCol].transform.position;
                    StartCoroutine(MoveObjectCoroutine(obj, neighborCellPosition));
                        
                    Debug.Log("Обжект переїхав на сосідню клітинку: (" + neighborRow + ", " + neighborCol + ")");
                    return;
                }
            }
        }

        Debug.Log("Немає сусідних клітинок для переміщення");
    }

    IEnumerator MoveObjectCoroutine(GameObject obj, Vector3 targetPosition)
    {
        float duration = 0.5f; 
        float elapsedTime = 0f;
        Vector3 initialPosition = obj.transform.position;

        while (elapsedTime < duration)
        {
            float t = Mathf.Clamp01(elapsedTime / duration);

            // інтерполація між об'єктом початкової та кінцевої точки
            obj.transform.position = Vector3.Lerp(initialPosition, targetPosition, t);
            
            // анімка
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        obj.transform.position = targetPosition;
    }


    bool IsValidCell(int rowIndex, int colIndex)
    {
        return rowIndex >= 0 && rowIndex < rows && colIndex >= 0 && colIndex < columns;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Ліва кнопка натиснута.");

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit)
            {
                Debug.Log("Луч пройшов через об'єкти : " + hit.collider.gameObject.name);

                GameObject hitObject = hit.collider.gameObject;
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        if (hitObject == gridObjects[i, j, 0] && gridObjects[i, j, 1] != null) // Check the cell and if there is an object
                        {
                            MoveObjectToNeighborFreeCell(gridObjects[i, j, 1], i, j); // Move the object not the cell
                            return; 
                        }
                    }
                }
            }
        }
    }
}