using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PopupContent : MonoBehaviour
{
    public abstract string PopupTitle { get; }

    public abstract void OnInitShow();
    public abstract void OnOkClick(TournamentSimulator simulator);
}
