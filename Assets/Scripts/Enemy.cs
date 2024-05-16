using UnityEngine;

public class Enemy : MyMonoBehaviour
{
    public Transform player; // Referencia al transform del jugador
    [SerializeField]
    private float speed = 15f; // Velocidad de movimiento del enemigo

    [SerializeField]
    private Orchestrator orchestrator;

    [SerializeField]
    private Customization customization;

    override public void myUpdate()
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

    override public void myStart()
    {
        orchestrator = FindObjectOfType<Orchestrator>();
        customization = GetComponentInChildren<Customization>();
        if (customization != null)
        {
            int rand = Random.Range(0, customization.Hairs.Length);
            customization.SetHair(rand);

            rand = Random.Range(0, customization.HairsColors.Length);
            customization.SetHairColor(rand);

            rand = Random.Range(0, customization.Skins.Length);
            customization.SetSkin(rand);

            customization.SetClothesColor(player.GetComponentInChildren<Customization>().GetUniformIndex() + 1); //TODO: Mirar si està sobre el màxim
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
