﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyStatus : MonoBehaviour
{
    private Canvas enemyCanvas;                         //HPバー用のCanvas
    public int maxHP = 50;                              //最大体力
    public int HP;                                      //体力
    private GameObject HPbarPrefab;                          //読み込み用
    private Slider HPbar;                               //体力ゲージ
    public int strong = 5;                              //強攻撃
    public int normal = 2;                              //中攻撃
    public int weak = 1;                                //弱攻撃
    public float lerpTime = 1;                          //値が大きいほど早くなる
    public int startIndex = -1;                         //初期位置, RoopGeneratorのnextPos[]へプリセット
    public Vector2 enemyOffset = new Vector2(0f, 0f);   //エネミーの初期位置の調整
    public Vector2 barOffset = new Vector2(0f, -0f);    //バーの初期位置の調整

    private ResultCtrl resultCtrl;
    private bool isDied = false;                    //死んだかどうか


    public enum LerpMode
    {
        normal,
        sin
    }
    public LerpMode lerpMode = LerpMode.normal;


    void Start()
    {
        transform.position = InitGenerator.InitPos(startIndex);
        //初期位置をoffsetを加味した位置にする
        transform.position = new Vector2(
            transform.position.x + 3f + enemyOffset.x,
            transform.position.y + enemyOffset.y
            );

        enemyCanvas = GameObject.FindWithTag("EnemyCanvas").GetComponent<Canvas>();
        HPbarPrefab = Instantiate(Resources.Load("EnemyHPbar")) as GameObject; //HPバー生成
        HPbar = this.HPbarPrefab.GetComponent<Slider>();
        HPbar.name += startIndex;                           //名前セット
        HPbar.transform.SetParent(enemyCanvas.transform);   //親子関連付け
        HP = maxHP;                                         //体力を初期化
        HPbar.maxValue = maxHP;                             //スライダーの最大値を最大体力に合わせる
        HPbar.value = HPbar.maxValue;                       //最大値を変化させた分、初期valueも合わせる
        //位置も合わせる
        HPbar.transform.position = new Vector2(transform.position.x + barOffset.x, transform.position.y - 1.5f + barOffset.y);

        resultCtrl = FindObjectOfType<ResultCtrl>();
    }


    void Update()
    {
        HPbar.value = HP; //受けたダメージをスライダーに反映させる
        if(HP <= 0 && !isDied)
        {
            resultCtrl.EnemyDead();
            isDied = true;
        }
    }
}
