using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform cameraHolder;
    
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private bool invertY = false;
    [SerializeField] private float verticalClamp = 80f;
    
    [Header("Third Person Settings")]
    [SerializeField] private float distanceFromPlayer = 5f;
    [SerializeField] private float cameraHeight = 1.5f;
    [SerializeField] private float cameraSmoothSpeed = 10f;
    [SerializeField] private float collisionOffset = 0.3f;
    
    [Header("First Person Settings")]
    [SerializeField] private Vector3 firstPersonOffset = new Vector3(0, 1.7f, 0.2f);
    
    private float mouseX, mouseY;
    private bool isFirstPerson = false; // Começa em terceira pessoa por padrão
    private Vector3 cameraOffset;
    private Vector3 targetCameraPosition;
    
    // Propriedade pública para acessar o modo da câmera
    public bool IsFirstPerson
    {
        get { return isFirstPerson; }
        set { isFirstPerson = value; }
    }
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Configuração inicial automática
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        // Se cameraHolder não foi atribuído, usa este transform
        if (cameraHolder == null)
        {
            cameraHolder = transform;
        }
        
        // Inicializa o offset da câmera em terceira pessoa
        cameraOffset = new Vector3(0, cameraHeight, -distanceFromPlayer);
        targetCameraPosition = player.position + cameraOffset;
    }
    
    void Update()
    {
        // Trocar entre primeira e terceira pessoa (Tecla V)
        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log("Modo câmera: " + (isFirstPerson ? "Primeira Pessoa" : "Terceira Pessoa"));
        }
        
        // Captura do mouse apenas se o cursor estiver travado
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            HandleMouseInput();
        }
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
    }
    
    void HandleMouseInput()
    {
        mouseX += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        
        // Inverte o eixo Y se necessário
        if (invertY)
        {
            mouseY += Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        }
        else
        {
            mouseY -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        }
        
        // Limita a rotação vertical
        mouseY = Mathf.Clamp(mouseY, -verticalClamp, verticalClamp);
    }
    
    void HandleFirstPerson()
    {
        // Posição da câmera (primeira pessoa) - relativa ao player
        Vector3 desiredPosition = player.position + 
                                 player.up * firstPersonOffset.y + 
                                 player.forward * firstPersonOffset.z;
        
        // Aplica posição diretamente
        cameraHolder.position = desiredPosition;
        
        // Rotação da câmera baseada no mouse
        Quaternion cameraRotation = Quaternion.Euler(mouseY, mouseX, 0);
        cameraHolder.rotation = cameraRotation;
        
        // Rotaciona o personagem apenas horizontalmente
        Vector3 playerRotation = player.eulerAngles;
        playerRotation.y = mouseX;
        player.eulerAngles = playerRotation;
    }
    
    void HandleThirdPerson()
    {
        // Calcula a rotação baseada no mouse
        Quaternion cameraRotation = Quaternion.Euler(mouseY, mouseX, 0);
        
        // Calcula a posição desejada da câmera
        Vector3 desiredPosition = player.position + cameraRotation * cameraOffset;
        
        // Verifica colisões para evitar que a câmera passe por paredes
        HandleCameraCollision(ref desiredPosition);
        
        // Suaviza o movimento da câmera
        targetCameraPosition = Vector3.Lerp(
            targetCameraPosition, 
            desiredPosition, 
            cameraSmoothSpeed * Time.deltaTime
        );
        
        cameraHolder.position = targetCameraPosition;
        
        // Faz a câmera olhar para o player (com um pouco de altura)
        Vector3 lookTarget = player.position + Vector3.up * cameraHeight * 0.5f;
        Vector3 lookDirection = lookTarget - cameraHolder.position;
        
        if (lookDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
            cameraHolder.rotation = Quaternion.Slerp(
                cameraHolder.rotation, 
                lookRotation, 
                cameraSmoothSpeed * Time.deltaTime
            );
        }
    }
    
    void HandleCameraCollision(ref Vector3 desiredPosition)
    {
        RaycastHit hit;
        Vector3 direction = desiredPosition - player.position;
        float distance = direction.magnitude;
        
        // Ignora colisão com o próprio player
        int layerMask = ~LayerMask.GetMask("Player", "Ignore Raycast");
        
        // Raycast da posição do player até a posição desejada da câmera
        if (Physics.Raycast(player.position, direction.normalized, out hit, distance, layerMask))
        {
            // Ajusta a posição da câmera para ficar um pouco antes do ponto de colisão
            desiredPosition = hit.point - direction.normalized * collisionOffset;
            
            // Garante que a câmera não fique muito perto do player
            float minDistance = 1.0f;
            float currentDistance = Vector3.Distance(player.position, desiredPosition);
            if (currentDistance < minDistance)
            {
                desiredPosition = player.position + direction.normalized * minDistance;
            }
        }
    }
    
    // Método para mudar o modo da câmera programaticamente
    public void SetCameraMode(bool firstPerson)
    {
        isFirstPerson = firstPerson;
    }
    
    // Método para obter a posição da câmera em primeira pessoa
    public Vector3 GetFirstPersonCameraPosition()
    {
        return firstPersonOffset;
    }
    
    // Método para obter a distância da câmera em terceira pessoa
    public float GetThirdPersonCameraDistance()
    {
        return distanceFromPlayer;
    }
    
    // Método para ajustar a sensibilidade do mouse
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = Mathf.Clamp(sensitivity, 10f, 500f);
    }
}