using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionAnimEvents : MonoBehaviour
{
    public TransitionManager manager;

    public void OnEnterFinished()
    {
        if (manager != null)
            manager.OnEnterFinished();
    }
}
