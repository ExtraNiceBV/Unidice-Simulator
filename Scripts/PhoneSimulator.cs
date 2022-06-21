using System;
using Unidice.Simulator.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unidice.Simulator
{
    [ExecuteAlways]
    public class PhoneSimulator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Mode portrait;
        [SerializeField] private Mode landscape;

        [SerializeField] private float zoomSpeed = 2;
        [SerializeField] private RenderTextureRaycaster screenRaycaster;
        [SerializeField] private PhoneScreen phoneScreen;
        [SerializeField] private ButtonLock buttonLock;

        private bool _pointerInside;
        private float _zoomPercentage;
        private float _zoomVelocity;
        private bool _locked;

        private Mode ActiveMode => phoneScreen ? phoneScreen.UseLandscapeMode ? landscape : portrait : portrait;

        public void Start()
        {
            buttonLock.onToggled.AddListener(() => _locked = buttonLock.Locked);
            if (Application.isEditor)
            {
                buttonLock.Press(); // Has to be in Start
                _zoomPercentage = 1;
            }
        }

        public void Update()
        {
            if (Application.isPlaying)
            {
                var zoomIn = _pointerInside || screenRaycaster.IsCursorInside || _locked;
                
                _zoomPercentage = Mathf.SmoothDamp(_zoomPercentage, zoomIn ? 1 : 0, ref _zoomVelocity, 1 / zoomSpeed);

                transform.position = Vector3.Lerp(ActiveMode.zoomedOutLocation.position, ActiveMode.zoomedInLocation.position, _zoomPercentage);
                transform.rotation = Quaternion.Lerp(ActiveMode.zoomedOutLocation.rotation, ActiveMode.zoomedInLocation.rotation, _zoomPercentage);
                screenRaycaster.UseLandscapeCoordinates = phoneScreen && phoneScreen.UseLandscapeMode;
            }
            else
            {
                transform.position = ActiveMode.zoomedInLocation.position;
                transform.rotation = ActiveMode.zoomedInLocation.rotation;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.ClearDirty(transform);
#endif
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _pointerInside = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _pointerInside = false;
        }

        [Serializable]
        public class Mode
        {
            public Transform zoomedOutLocation;
            public Transform zoomedInLocation;
        }
    }
}