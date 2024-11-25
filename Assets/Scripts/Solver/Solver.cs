using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solver : MonoBehaviour
{
    [SerializeField]
    private BeakerContainer beakerContainer;
    [SerializeField]
    private DialogHUD dialogHUD;
    [SerializeField]
    private GameObject go_solutionDisplayer;
    [SerializeField]
    private GameObject go_initializer;


    public void Begin()
    {
        var data = beakerContainer.GetData();
        string errorMessage = string.Empty;

        if (CheckData(data, ref errorMessage))
        {
            dialogHUD.Display("Calculating . . .", "Cancel", Cancel);
            StartCoroutine(Calculate(data));
        }
        else
        {
            dialogHUD.Display(errorMessage, "Ok");
        }
    }

    private bool CheckData(List<Beaker> data, ref string errorMessage)
    {
        // check the min number of beakers

        if (data.Count < 2)
        {
            errorMessage = "The number of beakers can't be lesser than 2.";
            return false;
        }


        // content check


        var contentCounter = new Dictionary<int, int>();

        foreach (Beaker beaker in data)
        {
            foreach (int id in beaker.Contents)
            {
                if (!contentCounter.ContainsKey(id))
                    contentCounter.Add(id, 0);
                ++contentCounter[id];

            }
        }

        // check if there are no colors at all

        if (contentCounter.Count < 2)
        {
            errorMessage = "The number of colors can't be lesser than 2.";
            return false;
        }

        // check if the number of colors is lesser or equal to the number of beakers
        if (contentCounter.Count > data.Count)
        {
            errorMessage = "The number of colors can't be gerater than the number of beakers.";
            return false;
        }

        // check if each color has maxCapacity apparitions
        int maxCapacity = BeakerUI.MaxCapacity;

        foreach (KeyValuePair<int, int> pair in contentCounter)
        {
            if (pair.Value == 0 || pair.Value % maxCapacity != 0)
            {
                errorMessage = $"All colors must appear {maxCapacity} times.";
                return false;
            }
        }

        return true;
    }

    private void Cancel()
    {
        StopAllCoroutines();
    }

    public enum ECalculateResult
    {
        Success,
        Failure,
    }

    private IEnumerator Calculate(List<Beaker> data)
    {
        var result = CalculateStep(data);
        if (result.Item1 == ECalculateResult.Success)
        {
            dialogHUD.Display(result.Item2, "Ok");
            if (result.Item3 != null)
            {
                HandleSolution(result.Item3, result.Item4);
            }
            else
            {
                dialogHUD.Display("This state is already final.", "Ok");
            }
        }
        else
        {
            HandleFailure();
        }
        yield break;
    }

    static private Tuple<ECalculateResult, string, State, List<Action>> CalculateStep(List<Beaker> data)
    {
        Beaker.maxCapacity = BeakerUI.MaxCapacity;

        var opened = new SortedSet<State>(new StateComparer());
        var closed = new List<State>();

        opened.Add(new State(data));

        if (opened.Max.IsFinal)
        {
            return new Tuple<ECalculateResult, string, State, List<Action>>(ECalculateResult.Success, "This state is already final.", null, null);
        }

        var elapsedTime = new DateTime();

        while (opened.Count > 0 && !opened.Max.IsFinal)
        {
            var currentState = opened.Max;
            opened.Remove(currentState);

            var children = currentState.Expand();
            foreach (var child in children)
            {
                if (!closed.ContainsValue(child))
                    opened.Add(child);
            }

            closed.Add(currentState);
            elapsedTime = elapsedTime.AddSeconds(Time.deltaTime);
        }

        if (opened.Max != null)
        {
            string msg = $"Processed {closed.Count} states.\nPending: {opened.Count}.\nElapsed time: {elapsedTime.ToString("HH:mm:ss")}";
            return new Tuple<ECalculateResult, string, State, List<Action>>(ECalculateResult.Success, msg, closed[0], GetActions(closed[0], opened.Max));
        }

        return new Tuple<ECalculateResult, string, State, List<Action>>(ECalculateResult.Failure, "Failed to find a solution.", null, null);
    }


    private void HandleFailure()
    {
        dialogHUD.Display("Failed to find a solution.", "Ok");
    }

    private void HandleSolution(State initial, List<Action> steps)
    {
        dialogHUD.Display("Success", "Ok");
        go_initializer.SetActive(false);
        go_solutionDisplayer.SetActive(true);
        go_solutionDisplayer.GetComponent<SolutionDisplayer>().Initialize(initial, steps);
    }

    private static List<Action> GetActions(State initial, State final)
    {
        var list = new List<Action>();

        var currentState = final;

        while (currentState != initial)
        {
            list.Add(currentState.Action);
            currentState = currentState.Parent;
        }

        list.Reverse();

        return list;
    }
}
