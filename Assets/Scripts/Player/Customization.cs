using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customization : MonoBehaviour
{
    [SerializeField]
    private Material Clothes;
    [SerializeField]
    private Material Skin;
    [SerializeField]
    private Material Head;
    [SerializeField] 
    private Material Wheels;

    [SerializeField]
    private GameObject HairsParent;

    [System.Serializable]
    public class Uniform
    {
        public Color Vest;
        public Color Pants;
        public Color Shirt;
        public Color Shoes;
        public Color Wheels;
        public Color Socks_1;
        public Color Socks_2;
        public Color Wrists;
        public Color HairPiece_1;
        public Color HairPiece_2;
        public string Name;
    }

    public Uniform[] Uniforms;

    [System.Serializable]
    public class NamedColor
    {
        public string Name;
        public Color Color;
    }

    public NamedColor[] Skins;
    public NamedColor[] HairsColors;

    [HideInInspector]
    public GameObject[] Hairs;
    // Start is called before the first frame update
    private int lastHairIndex = -1;

    private int uniformIn = 0;
    private int skinIn = 0;
    private int hairIn = 0;
    private int hairColorIn = 0;

    private int toSaveUniform = 0;
    private int toSaveSkin = 0;
    private int toSaveHair = 0;
    private int toSaveHairColor = 0;

    public void SetHair(int index)
    {
        if (lastHairIndex != -1)
            Hairs[lastHairIndex].SetActive(false);
        else
        {
            for (int i = 0; i < Hairs.Length; i++)
            {
                if (i != index)
                    Hairs[i].SetActive(false);
            }
        }
        Hairs[index].SetActive(true);
        lastHairIndex = index;
        toSaveHair = index;
    }
    public void SetClothesColor(Color vest, Color pants, Color shirt, Color shoes, Color wheels, Color socks_1, Color socks_2, Color wrists, Color hairPiece_1, Color hairPiece_2)
    {
        Clothes.SetColor("_Color_1_Out", vest);
        Clothes.SetColor("_Color_2_Out", pants);
        Clothes.SetColor("_Color_3_Out", hairPiece_1);
        Clothes.SetColor("_Color_4_Out", shoes);
        Clothes.SetColor("_Color_5_Out", wrists);
        Skin.SetColor("_Color_1_Out", shirt);
        Skin.SetColor("_Color_2_Out", socks_1);
        Skin.SetColor("_Color_3_Out", socks_2);
        Head.SetColor("_Color_4_Out", hairPiece_2);
        Wheels.SetColor("_Color", wheels);
    }
    public void SetClothesColor(int uniformIndex)
    {
        toSaveUniform = uniformIndex;
        SetClothesColor(Uniforms[uniformIndex].Vest, Uniforms[uniformIndex].Pants, Uniforms[uniformIndex].Shirt, Uniforms[uniformIndex].Shoes, Uniforms[uniformIndex].Wheels, Uniforms[uniformIndex].Socks_1, Uniforms[uniformIndex].Socks_2, Uniforms[uniformIndex].Wrists, Uniforms[uniformIndex].HairPiece_1, Uniforms[uniformIndex].HairPiece_2);
    }
    public void SetSkin(int skinIndex)
    {
        toSaveSkin = skinIndex;
        Skin.SetColor("_Color_4_Out", Skins[skinIndex].Color);
        Head.SetColor("_Color_1_Out", Skins[skinIndex].Color);
    }
    public void SetHairColor(int hairColorIndex)
    {
        toSaveHairColor = hairColorIndex;
        Head.SetColor("_Color_2_Out", HairsColors[hairColorIndex].Color);
    }
    public string GetHairColorName(int hairColorIndex)
    {
        return HairsColors[hairColorIndex].Name;
    }
    public string GetSkinName(int skinIndex)
    {
        return Skins[skinIndex].Name;
    }
    public string GetUniformName(int uniformIndex)
    {
        return Uniforms[uniformIndex].Name;
    }
    public string GetHairName(int hairIndex)
    {
        return Hairs[hairIndex].name;
    }
    public void Save()
    {
        uniformIn = toSaveUniform;
        skinIn = toSaveSkin;
        hairIn = toSaveHair;
        hairColorIn = toSaveHairColor;
    }
    public void Rebert()
    {
        SetClothesColor(uniformIn);
        SetSkin(skinIn);
        SetHair(hairIn);
        SetHairColor(hairColorIn);
    }

    public int GetUniformIndex()
    {
        return uniformIn;
    }
    public int GetSkinIndex()
    {
        return skinIn;
    }
    public int GetHairIndex()
    {
        if (Hairs.Length == 0)
        {
            Hairs = new GameObject[HairsParent.transform.childCount];
            for (int i = 0; i < HairsParent.transform.childCount; i++)
            {
                Hairs[i] = HairsParent.transform.GetChild(i).gameObject;
            }
        }
        return hairIn;
    }
    public int GetHairColorIndex()
    {
        return hairColorIn;
    }
    void Start()
    {
        if (Hairs.Length == 0)
        {
            Hairs = new GameObject[HairsParent.transform.childCount];
            for (int i = 0; i < HairsParent.transform.childCount; i++)
            {
                Hairs[i] = HairsParent.transform.GetChild(i).gameObject;
            }
        }
        SetClothesColor(0);
        SetSkin(0);
        SetHair(0);
        SetHairColor(0);
    }
}
