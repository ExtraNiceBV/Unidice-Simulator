using Unidice.Simulator.Unidice;
using UnityEngine;
using UnityEngine.UI;

namespace Unidice.Simulator.UI
{
    public class ButtonShake : MonoBehaviour
    {
        private UnidiceRotator _rotator;
        [SerializeField] private Button button;

        public void Start()
        {
            _rotator = FindObjectOfType<UnidiceRotator>();
            button.onClick.AddListener(_rotator.Shake);
        }

        public void Shake()
        {
            _rotator.Shake();
        }
    }
}