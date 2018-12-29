using System;
using System.Collections.Generic;
using SimpleProto.Scripting;
using UnityEditor;
using UnityEngine;

namespace SimpleProtoEditor.Scripting
{
    [CustomPropertyDrawer(typeof(Script), true)]
    public sealed class ScriptPropertyDrawer : PropertyDrawer
    {
        private static readonly GUIContent MenuLabel = new GUIContent("*");
        private static readonly GUIContent FunctionLabel = new GUIContent("func");
        private static readonly GUIContent ObjectLabel = new GUIContent("obj");
        private static readonly GUIContent IntegerLabel = new GUIContent("num");
        private static readonly GUIContent FloatLabel = new GUIContent("float");
        private static readonly GUIContent BooleanLabel = new GUIContent("bool");
        private static readonly GUIContent StringLabel = new GUIContent("str");

        private SerializedProperty _serializedProperty;
        private SerializedProperty _blocks;
        private Stack<int> _intentStack = new Stack<int>();
        private Stack<FunctionInfo> _functionsStack = new Stack<FunctionInfo>();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (_blocks.arraySize + 2);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            EditorGUI.BeginProperty(position, label, property);
            var line = position.Top(EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(line, label);

            EditorGUI.indentLevel++;
            line = line.NextVertical(EditorGUIUtility.singleLineHeight);
            var counter = 0;
            FunctionInfo functionInfo = null;
            for (var i = 0; i < _blocks.arraySize; ++i)
            {
                var block = _blocks.GetArrayElementAtIndex(i);
                var blockType = block.FindPropertyRelative("_type");

                var labelRect = line.Left(48);
                var valueRect = line.Margin(48, 32, 0, 0);
                var menuRect = line.Right(32);
                EditorGUIUtility.labelWidth = 1;
                switch ((ScriptBlockType)blockType.enumValueIndex)
                {
                    case ScriptBlockType.Function:
                        var functionName = block.FindPropertyRelative("_functionName");
                        EditorGUI.LabelField(labelRect, FunctionLabel);
                        EditorGUI.PropertyField(valueRect.Margin(0, 48, 0, 0), functionName, GUIContent.none);
                        EditorGUI.PropertyField(valueRect.Right(48), block.FindPropertyRelative("_arity"), GUIContent.none);
                        _intentStack.Push(counter);
                        EditorGUI.indentLevel++;
                        counter = block.FindPropertyRelative("_arity").intValue;
                        functionInfo = ScriptLibrary.FindFunction(functionName.stringValue);
                        _functionsStack.Push(functionInfo);
                        break;
                    case ScriptBlockType.Object:
                        var objectValue = block.FindPropertyRelative("_objectValue");
                        var type = functionInfo != null
                            ? functionInfo.ArgTypes[functionInfo.Arity - counter]
                            : typeof(UnityEngine.Object);
                        EditorGUI.LabelField(labelRect, ObjectLabel);
                        EditorGUI.ObjectField(valueRect, objectValue, type, GUIContent.none);
                        counter--;
                        break;
                    case ScriptBlockType.Boolean:
                        EditorGUI.LabelField(labelRect, BooleanLabel);
                        EditorGUI.PropertyField(valueRect, block.FindPropertyRelative("_booleanValue"), GUIContent.none);
                        counter--;
                        break;
                    case ScriptBlockType.String:
                        EditorGUI.LabelField(labelRect, StringLabel);
                        EditorGUI.PropertyField(valueRect, block.FindPropertyRelative("_stringValue"), GUIContent.none);
                        counter--;
                        break;
                    case ScriptBlockType.Integer:
                        EditorGUI.LabelField(labelRect, IntegerLabel);
                        EditorGUI.PropertyField(valueRect, block.FindPropertyRelative("_intValue"), GUIContent.none);
                        counter--;
                        break;
                    case ScriptBlockType.Float:
                        EditorGUI.LabelField(labelRect, FloatLabel);
                        EditorGUI.PropertyField(valueRect, block.FindPropertyRelative("_floatValue"), GUIContent.none);
                        counter--;
                        break;
                }

                if (counter == 0 && _intentStack.Count > 0)
                {
                    EditorGUI.indentLevel--;
                    counter = _intentStack.Pop();
                    functionInfo = _functionsStack.Pop();
                }

                if (EditorGUI.DropdownButton(menuRect, MenuLabel, FocusType.Passive))
                {
                    var index = i;
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Convert to Function"), false, () =>
                    {
                        blockType.enumValueIndex = (int) ScriptBlockType.Function;
                        blockType.serializedObject.ApplyModifiedProperties();
                    });
                    menu.AddItem(new GUIContent("Convert to Object"), false, () =>
                    {
                        blockType.enumValueIndex = (int)ScriptBlockType.Object;
                        blockType.serializedObject.ApplyModifiedProperties();
                    });
                    menu.AddItem(new GUIContent("Convert to String"), false, () =>
                    {
                        blockType.enumValueIndex = (int)ScriptBlockType.String;
                        blockType.serializedObject.ApplyModifiedProperties();
                    });
                    menu.AddItem(new GUIContent("Convert to Number"), false, () =>
                    {
                        blockType.enumValueIndex = (int)ScriptBlockType.Integer;
                        blockType.serializedObject.ApplyModifiedProperties();
                    });
                    menu.AddItem(new GUIContent("Convert to Boolean"), false, () =>
                    {
                        blockType.enumValueIndex = (int)ScriptBlockType.Boolean;
                        blockType.serializedObject.ApplyModifiedProperties();
                    });
                    menu.AddItem(new GUIContent("Add"), false, () =>
                    {
                        var insertationIndex = index + 1;
                        AddScriptBlock(insertationIndex);
                    });
                    menu.AddItem(new GUIContent("Remove"), false, () =>
                    {
                        _blocks.DeleteArrayElementAtIndex(index);
                        _blocks.serializedObject.ApplyModifiedProperties();
                    });
                    menu.ShowAsContext();
                }

                line = line.NextVertical(EditorGUIUtility.singleLineHeight);
            }

            if (GUI.Button(line.Right(32).Margin(0,0,0, EditorGUIUtility.standardVerticalSpacing), "+"))
            {
                AddScriptBlock(_blocks.arraySize);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        private void AddScriptBlock(int insertationIndex)
        {
            _blocks.InsertArrayElementAtIndex(insertationIndex);
            var newBlock = _blocks.GetArrayElementAtIndex(insertationIndex);
            newBlock.FindPropertyRelative("_functionName").stringValue = "Foo";
            newBlock.FindPropertyRelative("_arity").intValue = 0;
            newBlock.FindPropertyRelative("_type").enumValueIndex = (int) ScriptBlockType.Function;
            newBlock.FindPropertyRelative("_objectValue").objectReferenceValue = null;
            _blocks.serializedObject.ApplyModifiedProperties();
        }

        private void Initialize(SerializedProperty property)
        {
            if (_serializedProperty != property)
            {
                _serializedProperty = property;

                _blocks = _serializedProperty.FindPropertyRelative("_blocks");
            }
        }

        public static void SetDefaultValues(SerializedProperty property)
        {
            property.FindPropertyRelative("_blocks").arraySize = 0;
        }
    }
}
