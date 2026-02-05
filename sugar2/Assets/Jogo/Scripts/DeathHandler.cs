using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathHandler : MonoBehaviour
{
    [Header("Configurações de Morte")]
    public string gameOverScene = "GameOverScene";
    public string playerTag = "Player";
    
    [Header("Configurações do Cursor")]
    public bool unlockCursorOnDeath = true;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            HandlePlayerDeath(other.gameObject);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            HandlePlayerDeath(collision.gameObject);
        }
    }
    
    void HandlePlayerDeath(GameObject player)
    {
        Debug.Log("Jogador morreu! Carregando Game Over...");
        
        // 1. DESATIVA O CONTROLE DO JOGADOR
        DisablePlayerController(player);
        
        // 2. LIBERA O CURSOR (se configurado)
        if (unlockCursorOnDeath)
        {
            UnlockCursor();
        }
        
        // 3. CARREGA A CENA DE GAME OVER
        LoadGameOverScene();
    }
    
    void DisablePlayerController(GameObject player)
    {
        // Método 1: Desativa todos os scripts
        MonoBehaviour[] allScripts = player.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in allScripts)
        {
            if (script != this) // Não desativa este script
                script.enabled = false;
        }
        
        // Método alternativo: Desativa apenas o movimento
        // Procura por scripts comuns de controle
        if (player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>() != null)
        {
            player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = false;
        }
        
        // Desativa Rigidbody se existir
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        Debug.Log("Controles do jogador desativados.");
    }
    
    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Cursor liberado: " + Cursor.visible);
    }
    
    void LoadGameOverScene()
    {
        if (!string.IsNullOrEmpty(gameOverScene))
        {
            SceneManager.LoadScene(gameOverScene);
        }
        else
        {
            Debug.LogError("Nome da cena de Game Over não configurado!");
            // Carrega a próxima cena no build settings
            int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextScene < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextScene);
            }
        }
    }
}