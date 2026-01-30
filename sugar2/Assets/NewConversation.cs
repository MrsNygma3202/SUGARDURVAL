using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DialogueEditor;

public class NewConversation : MonoBehaviour
{
    [SerializeField] private NPCConversation myConversation;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ConversationManager.Instance.StartConversation(myConversation);
            }
        }
    }
}
