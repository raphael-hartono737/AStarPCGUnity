using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProceduralQuestGenerator : MonoBehaviour
{
    [System.Serializable]
    public class Quest
    {
        public string questName;
        public string description;
        public string objective;
        public string reward;
        public int difficultyLevel;
    }

    [SerializeField] private List<string> questNames = new List<string>();
    [SerializeField] private List<string> descriptions = new List<string>();
    [SerializeField] private List<string> objectives = new List<string>();
    [SerializeField] private List<string> rewards = new List<string>();

    // UI Elements
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI objectiveText;
    public TextMeshProUGUI rewardText;

    public Quest GenerateRandomQuest()
    {
        Quest newQuest = new Quest
        {
            questName = questNames[Random.Range(0, questNames.Count)],
            description = descriptions[Random.Range(0, descriptions.Count)],
            objective = objectives[Random.Range(0, objectives.Count)],
            reward = rewards[Random.Range(0, rewards.Count)],
            difficultyLevel = Random.Range(1, 10)
        };

        return newQuest;
    }

    public void DisplayQuest(Quest quest)
    {
        questNameText.text = $"{quest.questName}";
        objectiveText.text = $"{quest.objective}";
        rewardText.text = $"{quest.reward}";
    }

    void Start()
    {
        Quest randomQuest = GenerateRandomQuest();
        DisplayQuest(randomQuest);
    }
}
