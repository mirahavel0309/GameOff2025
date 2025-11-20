using UnityEngine;

[System.Serializable]
public class CameraSaveData
{
    public Vector3 position;
    public Quaternion rotation;
    public float distance;
    public float pitch;
    public float yaw;
    public int cutawayHeight;

    public CameraSaveData() { }

    public CameraSaveData(Transform camTransform, Quaternion rotation, float distance, int cutawayHeight)
    {
        this.position = camTransform.position;
        this.rotation = rotation;
        this.distance = distance;
        this.cutawayHeight = cutawayHeight;
    }
}
public class FreeCameraControl : MonoBehaviour
{
    public Transform focusTarget;  // The object the camera follows
    public Vector3 originalPosition;
    public float moveSpeed = 10f;
    public float zoomSpeed = 5f;
    public float rotationSpeed = 90f;

    [Header("Distance Settings")]
    public float minDistance = 5f;
    public float maxDistance = 40f;
    public float currentDistance = 15f;

    [Header("Pitch Settings")]
    public float pitch = 45f;
    public float minPitch = 15f;
    public float maxPitch = 75f;
    public float pitchSensitivity = 100f;

    private float yaw = 0f;
    public static int CutawayHeight = 10;


    void Update()
    {
        if (focusTarget == null) return;

        HandleMovement();
        HandleRotation();
        HandleZoom();
        UpdateCameraPosition();
        HandlePitchAdjustment();
    }

    void HandleMovement()
    {
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) direction += forward;
        if (Input.GetKey(KeyCode.S)) direction -= forward;
        if (Input.GetKey(KeyCode.D)) direction += right;
        if (Input.GetKey(KeyCode.A)) direction -= right;

        focusTarget.position += direction * moveSpeed * Time.deltaTime;
    }

    void HandleRotation()
    {
        if (Input.GetKey(KeyCode.Q)) yaw -= rotationSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) yaw += rotationSpeed * Time.deltaTime;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentDistance -= scroll * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);  // fixed vertical angle, adjustable yaw
        Vector3 offset = rotation * new Vector3(0f, 0f, -currentDistance);
        transform.position = focusTarget.position + offset;
        transform.LookAt(focusTarget.position);
    }
    void HandlePitchAdjustment()
    {
        if (Input.GetMouseButton(1)) // right mouse button held
        {
            float deltaX = Input.GetAxis("Mouse X");
            float deltaY = Input.GetAxis("Mouse Y");

            yaw += deltaX * pitchSensitivity * Time.deltaTime;
            pitch -= deltaY * pitchSensitivity * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
    }
    public CameraSaveData GetCameraStateData()
    {
        //CameraSaveSystem.SaveCamera(transform, currentDistance, CutawayHeight);
        CameraSaveData data = new CameraSaveData(focusTarget.transform, transform.rotation, currentDistance, CutawayHeight);
        data.pitch = pitch;
        data.yaw = yaw;
        string json = JsonUtility.ToJson(data, true);

        return data;
    }

    public void LoadCameraState(CameraSaveData data)
    {
        if (data == null) return;

        focusTarget.transform.position = data.position;
        transform.rotation = data.rotation;
        currentDistance = data.distance;
        CutawayHeight = data.cutawayHeight;
        pitch = data.pitch;
        yaw = data.yaw;
    }

    public void ResetValues()
    {

        currentDistance = 15f;
        pitch = 30f;
        yaw = 0f;
        focusTarget.position = originalPosition;
    }
}
