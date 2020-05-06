using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ScoreEvent
{
    Draw,
    Mine,
    MineGold,
    GameWin,
    GameLoss,
}
public class Prospector : MonoBehaviour
{
    public static Prospector S;

    public static int ScoreFromPrevRound = 0;
    public static int HighScore = 0;

    public Vector3 FsPosMid = new Vector3(0.5f,0.9f,0);
    public Vector3 FsPosRun = new Vector3(0.5f,0.75f,0);
    public Vector3 FsPosMid2 = new Vector3(0.5f,0.5f,0);
    public Vector3 FsPosEnd = new Vector3(1.0f,0.65f,0);

    public Deck Deck;

    public TextAsset DeckXml;

    public Layout Layout;

    public TextAsset LayoutXml;

    public Vector3 LayoutCenter;

    public float Xoffset = 3;

    public float Yoffsrt = -2.5f;

    public Transform LayoutAnchor;

    public CardProspector Target;

    public List<CardProspector> Tableau;

    public List<CardProspector> DiscardPile;

    public List<CardProspector> DrawPile;

    public int Chain = 0;

    public int ScoreRun = 0;

    public int Score = 0;

    public FloatingScore FsRun;

    private void Awake()
    {
        S = this;
        if (PlayerPrefs.HasKey("ProspectorHighScore"))
            HighScore = PlayerPrefs.GetInt("ProspectorHighScore");
        this.Score += ScoreFromPrevRound;
        ScoreFromPrevRound = 0;
    }
    // Start is called before the first frame update
    private void Start()
    {
        Scoreboard.S.Score = this.Score;
        this.Deck = this.GetComponent<Deck>();
        this.Deck.InitDeck(this.DeckXml.text);
        this.Deck.Shuffle(ref this.Deck.Cards);

        this.Layout = this.GetComponent<Layout>();
        this.Layout.ReadLayout(this.LayoutXml.text);
        this.DrawPile = this.ConvertListCardsToListCardProspectors(this.Deck.Cards);

        //从_Deck根据游戏规则插入至_LayoutAnchor
        this.LayoutGame();

        
    }

    private CardProspector Draw()
    {
        var cd = this.DrawPile[0];
        this.DrawPile.RemoveAt(0);
        return cd;
    }

    private CardProspector FindCardByLayoutId(int layoutID)
    {
        foreach (var tCardProspector in this.Tableau)
        {
            if (tCardProspector.LayoutId == layoutID)
            {
                return tCardProspector;
            }
        }

        return null;
    }

    private void LayoutGame()
    {
        if (this.LayoutAnchor == null)
        {
            var tGameObject = new GameObject("_LayoutAnchor");
            this.LayoutAnchor = tGameObject.transform;
            this.LayoutAnchor.transform.position = this.LayoutCenter;
        }

        CardProspector cardProspector;

        foreach (var tSlotDef in this.Layout.SlotDefs)
        {
            cardProspector = this.Draw();
            cardProspector.FaceUp = tSlotDef.FaceUp;
            cardProspector.transform.parent = this.LayoutAnchor;
            cardProspector.transform.localPosition = new Vector3(this.Layout.Multiplier.x * tSlotDef.X,this.Layout.Multiplier.y * tSlotDef.Y,-tSlotDef.LayerId);
            cardProspector.LayoutId = tSlotDef.Id;
            cardProspector.SlotDef = tSlotDef;
            cardProspector.State = CardState.TableAu;
            cardProspector.SetSortingLayerName(tSlotDef.LayerName);
            this.Tableau.Add(cardProspector);
        }

        foreach (var tCardProspector in this.Tableau)
        {
            foreach (var hid in tCardProspector.SlotDef.HiddenBy)
            {
                cardProspector = FindCardByLayoutId(hid);
                tCardProspector.HiddenBy.Add(cardProspector);
            }
        }

        this.MoveToTarget(this.Draw());
        this.UpdateDrawPile();
    }

    public void CardClicked(CardProspector card)
    {
        switch (card.State)
        {
            case CardState.Target:
                break;
            case CardState.DrawPile:
                this.MoveToDiscard(this.Target);
                this.MoveToTarget(this.Draw());
                this.UpdateDrawPile();
                this.ScoreManager(ScoreEvent.Draw);
                break;
            case CardState.TableAu:
                bool validMatch = card.FaceUp;
                if (!this.AdjacentRank(card, this.Target)) validMatch = false;
                
                //若为无效匹配(==false)
                if(!validMatch) return;

                this.Tableau.Remove(card);
                this.MoveToTarget(card);
                this.SetTableauFaces();
                this.ScoreManager(ScoreEvent.Mine);
                break;

        }

        this.CheckForGameOver();
    }

    private void CheckForGameOver()
    {
        if (this.Tableau.Count == 0)
        {
            this.GameOver(true);
            return;
        }

        if(this.DrawPile.Count > 0) return;

        foreach (var card in this.Tableau)
        {
            if(AdjacentRank(card,this.Target)) return;
        }

        this.GameOver(false);
    }

    private void GameOver(bool won)
    {
        //print(won ? "Game Over. You won!:)" : "Game Over.You Lose :(");
        this.ScoreManager(won ? ScoreEvent.GameWin : ScoreEvent.GameLoss);
        SceneManager.LoadScene("_Prospector_Scene_0");

    }

