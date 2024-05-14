using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player; // Referencia al transform del jugador
    public float speed = 5f; // Velocidad de movimiento del enemigo

    void Update()
    {
        if (player != null)
        {
            // Calcula la dirección hacia el jugador
            Vector3 direction = player.position - transform.position;
            direction.Normalize(); // Normaliza la dirección para obtener un vector de longitud 1

            // Mueve al enemigo hacia el jugador
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }
}
