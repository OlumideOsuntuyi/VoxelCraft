
using System.Collections.Generic;

using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    public static MainThreadDispatcher instance;
    public Queue<System.Action> actions;
    private void Awake()
    {
        instance = this;
        actions = new();
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        while(actions.Count > 0)
        {
            actions.Dequeue()();
        }
    }
    public static void RunOnMainThread(System.Action action)
    {
        if(instance != null)
        {

            instance.RunAction(action);
        }
    }
    private void RunAction(System.Action action)
    {
        if (action != null)
        {
            actions.Enqueue(action);
        }
    }

}