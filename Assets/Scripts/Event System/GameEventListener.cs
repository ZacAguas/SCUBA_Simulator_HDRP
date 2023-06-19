using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    // a component on every object we want to do something when "gameEvent" occurs
    
    [SerializeField] private GameEvent gameEvent; // the channel we are listening on
    [SerializeField] private UnityEvent response; // allows us to call a method in response to the event triggering


    private void OnEnable()
    {
        gameEvent.Register(this); // subscribe to event
    }
    private void OnDisable()
    {
        gameEvent.Deregister(this); // unsubscribe from event
    }

    public void RaiseEvent()
    {
        response.Invoke(); // invoke the actual unity event when the event is raised
    }
}
