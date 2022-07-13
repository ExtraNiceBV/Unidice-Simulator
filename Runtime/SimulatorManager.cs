using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Unidice.Simulator
{
    public class SimulatorManager : MonoBehaviour
    {
        [SerializeField] private Button buttonQuit;
        [SerializeField] private Button buttonLogo;

        private Canvas[] _canvases;

        public void Start()
        {
            buttonQuit.onClick.AddListener(OnQuitButton);
            buttonLogo.onClick.AddListener(OnLogoButton);
            _canvases = FindObjectsOfType<Canvas>().Where(c=>c.gameObject.scene == gameObject.scene).ToArray();
            // TODO: Rebuild canvases if resolution changes (for WebGL)
        }

        private static void OnLogoButton()
        {
            Application.OpenURL(@"https://unidice.world/en/");
        }

        private static void OnQuitButton()
        {
            if (Application.isEditor || Application.platform == RuntimePlatform.WebGLPlayer)
                SceneManager.LoadScene(0);
            else
                //Application.Quit();
                SceneManager.LoadScene(0);
            // TODO: Show different button based on situation
        }
    }
}