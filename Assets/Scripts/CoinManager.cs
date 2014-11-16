﻿using UnityEngine;
using UnityEngineExt;
using System.Collections;
using System;
using System.Text;

public class CoinManager : MonoBehaviour {
    public Transform dynamicObjects;
    public GameObject coinPrefab;

    private int coins;
    private StringBuilder coinsStr;

    void Start()
    {
        coinsStr = new StringBuilder("Coins: XXX");

        UpdateCoinsStr();
    }

    void OnEnable()
    {
        EventBus.OnBecomeDying += OnBecomeDying;
    }

    void OnDisable()
    {
        EventBus.OnBecomeDying -= OnBecomeDying;
    }

    void OnGUI () 
    {
        GUI.color = Color.red;
        GUILayout.Label(coinsStr.ToString());
    }

    private void OnBecomeDying(GameObject enemy)
    {
        if (coinPrefab == null) return;
        GameObject newCoin = GameObjectPool.Instance.Instantiate(coinPrefab, enemy.transform.position, Quaternion.identity);
        
        Transform sprite = enemy.transform.GetChild(0);
        BoxCollider2D bb = sprite.collider2D as BoxCollider2D;
        
        newCoin.transform.parent = dynamicObjects.transform;
        newCoin.transform.position += new Vector3(bb.center[0], bb.center[1], 0);

        coins++;

        UpdateCoinsStr();
    }

    private void UpdateCoinsStr()
    {
        coinsStr.Remove(0, coinsStr.Length);
        coinsStr.Append("Coins: ");
        coinsStr.Append(coins.ToStringLookup());
    }
}
