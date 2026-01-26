using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement playerMovement;
    
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private bool invertY = false;
    
    [Header("Third Person Settings")]
    [SerializeField] private float distanceFromPlayer = 5f;
    [SerializeField] private float cameraHeight = 2f;
    [SerializeField] private float cameraSmoothSpeed = 10f;
    
    [Header("First Person Settings")]
    [SerializeField] private Vector3 firstPersonPosition = new Vector3(0, 1.7f, 0.2f);
    
    [Header("Head Bob Settings")]
    [SerializeField] private bool enableHeadBob = true;
    [SerializeField] private float walkBobSpeed = 10f;
    [SerializeField] private float runBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float runBobAmount = 0.1f;
    [SerializeField] private float bobSmoothing = 10f;
    [SerializeField] private float moveThreshold = 0.1f; // Limite mínimo para considerar movimento
    
    [Header("Camera Tilt (Walking)")]
    [SerializeField] private bool enableCameraTilt = true;
    [SerializeField] private float tiltAmount = 2f;
    [SerializeField] private float tiltSpeed = 4f;
    
    private float mouseX, mouseY;
    private bool isFirstPerson = true;
    private Vector3 cameraOffset;
    
    // Variáveis para Head Bob
    private float defaultYPos;
    private float timer = 0f;
    private float currentBobSpeed;
    private float currentBobAmount;
    private Vector3 initialCameraPosition;
    private float bobResetTimer = 0f;
    private const float bobResetTime = 0.5f; // Tempo para resetar o bob completamente
    
    // Variáveis para Camera Tilt
    private float tiltAngle = 0f;
    private float targetTiltAngle = 0f;
    
    // Controle de movimento
    private float lastHorizontalInput = 0f;
    private float lastVerticalInput = 0f;
    private bool wasMoving = false;
    private Vector3 lastPlayerPosition;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Configuração inicial automática se o player não for atribuído
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("Player encontrado automaticamente pela tag 'Player'");
            }
            else
            {
                Debug.LogError("Nenhum Player encontrado! Atribua manualmente no Inspector.");
            }
        }
        
        // Tenta encontrar o PlayerMovement automaticamente
        if (playerMovement == null && player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }
        
        cameraOffset = new Vector3(0, cameraHeight, -distanceFromPlayer);
        
        // Guarda a posição inicial da câmera
        initialCameraPosition = transform.localPosition;
        defaultYPos = isFirstPerson ? firstPersonPosition.y : initialCameraPosition.y;
        
        // Inicializa a última posição do player
        if (player != null)
        {
            lastPlayerPosition = player.position;
        }
    }
    
    void Update()
    {
        // Trocar entre primeira e terceira pessoa (Tecla V)
        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log("Modo câmera: " + (isFirstPerson ? "Primeira Pessoa" : "Terceira Pessoa"));
            
            // Atualiza a posição default para head bob
            defaultYPos = isFirstPerson ? firstPersonPosition.y : cameraHeight;
        }
        
        // Captura do mouse
        mouseX += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        
        if (invertY)
        {
            mouseY += Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        }
        else
        {
            mouseY -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        }
        
        // Limita a rotação vertical
        mouseY = Mathf.Clamp(mouseY, -80f, 80f);
        
        // Atualiza os parâmetros do head bob
        UpdateHeadBobParameters();
        
        // Atualiza o tilt da câmera
        UpdateCameraTilt();
    }
    
    void LateUpdate()
    {
        if (player == null) return;
        
        if (isFirstPerson)
        {
            HandleFirstPerson();
        }
        else
        {
            HandleThirdPerson();
        }
        
        // Aplica o head bob apenas se estiver habilitado e em primeira pessoa
        if (enableHeadBob && isFirstPerson)
        {
            ApplyHeadBob();
        }
        
        // Aplica o tilt da câmera
        if (enableCameraTilt && isFirstPerson)
        {
            ApplyCameraTilt();
        }
    }
    
    void HandleFirstPerson()
    {
        // Posição base da câmera (na "cabeça" do player)
        Vector3 basePosition = player.position + 
                              player.TransformDirection(firstPersonPosition);
        
        // Rotação da câmera
        Quaternion cameraRotation = Quaternion.Euler(mouseY, mouseX, 0);
        
        // Aplica posição e rotação
        transform.position = basePosition;
        transform.rotation = cameraRotation;
        
        // Player rotaciona apenas horizontalmente
        player.rotation = Quaternion.Euler(0, mouseX, 0);
    }
    
    void HandleThirdPerson()
    {
        // Calcula a rotação da câmera
        Quaternion rotation = Quaternion.Euler(mouseY, mouseX, 0);
        
        // Calcula a posição desejada da câmera
        Vector3 desiredPosition = player.position + rotation * cameraOffset;
        
        // Raycast para evitar paredes
        RaycastHit hit;
        Vector3 direction = desiredPosition - player.position;
        
        if (Physics.Raycast(player.position, direction.normalized, out hit, direction.magnitude))
        {
            desiredPosition = hit.point - direction.normalized * 0.3f;
        }
        
        // Suaviza o movimento da câmera
        transform.position = Vector3.Lerp(transform.position, desiredPosition, 
            cameraSmoothSpeed * Time.deltaTime);
        
        // Faz a câmera olhar para o player
        Vector3 lookDirection = player.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 
            cameraSmoothSpeed * Time.deltaTime);
        
        // Player rotaciona na direção da câmera (horizontalmente)
        Vector3 playerLookDirection = new Vector3(lookDirection.x, 0, lookDirection.z);
        if (playerLookDirection.magnitude > 0.1f)
        {
            Quaternion playerRotation = Quaternion.LookRotation(playerLookDirection);
            player.rotation = Quaternion.Slerp(player.rotation, playerRotation, 
                5f * Time.deltaTime);
        }
    }
    
    void UpdateHeadBobParameters()
    {
        // Obtém os inputs atuais
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        // Calcula a magnitude do input com deadzone
        float inputMagnitude = new Vector2(horizontalInput, verticalInput).magnitude;
        
        // Verifica se está se movendo (considerando o threshold)
        bool isMoving = inputMagnitude > moveThreshold;
        
        // Verifica se está correndo
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        
        // Atualiza parâmetros do head bob
        if (isMoving)
        {
            currentBobSpeed = isRunning ? runBobSpeed : walkBobSpeed;
            currentBobAmount = isRunning ? runBobAmount : walkBobAmount;
            wasMoving = true;
            bobResetTimer = 0f; // Reseta o timer de reset
        }
        else
        {
            // Se estava movendo e parou, inicia o reset suave
            if (wasMoving)
            {
                bobResetTimer += Time.deltaTime;
                float resetProgress = Mathf.Clamp01(bobResetTimer / bobResetTime);
                
                // Reduz gradualmente o bob amount
                currentBobAmount = Mathf.Lerp(currentBobAmount, 0f, resetProgress * 5f * Time.deltaTime);
                
                if (bobResetTimer >= bobResetTime)
                {
                    wasMoving = false;
                    currentBobAmount = 0f;
                    currentBobSpeed = walkBobSpeed;
                    timer = 0f; // Reseta o timer completamente
                }
            }
            else
            {
                currentBobAmount = 0f;
                currentBobSpeed = walkBobSpeed;
            }
        }
        
        // Atualiza os últimos inputs
        lastHorizontalInput = horizontalInput;
        lastVerticalInput = verticalInput;
    }
    
    void ApplyHeadBob()
    {
        if (currentBobAmount > 0.001f) // Threshold muito baixo para evitar micro-movimentos
        {
            // Calcula o movimento do head bob
            float waveSlice = Mathf.Sin(timer);
            
            // Incrementa o timer baseado na velocidade
            timer += currentBobSpeed * Time.deltaTime;
            
            // Reseta o timer se passar de 2π
            if (timer > Mathf.PI * 2)
            {
                timer -= Mathf.PI * 2;
            }
            
            // Calcula a magnitude total do movimento (com suavização)
            float totalAxes = Mathf.Abs(lastHorizontalInput) + Mathf.Abs(lastVerticalInput);
            totalAxes = Mathf.Clamp(totalAxes, 0, 1);
            
            // Aplica uma curva de suavização para evitar início/parada brusca
            float smoothFactor = Mathf.SmoothStep(0, 1, totalAxes);
            
            // Calcula a mudança de posição
            float verticalBob = waveSlice * currentBobAmount * smoothFactor;
            float horizontalBob = Mathf.Cos(timer * 0.5f) * currentBobAmount * 0.3f * smoothFactor;
            
            // Aplica o head bob à posição local da câmera
            Vector3 bobOffset = new Vector3(horizontalBob, verticalBob, 0);
            
            // Aplica suavemente
            transform.localPosition = Vector3.Lerp(
                transform.localPosition, 
                initialCameraPosition + bobOffset, 
                bobSmoothing * Time.deltaTime
            );
        }
        else
        {
            // Retorna suavemente à posição original
            transform.localPosition = Vector3.Lerp(
                transform.localPosition, 
                initialCameraPosition, 
                bobSmoothing * Time.deltaTime
            );
            
            // Reseta o timer quando completamente parado
            if (!wasMoving)
            {
                timer = 0f;
            }
        }
    }
    
    void UpdateCameraTilt()
    {
        // Calcula o tilt baseado no movimento horizontal
        float horizontalInput = Input.GetAxis("Horizontal");
        
        // Aplica um deadzone para evitar tilt com inputs mínimos
        if (Mathf.Abs(horizontalInput) > 0.2f)
        {
            targetTiltAngle = -horizontalInput * tiltAmount;
        }
        else
        {
            targetTiltAngle = 0f;
        }
    }
    
    void ApplyCameraTilt()
    {
        // Suaviza o tilt
        tiltAngle = Mathf.Lerp(tiltAngle, targetTiltAngle, tiltSpeed * Time.deltaTime);
        
        // Aplica o tilt apenas se houver movimento significativo
        if (Mathf.Abs(tiltAngle) > 0.01f || Mathf.Abs(targetTiltAngle) > 0.01f)
        {
            Quaternion currentRotation = transform.localRotation;
            Vector3 eulerRotation = currentRotation.eulerAngles;
            eulerRotation.z = tiltAngle;
            transform.localRotation = Quaternion.Euler(eulerRotation);
        }
        else
        {
            // Retorna suavemente à rotação neutra
            Quaternion currentRotation = transform.localRotation;
            Vector3 eulerRotation = currentRotation.eulerAngles;
            eulerRotation.z = Mathf.Lerp(eulerRotation.z, 0f, tiltSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(eulerRotation);
        }
    }
    
    // Método para debug (opcional)
    void OnGUI()
    {
        if (enableHeadBob)
        {
            GUI.Label(new Rect(10, 100, 300, 20), $"Head Bob Amount: {currentBobAmount:F4}");
            GUI.Label(new Rect(10, 120, 300, 20), $"Timer: {timer:F4}");
            GUI.Label(new Rect(10, 140, 300, 20), $"Was Moving: {wasMoving}");
            GUI.Label(new Rect(10, 160, 300, 20), $"Bob Reset Timer: {bobResetTimer:F2}");
        }
    }
}