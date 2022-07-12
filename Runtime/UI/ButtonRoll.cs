using Unidice.Simulator.Unidice;
using UnityEngine;
using UnityEngine.UI;

namespace Unidice.Simulator.UI
{
    public class ButtonRoll : MonoBehaviour
    {
        private UnidiceRotator _rotator;
        [SerializeField] private Button button;

        public void Start()
        {
            _rotator = FindObjectOfType<UnidiceRotator>();
            _rotator.OnStartedRolling.AddListener(OnRolling);
            _rotator.OnRolled.AddListener(OnRolled);
            button.onClick.AddListener(_rotator.Roll);
        }

        private void OnRolled()
        {
            button.interactable = true;
        }

        private void OnRolling()
        {
            button.interactable = false;
        }
    }
}