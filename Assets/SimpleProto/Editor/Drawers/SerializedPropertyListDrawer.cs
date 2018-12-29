using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SimpleProtoEditor.Drawers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SerializedPropertyListDrawer
    {
        private readonly GUIContent _header;
        private readonly ReorderableList _list;

        public SerializedPropertyListDrawer(SerializedObject serializedObject, SerializedProperty elements) : 
            this(serializedObject, elements, new GUIContent(elements.displayName))
        {
        }

        public SerializedPropertyListDrawer(SerializedObject serializedObject, SerializedProperty elements, GUIContent header)
        { 
            _header = header;
            _list = new ReorderableList(serializedObject, elements)
            {
                drawHeaderCallback = OnDrawHeader,
                drawElementCallback = OnDrawElement
            };
        }

        private void OnDrawElement(Rect rect, int index, bool isactive, bool isfocused)
        {
            var item = _list.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect.Margin(0, 0, 2, 2), item, GUIContent.none, false);
        }

        private void OnDrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, _header);
        }

        public void DoLayoutList()
        {
            _list.DoLayoutList();
        }
    }
}
