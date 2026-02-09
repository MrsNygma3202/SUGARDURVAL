using UnityEngine;
using System.Collections;

public class EmpurrarInimigo : MonoBehaviour
{
    [Header("Configurações do Empurrão")]
    public float distanciaEmpurrao = 1.5f;
    public float forcaEmpurrao = 3f;
    public LayerMask layerInimigo;
    
    [Header("Configurações de Animação")]
    public Animator playerAnimator;
    public string parametroEmpurrando = "IsEmpurrando";
    public float duracaoAnimacaoEmpurrao = 0.5f;
    
    [Header("Configurações de Cooldown")]
    public float cooldownEmpurrao = 1f;
    private bool podeEmpurrar = true;
    
    [Header("Efeitos de Empurrão")]
    public AudioClip somEmpurrao;
    public float volumeSom = 0.7f;
    
    // Variáveis privadas
    private AudioSource audioSource;
    private float timerCooldown;
    private int hashEmpurrando;
    
    void Start()
    {
        // Tenta encontrar o Animator automaticamente se não foi atribuído
        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
            if (playerAnimator == null)
            {
                playerAnimator = GetComponentInChildren<Animator>();
            }
        }
        
        // Tenta encontrar o AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Cache do hash do parâmetro do Animator (melhor performance)
        hashEmpurrando = Animator.StringToHash(parametroEmpurrando);
        
        // Inicializa variáveis
        podeEmpurrar = true;
        timerCooldown = 0f;
    }
    
    void Update()
    {
        // Atualiza o cooldown
        if (!podeEmpurrar)
        {
            timerCooldown -= Time.deltaTime;
            if (timerCooldown <= 0)
            {
                podeEmpurrar = true;
                timerCooldown = 0;
            }
        }
        
        // Verifica se o jogador pode empurrar
        if (Input.GetKeyDown(KeyCode.F) && podeEmpurrar)
        {
            TentarEmpurrar();
        }
    }
    
    void TentarEmpurrar()
    {
        // Ativa a animação de empurrar
        AtivarAnimacaoEmpurrar();
        
        // Inicia o cooldown
        IniciarCooldown();
        
        // Toca o som do empurrão
        TocarSomEmpurrao();
        
        RaycastHit hit;
        
        // Raycast para frente do jogador
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanciaEmpurrao, layerInimigo))
        {
            AplicarEmpurrao(hit.transform);
        }
    }
    
    void AplicarEmpurrao(Transform inimigo)
    {
        // Calcula a direção do empurrão
        Vector3 direcao = inimigo.position - transform.position;
        direcao.y = 0; // Mantém no plano horizontal
        direcao.Normalize();
        
        // Tenta usar Rigidbody se disponível (melhor para física)
        Rigidbody rb = inimigo.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Empurra usando física
            rb.AddForce(direcao * forcaEmpurrao * 100f, ForceMode.Impulse);
            Debug.Log($"Empurrou inimigo com Rigidbody: {inimigo.name}");
        }
        else
        {
            // Empurra usando Transform (para objetos sem Rigidbody)
            inimigo.position += direcao * forcaEmpurrao;
            Debug.Log($"Empurrou inimigo sem Rigidbody: {inimigo.name}");
        }
        
        // Tenta ativar animação de reação no inimigo
        AtivarReacaoInimigo(inimigo);
    }
    
    void AtivarAnimacaoEmpurrar()
    {
        if (playerAnimator != null)
        {
            // Ativa o parâmetro de empurrar
            playerAnimator.SetBool(hashEmpurrando, true);
            
            // Desativa após a duração da animação
            Invoke(nameof(DesativarAnimacaoEmpurrar), duracaoAnimacaoEmpurrao);
        }
        else
        {
            Debug.LogWarning("Animator não encontrado para animação de empurrão!");
        }
    }
    
    void DesativarAnimacaoEmpurrar()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool(hashEmpurrando, false);
        }
    }
    
    void IniciarCooldown()
    {
        podeEmpurrar = false;
        timerCooldown = cooldownEmpurrao;
    }
    
    void TocarSomEmpurrao()
    {
        if (somEmpurrao != null && audioSource != null)
        {
            audioSource.PlayOneShot(somEmpurrao, volumeSom);
        }
    }
    
    void AtivarReacaoInimigo(Transform inimigo)
    {
        // Tenta encontrar um componente que controle a reação do inimigo
        Animator animatorInimigo = inimigo.GetComponent<Animator>();
        if (animatorInimigo != null)
        {
            // Verifica se o parâmetro existe antes de usá-lo
            if (HasParameter(animatorInimigo, "LevouDano"))
            {
                animatorInimigo.SetTrigger("LevouDano");
            }
            else if (HasParameter(animatorInimigo, "IsEmpurrado"))
            {
                animatorInimigo.SetBool("IsEmpurrado", true);
                // Desativa após um tempo
                StartCoroutine(DesativarReacaoInimigoCoroutine(animatorInimigo));
            }
        }
    }
    
    IEnumerator DesativarReacaoInimigoCoroutine(Animator animatorInimigo)
    {
        yield return new WaitForSeconds(1f);
        if (animatorInimigo != null && HasParameter(animatorInimigo, "IsEmpurrado"))
        {
            animatorInimigo.SetBool("IsEmpurrado", false);
        }
    }
    
    // Método auxiliar para verificar se um parâmetro existe no Animator
    bool HasParameter(Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
    
    // Método público para empurrar programaticamente
    public void Empurrar()
    {
        if (podeEmpurrar)
        {
            TentarEmpurrar();
        }
    }
    
    // Método para verificar se há inimigos no alcance
    public bool TemInimigoNoAlcance()
    {
        RaycastHit hit;
        return Physics.Raycast(transform.position, transform.forward, out hit, distanciaEmpurrao, layerInimigo);
    }
    
    // Método para ajustar a força do empurrão
    public void AjustarForcaEmpurrao(float novaForca)
    {
        forcaEmpurrao = Mathf.Max(0, novaForca);
    }
    
    // Método para ajustar a distância do empurrão
    public void AjustarDistanciaEmpurrao(float novaDistancia)
    {
        distanciaEmpurrao = Mathf.Max(0, novaDistancia);
    }
    
    // Método para verificar se pode empurrar
    public bool PodeEmpurrar()
    {
        return podeEmpurrar;
    }
    
    // Método para obter o tempo restante do cooldown
    public float GetCooldownRestante()
    {
        return Mathf.Max(0, timerCooldown);
    }
    
    // Apenas para visualizar o alcance no editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * distanciaEmpurrao);
        
        // Mostra área de alcance
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawCube(
            transform.position + transform.forward * (distanciaEmpurrao / 2),
            new Vector3(0.5f, 1f, distanciaEmpurrao)
        );
    }
}