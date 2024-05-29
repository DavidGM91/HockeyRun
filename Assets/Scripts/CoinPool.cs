using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine;

public class CoinPool : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private GameObject PrefabCoin;

    [SerializeField]
    private List<GameObject> CoinList = new List<GameObject>();

    [SerializeField]
    private ObjectPool<GameObject> coinPool;

    public int activeCoins = 0;
    public int unactiveCoins = 0;
    public int totalCoins = 0;


    [SerializeField]
    private int minPoolSize = 10;
    [SerializeField]
    private int maxPoolSize = 1000;

    private Transform playerTransform;
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        coinPool = new ObjectPool<GameObject>(() => {
            GameObject coin = Instantiate(PrefabCoin);
            coin.GetComponent<Coin>().coinPool = this;
            return coin;
        },
        coin => { 
            coin.gameObject.SetActive(true);
            CoinList.Add(coin);
        },
        coin => {
            coin.gameObject.SetActive(false);
            CoinList.Remove(coin);
        },
        coin => { 
            Destroy(coin.gameObject);
            CoinList.Remove(coin);

        }, true, minPoolSize, maxPoolSize);

    }
    public void ReturnCoin(GameObject coin)
    {
        coinPool.Release(coin);
    }
    public void Restart()
    {
        List<GameObject> coinsToRemove = new List<GameObject>();
        foreach (GameObject coin in CoinList)
        {
            coinsToRemove.Add(coin);
        }
        foreach (GameObject coin in coinsToRemove)
        {
            coinPool.Release(coin);
        }
    }
    public GameObject RequestCoin()
    {
        return coinPool.Get();
    }
}
