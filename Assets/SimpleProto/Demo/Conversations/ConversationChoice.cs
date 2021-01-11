using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleProto.Demo
{
    public class ConversationChoice : UIBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        
        public ConversationNode Data { get; private set; }

        public void Init(ConversationNode data, int index)
        {
            Data = data;
            _text.text = data.LocalizedText;
        }
    }
}