using Unidice.SDK.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HouseOfInfluence.UI
{
    public class FlexibleGridLayoutElement : UIBehaviour
    {
        public int Column
        {
            get => _column;
            set { if (_column == value) return; _column = value; EditorWrapper.SetDirty(this); }
        }

        public int Row
        {
            get => _row;
            set { if(_row==value) return; _row = value; EditorWrapper.SetDirty(this); }
        }

        public int RowSpan { get; private set; }
        public int ColumnSpan { get; private set; }

        [SerializeField] private int _rowSpan = 1;
        [SerializeField] private int _columnSpan = 1;
        [SerializeField] private int _row = -1;
        [SerializeField] private int _column = -1;

        protected override void Awake()
        {
            base.Awake();
            RowSpan = _rowSpan;
            ColumnSpan = _columnSpan;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            // Update properties
            if (RowSpan != _rowSpan)
            {
                if (_rowSpan < 1) _rowSpan = 1;
                RowSpan = _rowSpan;
                MarkForRebuild();
            }
            if (ColumnSpan != _columnSpan)
            {
                if (_columnSpan < 1) _columnSpan = 1;
                ColumnSpan = _columnSpan;
                MarkForRebuild();
            }
        }

        private void MarkForRebuild()
        {
            if (!IsActive()) return;
            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }
#endif
    }
}