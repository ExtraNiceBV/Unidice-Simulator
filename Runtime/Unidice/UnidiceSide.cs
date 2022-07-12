using System;
using Unidice.SDK.Unidice;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Unidice.Simulator.Unidice
{
    [Serializable]
    internal class UnidiceSide
    {
        private static readonly int ID_EmissionMap = Shader.PropertyToID("_EmissionMap");
        private static readonly int ID_MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int ID_Brightness = Shader.PropertyToID("_Brightness");

        [SerializeField] private TappableScreen screen;
        [SerializeField] private Vector3 normal;
        [SerializeField] private string name;

        private readonly UnityEvent _onTap = new UnityEvent();
        private Texture2D _currentTexture;
        private ImageDatabase _database;
        private double _startTime;
        private Material _material;
        private double _timeLastRandomFrame;
        private int _lastRandomFrame;
        public ImageSequence CurrentSequence { get; private set; }

        public bool CanTap { get; private set; }

        public float Brightness
        {
            set => _material.SetFloat(ID_Brightness, value);
            get => _material.GetFloat(ID_Brightness);
        }

        public override string ToString()
        {
            return name;
        }

        public void Initialize(ImageDatabase database)
        {
            _database = database;
            _material = screen.GetMaterial();
            Brightness = 1;
            screen.onDoubleClick.AddListener(Tap);
        }

        public void Set(ImageSequence sequence)
        {
            _startTime = Time.realtimeSinceStartupAsDouble;
            _timeLastRandomFrame = 0;
            CurrentSequence = sequence;
            Update();
        }

        public void Tap()
        {
            _onTap.Invoke();
        }

        public void AddTapListener(UnityAction callback)
        {
            _onTap.AddListener(callback);
            CanTap = true;
        }

        public void ClearTapListeners()
        {
            _onTap.RemoveAllListeners();
            CanTap = false;
        }

        private void SetTexture(Texture2D image)
        {
            _material.SetTexture(ID_EmissionMap, image);
            _material.SetTexture(ID_MainTex, image);
        }

        public void Update()
        {
            Texture2D texture = null;
            if (CurrentSequence)
            {
                if (CurrentSequence.Indices == null)
                {
                    Debug.LogError($"Sequence {CurrentSequence.name} has not been loaded.", CurrentSequence);
                }
                else
                {
                    var index = CurrentSequence.Indices[GetIndex(Time.realtimeSinceStartupAsDouble - _startTime, CurrentSequence)];
                    texture = _database.GetTexture(index);
                }
            }

            if (texture != _currentTexture)
            {
                _currentTexture = texture;
                SetTexture(_currentTexture);
            }
        }

        public bool MatchesDirection(Vector3 direction, Quaternion dieRotation)
        {
            var sideRotation = dieRotation * normal;
            return Vector3.Dot(direction, sideRotation) > 0.5f;
        }

        private int GetIndex(double time, ImageSequence sequence)
        {
            if (sequence.Animation.Count <= 1) return 0;

            switch (sequence.Loop)
            {
                case ImageSequence.LoopMode.Once:
                    return Mathf.FloorToInt(Mathf.Clamp((float)(time * sequence.FPS), 0, sequence.Indices.Count - 1));
                case ImageSequence.LoopMode.Loop:
                    return Mathf.FloorToInt((float)(time * sequence.FPS) % CurrentSequence.Animation.Count);
                case ImageSequence.LoopMode.PingPong:
                    // Offset so start and end aren't displayed 2x as long
                    return Mathf.FloorToInt(Mathf.PingPong((float)(time * sequence.FPS) - 0.5f, sequence.Animation.Count) + 0.5f);
                case ImageSequence.LoopMode.Random:
                    return GetRandom(time, sequence.FPS, sequence.Animation.Count);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int GetRandom(double time, float fps, int frames)
        {
            if (frames <= 1) return 0;
            if (time > _timeLastRandomFrame + 1f / fps)
            {
                _timeLastRandomFrame = time;

                int index;
                while ((index = Random.Range(0, frames)) == _lastRandomFrame) { }

                _lastRandomFrame = index;
                return index;
            }

            return _lastRandomFrame;
        }
    }
}