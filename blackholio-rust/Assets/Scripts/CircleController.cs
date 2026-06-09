using System;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

public class CircleController : EntityController
{
    private static Color[] ColorPalette = {
        //Yellow
        new Color32(175, 159, 49, 255),
        new Color32(175, 116, 49, 255),

        //Purple
        new Color32(112, 47, 252, 255),
        new Color32(51, 91, 252, 255),

        //Red
        new Color32(176, 54, 54, 255),
        new Color32(176, 109, 54, 255),
        new Color32(141, 43, 99, 255),

        //Blue
        new Color32(2, 188, 250, 255),
        new Color32(7, 50, 251, 255),
        new Color32(2, 28, 146, 255),
    };

    private PlayerController Owner { get; set; }

    public void Spawn(Circle circle, PlayerController owner)
    {
        base.Spawn(circle.EntityId);
        SetColor(ColorPalette[circle.PlayerId % ColorPalette.Length]);

        Owner = owner;
        GetComponentInChildren<TMPro.TextMeshPro>().text = owner.Username;
    }

    public override void OnDelete(EventContext context)
    {
        base.OnDelete(context);
        Owner.OnCircleDeleted(this);
    }
}
