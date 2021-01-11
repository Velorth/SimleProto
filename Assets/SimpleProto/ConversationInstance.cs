using System.Collections.Generic;
using JetBrains.Annotations;

namespace SimpleProto
{
    public class ConversationInstance
    {
        private readonly List<ConversationNode> _answers = new List<ConversationNode>();

        public Conversation Asset { get; }

        /// <summary>
        /// Gets current phrase.
        /// </summary>
        [CanBeNull]
        public ConversationNode Phrase { get; private set; }
        
        /// <summary>
        /// Gets collection of available answers to the current phrase
        /// </summary>
        [NotNull]
        public IReadOnlyList<ConversationNode> Answers
        {
            get { return _answers; }
        }

        /// <summary>
        /// Gets if conversation is completed.
        /// </summary>
        public bool IsCompleted { get; private set; }

        public ConversationInstance(Conversation asset)
        {
            Asset = asset;
        }

        /// <summary>
        /// Starts conversation with with given characters.
        /// </summary>
        /// <param name="actor">Player character.</param>
        /// <param name="target">Speaker character.</param>
        public void Start()
        {
            AnswerWith(Asset.Root);
        }

        /// <summary>
        /// Continues conversation with given phrase.
        /// </summary>
        /// <param name="phrase">Phrase to answer.</param>
        public void AnswerWith([NotNull] ConversationNode phrase)
        {
            phrase.Activate();

            _answers.Clear();
            Phrase = null;

            for (var i = 0; i < phrase.Children.Count; ++i)
            {
                var node = phrase.Children[i];
                if (node.Check())
                {
                    Phrase = node;

                    node.Activate();

                    for (var j = 0; j < node.Children.Count; ++j)
                    {
                        var child = node.Children[j];
                        if (child.Check())
                        {
                            _answers.Add(child);
                        }
                    }
                    break;
                }
            }

            if (_answers.Count == 0 || Phrase == null)
                IsCompleted = true;
        }

        /// <summary>
        /// Interrupts convrsation immideately.
        /// </summary>
        public void End()
        {
            Phrase = null;
            _answers.Clear();

            IsCompleted = true;
        }
    }
}
