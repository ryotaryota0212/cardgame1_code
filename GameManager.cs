﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GamePlayerManager player;
    public GamePlayerManager enemy;

    [SerializeField] AI enemyAI;
    [SerializeField] UIManager uiManager;

    public Transform playerHandTransform,
                               playerFieldTransform,
                               enemyHandTransform,
                               enemyFieldTransform;

    [SerializeField] CardController cardPrefab;

    public bool isPlayerTurn;
    public Transform playerHero;
    public Transform enemyHero;

    // 時間管理
    int timeCount;

    //乱数

    // シングルトン化（どこからでもアクセスできるようにする）
    public static GameManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        uiManager.HideResultPanel();
        player.Init(new List<int>() { 1, 2, 4, 3 });
        enemy.Init(new List<int>() { 4, 3, 2, 2, 1});
        
        player.deck = new List<int>() ;//{ 3,1,2,2,7,5,7,9,8,6,4,2,5,7,8,1,3,4,5,6,9,3,6,1,4,7,9,6,4,3,1,4,5,8};
        enemy.deck = new List<int>()  ;//{ 2,1,3,1,3,4,6,9,1,5,9,6,4,2,5,6,8,5,1,2,3,4,5,6,6,8,9,1,9,4,1,8,2,7};
        
        for (int i = 0; i < 10; i++)
        {
            int x=UnityEngine.Random.Range(1,9);
            player.deck.Add(x);
        }

        for (int i = 0; i < 10; i++)
        {
            int y=UnityEngine.Random.Range(1,9);
            enemy.deck.Add(y);
        }

        uiManager.ShowHeroHP(player.heroHp, enemy.heroHp);
        uiManager.ShowManaCost(player.manaCost, enemy.manaCost);
        SettingInitHand();
        isPlayerTurn = true;
        TurnCalc();
    }


    public void ReduceManaCost(int cost, bool isPlayerCard)
    {
        if (isPlayerCard)
        {
            player.manaCost -= cost;
        }
        else
        {
            enemy.manaCost -= cost;
        }
        uiManager.ShowManaCost(player.manaCost, enemy.manaCost);
    }

    public void Restart()
    {
        // handとFiledのカードを削除
        foreach (Transform card in playerHandTransform)
        {
            Destroy(card.gameObject);
        }
        foreach (Transform card in playerFieldTransform)
        {
            Destroy(card.gameObject);
        }
        foreach (Transform card in enemyHandTransform)
        {
            Destroy(card.gameObject);
        }
        foreach (Transform card in enemyFieldTransform)
        {
            Destroy(card.gameObject);
        }


        // デッキを生成
        player.deck = new List<int>() { 3,1,2,2,7,5,7,9,8,6,4,2,5,7,8,1,3,4,5,6,9,3,6,1,4,7,9,6,4,3,1,4,5,8};
        enemy.deck = new List<int>()  { 2,1,3,1,3,4,6,9,1,5,9,6,4,2,5,6,8,5,1,2,3,4,5,6,6,8,9,1,9,4,1,8,2,7};

        StartGame();
    }

    void SettingInitHand()
    {
        // カードをそれぞれに3まい配る
        for (int i = 0; i < 3; i++)
        {
            GiveCardToHand(player.deck, playerHandTransform);
            GiveCardToHand(enemy.deck, enemyHandTransform);
        }
    }
    void GiveCardToHand(List<int> deck, Transform hand)
    {
        if (deck.Count == 0)
        {
            return;
        }
        int cardID = deck[0];
        deck.RemoveAt(0);
        CreateCard(cardID, hand);
    }

    void CreateCard(int cardID, Transform hand)
    {
        // カードの生成とデータの受け渡し
        CardController card = Instantiate(cardPrefab, hand, false);
        if (hand.name == "PlayerHand")
        {
            card.Init(cardID, true);
        }
        else
        {
            card.Init(cardID, false);
        }
    }

    void TurnCalc()
    {
        StopAllCoroutines();
        StartCoroutine(CountDown());
        if (isPlayerTurn)
        {
            PlayerTurn();
        }
        else
        {
            StartCoroutine(enemyAI.EnemyTurn());
        }
    }

    IEnumerator CountDown()
    {
        timeCount = 20;
        uiManager.UpdateTime(timeCount);

        while (timeCount > 0)
        {
            yield return new WaitForSeconds(1); // 1秒待機
            timeCount--;
            uiManager.UpdateTime(timeCount);
        }
        ChangeTurn();
    }

    public CardController[] GetEnemyFieldCards(bool isPlayer)
    {
        if (isPlayer)
        {
            return enemyFieldTransform.GetComponentsInChildren<CardController>();
        }
        else
        {
            return playerFieldTransform.GetComponentsInChildren<CardController>();
        }
    }
    public CardController[] GetFriendFieldCards(bool isPlayer)
    {
        if (isPlayer)
        {
            return playerFieldTransform.GetComponentsInChildren<CardController>();
        }
        else
        {
            return enemyFieldTransform.GetComponentsInChildren<CardController>();
        }
    }


    public void OnClickTurnEndButton()
    {
        if (isPlayerTurn)
        {
            ChangeTurn();
        }
    }

    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn;

        CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(playerFieldCardList, false);
        CardController[] enemyFieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(enemyFieldCardList, false);


        if (isPlayerTurn)
        {
            player.IncreaseManaCost();
            GiveCardToHand(player.deck, playerHandTransform);
        }
        else
        {
            enemy.IncreaseManaCost();
            GiveCardToHand(enemy.deck, enemyHandTransform);
        }
        uiManager.ShowManaCost(player.manaCost, enemy.manaCost);
        TurnCalc();
    }

    public void SettingCanAttackView(CardController[] fieldCardList, bool canAttack)
    {
        foreach (CardController card in fieldCardList)
        {
            card.SetCanAttack(canAttack);
        }
    }

    void PlayerTurn()
    {
        Debug.Log("Playerのターン");
        // フィールドのカードを攻撃可能にする
        CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(playerFieldCardList, true);
    }

    public void CardsBattle(CardController attacker, CardController defender)
    {
        Debug.Log("CardsBattle");
        Debug.Log("attacker HP:" + attacker.model.hp);
        Debug.Log("defender HP:" + defender.model.hp);

        attacker.Attack(defender);
        defender.Attack(attacker);
        Debug.Log("attacker HP:" + attacker.model.hp);
        Debug.Log("defender HP:" + defender.model.hp);
        attacker.CheckAlive();
        defender.CheckAlive();
    }


    public void AttackToHero(CardController attacker)
    {
        if (attacker.model.isPlayerCard)
        {
            enemy.heroHp -= attacker.model.at;
        }
        else
        {
            player.heroHp -= attacker.model.at;
        }
        attacker.SetCanAttack(false);
        uiManager.ShowHeroHP(player.heroHp, enemy.heroHp);
    }
    public void HealToHero(CardController healer)
    {
        if (healer.model.isPlayerCard)
        {
            player.heroHp += healer.model.at;
        }
        else
        {
            enemy.heroHp += healer.model.at;
        }
        uiManager.ShowHeroHP(player.heroHp, enemy.heroHp);
    }
    public void CheckHeroHP()
    {
        if (player.heroHp <= 0 || enemy.heroHp <= 0)
        {
            ShowResultPanel(player.heroHp);
        }
    }
    void ShowResultPanel(int heroHp)
    {
        StopAllCoroutines();
        uiManager.ShowResultPanel(heroHp);
    }

    /*void Shuffle()
    {   /*枚数を取得*/
       /* int k = player.deck.Count;

        while(k>1)
        {
            k--;

            int q = Random.Range(0,k+1);

            ランダムな順番のカードと最後のカードを入れ替える
            最後のカードは固定させて、一つ手前のカードを最後のカードとする
            デッキが固定されるまでくりかえす*/
            /*
            int temp = player.deck[q];
            deck[q] =deck[k];
            deck[k]=temp;
        }
    }*/
}