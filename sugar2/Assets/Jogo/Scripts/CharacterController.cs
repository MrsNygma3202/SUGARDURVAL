using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 6.0f;
    public float crouchSpeed = 1.5f;
    public float acceleration = 10.0f;
    public float deceleration = 15.0f;
    public float turnSpeed = 180.0f; // Para movimento estilo tank/resident evil
    public float airControl = 0.3f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.2f;
    public float gravity = -20.0f;
    public float terminalVelocity = -50.0f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1.0f;
    public float standingHeight = 2.0f;
    public float crouchTransitionSpeed = 10.0f;

    [Header("Step Settings")]
    public float stepOffset = 0.3f;
    public float slopeLimit = 45.0f;

    [Header("Camera Settings")]
    public CameraController cameraController;
    public Transform cameraTransform;

    [Header("Audio & Effects")]
    public AudioSource footstepAudio;
    public float footstepIntervalWalk = 0.5f;
    public float footstepIntervalRun = 0.3f;

    // Componentes
    private CharacterController controller;
    private Animator animator;

    // Variáveis de estado
    private Vector3 velocity;
    private Vector3 moveDirection;
    private float currentSpeed;
    private float targetSpeed;
    private bool isRunning = false;
    private bool isCrouching = false;
    private bool isGrounded = false;
    private bool wasGrounded = true;
    private float verticalVelocity;
    private float footstepTimer = 0;
    private float originalControllerRadius;

    // Input
    private Vector2 inputAxis;
    private bool jumpPressed;
    private bool runHeld;
    private bool crouchPressed;
    private bool crouchHeld;

    // Propriedades públicas
    public bool IsGrounded => isGrounded;
    public bool IsRunning => isRunning;
    public bool IsCrouching => isCrouching;
    public Vector3 Velocity => velocity;
    public float CurrentSpeed => currentSpeed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Configurações do CharacterController
        controller.stepOffset = stepOffset;
        controller.slopeLimit = slopeLimit;
        originalControllerRadius = controller.radius;

        // Encontra a câmera se não atribuída
        if (cameraController == null)
            cameraController = GetComponent<CameraController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // Posição inicial
        velocity = Vector3.zero;
        moveDirection = Vector3.zero;
    }

    void Update()
    {
        GetInput();
        HandleCrouch();
        CalculateMovement();
        HandleJump();
        ApplyGravity();
        ApplyMovement();
        HandleFootsteps();
        UpdateAnimator();

        // Debug info
        DebugDisplay();
    }

    void GetInput()
    {
        // Input básico
        inputAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Normaliza para movimento diagonal não ser mais rápido
        if (inputAxis.magnitude > 1)
            inputAxis.Normalize();

        // Botões
        jumpPressed = Input.GetButtonDown("Jump");
        runHeld = Input.GetKey(KeyCode.LeftShift);
        crouchPressed = Input.GetKeyDown(KeyCode.LeftControl);
        crouchHeld = Input.GetKey(KeyCode.LeftControl);
    }

    void HandleCrouch()
    {
        // Toggle crouch quando pressionado
        if (crouchPressed)
        {
            isCrouching = !isCrouching;
        }

        // Ou segurar para agachar
        // isCrouching = crouchHeld;

        // Ajusta altura do CharacterController
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        // Ajusta o centro do controller baseado na altura
        Vector3 center = controller.center;
        center.y = controller.height / 2;
        controller.center = center;

        // Ajusta raio quando agachado para não ficar preso em lugares baixos
        controller.radius = isCrouching ? originalControllerRadius * 0.8f : originalControllerRadius;
    }

    void CalculateMovement()
    {
        // Define velocidade alvo baseado no estado
        targetSpeed = isCrouching ? crouchSpeed : (runHeld && !isCrouching ? runSpeed : walkSpeed);

        // Se não há input, velocidade alvo é 0
        if (inputAxis.magnitude < 0.1f)
            targetSpeed = 0;

        // Suaviza a velocidade atual em direção à velocidade alvo
        float accelerationRate = (inputAxis.magnitude > 0.1f) ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelerationRate * Time.deltaTime);

        // Direção de movimento relativo à câmera
        Vector3 forward = Vector3.zero;
        Vector3 right = Vector3.zero;

        if (cameraController != null && cameraController.IsFirstPerson)
        {
            // Primeira pessoa: movimento relativo à rotação do jogador
            forward = transform.forward;
            right = transform.right;
        }
        else
        {
            // Terceira pessoa: movimento relativo à câmera
            if (cameraTransform != null)
            {
                forward = cameraTransform.forward;
                right = cameraTransform.right;
            }
            else
            {
                forward = transform.forward;
                right = transform.right;
            }
        }

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Calcula direção do movimento
        moveDirection = (forward * inputAxis.y + right * inputAxis.x).normalized;

        // Se houver movimento, gira o personagem na direção do movimento (para 3ª pessoa)
        if (!cameraController.IsFirstPerson && moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        // Aplica velocidade
        velocity = moveDirection * currentSpeed;

        // Controle reduzido no ar
        if (!isGrounded)
        {
            velocity *= airControl;
        }
    }

    void HandleJump()
    {
        if (isGrounded && jumpPressed && !isCrouching)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // Pequeno impulso para frente ao pular
            if (velocity.magnitude > 0)
            {
                velocity += transform.forward * 0.5f;
            }
        }
    }

    void ApplyGravity()
    {
        if (isGrounded && verticalVelocity < 0)
        {
            // Pequena força para baixo para manter no chão
            verticalVelocity = -2f;
        }
        else
        {
            // Aplica gravidade
            verticalVelocity += gravity * Time.deltaTime;

            // Limita velocidade terminal
            verticalVelocity = Mathf.Max(verticalVelocity, terminalVelocity);
        }

        // Aplica velocidade vertical
        velocity.y = verticalVelocity;
    }

    void ApplyMovement()
    {
        // Move o CharacterController
        CollisionFlags flags = controller.Move(velocity * Time.deltaTime);

        // Atualiza estado grounded
        wasGrounded = isGrounded;
        isGrounded = (flags & CollisionFlags.Below) != 0;

        // Se estava no ar e agora está no chão, aterrissou
        if (!wasGrounded && isGrounded)
        {
            OnLand();
        }
    }

    void OnLand()
    {
        // Efeitos de aterrissagem
        // Pode adicionar som, partículas, etc.

        // Limita velocidade vertical após aterrissar
        if (verticalVelocity < -10f)
        {
            // Aterrissagem pesada
            // cameraController.ShakeCamera(0.3f, 0.2f);
        }

        verticalVelocity = 0;
    }

    void HandleFootsteps()
    {
        // Só toca passos se estiver no chão e se movendo
        if (isGrounded && currentSpeed > 0.1f && moveDirection.magnitude > 0.1f)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0)
            {
                PlayFootstep();

                // Define próximo intervalo baseado na velocidade
                float interval = isRunning ? footstepIntervalRun : footstepIntervalWalk;
                footstepTimer = interval * (walkSpeed / currentSpeed); // Ajusta intervalo pela velocidade
            }
        }
        else
        {
            footstepTimer = 0;
        }
    }

    void PlayFootstep()
    {
        if (footstepAudio != null)
        {
            // Aleatoriza pitch para variação
            footstepAudio.pitch = Random.Range(0.8f, 1.2f);
            footstepAudio.Play();
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        // Parâmetros para animação
        float forwardSpeed = Vector3.Dot(velocity, transform.forward);
        float strafeSpeed = Vector3.Dot(velocity, transform.right);

        animator.SetFloat("Speed", currentSpeed);
        animator.SetFloat("Forward", forwardSpeed);
        animator.SetFloat("Strafe", strafeSpeed);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsRunning", isRunning && !isCrouching);

        // Trigger de pulo
        if (jumpPressed && isGrounded && !isCrouching)
        {
            animator.SetTrigger("Jump");
        }
    }

    void DebugDisplay()
    {
        // Apenas para debug - remova na build final
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.F3))
        {
            DebugGUI();
        }
