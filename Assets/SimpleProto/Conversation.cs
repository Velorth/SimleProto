using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SimpleProto.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace SimpleProto
{
    [CreateAssetMenu(fileName = "Conversation", menuName = "Story/Conversation")]
    public class Conversation : DataAsset, ISerializationCallbackReceiver
    {
        [SerializeField, FormerlySerializedAs("Nodes"), HideInInspector] private List<ConversationNode> _nodes = new List<ConversationNode>();
        [NonSerialized] private readonly Dictionary<string, ConversationNode> _nodesMap = new Dictionary<string, ConversationNode>();

        /// <summary>
        /// Gets root node of the conversation.
        /// </summary>
        [NotNull]
        public ConversationNode Root
        {
            get { return Nodes[0]; }
        }

        [NotNull]
        public IReadOnlyList<ConversationNode> Nodes
        {
            get { return _nodes; }
        }

        /// <summary>
        /// Creates a new node attached to the given parent
        /// </summary>
        /// <returns>New conversation node.</returns>
        /// <param name="parent">Parent node.</param>
        [NotNull]
        public ConversationNode CreateNode([NotNull] ConversationNode parent)
        {
            var child = new ConversationNode
            {
                Id = Guid.NewGuid().ToString("N"),
                IsPlayerNode = !parent.IsPlayerNode
            };

            LinkNode(parent, child);

            return child;
        }

        public void LinkNode([NotNull]ConversationNode parent, [NotNull]ConversationNode child)
        {
            parent.AttachChild(child);
        }

        #region ISerializationCallbackReceiver

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (_nodes.Count == 0)
            {
                _nodes.Add(new ConversationNode
                {
                    Id = "Root",
                    Text = "[ROOT]",
                    IsPlayerNode = true
                });
            }

            _nodesMap.Clear();
            for (var i = 0; i < _nodes.Count; ++i)
            {
                var node = Nodes[i];
                _nodesMap[node.Id] = node;
            }

            for (var nodeIndex = 0; nodeIndex < _nodes.Count; ++nodeIndex)
            {
                var linkIndex = 0;
                var node = _nodes[nodeIndex];
                var children = (List<ConversationNode>)node.Children;
                children.Clear();
                while (linkIndex < node.LinkedGuids.Count)
                {
                    ConversationNode child;
                    if (!_nodesMap.TryGetValue(node.LinkedGuids[linkIndex], out child))
                    {
                        node.LinkedGuids.RemoveAt(linkIndex);
                    }
                    else
                    {
                        children.Add(child);
                        linkIndex++;
                    }
                }
            }
        }

        #endregion

        #if UNITY_EDITOR
        public void EnsureRootNodeExists()
        {
            if (_nodes.Count == 0)
            {
                _nodes.Add(new ConversationNode
                {
                    Id = "ROOT",
                    Text = "[ROOT]",
                    IsPlayerNode = true
                });
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
        #endif
    }
}