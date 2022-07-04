using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// keeps track of whats selected at a given moment
/// </summary>
public class SelectionManager : MonoBehaviour
{
    Selectable OldSelectable;
    public void OnSelection(Selectable NewSelectable)
    {
        if (OldSelectable is null)
        {
            OldSelectable = NewSelectable;
        }
        else if (!OldSelectable.Equals(NewSelectable))
        {
            OldSelectable.SetSelectionFalse();
            OldSelectable = NewSelectable;
        }
    }
}
