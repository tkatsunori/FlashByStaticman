﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class ResultCtrl : MonoBehaviour
{
    //スクリプト関係
    private PlayerStatus playerStatus;  //プレーヤーのステータス
    private PlayerAnim playerAnim;      //プレイヤーのアニメーション
    [HideInInspector]
    public EnemyStatus enemyStatus;     //敵のステータス
    private BattleRSP battleRSP;        //Input系の制御script
    private MoveStage moveStage;        //敵を撃破時に移動制御するscript
    private FadeManager fadeManager;    //フェードを管理するスクリプト
    private FlashingManager flashingManager;
    private StaticManager staticManager;

    //コンポーネント関係
    private AudioSource SoundBox;       //PlayOneShot用の空箱
    public Animator anim;               //Animation用の空箱

    //真偽値関係
    public bool isGameStop = true;      //ゲームが動いているかどうか
    private bool isGameEnd = false;     //クリア or ゲームオーバーしてゲームが終わったか
    private bool isOnHidden = false;    //手が隠されているか否か
    private bool isStampFinished = false;
    private int isOnce = 0;             //1回だけ通したい場合のbool値, 使う度に+1される

    //画像関係
    public Image result;                //じゃんけん判定を表示
    public Sprite imageCongra;          //終了画像
    public Sprite imageinvisible;       //透明の画像
    public Image startAtClick;          //クリックしてスタートの画像
    public Image deadLine;              //この線にじゃんけんの手が触れたらアウト
    public Image deadSkull;
    public Image predictBg;             //予測手の背景
    public Image predictHand;           //次の手の予測
    public Image rockPanel;             //グーのパネル, 何を押しているのか分かるように
    public Image scissorsPanel;         //チョキのパネル
    public Image paperPanel;            //パーのパネル
    public Image dangerImage;
    public Image scoreImage;
    public Text scoreText;
    public Image timerImage;
    public Text timertext;
    public GameObject playerHand;

    //音楽関係
    public AudioClip strong;
    public AudioClip normal;
    public AudioClip weak;
    public AudioClip damaged;
    public AudioClip dangerSound;

    //その他
    private GameObject[] enemys;        //Enemyを全て取得    
    [HideInInspector]
    public float scoreTimer;            //スコア加算用のタイマー
    public float startTime = 30f;      //初期時間


    void Start()
    {
        playerStatus = FindObjectOfType<PlayerStatus>();
        playerAnim = FindObjectOfType<PlayerAnim>();
        battleRSP = FindObjectOfType<BattleRSP>();
        moveStage = FindObjectOfType<MoveStage>();
        fadeManager = FindObjectOfType<FadeManager>();
        flashingManager = FindObjectOfType<FlashingManager>();
        staticManager = FindObjectOfType<StaticManager>();

        SoundBox = GameObject.Find("SoundBox").GetComponent<AudioSource>();
        anim = GameObject.FindWithTag("AnimCtrl").GetComponent<Animator>();

        GameObject[] enemy = GameObject.FindGameObjectsWithTag("Enemy");//.ToArray();
        enemys = enemy.OrderBy(e => Vector2.Distance(e.transform.position, transform.position)).ToArray(); //距離順でソートする
        enemyStatus = enemys[moveStage.winCounter].GetComponent<EnemyStatus>();

        fadeManager.FadeStart(null, waitForSeconds: 2f);  //指定秒待ってからスタートする
        StaticManager.resultScore = 0;  //バトル毎に初期化しておく
        scoreTimer = startTime;
    }


    void Update()
    {
        //ゲームが終わっていれば何もしない
        if (isGameEnd)
        {
            if (flashingManager.isAllRepeatFinished)
            {
                dangerImage.enabled = false;
                battleRSP.enabled = true;
                scoreImage.enabled = true;
                scoreText.enabled = true;
                playerHand.SetActive(true);
                isGameEnd = false;

            }
            else
            {
                predictHand.sprite = imageinvisible;
                deadLine.enabled = false;
                deadSkull.enabled = false;
                predictBg.enabled = false;
                battleRSP.enabled = false;
                //scoreImage.enabled = false;
                //scoreText.enabled = false;
                timerImage.enabled = false;
                timertext.enabled = false;
                return;
            }
        }
        else
        {
            IsPlayGame();   //ゲームが動いているか
            //OnHidden();

            float pingpong = Mathf.PingPong(Time.time * 1.2f, 1f); //pingpong関数で0と1を行ったり来たり
            startAtClick.color = new Color(startAtClick.color.r, startAtClick.color.g, startAtClick.color.b, pingpong); //点めつするようにする

            if (fadeManager.isFadeFinished && isOnce == 0)
            {
                isOnce++;
                anim.SetTrigger("Stamp"); //スタンプを表示
            }
        }

        scoreText.text = " Score: " + StaticManager.GetResultSocre();
        timertext.text = " Time : " + scoreTimer.ToString("N2");
    }

    void IsPlayGame()
    {
        //ゲームが止まっているなら
        if (isGameStop)
        {
            //ゲームスタート, 初めだけだけここ
            if (fadeManager.isFadeFinished && isOnce == 1 && isStampFinished)
            {
                isOnce++;
                isGameStop = false;
                deadLine.enabled = true;
                deadSkull.enabled = true;
                predictBg.enabled = true;
                startAtClick.enabled = false;   //点滅を消す
                scoreImage.enabled = true;
                scoreText.enabled = true;
                timerImage.enabled =true;
                timertext.enabled = true;
                EndAnim();
                battleRSP.StartGame();
            }
            //2回目以降はここ
            else if (fadeManager.isFadeFinished && isOnce == 2 && !moveStage.isFrying)
            {
                isGameStop = false;
                deadLine.enabled = true;
                deadSkull.enabled = true;
                predictBg.enabled = true;
                startAtClick.enabled = false;   //点滅を消す
                scoreImage.enabled = true;
                scoreText.enabled = true;
                timerImage.enabled =true;
                timertext.enabled = true;
                EndAnim();
                battleRSP.StartGame();
            }
            else
            {
                predictHand.sprite = imageinvisible;
                deadLine.enabled = false;
                deadSkull.enabled = false;
                predictBg.enabled = false;
                battleRSP.enabled = false;
                //scoreImage.enabled = false;
                //scoreText.enabled = false;
                timerImage.enabled = false;
                timertext.enabled = false;
            }
        }
        //止まっていないならscriptをオンにして正常に動かす
        else
        {
            scoreTimer -= Time.deltaTime;
            if(scoreTimer <= 0)
            {
                scoreTimer = 0f;
            }
            battleRSP.enabled = true;
            predictHand.sprite = battleRSP.appearHand[1].sprite;
        }
    }

    public void EnemyDead()
    {
        //敵がまだ居るのならステージ移動する
        if (moveStage.winCounter < InitGenerator.nextPos.Length - 1)
        {
            //result.GetComponent<Image>().sprite = imageinvisible; //ステージ移動時は画像が無いようにする
            moveStage.NextStage(GetComponent<ResultCtrl>(), battleRSP); //ステージ移動
            enemyStatus = enemys[moveStage.winCounter].GetComponent<EnemyStatus>(); //新しい敵のステータスを取得
            if (moveStage.winCounter == 4)
            {
                SoundBox.PlayOneShot(dangerSound, 3f);
                isGameEnd = true;
                dangerImage.enabled = true;
                deadSkull.enabled = true;
                flashingManager.FadeStart();
                scoreImage.enabled = false;
                scoreText.enabled = false;
                playerHand.SetActive(false);
            }
            scoreTimer = startTime; //スコアタイマーを初期化
        }
        //もう居ないのなら終了
        else
        {
            isGameStop = true;
            isGameEnd = true;
            result.GetComponent<Image>().sprite = imageCongra;
            fadeManager.isFadeFinished = false;
            fadeManager.fadeMode = FadeManager.FadeMode.close;
            fadeManager.FadeStart("Clear", waitForSeconds: 3f);
        }
    }

    //アニメーションを終了させる
    public void EndAnim()
    {
        anim.SetTrigger("Wait");
    }

    //勝ったときの手でアニメーションを変更
    public void StartAnim(int playerRSP)
    {
        string trigger = null;
        if (playerRSP == 0)
        {
            trigger = "Rock";
        }
        else if (playerRSP == 1)
        {
            trigger = "Scissors";
        }
        else if (playerRSP == 2)
        {
            trigger = "Paper";

        }
        anim.SetTrigger(trigger);
        playerAnim.DoingAnim(trigger);
    }

    public void StartAnim(string trigger)
    {
        playerAnim.DoingAnim(trigger);
    }

    //どのボタンを押しているか可視化する
    public void DownColorHand(int playerRSP)
    {
        Color tmp = new Color(255, 255, 255, 0);    //アルファ値は0～1のため
        float alpha = 0.15f;
        if (playerRSP == 0)
        {
            tmp = rockPanel.color;
            tmp.a = alpha;
            rockPanel.color = tmp;
            StartCoroutine(ReturnColor(rockPanel));

            Image child = rockPanel.transform.FindChild("Panel").GetComponent<Image>();
            tmp = child.color;
            child.color = tmp;
            tmp.a = alpha;
            child.color = tmp;
            StartCoroutine(ReturnColorBlack(child));
        }
        else if (playerRSP == 1)
        {
            tmp = scissorsPanel.color;
            tmp.a = alpha;
            scissorsPanel.color = tmp;
            StartCoroutine(ReturnColor(scissorsPanel));

            Image child = scissorsPanel.transform.FindChild("Panel").GetComponent<Image>();
            tmp = child.color;
            child.color = tmp;
            tmp.a = alpha;
            child.color = tmp;
            StartCoroutine(ReturnColorBlack(child));
        }
        else if (playerRSP == 2)
        {
            tmp = paperPanel.color;
            tmp.a = alpha;
            paperPanel.color = tmp;
            StartCoroutine(ReturnColor(paperPanel));

            Image child = paperPanel.transform.FindChild("Panel").GetComponent<Image>();
            tmp = child.color;
            child.color = tmp;
            tmp.a = alpha;
            child.color = tmp;
            StartCoroutine(ReturnColorBlack(child));
        }

    }

    IEnumerator ReturnColor(Image panel)
    {
        yield return new WaitForSeconds(0.5f);      //少し待ってから色を元に戻す
        panel.color = new Color(255, 255, 255, 0);
    }

    IEnumerator ReturnColorBlack(Image panel)
    {
        yield return new WaitForSeconds(0.5f);      //少し待ってから色を元に戻す
        panel.color = new Color(0, 0, 0, 0);
    }

    //敵の次の手を隠す
    void OnHidden()
    {
        //スペースを押したらHiddenを実行する(プロトタイプ)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isOnHidden = !isOnHidden; //ボタン1つでオン・オフ
        }
        //trueなら1つ目以外を隠す
        if (isOnHidden)
        {
            predictHand.enabled = false;
        }
        //falseなら通常通り、全部表示する
        else
        {
            predictHand.enabled = true;
        }
    }

    public void Win()
    {
        enemyStatus.HP -= playerStatus.win;
        SoundBox.PlayOneShot(strong, 3f); //効果音を鳴らす

        Vector2 diff = deadLine.transform.position - battleRSP.appearHand[0].transform.position;
        float castedDiff = -Camera.main.ScreenToWorldPoint(diff).x;

        float coefficient = 1;
        switch (staticManager.difficultyMode)
        {
            case StaticManager.DifficultyMode.Easy:
                coefficient = 10;
                break;

            case StaticManager.DifficultyMode.Normal:
                coefficient = 20;
                break;

            case StaticManager.DifficultyMode.Hard:
                coefficient = 40;
                break;
        }
        castedDiff *= coefficient;
        StaticManager.AddScore((int)castedDiff);
    }

    public void Drow()
    {
        playerStatus.HP -= enemyStatus.drow;
        enemyStatus.HP -= playerStatus.drow;
        SoundBox.PlayOneShot(normal, 3f); //効果音を鳴らす
        scoreTimer -= 0.5f;

        //互いにぶつかり合って衝突しているように見せる
        float range = 1.5f;
        float time = 0.5f;
        iTween.MoveFrom(playerStatus.gameObject, iTween.Hash("x", playerStatus.gameObject.transform.position.x + range, "time", time));
        iTween.MoveFrom(enemyStatus.gameObject, iTween.Hash("x", enemyStatus.gameObject.transform.position.x - range, "time", time));
    }

    public void Lose()
    {
        playerStatus.HP -= enemyStatus.win;
        SoundBox.PlayOneShot(damaged, 3f); //効果音を鳴らす

        playerAnim.DoingAnim("Damaged");
        enemys[moveStage.winCounter].GetComponent<Animator>().SetTrigger("Attack");
        scoreTimer -= 1f;
    }

    public void IsStampFinished()
    {
        isStampFinished = true;
    }
}
