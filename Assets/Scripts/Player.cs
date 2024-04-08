using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Goal
{
    public string name;
    public float value;
    public TextMeshProUGUI label;

    public Goal(string goalName, float goalValue)
    {
        name = goalName;
        value = goalValue;
    }

    public float GetDiscontentment(float newValue)
    {
        return newValue * newValue;
    }
}

public class Action
{
    public string name;
    public List<Goal> targetGoals;

    public Action(string actionName)
    {
        name = actionName;
        targetGoals = new List<Goal>();
    }

    public float GetGoalChange(Goal goal)
    {
        foreach (Goal target in targetGoals)
        {
            if (target.name == goal.name)
            {
                return target.value;
            }
        }
        return 0f;
    }
}

public class Player : MonoBehaviour
{
    Goal[] goals;
    Action[] actions;
    Action changeOverTime;
    float ticks = 0f;
    const float TICK_LENGTH = 3f;

    public TextMeshProUGUI discontLabel;
    public TextMeshProUGUI timeLabel;
    public TextMeshProUGUI[] goalLabels = new TextMeshProUGUI[4];
    public GameObject speechBubble;

    void Start()
    {
        goals = new Goal[4];
        goals[0] = new Goal("Eat", 4);
        goals[1] = new Goal("Sleep", 3);
        goals[2] = new Goal("Pee", 3);
        goals[3] = new Goal("Happy", 4);

        actions = new Action[7];
        actions[0] = new Action("eat a meal");
        actions[0].targetGoals.Add(new Goal("Eat", -3f));
        actions[0].targetGoals.Add(new Goal("Sleep", +2f));
        actions[0].targetGoals.Add(new Goal("Pee", +1f));
        actions[0].targetGoals.Add(new Goal("Happy", -3f));

        actions[1] = new Action("eat a snack");
        actions[1].targetGoals.Add(new Goal("Eat", -2f));
        actions[1].targetGoals.Add(new Goal("Sleep", +1f));
        actions[1].targetGoals.Add(new Goal("Pee", +1f));
        actions[1].targetGoals.Add(new Goal("Happy", -1f));

        actions[2] = new Action("sleep in bed");
        actions[2].targetGoals.Add(new Goal("Eat", +2f));
        actions[2].targetGoals.Add(new Goal("Sleep", -4f));
        actions[2].targetGoals.Add(new Goal("Pee", +2f));
        actions[2].targetGoals.Add(new Goal("Happy", -3f));

        actions[3] = new Action("sleep on the sofa");
        actions[3].targetGoals.Add(new Goal("Eat", +1f));
        actions[3].targetGoals.Add(new Goal("Sleep", -2f));
        actions[3].targetGoals.Add(new Goal("Pee", +1f));
        actions[3].targetGoals.Add(new Goal("Happy", +2f));

        actions[4] = new Action("drink a soda");
        actions[4].targetGoals.Add(new Goal("Eat", -1f));
        actions[4].targetGoals.Add(new Goal("Sleep", -2f));
        actions[4].targetGoals.Add(new Goal("Pee", +3f));
        actions[4].targetGoals.Add(new Goal("Happy", -1f));

        actions[5] = new Action("go to the bathroom");
        actions[5].targetGoals.Add(new Goal("Eat", 0f));
        actions[5].targetGoals.Add(new Goal("Sleep", 0f));
        actions[5].targetGoals.Add(new Goal("Pee", -4f));
        actions[5].targetGoals.Add(new Goal("Happy", -1f));

        actions[6] = new Action("play video game");
        actions[6].targetGoals.Add(new Goal("Eat", 0f));
        actions[6].targetGoals.Add(new Goal("Sleep", +2f));
        actions[6].targetGoals.Add(new Goal("Pee", 0f));
        actions[6].targetGoals.Add(new Goal("Happy", -4f));

        changeOverTime = new Action("tick");
        changeOverTime.targetGoals.Add(new Goal("Eat", +3f));
        changeOverTime.targetGoals.Add(new Goal("Sleep", +2f));
        changeOverTime.targetGoals.Add(new Goal("Pee", +2f));
        changeOverTime.targetGoals.Add(new Goal("Happy", +4f));

        InvokeRepeating("Tick", 0f, TICK_LENGTH);
    }

    void Tick()
    {
        int iterations = 0;
        ticks++;
        timeLabel.text = "Hours Passed: " + ticks;
        foreach (Goal goal in goals)
        {
            goal.value += changeOverTime.GetGoalChange(goal);
            goal.value = Mathf.Max(goal.value, 0);

            goalLabels[iterations].text = (goal.value).ToString();
            iterations++;
        }
        discontLabel.text = "Discontentment: " + CurrentDiscontentment();
    }

    void PrintGoals()
    {
        int iterations = 0;
        string goalString = "";
        foreach (Goal goal in goals)
        {
            goalString += goal.name + ": " + goal.value + "; ";
            goalLabels[iterations].text = (goal.value).ToString();
            iterations++;
        }
        goalString += "Discontentment: " + CurrentDiscontentment();
        discontLabel.text = "Discontentment: " + CurrentDiscontentment();
        Debug.Log(goalString);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            Action bestThingToDo = ChooseAction(actions, goals);
            speechBubble.SetActive(true);
            speechBubble.GetComponentInChildren<TextMeshProUGUI>().text = "I will " + bestThingToDo.name;

            foreach (Goal goal in goals)
            {
                goal.value += bestThingToDo.GetGoalChange(goal);
                goal.value = Mathf.Max(goal.value, 0);
            }

            Debug.Log("New Stats: ");
            PrintGoals();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Current Stats: ");
            PrintGoals();
        }
    }

    Action ChooseAction(Action[] actions, Goal[] goals)
    {
        Action bestAction = null;
        float bestValue = float.PositiveInfinity;

        foreach (Action action in actions)
        {
            float thisValue = Discontentment(action, goals);
            if (thisValue < bestValue)
            {
                bestValue = thisValue;
                bestAction = action;
            }
        }

        return bestAction;
    }

    float Discontentment(Action action, Goal[] goals)
    {
        float discontentment = 0f;

        foreach (Goal goal in goals)
        {
            float newValue = goal.value + action.GetGoalChange(goal);
            newValue = Mathf.Max(newValue, 0);

            discontentment += goal.GetDiscontentment(newValue);
        }

        return discontentment;
    }

    float CurrentDiscontentment()
    {
        float total = 0f;
        foreach (Goal goal in goals)
        {
            total += (goal.value * goal.value);
        }
        return total;
    }
}
