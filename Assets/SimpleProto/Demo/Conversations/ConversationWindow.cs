using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleProto.Demo
{
    public class ConversationWindow : UIBehaviour
    {
        [SerializeField] private Conversation _conversation;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private ConversationChoice _choiceTemplate;
        [SerializeField] private RectTransform _choicesHost;
        
        private ConversationInstance _conversationInstance;
        private List<ConversationChoice> _choices = new List<ConversationChoice>();

        protected override void OnEnable()
        {
            _conversationInstance = new ConversationInstance(_conversation);
            _conversationInstance.Start();
            
            _choiceTemplate.gameObject.SetActive(false);
            UpdateState();
        }
        

        private void UpdateState()
        {
            if (_conversationInstance.IsCompleted || _conversationInstance.Phrase == null)
            {
                return;
            }

            _text.text = _conversationInstance.Phrase.LocalizedText;

            UpdateAnswers();
        }

        private void UpdateAnswers()
        {
            int index;
            for (index = 0; index < _conversationInstance.Answers.Count; ++index)
            {
                CreateChoise(_conversationInstance.Answers[index], index);
            }

            for (; index < _choices.Count; index++)
                _choices[index].gameObject.SetActive(false);
        }

        private void CreateChoise(ConversationNode child, int index)
        {
            ConversationChoice phrase;
            if (_choices.Count > index)
            {
                phrase = _choices[index];
            }
            else
            {
                phrase = Instantiate(_choiceTemplate, _choicesHost);
                _choices.Add(phrase);

                var button = phrase.GetComponent<Button>();
                button.onClick.AddListener(() => OnChoiceClicked(phrase));
            }

            phrase.gameObject.SetActive(true);
            phrase.Init(child, index);
        }

        private void OnChoiceClicked(ConversationChoice phraseView)
        {
            _conversationInstance.AnswerWith(phraseView.Data);
            if (_conversationInstance.IsCompleted)
            {
                _text.text = "[Done]";
                foreach (var choice in _choices)
                    choice.gameObject.SetActive(false);
            }
            else
            {
                UpdateState();
            }
        }
    }
}
