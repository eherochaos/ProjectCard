using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    public static Scoreboard S;

    public GameObject PrefabFloatingScore;

    public bool _________________________;

    [SerializeField]
    private int score = 0;

    [SerializeField]
    private string scoreString;

    public int Score
    {
        get => this.score;
        set
        {
            this.score = value;
            this.ScoreString = Utils.AddCommasToNumber(this.score);
        }
    }

    public string ScoreString
    {
        get => this.scoreString;
        set
        {
            this.scoreString = value;
            this.GetComponent<Text>().text = this.scoreString;
        }
    }

    private void Awake()
    {
        S = this;
    }

    public void FSCallBack(FloatingScore fs)
    {
        this.Score += fs.Score;
    }

    public FloatingScore CreateFloatingScore(int amt,List<Vector3> pts)
    {
        var canvas = this.GetComponentInParent<Canvas>();
        //var go = canvas.transform.Find("PrefabFloatingScore").gameObject;
        //go.SetActive(true);
        GameObject go = Instantiate(this.PrefabFloatingScore) as GameObject;
        go.transform.parent = canvas.transform;
        FloatingScore fs = go.GetComponent<FloatingScore>();
        fs.Score = amt;
        fs.ReportFinishTo = this.gameObject;
        fs.Init(pts);
        return fs;
    }
}
