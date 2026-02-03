using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DialogueEditor;

public class NewConversation : MonoBehaviour
{
    public NPCConversation DialogoNPC;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

           other.GetComponent<PlayerMovement>().hasNPC = true;
           other.GetComponent<PlayerMovement>().DialogoNPC = DialogoNPC;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            other.GetComponent<PlayerMovement>().hasNPC = false;
            other.GetComponent<PlayerMovement>().DialogoNPC = null;
        }
    }
}
