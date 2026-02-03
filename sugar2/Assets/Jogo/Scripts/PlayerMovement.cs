using DialogueEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityStandardAssets.Characters.FirstPerson;

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
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    
    [Header("Footstep Sounds")]
    [SerializeField] private AudioClip[] walkFootsteps;
    [SerializeField] private AudioClip[] runFootsteps;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;
    [SerializeField] private float stepVolume = 0.5f;

    public NPCConversation DialogoNPC; // temp
    public bool hasNPC;
    
    private CharacterController controller;
    private AudioSource audioSource;
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;
    private float stepTimer;
    private bool isMoving;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        currentSpeed = walkSpeed;
        
        // Configuração do AudioSource
        audioSource.volume = stepVolume;
        audioSource.spatialBlend = 1f; // Som 3D
        
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }
    
    void Update()
    {
        
        //Iniciar Dialogo
        if (Input.GetKeyDown(KeyCode.E) && hasNPC)
        {
            ConversationManager.Instance.StartConversation(DialogoNPC);
            hasNPC = false;
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        
        // Verifica se está no chão
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        // Movimento horizontal
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        // Verifica se o jogador está se movendo
        isMoving = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f) && isGrounded;
        
        Vector3 move = cameraTransform.right * x + cameraTransform.forward * z;
        move.y = 0f;
        
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }
        
        // Correr
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }
        
        controller.Move(move * currentSpeed * Time.deltaTime);
        
        // Rotaciona o personagem na direção do movimento
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
        
        // Gerenciar sons de passos
        HandleFootstepSounds();
        
        // Pular
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            // Toca som de pulo se quiser adicionar depois
        }
        
        // Gravidade
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
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
            // Escolhe um som aleatório do array
            int randomIndex = Random.Range(0, footstepClips.Length);
            AudioClip selectedClip = footstepClips[randomIndex];
            
            if (selectedClip != null)
            {
                audioSource.clip = selectedClip;
                audioSource.Play();
            }
        }
    }
    
    // Método para tocar um passo manualmente (útil se quiser sincronizar com animações)
    public void PlayFootstep()
    {
        PlayFootstepSound();
    }
    
    // Método público para verificar se está se movendo
    public bool IsMoving()
    {
        return isMoving;
    }
    
    // Método público para verificar se está correndo
    public bool IsRunning()
    {
        return currentSpeed == runSpeed;
    }
}