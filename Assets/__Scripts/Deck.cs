using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Random = UnityEngine.Random;

public class Deck : MonoBehaviour
{
    //花色
    public Sprite SuitClub;

    public Sprite SuitDiamond;

    public Sprite SuitHeart;

    public Sprite SuitSpade;

    public Sprite[] FaceSprites;

    public Sprite[] RankSprites;

    public Sprite CardBack;

    public Sprite CardBackGold;

    public Sprite CardFront;

    public Sprite CardFrontGold;

    //预设
    public GameObject PrefabSprite;

    public GameObject PrefabCard;

    public bool ______________;
    
    public PT_XMLReader XmlReader;

    public List<string> CardNames;

    public List<Card> Cards;

    public List<Decorator> Decorators;

    public List<CardDefinition> CardDefinitions;

    public Transform DeckAnchor;

    public Dictionary<string, Sprite> DictSuits;

    public void InitDeck(string deckXmlText)
    {
        if (GameObject.Find("_Deck") == null)
        {
            GameObject anchorGameObject = new  GameObject("_Deck");
            this.DeckAnchor = anchorGameObject.transform;
        }
        this.DictSuits = new Dictionary<string, Sprite>()
                             {
                                 {"C",this.SuitClub },
                                 {"D",this.SuitDiamond },
                                 {"H",this.SuitHeart },
                                 {"S",this.SuitSpade },
                             };
        this.ReadDeck(deckXmlText);
        this.MakeCards();
    }

    public void ReadDeck(string deckXmlText)
    {
        this.XmlReader = new PT_XMLReader();
        this.XmlReader.Parse(deckXmlText);

        this.Decorators = this.GetDecorators(this.XmlReader);

        this.CardDefinitions = this.GetCardDefinitions(this.XmlReader);
    }

    private List<Decorator> GetDecorators(PT_XMLReader xmlReader)
    {
        var decorators = new List<Decorator>();
        var xDecorators = xmlReader.xml["xml"][0]["decorator"];
        for (var i = 0; i < xDecorators.Count; i++)
        {
            var table = xDecorators[i];
            var decorator = this.GetDecorator(table);
            decorators.Add(decorator);
        }
        return decorators;
    }

    private Decorator GetDecorator(PT_XMLHashtable table)
    {
        var decorator = new Decorator
        {
            Type = table.att("type") ?? "pip",
            Flip = (table.att("flip") == "1"),
            Location =
                {
                    x = float.Parse(table.att("x")),
                    y = float.Parse(table.att("y")),
                    z = float.Parse(table.att("z"))
                }
        };
        if (table.HasAtt("scale"))
        {
            decorator.Scale = float.Parse(table.att("scale"));
        }
        return decorator;
    }

    private List<CardDefinition> GetCardDefinitions(PT_XMLReader xmlReader)
    {
        var cardDefinitions = new List<CardDefinition>();
        var xCardDefinitions = xmlReader.xml["xml"][0]["card"];
        for (var i = 0; i < xCardDefinitions.Count; i++)
        {
            CardDefinition cDefinition = new CardDefinition { Rank = int.Parse(xCardDefinitions[i].att("rank")) };

            PT_XMLHashList xPips = xCardDefinitions[i]["pip"];

            if (xPips != null)
            {
                for (var j = 0; j < xPips.Count; j++)
                {
                    var decorator = this.GetDecorator(xPips[j]);
                    cDefinition.Pips.Add(decorator);
                }
            }

            if (xCardDefinitions[i].HasAtt("face"))
            {
                cDefinition.Face = xCardDefinitions[i].att("face");
            }
            cardDefinitions.Add(cDefinition);
        }
        return cardDefinitions;
    }

    public CardDefinition GetCardDefinitionByRank(int rank)
    {
        foreach (var cardDef in this.CardDefinitions)
        {
            if (cardDef.Rank == rank)
            {
                return cardDef;
            }
        }

        return null;
    }

