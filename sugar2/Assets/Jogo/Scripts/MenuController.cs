using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    // Referências aos painéis do menu
    [Header("Referências UI")]
    [SerializeField] private GameObject pressAnyKeyScreen;
    [SerializeField] private GameObject mainMenuScreen;
    
    [Header("Botões")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    
    [Header("Configurações")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private float transitionDelay = 0.5f;
    
    // Controle de estado
    private bool isTransitioning = false;
    
    void Start()
    {
        // Configura estado inicial
        if (pressAnyKeyScreen != null)
            pressAnyKeyScreen.SetActive(true);
        
        if (mainMenuScreen != null)
            mainMenuScreen.SetActive(false);
        
        // Configura botões
        SetupButtons();
    }
    
    void Update()
    {
        // Detecta quando qualquer tecla é pressionada na tela inicial
        if (pressAnyKeyScreen.activeSelf && !isTransitioning && Input.anyKeyDown)
        {
            TransitionToMainMenu();
        }
    }
    
    void SetupButtons()
    {
        // Configura o botão Start
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
        }
        
        // Configura o botão Exit
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(ExitGame);
        }
    }
    
    void TransitionToMainMenu()
    {
        isTransitioning = true;
        
        // Desativa a tela de "Pressione qualquer tecla"
        if (pressAnyKeyScreen != null)
        {
            pressAnyKeyScreen.SetActive(false);
        }
        
        // Ativa o menu principal
        if (mainMenuScreen != null)
        {
            mainMenuScreen.SetActive(true);
        }
        
        // Opcional: Efeito de transição (pode adicionar animação aqui)
        Invoke("ResetTransition", transitionDelay);
    }
    
    void ResetTransition()
    {
        isTransitioning = false;
    }
    
    void StartGame()
    {
        if (isTransitioning) return;
        
        Debug.Log("Iniciando jogo...");
        
        // Carrega a cena do jogo
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogWarning("Nome da cena não definido! Configure no Inspector.");
            // Tenta carregar a próxima cena no build settings
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogError("Não há próxima cena configurada!");
            }
        }
    }
    
    void ExitGame()
    {
        if (isTransitioning) return;
        
        Debug.Log("Saindo do jogo...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // Métodos públicos para outras referências
    public void ShowPressAnyKeyScreen()
    {
        pressAnyKeyScreen.SetActive(true);
        mainMenuScreen.SetActive(false);
    }
    
    public void ShowMainMenuScreen()
    {
        pressAnyKeyScreen.SetActive(false);
        mainMenuScreen.SetActive(true);
    }
}