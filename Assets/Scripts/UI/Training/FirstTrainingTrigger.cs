using System;
using System.Collections.Generic;
using DialogueEditor;
using UnityEngine;

namespace UI.Training
{
    public class FirstTrainingTrigger : MonoBehaviour
    {
        [SerializeField] private NPCConversation firstTraining;

        [SerializeField] private GameObject table;


        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            table.GetComponent<RectTransform>().eulerAngles = Vector3.zero;
            table.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(-603f, -21, 7.5f);
            table.SetActive(true);
            ConversationManager.Instance.StartConversation(firstTraining);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                table.SetActive(false);
            }
        }
    }
}
