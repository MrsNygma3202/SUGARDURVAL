using DialogueEditor;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;
    
    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CameraController cameraController;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("Animation Settings")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private float animationSmoothTime = 0.1f;
    
    [Header("Footstep Sounds")]
    [SerializeField] private AudioClip[] walkFootsteps;
    [SerializeField] private AudioClip[] runFootsteps;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;
    [SerializeField] private float stepVolume = 0.5f;

    [Header("Jump Animation")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private float jumpAnimationSpeed = 1.5f;

    public NPCConversation DialogoNPC;
    public bool hasNPC;
    
    private CharacterController controller;
    private AudioSource audioSource;
    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded = true;
    private float currentSpeed;
    private float stepTimer;
    private bool isMoving;
    
    // Parâmetros do Animator
    private int isMovingHash;
    private int isRunningHash;
    private int isJumpingHash;
    private int isFallingHash;
    private int speedHash;
    
    // Variáveis de controle de animação
    private float currentAnimationSpeed = 0f;
    private float animationVelocity = 0f;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        currentSpeed = walkSpeed;
        
        // Configuração do AudioSource
        audioSource.volume = stepVolume;
        audioSource.spatialBlend = 1f;
        
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // Tenta encontrar o CameraController automaticamente
        if (cameraController == null && cameraTransform != null)
        {
            cameraController = cameraTransform.GetComponent<CameraController>();
        }
        
        // Inicializa o Animator
        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }
        
        // Cache dos hashes dos parâmetros do Animator
        if (playerAnimator != null)
        {
            isMovingHash = Animator.StringToHash("IsMoving");
            isRunningHash = Animator.StringToHash("IsRunning");
            isJumpingHash = Animator.StringToHash("IsJumping");
            isFallingHash = Animator.StringToHash("IsFalling");
            speedHash = Animator.StringToHash("Speed");
            playerAnimator.SetFloat("JumpSpeed", jumpAnimationSpeed);
        }
    }
    
    void Update()
    {
        // Iniciar Dialogo
        if (Input.GetKeyDown(KeyCode.E) && hasNPC)
        {
            ConversationManager.Instance.StartConversation(DialogoNPC);
            hasNPC = false;
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        // Verifica se está no chão
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        // Detecta se o personagem acabou de tocar o chão após uma queda
        if (isGrounded && !wasGrounded)
        {
            OnLand();
        }
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        // Movimento horizontal
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        // Verifica se o jogador está se movendo
        isMoving = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f) && isGrounded;
        
        // Calcula a direção do movimento baseado na câmera
        Vector3 move = Vector3.zero;
        
        if (cameraTransform != null)
        {
            // Em terceira pessoa, move baseado na direção da câmera
            if (cameraController != null && !cameraController.IsFirstPerson)
            {
                // Faz o personagem olhar na direção da câmera em terceira pessoa
                RotateTowardsCamera();
                
                // Movimento relativo à rotação atual do personagem
                move = transform.right * x + transform.forward * z;
            }
            else
            {
                // Em primeira pessoa, movimento relativo à câmera
                move = cameraTransform.right * x + cameraTransform.forward * z;
            }
        }
        else
        {
            move = transform.right * x + transform.forward * z;
        }
        
        move.y = 0f;
        
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }
        
        // Correr
        if (Input.GetKey(KeyCode.LeftShift) && z > 0) // Só corre para frente
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }
        
        // Move o personagem
        controller.Move(move * currentSpeed * Time.deltaTime);
        
        // Atualiza animações
        UpdateAnimations(x, z);
        
        // Gerenciar sons de passos
        HandleFootstepSounds();
        
        // Pular
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            OnJump();
        }
        
        // Atualiza estado de queda (para animação)
        UpdateFallingState();
        
        // Gravidade
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        // Atualiza estado anterior
        wasGrounded = isGrounded;
    }
    
    void RotateTowardsCamera()
    {
        if (cameraTransform == null || cameraController == null) return;
        
        // Faz o personagem olhar na mesma direção que a câmera (apenas horizontal)
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        
        if (cameraForward.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
        }
    }
    
    void UpdateAnimations(float horizontalInput, float verticalInput)
    {
        if (playerAnimator == null) return;
        
        // Calcula a velocidade de movimento atual para suavização
        float targetSpeed = 0f;
        if (isMoving)
        {
            targetSpeed = (currentSpeed == runSpeed) ? 2f : 1f;
        }
        
        // Suaviza a transição da velocidade
        currentAnimationSpeed = Mathf.SmoothDamp(
            currentAnimationSpeed, 
            targetSpeed, 
            ref animationVelocity, 
            animationSmoothTime
        );
        
        // Define os parâmetros do Animator
        playerAnimator.SetFloat(speedHash, currentAnimationSpeed);
        playerAnimator.SetBool(isMovingHash, isMoving);
        playerAnimator.SetBool(isRunningHash, currentSpeed == runSpeed && isMoving);
        
        // Calcula a magnitude do movimento
        float moveMagnitude = new Vector2(horizontalInput, verticalInput).magnitude;
        playerAnimator.SetFloat("MoveMagnitude", moveMagnitude);
        
        // Define a direção para animação de strafe (opcional)
        playerAnimator.SetFloat("Horizontal", horizontalInput);
        playerAnimator.SetFloat("Vertical", verticalInput);
    }
    
    void UpdateFallingState()
    {
        if (playerAnimator == null) return;
        
        // Ativa animação de queda quando não está no chão e está descendo
        bool isFalling = !isGrounded && velocity.y < 0;
        playerAnimator.SetBool(isFallingHash, isFalling);
        
        // Atualiza parâmetro de velocidade vertical
        playerAnimator.SetFloat("VerticalVelocity", velocity.y);
    }
    
    void OnJump()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool(isJumpingHash, true);
            playerAnimator.SetTrigger("Jump");
        }
        
        // Toca som de pulo
        if (jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound, 0.7f);
        }
    }
    
    void OnLand()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool(isJumpingHash, false);
            playerAnimator.SetBool(isFallingHash, false);
            playerAnimator.SetTrigger("Land");
        }
        
        // Toca som de aterrissagem
        if (landSound != null)
        {
            audioSource.PlayOneShot(landSound, 0.5f);
        }
    }
    
    void HandleFootstepSounds()
    {
        if (!isMoving)
        {
            stepTimer = 0;
            return;
        }
        
        // Atualiza timer
        stepTimer -= Time.deltaTime;
        
        if (stepTimer <= 0)
        {
            PlayFootstepSound();
            
            // Define o próximo intervalo baseado na velocidade
            float stepInterval = (currentSpeed == runSpeed) ? runStepInterval : walkStepInterval;
            stepTimer = stepInterval;
        }
    }
    
    void PlayFootstepSound()
    {
        if (audioSource == null) return;
        
        AudioClip[] footstepClips = (currentSpeed == runSpeed) ? runFootsteps : walkFootsteps;
        
        if (footstepClips != null && footstepClips.Length > 0)
        {
            int randomIndex = Random.Range(0, footstepClips.Length);
            AudioClip selectedClip = footstepClips[randomIndex];
            
            if (selectedClip != null)
            {
                audioSource.clip = selectedClip;
                audioSource.Play();
            }
        }
    }
}