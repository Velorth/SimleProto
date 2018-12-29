using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SimpleProto.Expressions;
using SimpleProto.Scripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace SimpleProto
{
    [Serializable]
    public sealed class ConversationNode
    {
        [SerializeField, FormerlySerializedAs("Guid")] private string _id = "";
        [SerializeField] private string _parentId = "";
        [SerializeField] private bool _isPlayerNode;
        [SerializeField] private string _animationTrigger;
        [SerializeField] private Sprite _icon;

        [Multiline, FormerlySerializedAs("Text")]
        [SerializeField] private string _text;

        [SerializeField] Script _condition = new Script();
        [SerializeField] Script _action = new Script();
        
        [SerializeField, FormerlySerializedAs("LinksGuids")] private List<string> _linkedGuids = new List<string>();

        [NonSerialized]
        private readonly List<ConversationNode> _children = new List<ConversationNode>();


        public string Id
        {
            get { return _id; }
            internal set { _id = value; }
        }


        public string AnimationTrigger
        {
            get { return _animationTrigger; }
            set { _animationTrigger = value; }
        }

        public Sprite Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }

        [NotNull]
        public Script Condition
        {
            get { return _condition; }
        }

        [NotNull]
        public Script Action
        {
            get { return _action; }
        }

        /// <summary>
        /// Gets localized text in the conversation node.
        /// </summary>
        /// TODO: Add localization module
        public string LocalizedText
        {
            get { return EvaluationUtils.InterpolateString(Text); }
        }

        public bool IsPlayerNode
        {
            get { return _isPlayerNode; }
            set { _isPlayerNode = value; }
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        /// <summary>
        /// Gets collection of all attached children nodes
        /// </summary>
        public IReadOnlyList<ConversationNode> Children
        {
            get { return _children; }
        }

        internal List<string> LinkedGuids
        {
            get { return _linkedGuids; }
        }

        internal string ParentId
        {
            get { return _parentId; }
            private set { _parentId = value; }
        }

        public bool Check()
        {
            try
            {
                var checkResult = Condition.Evaluate();
                if (checkResult is bool)
                    return (bool)checkResult;

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        public void Activate()
        {
            try
            {
                Action.Evaluate();
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        internal void AttachChild([NotNull] ConversationNode child)
        {
            if (string.IsNullOrEmpty(child.ParentId))
                child.ParentId = Id;

            LinkedGuids.Add(child.Id);
            _children.Add(child);
        }

        internal void DetachChild([NotNull] ConversationNode child)
        {
            if (child.ParentId == Id)
            {
                child.ParentId = "";
            }

            LinkedGuids.Remove(child.Id);
            _children.Remove(child);
        }
    }
}