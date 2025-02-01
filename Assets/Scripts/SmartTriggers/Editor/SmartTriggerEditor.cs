using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Reflection;
using System.Linq;
using NUnit.Framework;
using UnityEngine.UIElements;

[CustomEditor(typeof(SmartTrigger))]
public class SmartTriggerEditor : Editor
{
    private SerializedProperty optionsProperty;
    private SerializedProperty triggerLayersProperty;
    private SerializedProperty triggerTagsProperty;

    ReorderableList triggerList, untriggerList;

    private void OnEnable()
    {
        optionsProperty = serializedObject.FindProperty("triggerOptions");
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

            SerializedObject elementSerializedObject = element.serializedObject;

            elementSerializedObject.Update();
            
            EditorGUI.BeginProperty(rect, GUIContent.none, element);

            EditorGUI.PropertyField(rect, element, new GUIContent(element.managedReferenceValue != null ? element.managedReferenceValue.ToString() : "Null"), true);
            
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

            SerializedObject elementSerializedObject = element.serializedObject;

            elementSerializedObject.Update();

            EditorGUI.BeginProperty(rect, GUIContent.none, element);

            EditorGUI.PropertyField(rect, element, new GUIContent(element.managedReferenceValue != null ? element.managedReferenceValue.ToString() : "Null"), true);

            //// Apply the changes to the SerializedObject
            elementSerializedObject.ApplyModifiedProperties();

            //// End the drawing of the element
            EditorGUI.EndProperty();
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
    }

    private void addClickHandlerUntrigger(object t)
    {
        var addType = (Type)t;
        var newInstance = System.Activator.CreateInstance(addType) as TriggerAction;
        ((SmartTrigger)target).SetUnTriggerListElement(untriggerList.serializedProperty.arraySize, newInstance);
        serializedObject.Update();
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

        optionsProperty.intValue = EditorGUI.MaskField(EditorGUILayout.GetControlRect(), new GUIContent("Options"), optionsProperty.intValue, optionsProperty.enumNames);

        serializedObject.ApplyModifiedProperties();

        serializedObject.Update();
        EditorGUILayout.PropertyField(triggerLayersProperty);
        EditorGUILayout.PropertyField(triggerTagsProperty);


        EditorGUILayout.Space();
        if (triggerList != null) triggerList.DoLayoutList();

        TriggerOptions enumFlagsProperty = (TriggerOptions)optionsProperty.enumValueFlag;
        if ((enumFlagsProperty.HasFlag(TriggerOptions.UntriggerOtherwise)))
        {
            EditorGUILayout.Space();
            untriggerList.DoLayoutList();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
