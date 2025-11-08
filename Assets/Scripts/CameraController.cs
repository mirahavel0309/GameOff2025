using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    public float moveSpeed = 10f;
    public float smoothMoveDuration = 1f;
    public Camera cam;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private float moveTimer = 0f;
    private Vector3 moveStartPosition;
    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        HandleInput();

        if (isMoving)
        {
            moveTimer += Time.deltaTime;
            float t = Mathf.Clamp01(moveTimer / smoothMoveDuration);
            transform.position = Vector3.Lerp(moveStartPosition, targetPosition, t);

            if (t >= 1f)
                isMoving = false;
        }
    }

    void HandleInput()
    {
        if (isMoving) return;

        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 dir = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) dir += forward;
        if (Input.GetKey(KeyCode.S)) dir -= forward;
        if (Input.GetKey(KeyCode.D)) dir += right;
        if (Input.GetKey(KeyCode.A)) dir -= right;

        if (dir != Vector3.zero)
        {
            transform.position += dir.normalized * moveSpeed * Time.deltaTime;
        }

    }

    public void JumpToPosition(Vector3 pos)
    {
        isMoving = false;
        transform.position = pos;
    }

    public void MoveToPosition(Vector3 pos)
    {
        moveStartPosition = transform.position;
        targetPosition = pos;
        moveTimer = 0f;
        isMoving = true;
    }
}
