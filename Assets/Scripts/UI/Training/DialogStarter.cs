using DialogueEditor;
using UnityEngine;

namespace UI.Training
{
    public class DialogStarter : MonoBehaviour
    {
        [SerializeField] private NPCConversation firstTable;

        private void Start()
        {
            ConversationManager.Instance.StartConversation(firstTable);
        }
    }
}
