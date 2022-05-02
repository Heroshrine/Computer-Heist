using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class HideObject : Interactable
{
    public override void OnInteract()
    {
        ((SneakyPlayerMovement)PlayerMovement.Instance).Hide(transform.position);
    }

    public override bool Conditions()
    {
        if (((SneakyPlayerMovement)PlayerMovement.Instance).seen)
            return false;
        else
            return true;
    }
}
