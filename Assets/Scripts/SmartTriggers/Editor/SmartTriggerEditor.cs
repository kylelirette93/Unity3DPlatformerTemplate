using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Reflection;
using System.Linq;
using NUnit.Framework;

[CustomEditor(typeof(SmartTrigger))]
public class SmartTriggerEditor : Editor
{
    private SerializedProperty optionsProperty;
    private SerializedProperty triggerLayersProperty;
    private SerializedProperty triggerTagsProperty;

    ReorderableList triggerList, untriggerList;

    private void OnEnable()
    {
        optionsProperty = serializedObject.FindProperty("options");
        triggerLayersProperty = serializedObject.FindProperty("triggerLayers");
        triggerTagsProperty = serializedObject.FindProperty("triggerTags");
        SerializedProperty triggerListProperty = serializedObject.FindProperty("onTriggerActions");

        triggerList = new ReorderableList(serializedObject,
                    triggerListProperty,
                    true,
                    true,
                    true,
                    true);

        triggerList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Trigger Actions");
        };

        triggerList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            var element = triggerList.serializedProperty.GetArrayElementAtIndex(index);

            // Adjust rect to create space between elements
            //rect.y += 2;
            //rect.height = EditorGUIUtility.singleLineHeight;

            //// Draw the property field with full property height
            //EditorGUI.PropertyField(rect, element, new GUIContent(element.managedReferenceValue != null ? element.managedReferenceValue.ToString() : "Null"));


            //// Create a temporary SerializedObject for the list element
            SerializedObject elementSerializedObject = element.serializedObject;

            elementSerializedObject.Update();
            //// Begin the drawing of the element
            EditorGUI.BeginProperty(rect, GUIContent.none, element);

            //EditorGUI.LabelField(rect, element.managedReferenceValue != null ? element.managedReferenceValue.ToString() : "Null");
            //// Indent the property field
            //EditorGUI.indentLevel++;
            //rect = EditorGUI.IndentedRect(rect);
            // Draw the default inspector for the element
            EditorGUI.PropertyField(rect, element, new GUIContent(element.managedReferenceValue != null ? element.managedReferenceValue.ToString() : "Null"), true);
            //EditorGUI.indentLevel--;
            // Get the actual object reference

            // Draw the default inspector for the element
            //EditorGUI.PropertyField(rect, element, GUIContent.none);

            //// Apply the changes to the SerializedObject
            elementSerializedObject.ApplyModifiedProperties();

            //// End the drawing of the element
            EditorGUI.EndProperty();
        };

        triggerList.elementHeightCallback = (int index) => {
            var element = triggerList.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true);
        };

        triggerList.onCanRemoveCallback = (ReorderableList l) => {
            return l.count > 0;
        };
        triggerList.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) => {
            var menu = new GenericMenu();
            List<Type> inheritingTypes = GetListOfTypesInheritingTriggerAction<TriggerAction>();
            foreach (var inhType in inheritingTypes)
            {
                menu.AddItem(new GUIContent(inhType.Name), false, addClickHandler, inhType);
            }
            menu.ShowAsContext();
        };

        untriggerList = new ReorderableList(serializedObject,
                    serializedObject.FindProperty("onUntriggerActions"),
                    true,
                    true,
                    true,
                    true);

        untriggerList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Untrigger Actions");
        };

        untriggerList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            var element = untriggerList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(rect, element, GUIContent.none, true);
        };

        untriggerList.elementHeightCallback = (int index) => {
            var element = untriggerList.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true);
        };

        untriggerList.onCanRemoveCallback = (ReorderableList l) => {
            return l.count > 0;
        };
        untriggerList.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) => {
            var menu = new GenericMenu();
            List<Type> inheritingTypes = GetListOfTypesInheritingTriggerAction<TriggerAction>();
            foreach (var inhType in inheritingTypes)
            {
                menu.AddItem(new GUIContent(inhType.Name), false, addClickHandlerUntrigger, inhType);
            }
            menu.ShowAsContext();
        };
    }

    private void addClickHandler(object t)
    {
        var addType = (Type)t;
        var newInstance = System.Activator.CreateInstance(addType) as TriggerAction;
        ((SmartTrigger)target).SetTriggerListElement(triggerList.serializedProperty.arraySize, newInstance);

        serializedObject.Update();

        //var index = triggerList.serializedProperty.arraySize;
        //triggerList.serializedProperty.InsertArrayElementAtIndex(index);
        //var element = triggerList.serializedProperty.GetArrayElementAtIndex(index);

        //element.managedReferenceValue = newInstance;

        //serializedObject.ApplyModifiedProperties();
    }

    private void addClickHandlerUntrigger(object target)
    {
        var addType = (Type)target;
        serializedObject.Update();

        var index = untriggerList.serializedProperty.arraySize;
        untriggerList.serializedProperty.InsertArrayElementAtIndex(index);
        var element = untriggerList.serializedProperty.GetArrayElementAtIndex(index);

        var newInstance = System.Activator.CreateInstance(addType) as TriggerAction;
        element.managedReferenceValue = newInstance;

        serializedObject.ApplyModifiedProperties();
    }

    public List<Type> GetListOfTypesInheritingTriggerAction<T>()
    {
        List<Type> objects = new List<Type>();
        foreach (Type type in
            Assembly.GetAssembly(typeof(T)).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
        {
            //objects.Add(typeof(T)Activator.CreateInstance(type, constructorArgs));
            objects.Add(type);
        }
        return objects;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(optionsProperty);
        EditorGUILayout.PropertyField(triggerLayersProperty);
        EditorGUILayout.PropertyField(triggerTagsProperty);

        EditorGUILayout.Space();
        if (triggerList != null) triggerList.DoLayoutList();

        if ((((TriggerOptions)optionsProperty.intValue) & (TriggerOptions.UntriggerOtherwise)) == TriggerOptions.UntriggerOtherwise)
        {
            EditorGUILayout.Space();
            untriggerList.DoLayoutList();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
