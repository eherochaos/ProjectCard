using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FloatingScoreState
{
    Idle,
    Pre,
    Active,
    Post,
}
public class FloatingScore : MonoBehaviour
{
    public FloatingScoreState State = FloatingScoreState.Idle;

    [SerializeField]
    private int score = 0;

    public string ScoreString;

    public int Score
    {
        get => this.score;
        set
        {
            print("Report:" + value);
            this.score = value;
            this.ScoreString = Utils.AddCommasToNumber(this.score);
            print("ReportScoreString:" + this.ScoreString);
            this.GetComponent<Text>().text = this.ScoreString;
        }
    }

    public List<Vector3> BezierPoints;
    public List<float> FontSizes;
    public float TimeStart = -1f;
    public float TimeDuration = 1f;
    public string EasingCuve = Easing.InOut;

    public GameObject ReportFinishTo = null;

    public void Init(List<Vector3> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        this.BezierPoints = new List<Vector3>(ePts);
        if (ePts.Count == 1)
        {
            this.transform.position = ePts[0];
            return;
        }

        if (Math.Abs(eTimeS) <= 0.001f) eTimeS = Time.time;

        this.TimeStart = eTimeS;
        this.TimeDuration = eTimeD;

        this.State = FloatingScoreState.Pre;
    }

    public void FSCallBack(FloatingScore fs)
    {
        this.Score += fs.Score;
    }

    // Start is called before the first frame update
    void Update()
    {
        if(this.State == FloatingScoreState.Idle) return;

        float u = (Time.time - this.TimeStart) / this.TimeDuration;
        float uC = Easing.Ease(u, this.EasingCuve);
        if (u < 0)
        {
            this.State = FloatingScoreState.Pre;
            var point = this.BezierPoints[0];
            point.x *= Screen.width;
            point.y *= Screen.height;
            this.transform.position = point;
        }
        else
        {
            if (u >= 1)
            {
                uC = 1;
                this.State = FloatingScoreState.Post;
                if (this.ReportFinishTo != null)
                {
                    this.ReportFinishTo.SendMessage("FSCallBack",this);
                    Destroy(this.gameObject);
                }
                else
                {
                    this.State = FloatingScoreState.Idle;
                }
            }
            else
            {
                this.State = FloatingScoreState.Active;
            }

            Vector3 pos = Utils.Bezier(uC, this.BezierPoints);
            pos.x *= Screen.width;
            pos.y *= Screen.height;
            this.transform.position = pos;
            if (this.FontSizes == null || this.FontSizes.Count <= 0) return;
            int size = Mathf.RoundToInt(Utils.Bezier(uC, this.FontSizes));
            this.GetComponent<Text>().fontSize = size;
        }
    }

}
