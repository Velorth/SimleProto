using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SimpleProto;
using SimpleProtoEditor.Scripting;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;

namespace SimpleProtoEditor
{
    public sealed class ConversationEditor : EditorWindow
    {
        private static readonly GUIContent CreateNodeLabel = new GUIContent("Create", "Create new node in conversation");
        private static readonly GUIContent DeleteNodeLabel = new GUIContent("Delete", "Permanently delete selected node");
        private static readonly GUIContent CopyNodeLabel = new GUIContent("Copy", "Remember reference to selected node");
        private static readonly GUIContent MoveNodeLabel = new GUIContent("Move", "Attaches node in clipboard to selected node");
        private static readonly GUIContent LinkNodeLabel = new GUIContent("Link", "Links node in clipboard");
        private static readonly GUIContent MoveNodeUpLabel = new GUIContent("Up", "Move selected node up");
        private static readonly GUIContent MoveNodeDownLabel = new GUIContent("Down", "Move selected node down");
        private static readonly GUIContent NodeTextLabel = new GUIContent("Text", "This text will be displayed for character's phrase");
        private static readonly GUIContent NodeAnimationLabel = new GUIContent("Animation", "Animation trigger for the phrase");


        private static readonly string[] AnimationTriggers =
        {
            "", "GestureTalk", "GestureExplain", "GestureDesperation",
            "GestureShrugs", "GestureWarning", "GestureWaving", "GestureSayNo", "GestureThinking", "GestureAnger"
        };

        private static readonly GUIContent[] AnimationTriggerLabels =
        {
            new GUIContent("[None]"),
            new GUIContent("Talk"),
            new GUIContent("Explain"),
            new GUIContent("Desperation"),
            new GUIContent("Shrugs"),
            new GUIContent("Warning"),
            new GUIContent("Waving"),
            new GUIContent("SayNo"),
            new GUIContent("Thinking"),
            new GUIContent("Anger")
        };

        [SerializeField] private TreeViewState _treeState;
        [SerializeField] private Conversation _target;
        [SerializeField] private Vector2 _treeViewScrollPosition;

        private SerializedObject _serializedObject;
        private ConversationTreeView _treeView;
        private GUIStyle _textAreaStyle;

        [MenuItem("Window/Conversation Tree")]
        public static void ShowEditor()
        {
            GetWindow<ConversationEditor>();
        }

        private void OnEnable()
        {
            var iconTexture = (Texture)EditorGUIUtility.Load("Assets/SimpleProto/Editor Resources/Textures/conversation_icon.png");
            titleContent = new GUIContent("Conversation", iconTexture);
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            Repaint();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnUndoRedoPerformed()
        {
            if (_treeView != null)
            {
                _serializedObject.Update();

                _treeView.Reload();
            }
        }

        private void OnGUI()
        {
            _textAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };

            SelectTarget();

            if (_target == null)
                return;

            DrawConversationTree();

            _serializedObject.ApplyModifiedProperties();
        }

        private void DrawConversationTree()
        {
            var right = 300;
            var contentRect = new Rect(8, 8, position.width - 16, position.height - 16);
            var menuRect = contentRect.Top(24);
            DrawMenu(menuRect);

            // Conversation tree
            var treeRect = contentRect.Margin(0, right + 4, 24 + 8, 8);
            var fullTreeRect = new Rect(0, 0, treeRect.width, Mathf.Max(_treeView.totalHeight, treeRect.height));
            _treeViewScrollPosition = GUI.BeginScrollView(treeRect, _treeViewScrollPosition, fullTreeRect);
            _treeView.OnGUI(fullTreeRect);
            GUI.EndScrollView();

            var nodeRect = contentRect.Right(right);
            if (_treeView.SelectedItem != null &&
                _treeView.SelectedItem.Type != ConversationNodeType.Root &&
                _treeView.SelectedItem.Type != ConversationNodeType.BrokenLink)
            {
                DrawNode(nodeRect, _treeView.SelectedItem);
            }
        }

