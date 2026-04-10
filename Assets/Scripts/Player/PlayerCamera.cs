using UnityEngine;
using Mirror;

public class PlayerCamera : NetworkBehaviour
{
    [Header("Referências")]
    public Transform CameraTarget;
    public Transform FPSTarget;

    [Header("TPS")]
    public float TPSDistance = 4f;
    public float TPSHeight = 1.5f;
    public float TPSCollisionRadius = 0.2f;
    public LayerMask TPSCollisionLayers;

    [Header("FPS")]
    public float FPSMinY = -80f;
    public float FPSMaxY = 80f;

    [Header("Geral")]
    public float Sensitivity = 3f;
    public float SmoothSpeed = 15f;

    private float rotX;
    private float rotY;
    private bool isFPS = false;
    private Camera PlayerCam;
    private PlayerMovement playerMovement;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    public override void OnStartLocalPlayer()
    {
        if (!isLocalPlayer) return;
        PlayerCam = Camera.main;
        rotY = transform.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.V)) isFPS = !isFPS;

        if (!InventoryUI.isOpen)
        {
            rotY += Input.GetAxis("Mouse X") * Sensitivity;
            rotX -= Input.GetAxis("Mouse Y") * Sensitivity;
            rotX = Mathf.Clamp(rotX, isFPS ? FPSMinY : -40f, isFPS ? FPSMaxY : 70f);
        }

        if (isFPS) HandleFPS();
        else HandleTPS();
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (InventoryUI.isOpen) return;
        RotatePlayer();
    }

    void HandleFPS()
    {
        PlayerCam.transform.position = FPSTarget.position;
        PlayerCam.transform.rotation = Quaternion.Euler(rotX, rotY, 0);
    }

    void HandleTPS()
    {
        Quaternion camRotation = Quaternion.Euler(rotX, rotY, 0);
        Vector3 camDirection = camRotation * Vector3.back;
        Vector3 idealPosition = CameraTarget.position + camDirection * TPSDistance + Vector3.up * TPSHeight;

        Vector3 finalPosition = idealPosition;
        if (Physics.SphereCast(CameraTarget.position, TPSCollisionRadius, camDirection, out RaycastHit hit, TPSDistance, TPSCollisionLayers))
        {
            finalPosition = CameraTarget.position + camDirection * (hit.distance - TPSCollisionRadius) + Vector3.up * TPSHeight;
        }

        PlayerCam.transform.position = Vector3.Lerp(PlayerCam.transform.position, finalPosition, Time.deltaTime * SmoothSpeed);
        PlayerCam.transform.LookAt(CameraTarget.position + Vector3.up * 0.5f);
    }

    void RotatePlayer()
    {
        if (isFPS)
        {
            transform.rotation = Quaternion.Euler(0, rotY, 0);
        }
        else
        {
            if (playerMovement.MoveX == 0 && playerMovement.MoveY == 0) return;
            float targetRotY = Mathf.MoveTowardsAngle(transform.eulerAngles.y, rotY, SmoothSpeed * 10f * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, targetRotY, 0);
        }
    }
}