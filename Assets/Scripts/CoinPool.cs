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

    private Transform playerTransform;
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
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
       checkPositionCoins();
    }
    public void ReturnCoin(GameObject coin)
    {
        coin.SetActive(false);
    }
    public void Restart()
    {
        foreach (GameObject coin in CoinList)
        {
            coin.SetActive(false);
        }
    }

    public void checkPositionCoins()
    {
        foreach (GameObject coin in CoinList)
        {
            // mirar posicio de cada moneda respecte jugador i tornarla a la pool si cal
            if (coin.activeSelf && coin.transform.position.z + 5 < playerTransform.position.z)
            {
                ReturnCoin(coin);
            }
        }
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
        //AddCoins(1);
        //CoinList[CoinList.Count - 1].SetActive(true);
        return null;//CoinList[CoinList.Count - 1];
    }
}
