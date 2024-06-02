using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    public Customization player;
    [SerializeField]
    private Customization customization;

    public void myStart()
    {
        if (customization == null)
            customization = GetComponent<Customization>();
        if (customization != null)
        {
            int rand = Random.Range(0, customization.Hairs.Length);
            customization.SetHair(rand);

            rand = Random.Range(0, customization.HairsColors.Length);
            customization.SetHairColor(rand);

            rand = Random.Range(0, customization.Skins.Length);
            customization.SetSkin(rand);

            int indx = player.GetUniformIndex() + 1;
            if (indx >= customization.Uniforms.Length)
                indx = 0;
            customization.SetClothesColor(indx);
        }
    }
}
