using UnityEngine;
using UnityEngine.SceneManagement;

public class PassagemTrigger : MonoBehaviour
{
    [Header("Configurações")]
    public string playerTag = "Player";  // Tag do jogador
    public string sceneName = "RuaPerseguição";  // Nome da cena para carregar
    public bool useTrigger = true;  // Usar trigger em vez de colisão
    
    void OnTriggerEnter(Collider other)
    {
        if (useTrigger && other.CompareTag(playerTag))
        {
            LoadScene();
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!useTrigger && collision.gameObject.CompareTag(playerTag))
        {
            LoadScene();
        }
    }
    
    void LoadScene()
    {
        Debug.Log("Carregando cena: " + sceneName);
        
        // Carrega a cena
        SceneManager.LoadScene(sceneName);
    }
}