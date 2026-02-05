using UnityEngine;
using System.Collections;

public class PlayerPush : MonoBehaviour
{
    public GameObject enemy;  // Arraste o inimigo aqui no Inspector
    public float pushDepth = 3.0f; // Quanto ele vai afundar
    public float speed = 2.0f; // Velocidade do movimento

    void Update()
    {
        // Pressione "F" para ativar
        if (Input.GetKeyDown(KeyCode.W) && enemy != null)
        {
            StopAllCoroutines(); // Para movimentos anteriores se houver
            StartCoroutine(SinkIntoGround());
        }
    }

    IEnumerator SinkIntoGround()
    {
        // Define o ponto de destino (posição atual menos a profundidade no eixo Y)
        Vector3 targetPosition = enemy.transform.position + Vector3.down * pushDepth;

        // Move o inimigo frame a frame até chegar no destino
        while (Vector3.Distance(enemy.transform.position, targetPosition) > 0.01f)
        {
            enemy.transform.position = Vector3.MoveTowards(
                enemy.transform.position, 
                targetPosition, 
                speed * Time.deltaTime
            );
            yield return null; // Espera o próximo frame
        }
    }
}
