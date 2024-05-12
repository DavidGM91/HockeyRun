using UnityEngine;
using UnityEngine.SceneManagement;

public class Coin : MonoBehaviour
{
    public int scoreValue = 10; // Valor de la puntuación que añadirá al colisionar
    public float rotationSpeed = 1; // Velocidad de rotación de la moneda
    public AudioSource coinFX;

    private Orchestrator orchestrator;

    private void Start()
    {
        orchestrator = GameObject.FindObjectOfType<Orchestrator>();
    }

    private void Update()
    {
        transform.Rotate(0, rotationSpeed, 0, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        coinFX.Play();
        this.gameObject.SetActive(false);
        // Comprobar si la colisión es con el jugador
        if (other.CompareTag("Player"))
        {
            // Aumentar la puntuación
            orchestrator?.IncrementScoreWithCoins(scoreValue);

            // Desactivar la moneda
            gameObject.SetActive(false);
        }
    }

}
