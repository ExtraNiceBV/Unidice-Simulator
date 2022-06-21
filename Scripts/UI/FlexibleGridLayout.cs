using UnityEngine;
using UnityEngine.UI;

namespace HouseOfInfluence.UI
{
    public class FlexibleGridLayout : LayoutGroup
    {
        [SerializeField] private Vector2 cellSize;
        [SerializeField] private Vector2 spacing;
        [SerializeField] private Vector2Int grid;

        private Vector4[] _childPositions;

        public override void CalculateLayoutInputVertical()
        {
            if (_childPositions?.Length != rectChildren.Count) _childPositions = new Vector4[rectChildren.Count];

            grid.x = Mathf.CeilToInt(rectTransform.rect.width / (cellSize.x + spacing.x));
            grid.y = Mathf.CeilToInt(rectTransform.rect.height / (cellSize.y + spacing.y));

            for (var i = 0; i < rectChildren.Count; i++)
            {
                var child = rectChildren[i];
                var element = child.GetComponent<FlexibleGridLayoutElement>();
                if (!element) continue;

                child.anchorMin = Vector2.up;
                child.anchorMax = Vector2.up;

                var xPos = child.anchoredPosition.x - child.pivot.x * child.sizeDelta.x;
                var yPos = -child.anchoredPosition.y - child.pivot.y * child.sizeDelta.y;
                var cellSizeX = cellSize.x+spacing.x;
                var cellSizeY = cellSize.y+spacing.y;

                // When the scene is freshly loaded, positions are not set yet (because a LayoutGroup like this prevents storing those positions)
                if (child.anchoredPosition == Vector2.zero)
                {
                    xPos = cellSizeX * element.Column;
                    yPos = cellSizeY * element.Row;
                }

                element.Column = Mathf.RoundToInt(xPos / cellSizeX);
                element.Row = Mathf.RoundToInt(yPos / cellSizeY);
                xPos = cellSizeX * element.Column;
                yPos = cellSizeY * element.Row;
                var width = cellSize.x * element.ColumnSpan + spacing.x * (element.ColumnSpan-1);
                var height = cellSize.y * element.RowSpan + spacing.y * (element.RowSpan - 1);

                _childPositions[i] = new Vector4(xPos, yPos, width, height);
            }

            SetLayoutInputForAxis(grid.x * (cellSize.x + spacing.x), grid.x * (cellSize.x + spacing.x), -1, 0);
            SetLayoutInputForAxis(grid.y * (cellSize.y + spacing.y), grid.y * (cellSize.y + spacing.y), -1, 1);
        }


        public override void SetLayoutHorizontal()
        {
        }

        public override void SetLayoutVertical()
        {
            if (_childPositions == null) return;
            for (var i = 0; i < _childPositions.Length; i++)
            {
                var child = rectChildren[i];
                var pos = _childPositions[i];
                SetChildAlongAxis(child, 0, pos.x, pos.z);
                SetChildAlongAxis(child, 1, pos.y, pos.w);
                SetDirty();
            }
        }
    }
}
