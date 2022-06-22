using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using Unidice.SDK.Unidice;
using UnityEngine;

namespace Template.Generators
{
    public class NumberGenerator : SequenceGenerator
    {
        [Header("References")]
        [SerializeField] private TMP_Text text;
        [SerializeField] private Renderer orientationMarker;
        private readonly Dictionary<int, ImageSequence> _numbers = new Dictionary<int, ImageSequence>();

        public List<ImageSequence> Sequences { get; } = new List<ImageSequence>();

        public void Awake()
        {
            camera.targetTexture = renderTexture;
        }

        public ImageSequence GetNumber(int number)
        {
            if (_numbers.TryGetValue(number, out var sequence)) return sequence;
            sequence = CreateSequence(number);
            _numbers.Add(number, sequence);
            return sequence;
        }

        private ImageSequence CreateSequence(int i)
        {
            var numberString = i.ToString(CultureInfo.InvariantCulture);
            text.text = numberString;

            orientationMarker.enabled = numberString.All(c => c == '6') || numberString.All(c => c == '9');

            var title = $"Number {i}";
            var sequence = CreateSequence(CreateTexture(title), title);
            return sequence;
        }

        public IEnumerable<ImageSequence> GetSequences(int from, int to)
        {
            for (var i = from; i < to + 1; i++) yield return GetNumber(i);
        }
    }
}