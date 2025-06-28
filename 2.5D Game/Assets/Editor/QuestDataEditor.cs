using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System;
/// <summary>
/// Custom editor for QuestData ScriptableObject.
/// </summary>
[CustomEditor(typeof(QuestData))]
public class QuestDataEditor : Editor
{
    #region Private Fields
    private bool[] objectiveFoldouts;
    private bool showDefaultObjectives = false;
    #endregion

    #region Unity Inspector
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawQuestInformationSection();

        DrawObjectivesSection();

        DrawQuestRewardsSection();
        DrawPrerequisiteQuestsSection();
        DrawQuestSettingsSection();

        serializedObject.ApplyModifiedProperties();
    }
    #endregion

    #region Section Drawing Methods

    private void DrawQuestInformationSection()
    {
        EditorGUILayout.LabelField("Quest Information", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("questID"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("questName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("questDescription"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("questType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("questSubType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("questStatus"));
        EditorGUILayout.Space();
    }

    private void DrawObjectivesSection()
    {
        EditorGUILayout.LabelField("Objectives", EditorStyles.boldLabel);
        SerializedProperty objectives = serializedObject.FindProperty("objectives");

        if (objectiveFoldouts == null || objectiveFoldouts.Length != objectives.arraySize)
            objectiveFoldouts = new bool[objectives.arraySize];

        for (int i = 0; i < objectives.arraySize; i++)
        {
            SerializedProperty objective = objectives.GetArrayElementAtIndex(i);
            objectiveFoldouts[i] = EditorGUILayout.Foldout(objectiveFoldouts[i], $"Objective {i + 1}: {objective.FindPropertyRelative("ObjectiveName").stringValue}");
            if (objectiveFoldouts[i])
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(objective.FindPropertyRelative("ObjectiveName"));
                
                // Conditional: Show different enum for tutorial objectives
                SerializedProperty isTutorial = objective.FindPropertyRelative("isTutorialObjective");
                if (isTutorial.boolValue)
                {
                    EditorGUILayout.PropertyField(objective.FindPropertyRelative("tutorialType"), new GUIContent("Tutorial Type"));
                    EditorGUILayout.PropertyField(objective.FindPropertyRelative("tutorialInstruction"));
                    // Dropdown for required action
                    string[] actions = { "move", "look", "interact", "attack", "crouch", "sprint", "inventory" };
                    SerializedProperty requiredActionProp = objective.FindPropertyRelative("requiredAction");
                    int actionIndex = Mathf.Max(0, System.Array.IndexOf(actions, requiredActionProp.stringValue));
                    actionIndex = EditorGUILayout.Popup(new GUIContent("Required Action"), actionIndex, actions);
                    requiredActionProp.stringValue = actions[actionIndex];
                }
                else
                {
                    EditorGUILayout.PropertyField(objective.FindPropertyRelative("objectiveType"));
                }

                EditorGUILayout.PropertyField(objective.FindPropertyRelative("objectiveStatus"));
                EditorGUILayout.PropertyField(isTutorial);
                EditorGUILayout.PropertyField(objective.FindPropertyRelative("requiredAmount"));
                EditorGUILayout.PropertyField(objective.FindPropertyRelative("isCompleted"));

                // Remove Objective Button
                if (GUILayout.Button("Remove Objective", GUILayout.Height(20)))
                {
                    RemoveObjective(objectives, i);
                    break;
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(5);
        }

        if (GUILayout.Button("Add Objective"))
        {
            AddNewObjective(objectives);
        }
        EditorGUILayout.Space();
    }

    private void DrawQuestRewardsSection()
    {
        EditorGUILayout.LabelField("Quest Rewards", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rewards"));
        EditorGUILayout.Space();
    }

    private void DrawPrerequisiteQuestsSection()
    {
        EditorGUILayout.LabelField("Prerequisite Quests", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prerequisiteQuests"));
        EditorGUILayout.Space();
    }

    private void DrawQuestSettingsSection()
    {
        EditorGUILayout.LabelField("Quest Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isRepeatable"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("autoComplete"));
    }

    #endregion

    #region Objective Management

    /// <summary>
    /// Adds a new objective to the objectives array with default values.
    /// </summary>
    private void AddNewObjective(SerializedProperty objectives)
    {
        if (objectives == null)
        {
            Debug.LogError("Objectives property is null!");
            return;
        }

        objectives.arraySize++;
        SerializedProperty newObjective = objectives.GetArrayElementAtIndex(objectives.arraySize - 1);

        // Set default values for the new objective
        int maxId = 0;
        if (objectives.arraySize > 1)
        {
            for (int i = 0; i < objectives.arraySize - 1; i++)
            {
                int currentId = objectives.GetArrayElementAtIndex(i).FindPropertyRelative("ObjectiveID").intValue;
                maxId = Mathf.Max(maxId, currentId);
            }
        }
        newObjective.FindPropertyRelative("ObjectiveID").intValue = maxId + 1;
        newObjective.FindPropertyRelative("ObjectiveName").stringValue = $"New Objective {objectives.arraySize}";
        newObjective.FindPropertyRelative("objectiveDescription").stringValue = "Enter description here";
        newObjective.FindPropertyRelative("objectiveType").enumValueIndex = 0;
        newObjective.FindPropertyRelative("objectiveStatus").enumValueIndex = 0;
        newObjective.FindPropertyRelative("requiredAmount").intValue = 1;
        newObjective.FindPropertyRelative("currentAmount").intValue = 0;
        newObjective.FindPropertyRelative("isCompleted").boolValue = false;
        newObjective.FindPropertyRelative("isFailed").boolValue = false;
        newObjective.FindPropertyRelative("questStatus").enumValueIndex = 0;
        newObjective.FindPropertyRelative("isTutorialObjective").boolValue = false;
        newObjective.FindPropertyRelative("tutorialInstruction").stringValue = "";
        newObjective.FindPropertyRelative("requiredAction").stringValue = "";

        if (objectiveFoldouts == null || objectiveFoldouts.Length < objectives.arraySize)
            System.Array.Resize(ref objectiveFoldouts, objectives.arraySize);

        if (objectiveFoldouts != null && objectiveFoldouts.Length > objectives.arraySize - 1)
            objectiveFoldouts[objectives.arraySize - 1] = true;

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Removes an objective from the objectives array at the specified index.
    /// </summary>
    private void RemoveObjective(SerializedProperty objectives, int index)
    {
        if (objectives == null || index < 0 || index >= objectives.arraySize)
        {
            Debug.LogError("Invalid objective index!");
            return;
        }

        objectives.DeleteArrayElementAtIndex(index);

        if (objectiveFoldouts != null && objectiveFoldouts.Length > objectives.arraySize)
            System.Array.Resize(ref objectiveFoldouts, objectives.arraySize);

        serializedObject.ApplyModifiedProperties();
    }

    #endregion
}
