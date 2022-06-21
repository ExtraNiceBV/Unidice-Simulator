using System.Collections.Generic;
using Unidice.SDK.System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Unidice.Simulator
{
    public class RenderTextureRaycaster : BaseRaycaster
    {
#pragma warning disable CS0109 // complaining about new being not needed
        [SerializeField] private new Camera camera;
#pragma warning restore CS0109

        private AppRaycasterTarget _target;
        private Collider _collider;

        public bool UseLandscapeCoordinates { get; set; }

        protected override void Start()
        {
            _collider = GetComponent<Collider>();
            
            // _target can be null if the other scene isn't loaded yet
            _target = FindObjectOfType<AppRaycasterTarget>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // When we come from loader, we might not find it then
            if (!_target) _target = FindObjectOfType<AppRaycasterTarget>();

            //foreach (var targetCamera in _target.cameras)
            //{
            //    if(!targetCamera.depth.Equals(camera.depth)) Debug.LogError("The depth of the camera drawing the texture has to be equal to the depth of the camera rendering the texture.");
            //}
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (!eventData.pointerCurrentRaycast.isValid) return;
            if (!_target) return;

            var ray = camera.ScreenPointToRay(Input.mousePosition);

            if (!_collider.Raycast(ray, out var hitInfo, 50))
            {
                Debug.DrawRay(ray.origin, ray.direction*50, Color.red);
                IsCursorInside = false;
                return;
            }
            Debug.DrawRay(ray.origin, ray.direction*50, Color.green);
            IsCursorInside = true;


            var results = new List<RaycastResult>();
            var newEventData = new PointerEventData(_target.eventSystem);

            var matrixLandscape = new Matrix4x4(new Vector2(0, 1), new Vector2(1, 0), Vector4.zero, Vector4.zero);

            var coord = hitInfo.textureCoord;
            if (UseLandscapeCoordinates)
            {
                coord = matrixLandscape * coord;
                coord.x = 1 - coord.x;
            }

            CursorPosition = Vector2.Scale(coord, _target.cameras[0].pixelRect.size);

            eventData.position = CursorPosition;
            newEventData.position = CursorPosition;
            _target.eventSystem.RaycastAll(newEventData, results);

            foreach (var result in results)
            {
                // Don't include results from this scene
                // Not using LINQ because this runs A LOT
                var all = true;
                foreach (var c in _target.cameras)
                {
                    if (c.gameObject.scene != result.gameObject.scene) continue;
                    all = false;
                    break;
                }
                if (all) continue;

                resultAppendList.Add(new RaycastResult
                {
                    gameObject = result.gameObject,
                    module = result.module,
                    distance = 0,
                    depth = result.depth,
                    index = resultAppendList.Count,
                    worldPosition = result.worldPosition,
                    worldNormal = result.worldNormal
                });
            }

            // No hits? Block other raycasters
            if (resultAppendList.Count == 0)
            {
                resultAppendList.Add(new RaycastResult()
                {
                    module = this,
                    distance = 0,
                    index = resultAppendList.Count,
                    gameObject = gameObject
                });
            }
        }

        public override Camera eventCamera => _target.cameras[0];
        public Vector2 CursorPosition { get; private set; }
        public bool IsCursorInside { get; private set; }

        protected override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            base.OnDestroy();
        }
    }
}