        private void DrawNode(Rect rect, ConversationTreeViewItem selection)
        {
            var contentRect = rect.Margin(0, 0, 24, 0);
            var line = contentRect.Top(EditorGUIUtility.singleLineHeight);

            //if (!selection.IsPlayerNode)
            //{
            //    line = line.NextVertical();
            //    EditorGUI.PropertyField(line, selection.Property.FindPropertyRelative("_overrideSpeaker"), new GUIContent("Actor"));
            //}

            //line = line.NextVertical();
            //EditorGUI.PropertyField(line, selection.Property.FindPropertyRelative("_overrideListener"), new GUIContent("Listener"));

            // Text
            line = line.NextVertical(EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(line, NodeTextLabel);
            line = line.NextVertical(EditorGUIUtility.singleLineHeight * 5);
            selection.Text = EditorGUI.TextArea(line, selection.Text, _textAreaStyle);

            // Animation
            line = line.NextVertical(EditorGUIUtility.singleLineHeight);
            var animationIndex = Array.IndexOf(AnimationTriggers, selection.Animation);
            animationIndex = EditorGUI.Popup(line, new GUIContent("Animation (Legacy)"), animationIndex, AnimationTriggerLabels);
            if (animationIndex >= 0)
            {
                selection.Animation = AnimationTriggers[animationIndex];
            }

            line = line.NextVertical(EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(line, selection.Property.FindPropertyRelative("_icon"), new GUIContent("Icon"));
            
            // Scripts
            var condition = selection.Property.FindPropertyRelative("_condition");
            var action = selection.Property.FindPropertyRelative("_action");
            line = line.NextVertical(EditorGUI.GetPropertyHeight(condition));
            EditorGUI.PropertyField(line, condition, new GUIContent("Condition"));
            line = line.NextVertical(EditorGUI.GetPropertyHeight(action));
            EditorGUI.PropertyField(line, action, new GUIContent("Action"));
        }

        private void DrawMenu(Rect rect)
        {
            const float buttonWidth = 64f;
            const float buttonSpacing = 2f;
            const float groupSpacing = 8f;

            var buttonRect = rect.Left(buttonWidth);
            GUI.enabled = _treeView.CanCreateNode(_treeView.SelectedItem);
            if (GUI.Button(buttonRect, CreateNodeLabel))
            {
                _treeView.ExecuteCreateNode(_treeView.SelectedItem);
            }

            buttonRect = buttonRect.NextHorizontal(buttonWidth, buttonSpacing);
            GUI.enabled = _treeView.CanDeleteNode(_treeView.SelectedItem);
            if (GUI.Button(buttonRect, DeleteNodeLabel))
            {
                _treeView.ExecuteDeleteNode(_treeView.SelectedItem);
            }

            buttonRect = buttonRect.NextHorizontal(buttonWidth, groupSpacing);
            GUI.enabled = _treeView.CanCopyNode(_treeView.SelectedItem);
            if (GUI.Button(buttonRect, CopyNodeLabel))
            {
                _treeView.ExecuteCopyNode(_treeView.SelectedItem);
            }

            buttonRect = buttonRect.NextHorizontal(buttonWidth, buttonSpacing);
            GUI.enabled = _treeView.CanMoveNode(_treeView.SelectedItem);
            if (GUI.Button(buttonRect, MoveNodeLabel))
            {
                _treeView.ExecuteMoveNode(_treeView.SelectedItem);
            }

            buttonRect = buttonRect.NextHorizontal(buttonWidth, buttonSpacing);
            GUI.enabled = _treeView.CanLinkNode(_treeView.SelectedItem);
            if (GUI.Button(buttonRect, LinkNodeLabel))
            {
                _treeView.ExecuteLinkNode(_treeView.SelectedItem);
            }

            buttonRect = buttonRect.NextHorizontal(buttonWidth, groupSpacing);
            GUI.enabled = _treeView.CanMoveNodeUp(_treeView.SelectedItem);
            if (GUI.Button(buttonRect, MoveNodeUpLabel))
            {
                _treeView.ExecuteMoveNodeUp(_treeView.SelectedItem);
            }

            buttonRect = buttonRect.NextHorizontal(buttonWidth, buttonSpacing);
            GUI.enabled = _treeView.CanMoveNodeDown(_treeView.SelectedItem);
            if (GUI.Button(buttonRect, MoveNodeDownLabel))
            {
                _treeView.ExecuteMoveNodeDown(_treeView.SelectedItem);
            }

            GUI.enabled = true;
        }

        private void SelectTarget()
        {
            // User can select Conversation directly
            var selectedAsset = Selection.activeObject as Conversation;
            
            // SerializedObject for previously selected conversation should be destroyed
            if (selectedAsset != null && selectedAsset != _target)
            {
                _target = selectedAsset;
                _target.EnsureRootNodeExists();
                
                _treeState = null;
                if (_serializedObject != null)
                {
                    _serializedObject.Dispose();
                    _serializedObject = null;
                }
            }

            if (_serializedObject == null && _target != null)
            {
                if (_treeState == null)
                {
                    _treeState = new TreeViewState();
                }

                _serializedObject = new SerializedObject(_target);
                _treeView = new ConversationTreeView(_treeState, _serializedObject);
            }
        }
    }

    internal sealed class ConversationTreeView : TreeView
    {
        private static class Properties
        {
            public const string Id = "_id";
            public const string ParentId = "_parentId";
            public const string IsPlayerNode = "_isPlayerNode";
            public const string Text = "_text";
            public const string LinkedGuids = "_linkedGuids";
            public const string AnimationTrigger = "_animationTrigger";
            public const string Condition = "_condition";
            public const string Action = "_action";
            public const string Icon = "_icon";
        }

        private SerializedObject _serializedObject;

        private Texture _playerIconTexture;
        private Texture _npcIconTexture;
        private Texture _linkTexture;
        private Texture _deadNodeTexture;
        private SerializedProperty _nodes;
        private int _nextNodeId;

        private ConversationTreeViewItem _clipboard;
        private string _selectedGuid;
        private string _selectedParentGuid;

        public ConversationTreeViewItem SelectedItem { get; private set; }
        public SerializedProperty SelectedProperty { get; private set; }

        public ConversationTreeView(TreeViewState state, SerializedObject serializedObject) : base(state)
        {
            _serializedObject = serializedObject;
            _nodes = _serializedObject.FindProperty("_nodes");

            showAlternatingRowBackgrounds = true;
            showBorder = true;

            _playerIconTexture = (Texture)EditorGUIUtility.Load("Assets/SimpleProto/Editor Resources/Textures/conversation_player.png");
            _npcIconTexture = (Texture)EditorGUIUtility.Load("Assets/SimpleProto/Editor Resources/Textures/conversation_npc.png");
            _linkTexture = (Texture)EditorGUIUtility.Load("Assets/SimpleProto/Editor Resources/Textures/conversation_link.png");
            _deadNodeTexture = (Texture)EditorGUIUtility.Load("Assets/SimpleProto/Editor Resources/Textures/conversation_dead.png");

            Reload();
        }

        protected override void DoubleClickedItem(int id)
        {
            var clickedItem = FindItem(id, rootItem) as ConversationTreeViewItem;
            if (clickedItem == null)
                return;

            if (clickedItem.Type == ConversationNodeType.Link)
            {
                var original = FindItem(item => item.Type == ConversationNodeType.Regular && item.Guid == clickedItem.Guid, rootItem);
                if (original != null)
                {
                    SetSelection(new[] { original.id }, TreeViewSelectionOptions.RevealAndFrame | TreeViewSelectionOptions.FireSelectionChanged);
                }
            }
        }

        [CanBeNull]
        private ConversationTreeViewItem FindItem(Predicate<ConversationTreeViewItem> predicate, TreeViewItem searchFrom)
        {
            var node = searchFrom as ConversationTreeViewItem;
            if (node != null && predicate(node))
                return node;

            if (searchFrom.children != null)
            {
                foreach (var child in searchFrom.children)
                {
                    var test = FindItem(predicate, child);
                    if (test != null)
                        return test;

                }
            }

            return null;
        }

        public bool CanMoveNodeUp(ConversationTreeViewItem selectedItem)
        {
            if (selectedItem == null)
                return false;

            var index = selectedItem.parent.children.IndexOf(selectedItem);
            return index > 0;
        }

        public void ExecuteMoveNodeUp(ConversationTreeViewItem selectedItem)
        {
            var index = selectedItem.parent.children.IndexOf(selectedItem);
            var parent = (ConversationTreeViewItem)selectedItem.parent;
            var linkedGuids = parent.Property.FindPropertyRelative(Properties.LinkedGuids);
            linkedGuids.MoveArrayElement(index, index - 1);

            Reload();
        }

        public bool CanMoveNodeDown(ConversationTreeViewItem selectedItem)
        {
            if (selectedItem == null)
                return false;

            var index = selectedItem.parent.children.IndexOf(selectedItem);
            return index < selectedItem.parent.children.Count - 1;
        }

        public void ExecuteMoveNodeDown(ConversationTreeViewItem selectedItem)
        {
            var index = selectedItem.parent.children.IndexOf(selectedItem);
            var parent = (ConversationTreeViewItem)selectedItem.parent;
            var linkedGuids = parent.Property.FindPropertyRelative(Properties.LinkedGuids);
            linkedGuids.MoveArrayElement(index, index + 1);

            Reload();
        }

        public bool CanCreateNode(ConversationTreeViewItem selectedItem)
        {
            return selectedItem != null &&
                   (selectedItem.Type == ConversationNodeType.Root ||
                    selectedItem.Type == ConversationNodeType.Regular);
        }

        public void ExecuteCreateNode(ConversationTreeViewItem selectedItem)
        {
            var index = _nodes.arraySize;
            _nodes.InsertArrayElementAtIndex(index);
            var newNode = _nodes.GetArrayElementAtIndex(index);

            // Set default properties
            var guid = Guid.NewGuid().ToString();
            newNode.FindPropertyRelative(Properties.Id).stringValue = guid;
            newNode.FindPropertyRelative(Properties.ParentId).stringValue = selectedItem.Guid;
            newNode.FindPropertyRelative(Properties.IsPlayerNode).boolValue = !selectedItem.IsPlayerNode;
            newNode.FindPropertyRelative(Properties.Text).stringValue = "Text";
            newNode.FindPropertyRelative(Properties.LinkedGuids).arraySize = 0;
            newNode.FindPropertyRelative(Properties.AnimationTrigger).stringValue = "";
            newNode.FindPropertyRelative(Properties.Icon).objectReferenceValue = null;
            ScriptPropertyDrawer.SetDefaultValues(newNode.FindPropertyRelative(Properties.Condition));
            ScriptPropertyDrawer.SetDefaultValues(newNode.FindPropertyRelative(Properties.Action));

            selectedItem.AddLink(guid);

            _serializedObject.ApplyModifiedProperties();

            _selectedParentGuid = selectedItem.Guid;
            _selectedGuid = guid;

            Reload();
        }

        public bool CanDeleteNode(ConversationTreeViewItem selectedItem)
        {
            return selectedItem != null && selectedItem.Type != ConversationNodeType.Root;
        }

        public void ExecuteDeleteNode(ConversationTreeViewItem selectedItem)
        {
            // Unlink selected item
            var parent = (ConversationTreeViewItem)selectedItem.parent;
            if (parent?.Property != null)
            {
                Remove(parent.Property.FindPropertyRelative(Properties.LinkedGuids), selectedItem.Guid);
            }

            if (selectedItem.Type == ConversationNodeType.Regular || selectedItem.Type == ConversationNodeType.Detached)
            {
                var guids = new HashSet<string>();
                for (var i = 0; i < _nodes.arraySize; ++i)
                    guids.Add(_nodes.GetArrayElementAtIndex(i).FindPropertyRelative(Properties.Id).stringValue);

                ExcludeUsedNodes(guids, 0);

                foreach (var guid in guids)
                {
                    var index = FindIndex(_nodes, item => item.FindPropertyRelative(Properties.Id).stringValue == guid);
                    if (index != -1)
                    {
                        _nodes.DeleteArrayElementAtIndex(index);
                    }
                }
            }

            _serializedObject.ApplyModifiedProperties();

            _selectedGuid = parent?.Guid;
            _selectedParentGuid = parent?.ParentGuid;

            Reload();
        }

        public bool CanCopyNode(ConversationTreeViewItem selectedItem)
        {
            return selectedItem != null && selectedItem.Type == ConversationNodeType.Regular;
        }

        public void ExecuteCopyNode(ConversationTreeViewItem selectedItem)
        {
            _clipboard = selectedItem;
        }

        public bool CanLinkNode(ConversationTreeViewItem selectedItem)
        {
            return selectedItem != null &&
                   _clipboard != null &&
                   selectedItem.Type == ConversationNodeType.Regular &&
                   selectedItem.IsPlayerNode != _clipboard.IsPlayerNode;
        }

        public void ExecuteLinkNode(ConversationTreeViewItem selectedItem)
        {
            Assert.IsNotNull(selectedItem);
            Assert.IsNotNull(_clipboard);

            selectedItem.AddLink(_clipboard.Guid);

            _serializedObject.ApplyModifiedProperties();

            _selectedGuid = _clipboard.Guid;
            _selectedParentGuid = selectedItem.Guid;

            Reload();
        }

        public bool CanMoveNode(ConversationTreeViewItem selectedItem)
        {
            return selectedItem != null &&
                   _clipboard != null &&
                   _clipboard.Type == ConversationNodeType.Regular &&
                   _clipboard.IsPlayerNode != selectedItem.IsPlayerNode;
        }

        public void ExecuteMoveNode(ConversationTreeViewItem selectedItem)
        {
            var parent = (ConversationTreeViewItem)_clipboard.parent;
            if (parent?.Property != null)
            {
                Remove(parent.Property.FindPropertyRelative(Properties.LinkedGuids), selectedItem.Guid);
            }

            selectedItem.AddLink(_clipboard.Guid);
            _clipboard.Property.FindPropertyRelative(Properties.ParentId).stringValue = selectedItem.Guid;

            _serializedObject.ApplyModifiedProperties();

            Reload();
        }

        private void ExcludeUsedNodes(HashSet<string> guids, int index)
        {
            var node = _nodes.GetArrayElementAtIndex(index);
            var guid = node.FindPropertyRelative(Properties.Id).stringValue;
            var linkedGuids = node.FindPropertyRelative(Properties.LinkedGuids);
            if (guids.Remove(guid))
            {
                for (var i = 0; i < linkedGuids.arraySize; ++i)
                {
                    var childIndex = FindIndex(_nodes,
                        item => item.FindPropertyRelative(Properties.Id).stringValue ==
                                linkedGuids.GetArrayElementAtIndex(i).stringValue);
                    if (childIndex != -1)
                    {
                        ExcludeUsedNodes(guids, childIndex);
                    }
                }
            }
        }

        private int FindIndex(SerializedProperty array, Predicate<SerializedProperty> predicate)
        {
            for (var i = 0; i < array.arraySize; ++i)
                if (predicate(array.GetArrayElementAtIndex(i)))
                    return i;
            return -1;
        }

        private static int FindIndex(SerializedProperty array, string value)
        {
            for (var i = 0; i < array.arraySize; ++i)
                if (array.GetArrayElementAtIndex(i).stringValue == value)
                    return i;
            return -1;
        }

        private static bool Remove(SerializedProperty array, string value)
        {
            var index = FindIndex(array, value);
            if (index == -1)
                return false;
            array.DeleteArrayElementAtIndex(index);
            return true;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            SelectedItem = selectedIds.Count == 0
                ? null
                : FindItem(selectedIds[0], rootItem) as ConversationTreeViewItem;

            SelectedProperty = SelectedItem == null ? null : SelectedItem.Property;

            _selectedGuid = SelectedItem?.Guid;
            _selectedParentGuid = (SelectedItem?.parent as ConversationTreeViewItem)?.Guid;
        }

        protected override TreeViewItem BuildRoot()
        {
            SelectedItem = null;
            SelectedProperty = null;

            var nodeGuids = new HashSet<string>();
            for (var i = 0; i < _nodes.arraySize; ++i)
                nodeGuids.Add(_nodes.GetArrayElementAtIndex(i).FindPropertyRelative("_id").stringValue);

            _nextNodeId = 0;

            var root = new ConversationTreeViewItem(ConversationNodeType.Root, null, _nextNodeId++, -1);
            root.AddChild(BuildNode(ConversationNodeType.Root, _nodes.GetArrayElementAtIndex(0), nodeGuids, 0));
            for (var i = 1; i < _nodes.arraySize; ++i)
            {
                var node = _nodes.GetArrayElementAtIndex(i);
                var guid = node.FindPropertyRelative("_id").stringValue;
                if (nodeGuids.Contains(guid))
                {
                    root.AddChild(BuildNode(ConversationNodeType.Detached, node, nodeGuids, 0));
                }
            }

            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var node = (ConversationTreeViewItem)args.item;
            var rect = args.rowRect.Margin(GetContentIndent(args.item), 0, 0, 0);
            if (node.Type == ConversationNodeType.Root)
            {
                EditorGUI.LabelField(rect, new GUIContent("Root"));
                return;
            }
            if (node.Property == null)
            {
                EditorGUI.LabelField(rect, new GUIContent("(None)"));
                return;
            }

            Texture icon;
            switch (node.Type)
            {
                case ConversationNodeType.Detached:
                    icon = _deadNodeTexture;
                    break;
                default:
                    icon = node.IsPlayerNode ? _playerIconTexture : _npcIconTexture;
                    break;
            }

            GUI.DrawTexture(rect.Left(24), icon, ScaleMode.ScaleToFit);
            EditorGUI.LabelField(rect.Margin(25, 49, 0, 0), node.Text);

            if (node.Type == ConversationNodeType.Link)
            {
                GUI.DrawTexture(rect.Right(16), _linkTexture);
                {

                }
            }
        }

        private ConversationTreeViewItem BuildNode(ConversationNodeType type, SerializedProperty node, HashSet<string> nodes, int depth)
        {
            var viewItem = new ConversationTreeViewItem(type, node, _nextNodeId++, depth);
            var nodeId = node.FindPropertyRelative(Properties.Id).stringValue;

            if (type != ConversationNodeType.Link)
            {
                nodes.Remove(nodeId);
            }

            var linkedGuids = node.FindPropertyRelative(Properties.LinkedGuids);
            for (var i = 0; i < linkedGuids.arraySize; ++i)
            {
                var childGuid = linkedGuids.GetArrayElementAtIndex(i);
                var childParentId = "";
                SerializedProperty child = null;
                for (var k = 0; k < _nodes.arraySize; ++k)
                    if (_nodes.GetArrayElementAtIndex(k).FindPropertyRelative(Properties.Id).stringValue ==
                        childGuid.stringValue)
                    {
                        child = _nodes.GetArrayElementAtIndex(k);
                        childParentId = child.FindPropertyRelative(Properties.ParentId).stringValue;
                    }

                ConversationTreeViewItem childItem;
                if (child == null)
                {
                    childItem = new ConversationTreeViewItem(ConversationNodeType.BrokenLink, null, _nextNodeId++, depth + 1);
                    viewItem.AddChild(childItem);
                    nodes.Remove(childGuid.stringValue);
                }
                else if (string.IsNullOrEmpty(childParentId) && !nodes.Contains(childGuid.stringValue) ||
                         !string.IsNullOrEmpty(childParentId) && nodeId != childParentId)
                {
                    childItem = new ConversationTreeViewItem(ConversationNodeType.Link, child, _nextNodeId++, depth + 1);
                    viewItem.AddChild(childItem);
                }
                else
                {
                    if (string.IsNullOrEmpty(childParentId))
                    {
                        child.FindPropertyRelative(Properties.ParentId).stringValue = nodeId;
                        _serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    }

                    childItem = BuildNode(ConversationNodeType.Regular, child, nodes, depth + 1);
                    viewItem.AddChild(childItem);
                }

                if (childItem.Guid == _selectedGuid && childItem.ParentGuid == _selectedParentGuid)
                {
                    SelectedItem = childItem;
                    SelectedProperty = childItem.Property;
                }
            }

            return viewItem;
        }
    }

    internal enum ConversationNodeType
    {
        Root,
        Regular,
        Link,
        BrokenLink,
        Detached
    }

    internal sealed class ConversationTreeViewItem : TreeViewItem
    {
        private readonly SerializedProperty _text;
        private SerializedProperty _isPlayerNode;
        private SerializedProperty _id;
        private SerializedProperty _linkedGuids;
        private SerializedProperty _animationTrigger;

        public ConversationNodeType Type { get; private set; }

        public string Guid
        {
            get { return _id?.stringValue; }
        }

        public string ParentGuid
        {
            get { return (parent as ConversationTreeViewItem)?.Guid; }
        }

        public string Text
        {
            get { return _text.stringValue; }
            set { _text.stringValue = value; }
        }

        public bool IsPlayerNode
        {
            get { return _isPlayerNode?.boolValue ?? false; }
        }

        public string Animation
        {
            get { return _animationTrigger.stringValue; }
            set { _animationTrigger.stringValue = value; }
        }

        public ConversationTreeViewItem(ConversationNodeType type, SerializedProperty property, int id, int depth) : base(id, depth, property == null ? "(None)" : property.FindPropertyRelative("_text").stringValue)
        {
            Type = type;
            Property = property;

            if (Property != null)
            {
                _text = Property.FindPropertyRelative("_text");
                _isPlayerNode = Property.FindPropertyRelative("_isPlayerNode");
                _id = Property.FindPropertyRelative("_id");
                _linkedGuids = Property.FindPropertyRelative("_linkedGuids");
                _animationTrigger = Property.FindPropertyRelative("_animationTrigger");
            }
        }

        public SerializedProperty Property { get; set; }

        public void AddLink(string guid)
        {
            var index = _linkedGuids.arraySize;
            _linkedGuids.InsertArrayElementAtIndex(index);
            _linkedGuids.GetArrayElementAtIndex(index).stringValue = guid;
        }

        public void RemoveLink(string guid)
        {
            for (var i = 0; i < _linkedGuids.arraySize; ++i)
            {
                if (_linkedGuids.GetArrayElementAtIndex(i).stringValue == guid)
                {
                    _linkedGuids.DeleteArrayElementAtIndex(i);
                    return;
                }
            }

        }
    }
}