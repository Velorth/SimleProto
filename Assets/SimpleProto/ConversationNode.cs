using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SimpleProto.Expressions;
using SimpleProto.Scripting;
using UnityEngine;
using UnityEngine.Serialization;

// Not supported
// using SimpleProto.Localization;
// using SimpleProto.WorldObjects;

namespace SimpleProto
{
    [Serializable]
    public sealed class ConversationNode
    {
        [SerializeField, FormerlySerializedAs("Guid")] private string _id = "";
        [SerializeField] private string _parentId = "";
        [SerializeField] private bool _isPlayerNode;
        // Not supported
        //[SerializeField] private CharacterRef _overrideSpeaker;
        //[SerializeField] private CharacterRef _overrideListener;
        //[SerializeField] private CharacterAnimation _animation;
        [SerializeField] private string _animationTrigger;
        [SerializeField] private Sprite _icon;

        [Multiline, FormerlySerializedAs("Text")]
        [SerializeField] private string _text;
        
        [SerializeField] private Script _condition = new Script();
        
        [SerializeField] private Script _action = new Script();
        
        [SerializeField, FormerlySerializedAs("LinksGuids")] private List<string> _linkedGuids = new List<string>();

        [NonSerialized]
        private readonly List<ConversationNode> _children = new List<ConversationNode>();


        public string Id
        {
            get { return _id; }
            internal set { _id = value; }
        }

        // Not supported
        //public CharacterRef OverrideSpeaker
        //{
        //    get { return _overrideSpeaker; }
        //    set { _overrideSpeaker = value; }
        //}

        //public CharacterRef OverrideListener
        //{
        //    get => _overrideListener;
        //    set => _overrideListener = value;
        //}

        //public CharacterAnimation Animation
        //{
        //    get { return _animation; }
        //    set { _animation = value; }
        //}

        [Obsolete("Use Animation instead")]
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

        public string LocalizedText
        {
            get => Text; // { return EvaluationUtils.InterpolateString(Locale.Localize(Text)); }
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
                var checkResult = Condition.EvaluateBoolean();
                if (checkResult.HasValue)
                    return checkResult.Value;

                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
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
                Debug.LogException(e);
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