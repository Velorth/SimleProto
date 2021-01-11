using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleProto.Scripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SimpleProtoEditor.Scripting
{
    [CustomPropertyDrawer(typeof(Script), true)]
    public sealed class ScriptPropertyDrawer : PropertyDrawer
    {
        private static string[] _functionNames = ScriptLibrary.Functions.Select(fi => fi.Name).ToArray();

        private sealed class State
        {
            public ReorderableList BlocksList { get; set; }
            public int LastSelectedIndex { get; set; }
            public SerializedObject SerializedObject { get; set; }
        }

        private struct BlockDisplayInfo
        {
            public int Indent { get; set; }
            public Type ExpectedType { get; set; }
        }

        private SerializedProperty _serializedProperty;
        private SerializedProperty _blocks;
        private Stack<int> _indentStack = new Stack<int>();
        private Stack<FunctionInfo> _functionsStack = new Stack<FunctionInfo>();
        private Dictionary<string, State> _states = new Dictionary<string, State>();
        private List<BlockDisplayInfo> _displayInfo = new List<BlockDisplayInfo>();
        private GUIContent _label;

        private State GetState(SerializedProperty property)
        {
            var key = property.propertyPath;
            _states.TryGetValue(key, out var state);
            if (state == null || state.SerializedObject != property.serializedObject)
            {
                state = new State
                {
                    SerializedObject = property.serializedObject,
                    BlocksList = new ReorderableList(property.serializedObject, property.FindPropertyRelative("_blocks"))
                    {
                        drawHeaderCallback = OnDrawHeader,
                        drawElementCallback = OnDrawElement,
                        draggable = true
                    }
                };

                _states[key] = state;
            }

            return state;
        }

        private void OnDrawElement(Rect rect, int index, bool isactive, bool isfocused)
        {
            var blocks = _serializedProperty.FindPropertyRelative("_blocks");
            var blockProperty = blocks.GetArrayElementAtIndex(index);

            var blockTypeProperty = blockProperty.FindPropertyRelative("_type");

            var indent = _displayInfo[index].Indent;
            var expectedType = _displayInfo[index].ExpectedType;

            rect.y += 1.5f;
            rect.height = EditorGUIUtility.singleLineHeight;
            var valueRect = rect.Margin(indent * 20, 72 + 2, 0, 0);

            EditorGUI.PropertyField(rect.Right(72), blockTypeProperty, GUIContent.none);

            switch ((ScriptBlockType)blockTypeProperty.enumValueIndex)
            {
                case ScriptBlockType.Function:
                    var functionName = blockProperty.FindPropertyRelative("_functionName");
                    var functionIndex = Array.IndexOf(_functionNames, functionName.stringValue);
                    functionIndex = EditorGUI.Popup(valueRect, functionIndex, _functionNames);
                    if (functionIndex != -1)
                    {
                        functionName.stringValue = _functionNames[functionIndex];
                    }
                    break;
                case ScriptBlockType.Boolean:
                    EditorGUI.PropertyField(valueRect, blockProperty.FindPropertyRelative("_booleanValue"), GUIContent.none);
                    break;
                case ScriptBlockType.Integer:
                    var intValueProperty = blockProperty.FindPropertyRelative("_intValue");
                    if (expectedType.IsEnum)
                    {
                        var enumValue = (Enum)Enum.ToObject(expectedType, intValueProperty.intValue);
                        intValueProperty.intValue = Convert.ToInt32(EditorGUI.EnumPopup(valueRect, enumValue));
                    }
                    else
                    {
                        EditorGUI.PropertyField(valueRect, intValueProperty, GUIContent.none);
                    }
                    break;
                case ScriptBlockType.Float:
                    EditorGUI.PropertyField(valueRect, blockProperty.FindPropertyRelative("_floatValue"), GUIContent.none);
                    break;
                case ScriptBlockType.String:
                    EditorGUI.PropertyField(valueRect, blockProperty.FindPropertyRelative("_stringValue"), GUIContent.none);
                    break;
                case ScriptBlockType.Object:
                    var objectValueProperty = blockProperty.FindPropertyRelative("_objectValue");
                    objectValueProperty.objectReferenceValue = EditorGUI.ObjectField(valueRect, objectValueProperty.objectReferenceValue, expectedType, false);
                    break;
                default:
                    EditorGUI.LabelField(valueRect, "Unknown type");
                    break;
            }
        }

        private void OnDrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, _label);
        }

        private void BuildDisplayInfo()
        {
            _displayInfo.Clear();
            _indentStack.Clear();

            var counter = 0;
            FunctionInfo functionInfo = null;
            for (var i = 0; i < _blocks.arraySize; ++i)
            {
                if (functionInfo != null)
                {
                    var argIndex = functionInfo.Arity - counter;
                    _displayInfo.Add(new BlockDisplayInfo
                    {
                        ExpectedType = argIndex >= 0 && argIndex < functionInfo.ArgTypes.Length ? functionInfo.ArgTypes[argIndex] : typeof(void),
                        Indent = _indentStack.Count
                    });
                }
                else
                {
                    _displayInfo.Add(new BlockDisplayInfo
                    {
                        ExpectedType = typeof(void),
                        Indent = _indentStack.Count
                    });
                }

                var block = _blocks.GetArrayElementAtIndex(i);
                var blockType = block.FindPropertyRelative("_type");

                if ((ScriptBlockType) blockType.enumValueIndex == ScriptBlockType.Function)
                {
                    var functionName = block.FindPropertyRelative("_functionName");
                    _indentStack.Push(counter);
                    functionInfo = ScriptLibrary.FindFunction(functionName.stringValue);
                    counter = functionInfo?.Arity ?? 0;
                    _functionsStack.Push(functionInfo);
                }
                else
                {
                    counter--;
                }

                if (counter == 0 && _indentStack.Count > 0)
                {
                    counter = _indentStack.Pop();
                    functionInfo = _functionsStack.Pop();
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            var state = GetState(property);

            return state.BlocksList.GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Initialize(property);

            BuildDisplayInfo();

            EditorGUI.BeginProperty(position, label, property);

            var state = GetState(property);
            state.BlocksList.index = state.LastSelectedIndex;

            state.BlocksList.DoList(position);

            state.LastSelectedIndex = state.BlocksList.index;

            EditorGUI.EndProperty();
            EditorGUI.indentLevel = indent;
        }

        private void Initialize(SerializedProperty property)
        {
            _serializedProperty = property;
            _label = new GUIContent(property.displayName);
            _blocks = _serializedProperty.FindPropertyRelative("_blocks");
        }

        public static void SetDefaultValues(SerializedProperty property)
        {
            property.FindPropertyRelative("_blocks").arraySize = 0;
        }
    }
}
