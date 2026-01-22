using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class Caminteract : MonoBehaviour
{

    public Text InteractionText;

    private float InteractDistance;

    public bool CanInteract = true;
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


        if (CanInteract == true)
        {
            Ray ray1 = new Ray(transform.position, transform.forward);
            RaycastHit hit1;

            if (Physics.Raycast(ray1, out hit1, InteractDistance))
            {
                if (hit1.collider.CompareTag("Maneq"))
                {
                    InteractionText.text = "Talk to him";

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        CanInteract = false;
                        StartCoroutine(TalktoManeq());

                    }
                    
                }
                else
                {
                    InteractionText.text = " ";
                }
            }
        }
        
        
    }

    IEnumerator TalktoManeq()
    {
        yield return new WaitForSeconds(2f); 
    }
    
}
