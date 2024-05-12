using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPool : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private GameObject PrefabCoin;
    [SerializeField]
    private List<GameObject> CoinList;

    [SerializeField] 
    private int poolSize = 10;
    void Start()
    {
        AddCoins(poolSize);
    }

    private void AddCoins(int ncoins)
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject coin = Instantiate(PrefabCoin);
            coin.SetActive(false);
            CoinList.Add(coin);
            coin.transform.parent = transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject RequestCoin()
    {
        //trobar el primer inactiu, activarlo i retornarlo
        for (int i =0; i < CoinList.Count; i++)
        {
            if (!CoinList[i].activeSelf)
            {
                CoinList[i].SetActive(true);
                return CoinList[i];
            }
        }
        //per afegir dinamicament a la llista
        AddCoins(1);
        CoinList[CoinList.Count - 1].SetActive(true);
        return CoinList[CoinList.Count - 1];
    }
}
