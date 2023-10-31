using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintArea : InteractableObject
{
    [SerializeField, TextArea] string hint;

    protected override string Description { get => hint; }

    public override void StartInteract(Carrier carrier = null) { }

    public override void StopInteract(Carrier carrier = null) { }
}
