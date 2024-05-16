using UnityEngine;
using UnityEngine.SceneManagement;

public class Coin : MonoBehaviour
{
    public int scoreValue = 10; // Valor de la puntuaci�n que a�adir� al colisionar
    public float rotationSpeed = 1; // Velocidad de rotaci�n de la moneda
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

    void OnTriggerEnter(Collider other)
    {
        // Comprobar si la colisi�n es con el jugador
        if (other.CompareTag("Player"))
        {
            coinFX.Play();
            // Aumentar la puntuaci�n
            orchestrator?.IncrementScoreWithCoins(scoreValue);

            // Desactivar la moneda
            coinPool.ReturnCoin(gameObject);
        }
    }

}
