using UnityEngine;

public class EmpurrarInimigo : MonoBehaviour
{
    public float distanciaEmpurrao = 1.5f;
    public float forcaEmpurrao = 3f;
    public LayerMask layerInimigo;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TentarEmpurrar();
        }
    }

    void TentarEmpurrar()
    {
        RaycastHit hit;

        // Raycast para frente do jogador
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanciaEmpurrao, layerInimigo))
        {
            // Empurra usando Transform (sem Rigidbody)
            Vector3 direcao = hit.transform.position - transform.position;
            direcao.y = 0; // evita empurrar para cima ou para baixo
            direcao.Normalize();

            hit.transform.position += direcao * forcaEmpurrao;
        }
    }

    // Apenas para visualizar o alcance no editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * distanciaEmpurrao);
    }
}