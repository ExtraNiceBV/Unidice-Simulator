using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Unidice.Simulator.Unidice
{
    public class TappableScreen : MonoBehaviour
    {
        public readonly UnityEvent onClick = new UnityEvent();
        public readonly UnityEvent onDoubleClick = new UnityEvent();
        [SerializeField] private new MeshRenderer renderer;

        public void OnClick(PointerEventData eventData)
        {
            switch (eventData.clickCount)
            {
                case 1:
                    onClick.Invoke();
                    break;
                case 2:
                    onDoubleClick.Invoke();
                    break;
            }
        }

        public Material GetMaterial()
        {
            return renderer.sharedMaterial = new Material(renderer.sharedMaterial); // Create copy so changes don't affect assets
        }
    }
}