    public bool AdjacentRank(CardProspector card0, CardProspector card1)
    {
        if (!card0.FaceUp || !card1.FaceUp) return false;

        if (Mathf.Abs(card0.Rank - card1.Rank) == 1) return true;
        if (card0.Rank == 1 && card1.Rank == 13) return true;
        if (card0.Rank == 13 && card1.Rank == 1) return true;

        return false;
    }

    private void SetTableauFaces()
    {
        foreach (var card in this.Tableau)
        {
            var faceUp = true;
            foreach (var cover in card.HiddenBy)
            {
                if (cover.State == CardState.TableAu)
                {
                    faceUp = false;
                }
            }
            card.FaceUp = faceUp;
        }
    }

    private void MoveToDiscard(CardProspector card)
    {
        card.State = CardState.Discard;
        this.DiscardPile.Add(card);
        card.transform.parent = this.LayoutAnchor;
        card.transform.localPosition = new Vector3(this.Layout.Multiplier.x * this.Layout.DiscardPile.X,this.Layout.Multiplier.y * this.Layout.DiscardPile.Y,-this.Layout.DiscardPile.LayerId + 0.5f);
        card.FaceUp = true;

        card.SetSortingLayerName(this.Layout.DiscardPile.LayerName);
        card.SetSortOrder( -100 +this.DiscardPile.Count);
    }

    private void MoveToTarget(CardProspector card)
    {
        if(this.Target != null) this.MoveToDiscard(this.Target);
        this.Target = card;
        card.State = CardState.Target;
        card.transform.parent = this.LayoutAnchor;
        card.transform.localPosition = new Vector3(this.Layout.Multiplier.x * this.Layout.DiscardPile.X, this.Layout.Multiplier.y * this.Layout.DiscardPile.Y, -this.Layout.DiscardPile.LayerId);
        card.FaceUp = true;
        card.SetSortingLayerName(this.Layout.DiscardPile.LayerName);
        card.SetSortOrder(0);
    }

    private void UpdateDrawPile()
    {
        CardProspector card;
        for (int i = 0; i < this.DrawPile.Count; i++)
        {
            card = this.DrawPile[i];
            card.transform.parent = this.LayoutAnchor;

            Vector2 dpStagger = this.Layout.DrawPile.Stagger;

            card.transform.localPosition = new Vector3(
                this.Layout.Multiplier.x * (this.Layout.DrawPile.X + i*dpStagger.x), 
                this.Layout.Multiplier.y * (this.Layout.DrawPile.Y + i * dpStagger.y),
                -this.Layout.DrawPile.LayerId + 0.1f*i);

            card.FaceUp = false;
            card.State = CardState.DrawPile;
            card.SetSortingLayerName(this.Layout.DrawPile.LayerName);
            card.SetSortOrder(-10*i);
        }
    }

    private List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> cardDeck)
    {
        List<CardProspector> lCardProspectors = new List<CardProspector>();
        CardProspector tCardProspector;

        foreach (var card in cardDeck)
        {
            tCardProspector = card as CardProspector;
            lCardProspectors.Add(tCardProspector);
        }

        return lCardProspectors;
    }

    private void ScoreManager(ScoreEvent scoreEvent)
    {
        List<Vector3> fsPts;
        switch (scoreEvent)
        {
            case ScoreEvent.Draw:
            case ScoreEvent.GameWin:
            case ScoreEvent.GameLoss:
                this.Chain = 0;
                this.Score += this.ScoreRun;
                this.ScoreRun = 0;
                if (this.FsRun != null)
                {
                    fsPts = new List<Vector3>();
                    fsPts.Add(this.FsPosRun);
                    fsPts.Add(this.FsPosMid2);
                    fsPts.Add(this.FsPosEnd);
                    this.FsRun.ReportFinishTo = Scoreboard.S.gameObject;
                    this.FsRun.Init(fsPts,0,1);
                    this.FsRun.FontSizes = new List<float>(new float[]{28,36,4});
                    this.FsRun = null;
                }
                break;
            case ScoreEvent.Mine:
                this.Chain++;
                this.ScoreRun += this.Chain;
                FloatingScore fs;
                Vector3 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector3>();
                fsPts.Add(p0);
                fsPts.Add(this.FsPosMid);
                fsPts.Add(this.FsPosRun);
                fs = Scoreboard.S.CreateFloatingScore(this.Chain, fsPts);
                fs.FontSizes = new List<float>(new float[]{4,50,28});
                if (this.FsRun == null)
                {
                    this.FsRun = fs;
                    this.FsRun.ReportFinishTo = null;
                }
                else
                {
                    fs.ReportFinishTo = this.FsRun.gameObject;
                }
                break;

        }

        switch (scoreEvent)
        {
            case ScoreEvent.GameWin:
                Prospector.ScoreFromPrevRound = this.Score;
                print("You won this round!Round score:"+this.Score);
                break;
            case ScoreEvent.GameLoss:
                if (Prospector.HighScore <= this.Score)
                {
                    print("You got the high score! High score:"+ this.Score);
                    Prospector.HighScore = this.Score;
                    PlayerPrefs.SetInt("ProspectorHighScore",this.Score);
                }
                else
                {
                    print("Your final score for the game was:"+this.Score);
                }
                break;

            default:
                print("score:"+this.Score+" scoreRun:"+this.ScoreRun+" chain:" + this.Chain);
                break;
        }
    }
}