#endif
    }

    void DebugGUI()
    {
        // Debug na tela
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 20;

        string debugText = $"Speed: {currentSpeed:F2}\n" +
                          $"Grounded: {isGrounded}\n" +
                          $"Crouching: {isCrouching}\n" +
                          $"Running: {isRunning}\n" +
                          $"Velocity: {velocity:F2}\n" +
                          $"Camera: {(cameraController.IsFirstPerson ? "1st Person" : "3rd Person")}";

        // Desenha na tela
        GUI.Label(new Rect(10, 10, 300, 200), debugText, style);
    }

    // Métodos públicos para controle externo
    public void SetMovementEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            velocity = Vector3.zero;
            currentSpeed = 0;
        }
    }

    public void Teleport(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;
    }

    public void AddForce(Vector3 force, ForceMode mode = ForceMode.Impulse)
    {
        switch (mode)
        {
            case ForceMode.Force:
                velocity += force / 10f; // Aproximação
                break;
            case ForceMode.Impulse:
                velocity += force;
                break;
            case ForceMode.VelocityChange:
                velocity = force;
                break;
            case ForceMode.Acceleration:
                velocity += force * Time.deltaTime;
                break;
        }
    }

    // Getters para outros sistemas
    public Vector3 GetMoveDirection()
    {
        return moveDirection;
    }

    public float GetTargetSpeed()
    {
        return targetSpeed;
    }
}