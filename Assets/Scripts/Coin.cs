using UnityEngine;
using UnityEngine.SceneManagement;

public class Coin : MonoBehaviour
{
    public int scoreValue = 10; // Valor de la puntuación que añadirá al colisionar
    public float rotationSpeed = 1; // Velocidad de rotación de la moneda
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
        // Comprobar si la colisión es con el jugador
        if (other.CompareTag("Player"))
        {
            coinFX.Play();
            // Aumentar la puntuación
            orchestrator?.IncrementScoreWithCoins(scoreValue);

            // Desactivar la moneda
            coinPool.ReturnCoin(gameObject);
        }
    }

}
