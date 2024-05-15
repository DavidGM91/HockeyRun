using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCustomizeMenu : MonoBehaviour
{
    [SerializeField]
    private Customization customization;

    [SerializeField]
    private TextMeshProUGUI hairText;
    [SerializeField]
    private TextMeshProUGUI uniformText;
    [SerializeField]
    private TextMeshProUGUI skinText;
    [SerializeField]
    private TextMeshProUGUI hairColorText;

    private int hairIndex = 0;
    private int hairColorIndex = 0;
    private int uniformIndex = 0;
    private int skinIndex = 0;

    public void IndexHair(int index)
    {
        hairIndex+= index;
        if (hairIndex >= customization.Hairs.Length)
            hairIndex = 0;
        if (hairIndex < 0)
            hairIndex = customization.Hairs.Length - 1;
        customization.SetHair(hairIndex);
        hairText.text = customization.GetHairName(hairIndex);
    }
    public void IndexUniform(int index)
    {
        uniformIndex += index;
        if (uniformIndex >= customization.Uniforms.Length)
            uniformIndex = 0;
        if (uniformIndex < 0)
            uniformIndex = customization.Uniforms.Length - 1;
        customization.SetClothesColor(uniformIndex);
        uniformText.text = customization.GetUniformName(uniformIndex);
    }
    public void IndexSkin(int index)
    {
        skinIndex += index;
        if (skinIndex >= customization.Skins.Length)
            skinIndex = 0;
        if (skinIndex < 0)
            skinIndex = customization.Skins.Length - 1;
        customization.SetSkin(skinIndex);
        skinText.text = customization.GetSkinName(skinIndex);
    }
    public void IndexHairColor(int index)
    {
        hairColorIndex += index;
        if (hairColorIndex >= customization.HairsColors.Length)
            hairColorIndex = 0;
        if (hairColorIndex < 0)
            hairColorIndex = customization.HairsColors.Length - 1;
        customization.SetHairColor(hairColorIndex);
        hairColorText.text = customization.GetHairColorName(hairColorIndex);
    }
    public void Save()
    {
        customization.Save();
    }
    public void Cancel()
    {
        customization.Revert();
        UpdateTexts();
    }
    private void UpdateTexts()
    {
        hairText.text = customization.GetHairName(customization.GetHairIndex());
        uniformText.text = customization.GetUniformName(customization.GetUniformIndex());
        skinText.text = customization.GetSkinName(customization.GetSkinIndex());
        hairColorText.text = customization.GetHairColorName(customization.GetHairColorIndex());
    }

    private void Start()
    {
        UpdateTexts();
    }
}
