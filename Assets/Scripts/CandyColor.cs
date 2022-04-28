using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyColor : MonoBehaviour
{
    public enum ColorType
    {
        RED,
        ORANGE,
        BLUE,
        PURPLE,
        COUNT,
    }

    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType type;
        public Sprite sprite;
    }

    public ColorSprite[] colorSprites;
    private Dictionary<ColorType, Sprite> colorSpriteDict;

    private ColorType color;
    public ColorType Color
    {
        get { return color; }
        set {
            SetColor(value);
        }
    }

    private SpriteRenderer spriteRenderer;
    public void SetColor(ColorType value)
    {
        color = value;
        if (colorSpriteDict.ContainsKey(value))
            spriteRenderer.sprite = colorSpriteDict[value];
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        colorSpriteDict = new Dictionary<ColorType, Sprite>();
        for (int i = 0; i < colorSprites.Length; i++)
        {
            if (!colorSpriteDict.ContainsKey(colorSprites[i].type)) {
                colorSpriteDict.Add(colorSprites[i].type, colorSprites[i].sprite);
            }
        } 
    }

    public int NumColors
    {
        get { return colorSprites.Length; }
    }
}
