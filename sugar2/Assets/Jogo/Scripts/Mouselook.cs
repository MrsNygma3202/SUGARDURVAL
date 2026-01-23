using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2.0f;
    public float verticalClamp = 90.0f;

    [Header("Camera Reference")]
    public CameraController cameraController; // Referência pública

    private float verticalRotation = 0;
    private Transform playerBody;

    void Start()
    {
        playerBody = transform.parent;

        // Se não foi atribuído no Inspector, tenta encontrar
        if (cameraController == null)
        {
            cameraController = GetComponentInParent<CameraController>();

            if (cameraController == null)
            {
                Debug.LogError("CameraController não encontrado! Atribua manualmente no Inspector.");
                enabled = false;
                return;
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Agora usando a propriedade pública
        if (cameraController.IsFirstPerson)
        {
            RotateCamera();
        }

        // Opção para destravar o cursor (útil para debug)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Retravar o cursor
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void RotateCamera()
    {
        // Rotação horizontal do corpo do jogador
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        playerBody.Rotate(Vector3.up * mouseX);

        // Rotação vertical da câmera
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalClamp, verticalClamp);

        transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    // Método para ajustar sensibilidade em tempo real (útil para menu de opções)
    public void SetSensitivity(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;
    }
}