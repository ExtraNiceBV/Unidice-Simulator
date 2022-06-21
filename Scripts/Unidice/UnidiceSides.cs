using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unidice.SDK.Interfaces;
using Unidice.SDK.Unidice;
using UnityEngine;
using UnityEngine.Events;

namespace Unidice.Simulator.Unidice
{
    [Serializable]
    public class UnidiceSides : IUnidiceSides
    {
        [SerializeField] private UnidiceSide top;
        [SerializeField] private UnidiceSide bottom;
        [SerializeField] private UnidiceSide left;
        [SerializeField] private UnidiceSide right;
        [SerializeField] private UnidiceSide front;
        [SerializeField] private UnidiceSide back;

        private UnidiceSide[] _all;
        private Transform _transform;
        private ImageDatabase _database;

        public SideEvent OnTapEnabled { get; } = new SideEvent();
        public SideEvent OnTapDisabled { get; } = new SideEvent();

        public void Initialize(Transform transform, ImageDatabase database)
        {
            _all = new[] { top, bottom, left, right, front, back };
            _transform = transform;
            _database = database;

            foreach (var side in _all) side.Initialize(database);
        }

        public void Clear()
        {
            foreach (var side in _all)
            {
                side.Set(null);
            }
        }

        public void SetSide(ISide side, ImageSequence sequence)
        {
            if (side == SideWorld.All)
            {
                foreach (var s in _all)
                {
                    s.Set(sequence);
                }
            }
            else
            {
                GetSide(side).Set(sequence);
            }
        }

        public void SetAllSides(params ImageSequence[] sequences)
        {
            var i = 0;
            foreach (var side in _all)
            {
                // Use sequence or set to empty
                if (sequences != null)
                    side.Set(i < sequences.Length ? sequences[i++] : null);
                else side.Set(null);
            }
        }

        private UnidiceSide GetSide(ISide side)
        {
            if (side.IsLocal)
            {
                // Local
                if (side == SideLocal.Top)
                    return top;
                if (side == SideLocal.Bottom)
                    return bottom;
                if (side == SideLocal.Front)
                    return front;
                if (side == SideLocal.Back)
                    return back;
                if (side == SideLocal.Left)
                    return left;
                if (side == SideLocal.Right)
                    return right;
                throw new ArgumentOutOfRangeException();
            }
            else
            {
                // World
                var tDir = GetWorldDirection(side);

                foreach (var s in _all)
                {
                    if (s.MatchesDirection(tDir, _transform.rotation)) return s;
                }
                return null;
            }
        }

        private Vector3 GetWorldDirection(ISide side)
        {
            Vector3 tDir;
            if (side == SideWorld.Top)
                tDir = Vector3.up;
            else if (side == SideWorld.Bottom)
                tDir = -Vector3.up;
            //else if (side == SideWorld.Front)
            //    tDir = Vector3.forward;
            //else if (side == SideWorld.Back)
            //    tDir = -Vector3.forward;
            //else if (side == SideWorld.Left)
            //    tDir = Vector3.right;
            //else if (side == SideWorld.Right)
            //    tDir = -Vector3.right;
            else
                throw new ArgumentOutOfRangeException(nameof(side), side, null);

            return tDir;
        }

        public void Update()
        {
            foreach (var side in _all)
            {
                side.Update();
            }
        }

        public void EnableTap(ISide side, UnityAction<ISide> tapCallback)
        {
            if (tapCallback != null)
            {
                if (side == SideWorld.All)
                    for (var i = 0; i < _all.Length; i++)
                    {
                        var s = _all[i];
                        var local = SideLocal.Each[i];
                        s.AddTapListener(() => tapCallback.Invoke(local));
                    }
                else
                {
                    GetSide(side).AddTapListener(() => tapCallback.Invoke(side));
                }
            }

            OnTapEnabled.Invoke(side);
        }

        public void DisableTap(ISide side)
        {
            if (side == SideWorld.All)
                foreach (var s in _all)
                {
                    s.ClearTapListeners();
                }
            else
            {
                GetSide(side).ClearTapListeners();
            }
            OnTapDisabled.Invoke(side);
        }

        public void WaitForTap(ISide side, UnityAction<ISide> tapCallback)
        {
            void OnTapped(ISide local)
            {
                DisableTap(side);
                tapCallback?.Invoke(local);
            }

            EnableTap(side, OnTapped);
        }

        public async UniTask<bool> WaitForTapSequence(ISide side, CancellationToken cancellationToken = default)
        {
            return await WaitForTapSequence(side, null, cancellationToken);
        }

        public async UniTask<bool> WaitForTapSequence(ISide side, UnityAction<ISide> tapCallback, CancellationToken cancellationToken)
        {
            ISide tappedSide = null;
            var disabled = false;
            WaitForTap(side, local => tappedSide = local);
            OnTapDisabled.AddListener(s =>
            {
                if (s == side || s == SideWorld.All) disabled = true;
            });
            while (!disabled)
            {
                await UniTask.NextFrame(cancellationToken);
                if (tappedSide != null)
                {
                    tapCallback?.Invoke(tappedSide);
                    return true;
                }
            }

            return false;
        }

        public void Tap(ISide side)
        {
            if (side == SideWorld.All) throw new Exception("You can't tap all sides at once.");

            GetSide(side).Tap();
        }

        public bool CanTap(ISide side)
        {
            if (side == SideWorld.All) throw new Exception("You can't check all sides at once.");
            return GetSide(side).CanTap;
        }

        public ISide WorldSideToLocal(ISide side)
        {
            if (side.IsLocal) return side;
            var unidiceSide = GetSide(side);
            if (unidiceSide == top) return SideLocal.Top;
            if (unidiceSide == bottom) return SideLocal.Bottom;
            if (unidiceSide == front) return SideLocal.Front;
            if (unidiceSide == back) return SideLocal.Back;
            if (unidiceSide == left) return SideLocal.Left;
            if (unidiceSide == right) return SideLocal.Right;
            throw new ArgumentOutOfRangeException(nameof(side), side, "Side is not valid.");
        }

        public ImageSequence GetSideSequence(ISide side)
        {
            var unidiceSide = GetSide(side);
            return unidiceSide.CurrentSequence;
        }

        public void SetBrightness(ISide side, float percentage)
        {
            if (side == SideWorld.All)
                foreach (var s in SideLocal.Each)
                {
                    GetSide(s).Brightness = percentage;
                }
            else GetSide(side).Brightness = percentage;
        }

        public float GetBrightness(ISide side)
        {
            return side == SideWorld.All ? GetSide(SideWorld.Top).Brightness : GetSide(side).Brightness;
        }
    }
}