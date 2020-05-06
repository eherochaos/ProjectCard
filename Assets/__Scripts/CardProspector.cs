using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

public enum CardState
{
    DrawPile,
    TableAu,
    Target,
    Discard,
}
public class CardProspector : Card
{

    public CardState State = CardState.DrawPile;

    public List<CardProspector> HiddenBy = new List<CardProspector>();

    public int LayoutId;

    public SlotDef SlotDef;

    public override void OnMouseUpAsButton()
    {
        //Input.touchSupported
        Prospector.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
}
