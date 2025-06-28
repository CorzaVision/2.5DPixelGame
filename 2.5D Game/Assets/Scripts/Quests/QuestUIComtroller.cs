using UnityEngine;
using UnityEngine.UIElements;

public class QuestUIComtroller : MonoBehaviour
{
#region SerializeFields
[Header("UI References")]
[SerializeField] private UIDocument uiDocument;
[SerializeField] private VisualElement _activeQuest;
[SerializeField] private Label _activeQuestTitle;
[SerializeField] private Label _activeQuestDescription;
[SerializeField] private Label _activeQuestProgress;
[SerializeField] private VisualElement _questPanel;
[SerializeField] private VisualElement _questObjectivesList;
// UI References
#endregion
#region Private Fields
private QuestData currentQuest;
private int currentObjectiveIndex;
#endregion
#region Unity Lifecycle
// Awake, Start, Update, OnEnable, OnDisable
private void OnEnable()
{
    InitializeUIElements();
    SubscribeToEvents();
}

private void OnDisable()
{
    UnsubscribeFromEvents();
}

#endregion
#region Initialization
// Setup Methods

private void InitializeUIElements()
{
    _questPanel = uiDocument.rootVisualElement.Q<VisualElement>("quest-panel");
    _activeQuest = _questPanel.Q<VisualElement>("active-quest");
    _activeQuestTitle = _questPanel.Q<Label>("quest-title");
    _activeQuestDescription = _questPanel.Q<Label>("quest-description");
    _questObjectivesList = _questPanel.Q<VisualElement>("quest-objectives-list");
}
#endregion
#region Event Management
// Event Subscriptions and handling
private void SubscribeToEvents()
{
    if (QuestManager.Instance == null)
    {
        Debug.LogWarning("QuestManager.Instance is null! Events not subscribed.");
        return;
    }
    QuestManager.Instance.OnQuestUpdated += OnQuestUpdated;
    QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
    QuestManager.Instance.OnQuestAccepted += OnQuestAccepted;
}

private void UnsubscribeFromEvents()
{
    if (QuestManager.Instance == null)
    {
        Debug.LogWarning("QuestManager.Instance is null! Events not unsubscribed.");
        return;
    }
    QuestManager.Instance.OnQuestUpdated -= OnQuestUpdated;
    QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
    QuestManager.Instance.OnQuestAccepted -= OnQuestAccepted;
}

private void OnQuestAccepted(QuestData quest)
{
    currentQuest = quest;
    ShowQuestDisplay();
    UpdateQuestDisplay(quest, currentObjectiveIndex);
}

private void OnQuestCompleted(QuestData quest)
{
    if (currentQuest == quest)
    {
        HideQuestDisplay();
    }
}

private void OnQuestUpdated(QuestData quest)
{
    if (currentQuest == quest)
    {
        UpdateQuestDisplay(quest, currentObjectiveIndex);
    }
} 

#endregion
#region Public Interface
// Public methods for external access

public void ShowQuestDisplay()
{
    if (_questPanel != null)
    {
        _questPanel.style.display = DisplayStyle.Flex;
    }
}

public void HideQuestDisplay()
{
    if (_questPanel.style.display != null)
    {
        _questPanel.style.display = DisplayStyle.None;
    }
}

private void UpdateQuestDisplay(QuestData quest, int currentObjectiveIndex)
{
    Debug.Log($"Updating Quest Display for: {currentQuest?.questName}");
    if (currentQuest == null || _activeQuestTitle == null) {
        return;
    }
    _activeQuestTitle.text = currentQuest.questName;
    _activeQuestDescription.text = currentQuest.questDescription;

    if (_questObjectivesList == null) {
        return;
    }

    _questObjectivesList.Clear();

    for (int i = 0; i < currentQuest.objectives.Count; i++)
    {
        var objective = currentQuest.objectives[i];
        bool isCurrent = (i == currentObjectiveIndex);

        if (isCurrent)
        {
            if (objective.isTutorialObjective)
            {
                var header = new Label($"Tutorial Type: {objective.tutorialType}");
                header.AddToClassList("objective-header");
                _questObjectivesList.Add(header);

                var instruction = new Label($"Instruction: {objective.tutorialInstruction}");
                instruction.AddToClassList("objective-subheader");
                _questObjectivesList.Add(instruction);

                var requiredAction = new Label($"Required Action: {objective.requiredAction}");
                requiredAction.AddToClassList("objective-subheader");
                _questObjectivesList.Add(requiredAction);

                var status = new Label($"Status: {objective.objectiveStatus}");
                status.AddToClassList("objective-subheader");
                _questObjectivesList.Add(status);
            }
            else
            {
                var header = new Label($"Objective: {objective.ObjectiveName}");
                header.AddToClassList("objective-header");
                _questObjectivesList.Add(header);

                var description = new Label($"Description: {objective.objectiveDescription}");
                description.AddToClassList("objective-subheader");
                _questObjectivesList.Add(description);

                // Show progress if available
                if (objective.requiredAmount > 1)
                {
                    var progress = new Label($"Progress: {objective.currentAmount}/{objective.requiredAmount}");
                    progress.AddToClassList("objective-subheader");
                    _questObjectivesList.Add(progress);
                }

                var status = new Label($"Status: {objective.objectiveStatus}");
                status.AddToClassList("objective-subheader");
                _questObjectivesList.Add(status);
            }
        }
        else
        {
            // Inline view for non-active objectives
            var inline = new Label($"Objective: {objective.ObjectiveName}   |   Status: {objective.objectiveStatus}");
            inline.AddToClassList("objective-header-alt");
            _questObjectivesList.Add(inline);
        }
    }
}


#endregion
}

