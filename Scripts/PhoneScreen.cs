using UnityEngine;

namespace Unidice.Simulator
{
    [ExecuteAlways]
    public class PhoneScreen : MonoBehaviour
    {
        [SerializeField] private new MeshRenderer renderer;
        [SerializeField] private RenderTexture renderTexturePortrait;
        [SerializeField] private RenderTexture renderTextureLandscape;
        private static readonly int _texture = Shader.PropertyToID("_MainTex");

        private uint _portraitUpdateCount = uint.MaxValue;
        private uint _landscapeUpdateCount = uint.MaxValue;

        public bool UseLandscapeMode { get; private set; }

        public void OnRenderObject()
        {
            CheckForUpdatedTexture();
        }

        private void CheckForUpdatedTexture()
        {
            // If one of the textures has been updated, we change mode.
            // So if you want to switch modes, just start rendering to the other texture.

            if (_landscapeUpdateCount != renderTextureLandscape.updateCount)
            {
                _landscapeUpdateCount = renderTextureLandscape.updateCount;
                if (!UseLandscapeMode) SetLandscapeMode(true);
            }

            if (_portraitUpdateCount != renderTexturePortrait.updateCount)
            {
                _portraitUpdateCount = renderTexturePortrait.updateCount;
                if (UseLandscapeMode) SetLandscapeMode(false);
            }
        }

        private void SetLandscapeMode(bool value)
        {
            UseLandscapeMode = value;
            var material = Application.isPlaying ? renderer.materials[1] : renderer.sharedMaterials[1];

            if(value)
                material.EnableKeyword("_LANDSCAPE_ON");
            else 
                material.DisableKeyword("_LANDSCAPE_ON");

            material.SetTexture(_texture, value ? renderTextureLandscape : renderTexturePortrait);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearDirty(material);
#endif
        }
    }
}