﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RSP3D2 : MonoBehaviour
{
    public Sprite rock;                         //グー
    public Sprite scissors;                     //チョキ
    public Sprite paper;                        //パー
    public GameObject canvas;                   //Imageインスタンスを入れるためのcanvas
    public Image[] originHand;                  //インスタンスの元となるオブジェクト
    private Image[] appearHand = new Image[4];  //originHandを元に生成されたインスタンス
    private PlayerStatus playerStatus;          //プレーヤーのステータス
    private EnemyStatus enemyStatus;            //敵のステータス
    private AudioSource SEBox;                  //PlayOneShot用の空箱
    private bool isHiddenOn = false;            //手が隠されているか否か
    public Image result;                        //じゃんけん判定を表示
    public Sprite imageWin;                     //勝利画像
    public Sprite imageDrow;                    //引き分け画像
    public Sprite imageLose;                    //負け画像
    public float lerpTime = 0.4f;               //線形補間の速度
    private float startTime = -1f;              //線形補間の時間初期値


    void Start()
    {
        playerStatus = FindObjectOfType<PlayerStatus>();
        enemyStatus = FindObjectOfType<EnemyStatus>();
        SEBox = GameObject.Find("SEBox").GetComponent<AudioSource>();

        //出現する手をインスタンス生成
        for (int i = 0; i < originHand.Length; i++)
        {
            appearHand[i] = Instantiate(originHand[i], originHand[i].transform.position, Quaternion.identity) as Image;
            appearHand[i].GetComponent<Image>().sprite = GetNextHand();             // = GetNextHand(); //手の絵を入れる
            appearHand[i].transform.SetParent(canvas.transform);                    //canvasと親子にする。UIなのでcanvasがないと生きられない
            appearHand[i].tag = SetTag(appearHand[i].GetComponent<Image>().sprite); //tagをセット
            appearHand[i].name = originHand[i].name;                                //名前をセット
            appearHand[i].enabled = true;                                           //初期状態が見えないので
        }
    }


    void Update()
    {
        HiddenOn(); //手を隠す

        //自分の手を取得
        int playerRSP;
        //グー
        if (Input.GetKeyDown(KeyCode.A))
        {
            playerRSP = 0;
        }
        //チョキ
        else if (Input.GetKeyDown(KeyCode.S))
        {
            playerRSP = 1;
        }
        //パー
        else if (Input.GetKeyDown(KeyCode.D))
        {
            playerRSP = 2;
        }
        else
        {
            playerRSP = -1;
        }

        LerpHand();
        CheckRSP(playerRSP);
    }

    //敵の手を隠す
    void HiddenOn()
    {
        //スペースを押したらHiddenを実行する(プロトタイプ)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isHiddenOn = !isHiddenOn; //ボタン1つでオン・オフ
        }
        //trueなら1つ目以外を隠す
        if (isHiddenOn)
        {
            appearHand[0].GetComponent<Image>().enabled = true;
            for (int i = 1; i < appearHand.Length; i++)
            {
                appearHand[i].GetComponent<Image>().enabled = false;
            }
        }
        //falseなら通常通り、全部表示する
        else
        {
            for (int i = 0; i < appearHand.Length; i++)
            {
                appearHand[i].GetComponent<Image>().enabled = true;
            }
        }
    }

    void LerpHand()
    {
        //関数MoveHandが一度は呼ばれてから動作したいため
        if (startTime > 0)
        {
            float diff = Time.timeSinceLevelLoad - startTime;   //差異
            float rate = diff / lerpTime;                       //滑らかに動かす速度調整

            //線形補間させ、滑らかに動かす
            for (int i = 0; i < appearHand.Length - 1; i++)
            {
                if (originHand[i].transform.position.y < appearHand[i].transform.position.y)
                {
                    //線形補間
                    appearHand[i].transform.position = Vector3.Lerp(
                        originHand[i + 1].transform.position,
                        originHand[i].transform.position,
                        rate
                        );
                }
            }
        }
    }

    //自分の手と敵の手を判決
    void CheckRSP(int playerRSP)
    {
        //敵の手を判定
        int enemyRSP;
        if (appearHand[0].tag == "Rock")
        {
            enemyRSP = 0;
        }
        else if (appearHand[0].tag == "Scissors")
        {
            enemyRSP = 1;
        }
        else if (appearHand[0].tag == "Paper")
        {
            enemyRSP = 2;
        }
        else
        {
            enemyRSP = -1;
        }

        //手が設定されていないなら回さない
        if (playerRSP == -1 || enemyRSP == -1)
        {
            return;
        }

        //勝敗の計算
        int bout = (enemyRSP - playerRSP + 3) % 3;
        //引き分け
        if (bout == 0)
        {
            Drow();
        }
        //勝ち
        else if (bout == 1)
        {
            Win();
        }
        //負け
        else if (bout == 2)
        {
            Lose();
        }

        MoveHand();
    }

    //ランダムでじゃんけんの手を決める
    Sprite GetNextHand()
    {
        Sprite hand = null;
        int enemyHand = Random.Range(0, 3);
        switch (enemyHand)
        {
            case 0:
                hand = rock;
                break;

            case 1:
                hand = scissors;
                break;

            case 2:
                hand = paper;
                break;
        }

        return hand;
    }

    //手前を消し、1つずつずらし、最後尾に手を追加する
    void MoveHand()
    {
        //画像を1つずつ前にずらす
        Destroy(appearHand[0].gameObject);
        for (int i = 0; i < appearHand.Length - 1; i++)
        {
            appearHand[i] = appearHand[i+1];
        }

        //名前を１つずつずらす
        for (int i = 0; i < appearHand.Length - 1; i++)
        {
            appearHand[i].name = originHand[i].name;
        }

        //インスタンス生成
        appearHand[appearHand.Length - 1] = Instantiate(
            originHand[appearHand.Length - 1], 
            originHand[appearHand.Length - 1].
            transform.position, Quaternion.identity
            ) as Image;

        appearHand[appearHand.Length - 1].GetComponent<Image>().sprite = GetNextHand();                                 //ずらして空になったところに絵を入れる
        appearHand[appearHand.Length - 1].transform.SetParent(canvas.transform);                                        //canvasと親子にする。UIなのでcanvasがないと生きられない
        appearHand[appearHand.Length - 1].tag = SetTag(appearHand[appearHand.Length - 1].GetComponent<Image>().sprite); //tagを設定
        appearHand[appearHand.Length - 1].name = originHand[appearHand.Length - 1].name;                                //名前をセット
        appearHand[appearHand.Length - 1].enabled = true;                                                               //初期状態では見えないので
        appearHand[0].rectTransform.localScale = new Vector3(1.5f, 1.5f, 1f);                                           //手前だけ大きく

        startTime = Time.timeSinceLevelLoad; //線形補間用の時間初期値
    }

    //tagをセットする
    string SetTag(Sprite obj)
    {
        string tagName;
        if (obj == rock)
        {
            tagName = "Rock";
        }
        else if (obj == scissors)
        {
            tagName = "Scissors";
        }
        else if (obj == paper)
        {
            tagName = "Paper";
        }
        else
        {
            tagName = null;
        }

        return tagName;
    }

    void Win()
    {
        enemyStatus.HP -= 3;
        AudioClip SE = Resources.Load("strong") as AudioClip; //強攻撃の効果音を取得
        SEBox.PlayOneShot(SE, 3f); //効果音を鳴らす
        result.GetComponent<Image>().sprite = imageWin;
    }

    void Drow()
    {
        playerStatus.HP -= 1;
        enemyStatus.HP -= 1;
        AudioClip SE = Resources.Load("normal") as AudioClip; //強攻撃の効果音を取得
        SEBox.PlayOneShot(SE, 3f); //効果音を鳴らす
        result.GetComponent<Image>().sprite = imageDrow;
    }

    void Lose()
    {
        playerStatus.HP -= 3;
        AudioClip SE = Resources.Load("damaged") as AudioClip; //強攻撃の効果音を取得
        SEBox.PlayOneShot(SE, 3f); //効果音を鳴らす
        result.GetComponent<Image>().sprite = imageLose;
    }
}