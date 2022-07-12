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
            button.onClick.AddListener(_rotator.Roll);
        }

        public void Update()
        {
            button.interactable = !_rotator.IsBusy;
        }
    }
}