using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameEvent", fileName = "New Game Event")]
public class GameEvent : ScriptableObject
{
     // Acts as a channel/radio station

    private HashSet<GameEventListener> listeners = new HashSet<GameEventListener>(); // hashset prevents adding duplicates

    // raise event through different method signatures
    public void Invoke()
    {
        foreach (var listener in listeners)
        {
            listener.RaiseEvent();
        }
    }

    // manage listeners

    public void Register(GameEventListener listener)
    {
        listeners.Add(listener);
    }

    public void Deregister(GameEventListener listener)
    {
        listeners.Remove(listener);
    }
}
