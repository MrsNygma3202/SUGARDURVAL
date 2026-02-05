using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class WeepingAngel : MonoBehaviour
{
    public NavMeshAgent ai;
    public Transform player;
    private Vector3 dest;
    public Camera playercam, jumpscarecam;
    public float aiSpeed, catchDistance, jumpscareTime;
    public string sceneAfterDeath;

    void Update()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playercam);
        float distance = Vector3.Distance(transform.position, player.position);
        if (GeometryUtility.TestPlanesAABB(planes, this.gameObject.GetComponent<Renderer>().bounds))
        {
            ai.speed = 0;
            ai.SetDestination(transform.position);
        }

        if (!GeometryUtility.TestPlanesAABB(planes, this.gameObject.GetComponent<Renderer>().bounds))
        {
            ai.speed = aiSpeed;
            dest = player.position;
            ai.destination = dest;
            if (distance <= catchDistance)
            {
                player.gameObject.SetActive(false);
                jumpscarecam.gameObject.SetActive(true);
                StartCoroutine(killPlayer());
            }
        }
    }

    IEnumerator killPlayer()
    {
        yield return new WaitForSeconds(jumpscareTime);
        SceneManager.LoadScene(sceneAfterDeath);
    }
}
