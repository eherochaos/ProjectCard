using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlotDef
{
    public float X;

    public float Y;

    public bool FaceUp = false;

    public string LayerName = "Default";

    public int LayerId = 0;

    public int Id;

    public List<int> HiddenBy = new List<int>();

    public string Type = "Slot";

    public Vector2 Stagger;
}

public class Layout : MonoBehaviour
{
    public PT_XMLReader XmlReader;

    public PT_XMLHashtable Xml;

    /// <summary>
    /// 布局紧凑度
    /// </summary>
    public Vector2 Multiplier;

    /// <summary>
    /// 槽位信息List
    /// </summary>
    public List<SlotDef> SlotDefs;

    public SlotDef DrawPile;

    public SlotDef DiscardPile;

    public string[] SortingLayerNames = new string[]{"Row0","Row1","Row2","Row3","Discard","Draw"};

    public void ReadLayout(string xmlText)
    {
        this.XmlReader = new PT_XMLReader();
        this.XmlReader.Parse(xmlText);
        this.Xml = this.XmlReader.xml["xml"][0];

        this.Multiplier.x = float.Parse(this.Xml["multiplier"][0].att("x"));
        this.Multiplier.y = float.Parse(this.Xml["multiplier"][0].att("y"));

        SlotDef tSlotDef;

        PT_XMLHashList slotsX = this.Xml["slot"];

        for (var i = 0; i < slotsX.Count; i++)
        {
            tSlotDef = new SlotDef
           {
               Type = slotsX[i].HasAtt("type") ? slotsX[i].att("type") : "slot",
               X = float.Parse(slotsX[i].att("x")),
               Y = float.Parse(slotsX[i].att("y")),
               LayerId = int.Parse(slotsX[i].att("layer"))
           };

            tSlotDef.LayerName = this.SortingLayerNames[tSlotDef.LayerId];

            switch (tSlotDef.Type)
            {
                case "slot":
                    tSlotDef.FaceUp = (slotsX[i].att("faceup") == "1");
                    tSlotDef.Id = int.Parse(slotsX[i].att("id"));
                    if (slotsX[i].HasAtt("hiddenby"))
                    {
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach (var s in hiding)
                        {
                            tSlotDef.HiddenBy.Add(int.Parse(s));
                        }
                    }
                    this.SlotDefs.Add(tSlotDef);
                    break;

                case "drawpile":
                    tSlotDef.Stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    this.DrawPile = tSlotDef;
                    break;

                case "discardpile":
                    this.DiscardPile = tSlotDef;
                    break;
            }
        }
    }
}

