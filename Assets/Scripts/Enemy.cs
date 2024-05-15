using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player; // Referencia al transform del jugador
    [SerializeField]
    private float speed = 15f; // Velocidad de movimiento del enemigo

    [SerializeField]
    private Orchestrator orchestrator;

    void Update()
    {
        if (player != null)
        {
            // Calcula la direcció fins al jugador
            Vector3 direction = player.position - transform.position;
            direction.Normalize(); 
            direction.y = 0;
            direction.x = 0;

            transform.Translate(direction * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //TODO: fer funcio endgame
            orchestrator.ShowMenu();
        }
    }
}
