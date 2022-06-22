using TMPro;
using UnityEngine;

namespace Template.Generators
{
    public class PlayerImageGenerator : SequenceGenerator
    {
        [Header("References")] 
        [SerializeField] private TMP_Text textLabel;
        [SerializeField] private SpriteRenderer rendererSprite;

        public Texture2D CreateTexture(Sprite icon, string labelText)
        {
            rendererSprite.sprite = icon;
            camera.targetTexture = renderTexture;
            textLabel.text = labelText;
            return CreateTexture(labelText);
        }
    }
}