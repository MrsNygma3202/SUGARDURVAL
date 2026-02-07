using UnityEngine;
using UnityEngine.Events; // Adicione esta linha

public class LookAtTarget : MonoBehaviour
{
    [Header("Configurações da Câmera")]
    public Transform playerCamera;
    
    [Header("Configurações do Olhar")]
    public float viewAngle = 30f;
    public float maxDistance = 10f;

    [Header("Eventos de Áudio")]
    public UnityEvent onStartLooking; // Aparece no Inspector
    public UnityEvent onStopLooking;  // Aparece no Inspector

    private bool _isBeingLookedAt = false;

    void Update()
    {
        bool isLookingNow = CheckIfPlayerIsLooking();

        if (isLookingNow && !_isBeingLookedAt)
        {
            onStartLooking.Invoke(); // Dispara evento visível
        }
        else if (!isLookingNow && _isBeingLookedAt)
        {
            onStopLooking.Invoke(); // Dispara evento visível
        }

        _isBeingLookedAt = isLookingNow;
    }

    private bool CheckIfPlayerIsLooking()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main?.transform;
            if (playerCamera == null) return false;
        }

        Vector3 directionToTarget = transform.position - playerCamera.position;
        float distance = directionToTarget.magnitude;
        if (distance > maxDistance) return false;

        float angle = Vector3.Angle(playerCamera.forward, directionToTarget.normalized);
        if (angle > viewAngle) return false;

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, directionToTarget.normalized, out hit, maxDistance))
        {
            return hit.transform == transform;
        }

        return false;
    }
}