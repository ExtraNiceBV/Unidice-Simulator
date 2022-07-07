using Unidice.SDK.Unidice;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Template.Generators
{
    public abstract class SequenceGenerator : MonoBehaviour
    {
        [SerializeField] private Texture2D[] backgroundLayers;

#pragma warning disable CS0109 // complaining about new being not needed
        [SerializeField] protected new Camera camera;
#pragma warning restore CS0109

        [SerializeField] protected RenderTexture renderTexture;

        protected ImageSequence CreateSequence(Texture2D texture, string title)
        {
            return ImageSequence.Create(title, new[] { texture }, backgroundLayers, quality: 0.35f);
        }

        protected Texture2D CreateTexture(string title)
        {
            var tex = new Texture2D(renderTexture.width, renderTexture.height, renderTexture.graphicsFormat, TextureCreationFlags.None);
            RenderTexture.active = renderTexture;
            camera.Render();
            tex.name = title;
            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();
            return tex;
        }
    }
}