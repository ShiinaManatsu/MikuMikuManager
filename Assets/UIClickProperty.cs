using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

/// <summary>
/// Provide right click detection
/// </summary>
public class UIClickProperty : MonoBehaviour
{
    public bool IsMRBClicked { get; set; } = false;
    public ReactiveProperty<bool> isMRBClicked;

    void Start()
    {
        isMRBClicked = new ReactiveProperty<bool>(false);

        var t = isMRBClicked
            .Where(x => x)
            .Do(x => Debug.Log(x))
            .Subscribe(x => IsMRBClicked = x);

        var f = isMRBClicked
            .Where(x => !x)
            .Delay(TimeSpan.FromSeconds(1))
            .Do(x => Debug.Log(x))
            .Subscribe(x => IsMRBClicked = x);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isMRBClicked.Value = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isMRBClicked.Value = false;
        }
    }
}
