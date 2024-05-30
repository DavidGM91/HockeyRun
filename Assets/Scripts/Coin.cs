using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Coin : MonoBehaviour
{
    public int scoreValue = 10; // Valor de la puntuación que añadirá al colisionar
    public float rotationSpeed = 1; // Velocidad de rotación de la moneda
    public float timeAfterFail = 1; // Temps que trigarà en desaparèixer la moneda si no s'agafa
    public AudioSource coinFX;

    public CoinPool coinPool;
 

    [SerializeField]
    private Orchestrator orchestrator;

    private void Start()
    {
        if(orchestrator == null)
            orchestrator = GameObject.FindObjectOfType<Orchestrator>();
        if (coinFX == null)
            coinFX = GameObject.FindObjectOfType<AudioSource>();
        if (coinPool == null)
            coinPool = GameObject.FindObjectOfType<CoinPool>();
    }

    private void Update()
    {
        transform.Rotate(0, rotationSpeed, 0, Space.World);
    }

    IEnumerator ReturnCoinWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnCoin();
    }

    public void ReturnCoin()
    {
        coinPool.ReturnCoin(gameObject);
    }
    
    public void coinCollectorEvent(uint coinId, bool success)
    {
        if (success)
        {
            coinFX.Play();
            orchestrator.IncrementScoreWithCoins(scoreValue);
            ReturnCoin();
        }
        else
        {
            ReturnCoinWithDelay(timeAfterFail);
        }
    }

}
