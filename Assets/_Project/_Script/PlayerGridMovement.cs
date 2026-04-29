using UnityEngine;

public class PlayerGridMovement : MonoBehaviour
{
    public float moveSpeed = 5f;           
    public float gridSize = 1f;           

    private Vector3 targetPosition;
    private bool isMoving = false;

    void Start()
    {
        targetPosition = SnapToGrid(transform.position); 
        transform.position = targetPosition;
    }

    void Update()
    {
     
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (!isMoving && (h != 0 || v != 0))
        {
            Vector3 direction = new Vector3(h, 0, v).normalized;
            Vector3 nextPos = targetPosition + direction * gridSize;

          
            if (CanMoveTo(nextPos))
            {
                targetPosition = nextPos;
                isMoving = true;
            }
        }

       
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }

    Vector3 SnapToGrid(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x / gridSize) * gridSize,
            pos.y,                                   
            Mathf.Round(pos.z / gridSize) * gridSize
        );
    }

    bool CanMoveTo(Vector3 pos)
    {

        return true;
    }
}