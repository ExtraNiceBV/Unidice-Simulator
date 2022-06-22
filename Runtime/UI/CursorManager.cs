using System;
using Unidice.SDK.UI;
using UnityEngine;

namespace Unidice.Simulator.UI
{
    public class CursorManager : MonoBehaviour
    {
        [SerializeField] private Texture2D cursorNormal;
        [SerializeField] private Texture2D cursorHighlight;
        [SerializeField] private Texture2D cursorDrag;
        [SerializeField] private Texture2D cursorDragHighlight;
        private Vector2 _cursorHotspot;
        private CursorNotifier[] _notifiers;

        public void Start()
        {
            _cursorHotspot = new Vector2(cursorNormal.width / 2f, cursorNormal.height / 2f);
            Cursor.SetCursor(cursorNormal, _cursorHotspot, CursorMode.ForceSoftware);

            _notifiers = FindObjectsOfType<CursorNotifier>(true);
            foreach (var cursorNotifier in _notifiers)
            {
                cursorNotifier.onEnter.AddListener(OnChangeNotifier);
                cursorNotifier.onExit.AddListener(OnChangeNotifier);
                cursorNotifier.onHold.AddListener(OnChangeNotifier);
            }
        }

        private void OnChangeNotifier(CursorType type)
        {
            Cursor.SetCursor(GetCursor(type), _cursorHotspot, CursorMode.ForceSoftware);
        }

        private Texture2D GetCursor(CursorType type)
        {
            return type switch
            {
                CursorType.Normal => cursorNormal,
                CursorType.Highlight => cursorHighlight,
                CursorType.Drag => cursorDrag,
                CursorType.DragHighlight => cursorDragHighlight,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}