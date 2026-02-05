using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialSystem : MonoBehaviour
{
    [System.Serializable]
    public class TutorialTrigger
    {
        public string triggerTag = "TutorialTrigger";
        public string tutorialText = "Use WASD para andar";
        public float displayDuration = 3f;
        public bool showOnlyOnce = true;
        [HideInInspector] public bool hasBeenShown = false;
    }

    [Header("Referências UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Text tutorialText;
    [SerializeField] private Image tutorialIcon;
    [SerializeField] private Animator tutorialAnimator;

    [Header("Configurações dos Triggers")]
    [SerializeField] private TutorialTrigger[] tutorialTriggers;

    [Header("Configurações Gerais")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private KeyCode hideKey = KeyCode.Tab;
    [SerializeField] private bool canManuallyHide = true;

    private Coroutine currentTutorialCoroutine;
    private bool isShowingTutorial = false;

    void Start()
    {
        // Inicializa o painel de tutorial como invisível
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
        
        // Se não tiver animator, usa transição simples
        if (tutorialAnimator == null)
            tutorialAnimator = tutorialPanel?.GetComponent<Animator>();
    }

    void Update()
    {
        // Permite esconder o tutorial manualmente
        if (canManuallyHide && isShowingTutorial && Input.GetKeyDown(hideKey))
        {
            HideTutorial();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        CheckTutorialTrigger(other.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        CheckTutorialTrigger(other.gameObject);
    }

    void CheckTutorialTrigger(GameObject triggerObject)
    {
        foreach (var trigger in tutorialTriggers)
        {
            if (triggerObject.CompareTag(trigger.triggerTag))
            {
                if (trigger.showOnlyOnce && trigger.hasBeenShown)
                    return;
                
                ShowTutorial(trigger.tutorialText, trigger.displayDuration);
                trigger.hasBeenShown = true;
                
                // Se o trigger deve desaparecer após ser tocado
                if (triggerObject.CompareTag("DestroyAfterTrigger"))
                    Destroy(triggerObject);
                    
                break; // Para após encontrar o trigger correspondente
            }
        }
    }

    public void ShowTutorial(string text, float duration = 3f)
    {
        // Cancela tutorial atual se estiver mostrando
        if (currentTutorialCoroutine != null)
        {
            StopCoroutine(currentTutorialCoroutine);
        }
        
        currentTutorialCoroutine = StartCoroutine(ShowTutorialCoroutine(text, duration));
    }

    private IEnumerator ShowTutorialCoroutine(string text, float duration)
    {
        isShowingTutorial = true;
        
        // Atualiza o texto
        if (tutorialText != null)
            tutorialText.text = text;
        
        // Mostra o painel
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            
            // Usa animação se disponível
            if (tutorialAnimator != null)
            {
                tutorialAnimator.SetTrigger("Show");
            }
            else
            {
                // Transição simples (alpha)
                StartCoroutine(FadePanel(0f, 1f, fadeInDuration));
            }
        }
        
        // Aguarda a duração especificada
        yield return new WaitForSeconds(duration);
        
        // Esconde o tutorial
        HideTutorial();
    }

    public void HideTutorial()
    {
        if (currentTutorialCoroutine != null)
        {
            StopCoroutine(currentTutorialCoroutine);
        }
        
        StartCoroutine(HideTutorialCoroutine());
    }

    private IEnumerator HideTutorialCoroutine()
    {
        if (tutorialAnimator != null)
        {
            tutorialAnimator.SetTrigger("Hide");
            yield return new WaitForSeconds(fadeOutDuration);
        }
        else
        {
            yield return StartCoroutine(FadePanel(1f, 0f, fadeOutDuration));
        }
        
        tutorialPanel.SetActive(false);
        isShowingTutorial = false;
    }

    private IEnumerator FadePanel(float startAlpha, float endAlpha, float duration)
    {
        CanvasGroup canvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = tutorialPanel.AddComponent<CanvasGroup>();
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
    }

    // Método para mostrar tutorial específico por índice
    public void ShowTutorialByIndex(int index)
    {
        if (index >= 0 && index < tutorialTriggers.Length)
        {
            ShowTutorial(tutorialTriggers[index].tutorialText, tutorialTriggers[index].displayDuration);
        }
    }

    // Método para resetar todos os tutoriais (útil para reiniciar o nível)
    public void ResetAllTutorials()
    {
        foreach (var trigger in tutorialTriggers)
        {
            trigger.hasBeenShown = false;
        }
    }
}

// Script para colocar nos objetos de trigger
public class TutorialTriggerObject : MonoBehaviour
{
    [SerializeField] private string tutorialMessage = "Use WASD para andar";
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private bool showOnlyOnce = true;
    [SerializeField] private bool destroyAfterTrigger = true;
    
    private bool hasBeenTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            TutorialSystem tutorialSystem = other.GetComponent<TutorialSystem>();
            if (tutorialSystem == null)
                tutorialSystem = FindObjectOfType<TutorialSystem>();
            
            if (tutorialSystem != null)
            {
                tutorialSystem.ShowTutorial(tutorialMessage, displayDuration);
                hasBeenTriggered = true;
                
                if (destroyAfterTrigger)
                    Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            TutorialSystem tutorialSystem = other.GetComponent<TutorialSystem>();
            if (tutorialSystem == null)
                tutorialSystem = FindObjectOfType<TutorialSystem>();
            
            if (tutorialSystem != null)
            {
                tutorialSystem.ShowTutorial(tutorialMessage, displayDuration);
                hasBeenTriggered = true;
                
                if (destroyAfterTrigger)
                    Destroy(gameObject);
            }
        }
    }
}