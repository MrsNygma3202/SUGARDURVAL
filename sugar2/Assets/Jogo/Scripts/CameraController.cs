using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;
    public bool startInFirstPerson = true;

    [Header("Head Bob Settings")]
    public bool enableHeadBob = true;
    public float bobFrequency = 1.5f;
    public float bobHorizontalAmplitude = 0.1f;
    public float bobVerticalAmplitude = 0.1f;
    public float bobSprintMultiplier = 1.5f;

    [Header("Third Person Settings")]
    public Vector3 thirdPersonOffset = new Vector3(0, 2, -3);
    public float cameraSmoothness = 5f;

    // Altere para public ou crie uma propriedade pública
    [Header("Debug")]
    [SerializeField] private bool _isFirstPerson = true;

    // Propriedade pública para acesso externo
    public bool IsFirstPerson
    {
        get { return _isFirstPerson; }
        private set { _isFirstPerson = value; }
    }

    // Variáveis privadas
    private float defaultYPos = 0;
    private float timer = 0;
    private CharacterController characterController;
    private Rigidbody rb;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        if (startInFirstPerson)
        {
            firstPersonCamera.gameObject.SetActive(true);
            thirdPersonCamera.gameObject.SetActive(false);
            IsFirstPerson = true;
        }
        else
        {
            firstPersonCamera.gameObject.SetActive(false);
            thirdPersonCamera.gameObject.SetActive(true);
            IsFirstPerson = false;
        }

        if (firstPersonCamera.transform.localPosition.y > 0)
            defaultYPos = firstPersonCamera.transform.localPosition.y;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchCamera();
        }

        if (enableHeadBob)
        {
            ApplyHeadBob();
        }

        if (!IsFirstPerson)
        {
            UpdateThirdPersonCamera();
        }
    }

    void ApplyHeadBob()
    {
        if (!IsFirstPerson) return;

        Camera currentCamera = firstPersonCamera;
        Vector3 cameraPosition = currentCamera.transform.localPosition;

        bool isMoving = false;
        float speed = 0;

        if (characterController != null)
        {
            isMoving = characterController.velocity.magnitude > 0.1f;
            speed = characterController.velocity.magnitude;
        }
        else if (rb != null)
        {
            isMoving = rb.linearVelocity.magnitude > 0.1f;
            speed = rb.linearVelocity.magnitude;
        }

        if (isMoving)
        {
            timer += Time.deltaTime * bobFrequency * (speed / 3);

            float horizontalBob = Mathf.Sin(timer * 2) * bobHorizontalAmplitude;
            float verticalBob = (Mathf.Sin(timer) + 1) * 0.5f * bobVerticalAmplitude;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                horizontalBob *= bobSprintMultiplier;
                verticalBob *= bobSprintMultiplier;
            }

            cameraPosition.x = horizontalBob;
            cameraPosition.y = defaultYPos + verticalBob;
        }
        else
        {
            timer = 0;
            cameraPosition.x = Mathf.Lerp(cameraPosition.x, 0, Time.deltaTime * 10);
            cameraPosition.y = Mathf.Lerp(cameraPosition.y, defaultYPos, Time.deltaTime * 10);
        }

        currentCamera.transform.localPosition = cameraPosition;
    }

    void UpdateThirdPersonCamera()
    {
        Vector3 targetPosition = transform.position +
                                transform.forward * thirdPersonOffset.z +
                                transform.up * thirdPersonOffset.y +
                                transform.right * thirdPersonOffset.x;

        thirdPersonCamera.transform.position = Vector3.Lerp(
            thirdPersonCamera.transform.position,
            targetPosition,
            Time.deltaTime * cameraSmoothness
        );

        Vector3 lookTarget = transform.position + Vector3.up * 1.5f;
        thirdPersonCamera.transform.LookAt(lookTarget);

        if (enableHeadBob)
        {
            ApplyThirdPersonBob();
        }
    }

    void ApplyThirdPersonBob()
    {
        bool isMoving = false;

        if (characterController != null)
            isMoving = characterController.velocity.magnitude > 0.1f;
        else if (rb != null)
            isMoving = rb.linearVelocity.magnitude > 0.1f;

        if (isMoving)
        {
            timer += Time.deltaTime * bobFrequency * 0.5f;
            float subtleBob = Mathf.Sin(timer) * 0.05f;

            Vector3 pos = thirdPersonCamera.transform.position;
            pos.y += subtleBob * Time.deltaTime;
            thirdPersonCamera.transform.position = pos;
        }
    }

    public void SwitchCamera()
    {
        IsFirstPerson = !IsFirstPerson;

        if (IsFirstPerson)
        {
            firstPersonCamera.gameObject.SetActive(true);
            thirdPersonCamera.gameObject.SetActive(false);
        }
        else
        {
            firstPersonCamera.gameObject.SetActive(false);
            thirdPersonCamera.gameObject.SetActive(true);
        }

        timer = 0;
    }

    public void SetCameraMode(bool firstPerson)
    {
        IsFirstPerson = firstPerson;
        SwitchCamera();
    }
}