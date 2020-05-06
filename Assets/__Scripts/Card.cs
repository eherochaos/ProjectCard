using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public string Suit;

    public int Rank;

    public Color Color = Color.black;

    public string ColorString = "Black";

    public List<GameObject> DecoGameObjects = new List<GameObject>();

    public List<GameObject> PipGameObjects = new List<GameObject>();

    public GameObject Back;

    public CardDefinition Def;

    public SpriteRenderer[] SpriteRenderers;

    private void Start()
    {
        this.SetSortOrder(0);
    }

    public virtual void OnMouseUpAsButton()
    {
        //print(this.name);
    }

    public bool FaceUp
    {
        get => !this.Back.activeSelf;
        set => this.Back.SetActive(!value);

    }
    public void PopulateSpriteRenderers()
    {
        if (this.SpriteRenderers == null || this.SpriteRenderers.Length == 0)
        {
            this.SpriteRenderers = this.GetComponentsInChildren<SpriteRenderer>();
        }
    }

    public void SetSortingLayerName(string tSortingLayerName)
    {
        this.PopulateSpriteRenderers();
        foreach (var tSpriteRenderer in this.SpriteRenderers)
        {
            tSpriteRenderer.sortingLayerName = tSortingLayerName;
        }
    }

    public void SetSortOrder(int sortOrder)
    {
        this.PopulateSpriteRenderers();
        foreach (var tSpriteRenderer in this.SpriteRenderers)
        {
            if (tSpriteRenderer.gameObject == this.gameObject)
            {
                tSpriteRenderer.sortingOrder = sortOrder;
                continue;
            }
            switch (tSpriteRenderer.gameObject.name)
            {
                case "back":
                    tSpriteRenderer.sortingOrder = sortOrder + 2;
                    break;

                case "face":
                default:
                    tSpriteRenderer.sortingOrder = sortOrder + 1;
                    break;
            }
        }
    }
}

[System.Serializable]

public class Decorator //装饰器
{
    
    /// <summary>
    /// 花号
    /// </summary>
    public string Type;

    /// <summary>
    /// 坐标信息
    /// </summary>
    public Vector3 Location;

    /// <summary>
    /// 是否垂直翻转
    /// </summary>
    public bool Flip = false;

    /// <summary>
    /// 缩放比例
    /// </summary>
    public float Scale = 1f;
}

[System.Serializable]
public class  CardDefinition
{
    /// <summary>
    /// 花牌Sprite名称
    /// </summary>
    public string Face;

    /// <summary>
    /// 1-13点
    /// </summary>
    public int Rank;

    /// <summary>
    /// 对应花色符号
    /// </summary>
    public List<Decorator> Pips = new List<Decorator>();
    
}