    public void MakeCards()
    {
        this.CardNames = new List<string>();
        string[] letters = new string[]{"C","D","H","S"};
        foreach (var s in letters)
        {
            for (var i = 0; i < 13; i++)
            {
                this.CardNames.Add(s+(i+1));
            }
        }

        this.Cards = new List<Card>();

        Sprite tSprite = null;
        GameObject tGameObject = null;
        SpriteRenderer tSpriteRenderer = null;

        //遍历所有卡牌名称
        for (var i = 0; i < this.CardNames.Count; i++)
        {
            var cardGameObject = Instantiate(this.PrefabCard) as GameObject;
            cardGameObject.transform.parent = this.DeckAnchor;
            cardGameObject.transform.localPosition = new Vector3((i % 13) * 3, i / 13 * 4, 0);


            var card = cardGameObject.GetComponent<Card>();
            card.name = this.CardNames[i];
            card.Suit = card.name[0].ToString();
            card.Rank = int.Parse(card.name.Substring(1));//??
            if (card.Suit == "D" || card.Suit == "H")
            {
                card.ColorString = "Red";
                card.Color = Color.red;
            }

            card.Def = this.GetCardDefinitionByRank(card.Rank);

            //处理角标
            foreach (var decorator in this.Decorators)
            {
                if (decorator.Type == "suit")
                {
                    //花色部分
                    tGameObject = Instantiate(this.PrefabSprite) as GameObject;
                    tSpriteRenderer = tGameObject.GetComponent<SpriteRenderer>();
                    tSpriteRenderer.sprite = this.DictSuits[card.Suit];
                }
                else
                {
                    //文字部分
                    tGameObject = Instantiate(this.PrefabSprite) as GameObject;
                    tSpriteRenderer = tGameObject.GetComponent<SpriteRenderer>();
                    tSprite = this.RankSprites[card.Rank];
                    tSpriteRenderer.sprite = tSprite;
                    tSpriteRenderer.color = card.Color;
                }

                tSpriteRenderer.sortingOrder = 1;
                tGameObject.transform.parent = cardGameObject.transform;
                tGameObject.transform.localPosition = decorator.Location;

                if(decorator.Flip) tGameObject.transform.rotation = Quaternion.Euler(0,0,180);
                if(Math.Abs(decorator.Scale - 1) > 0.01f) tGameObject.transform.localScale = Vector3.one * decorator.Scale;
                
                tGameObject.name = decorator.Type;
                
                card.DecoGameObjects.Add(tGameObject);
            }

            //处理中间图案
            foreach (var pip in card.Def.Pips)
            {
                tGameObject = Instantiate(this.PrefabSprite) as GameObject;
                tGameObject.transform.parent = cardGameObject.transform;
                tGameObject.transform.localPosition = pip.Location;
                if(pip.Flip) tGameObject.transform.rotation = Quaternion.Euler(0,0,180);
                if (Math.Abs(pip.Scale - 1) > 0.01f) tGameObject.transform.localScale = Vector3.one * pip.Scale;

                tGameObject.name = "pip";
                tSpriteRenderer = tGameObject.GetComponent<SpriteRenderer>();
                tSpriteRenderer.sprite = this.DictSuits[card.Suit];
                tSpriteRenderer.sortingOrder = 1;
                card.PipGameObjects.Add(tGameObject);
            }

            //处理花牌JQK
            if (card.Def.Face != "")
            {
                tGameObject = Instantiate(this.PrefabSprite) as GameObject;
                tSpriteRenderer = tGameObject.GetComponent<SpriteRenderer>();
                tSprite = this.GetFace(card.Def.Face + card.Suit);
                tSpriteRenderer.sprite = tSprite;
                tSpriteRenderer.sortingOrder = 1;
                tGameObject.transform.parent = card.transform;
                tGameObject.transform.localPosition = Vector3.zero;
                tGameObject.name = "face";

            }

            //处理背景
            tGameObject = Instantiate(this.PrefabSprite) as GameObject;
            tSpriteRenderer = tGameObject.GetComponent<SpriteRenderer>();
            tSpriteRenderer.sprite = this.CardBack;
            tGameObject.transform.parent = card.transform;
            tGameObject.transform.localPosition = Vector3.zero;

            tSpriteRenderer.sortingOrder = 2;
            tGameObject.name = "back";
            card.Back = tGameObject;

            card.FaceUp = false;

            this.Cards.Add(card);

        }
    }

    

    public Sprite GetFace(string faceS) => this.FaceSprites.FirstOrDefault(tSprite => tSprite.name == faceS);


    public void Shuffle(ref List<Card> oCards)
    {
        var tCards = new List<Card>();
        int ndx;

        while (oCards.Count > 0)
        {
            ndx = Random.Range(0, oCards.Count);
            tCards.Add(oCards[ndx]);
            oCards.RemoveAt(ndx);
        }
        oCards = tCards;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
