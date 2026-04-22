#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Xuan25.UdonTelemetry
{
    public class ArraySummaryDisplay : BindableElement
    {
        private SerializedProperty _boundProperty;

        private Label _countLabel;
        private Foldout _sampleFoldout;
        private VisualElement _sampleList;

        public int headCount { get; set; } = 2;
        public int tailCount { get; set; } = 2;
        public string countPrefix { get; set; } = "Count: ";
        public string ellipsisText { get; set; } = "...";
        public string emptyText { get; set; } = "(empty)";
        public bool defaultExpanded { get; set; } = false;

        public new class UxmlFactory : UxmlFactory<ArraySummaryDisplay, UxmlTraits> { }

        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            private readonly UxmlIntAttributeDescription _headCount =
                new UxmlIntAttributeDescription { name = "head-count", defaultValue = 2 };

            private readonly UxmlIntAttributeDescription _tailCount =
                new UxmlIntAttributeDescription { name = "tail-count", defaultValue = 2 };

            private readonly UxmlStringAttributeDescription _countPrefix =
                new UxmlStringAttributeDescription { name = "count-prefix", defaultValue = "Count: " };

            private readonly UxmlStringAttributeDescription _ellipsisText =
                new UxmlStringAttributeDescription { name = "ellipsis-text", defaultValue = "..." };

            private readonly UxmlStringAttributeDescription _emptyText =
                new UxmlStringAttributeDescription { name = "empty-text", defaultValue = "(empty)" };

            private readonly UxmlBoolAttributeDescription _defaultExpanded =
                new UxmlBoolAttributeDescription { name = "default-expanded", defaultValue = false };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var e = (ArraySummaryDisplay)ve;
                e.headCount = _headCount.GetValueFromBag(bag, cc);
                e.tailCount = _tailCount.GetValueFromBag(bag, cc);
                e.countPrefix = _countPrefix.GetValueFromBag(bag, cc);
                e.ellipsisText = _ellipsisText.GetValueFromBag(bag, cc);
                e.emptyText = _emptyText.GetValueFromBag(bag, cc);
                e.defaultExpanded = _defaultExpanded.GetValueFromBag(bag, cc);
            }
        }

        public ArraySummaryDisplay()
        {
        }

        public void SetTemplate(VisualTreeAsset template)
        {
            Clear();

            if (template == null)
            {
                Add(new Label("(missing template)"));
                return;
            }

            template.CloneTree(this);

            _countLabel = this.Q<Label>("count-label");
            _sampleFoldout = this.Q<Foldout>("sample-foldout");
            _sampleList = this.Q<VisualElement>("sample-list");

            if (_sampleFoldout != null)
                _sampleFoldout.value = defaultExpanded;
        }

        public void BindProperty(SerializedProperty property)
        {
            _boundProperty = property;
            Refresh();
        }

        public void Refresh()
        {
            if (_countLabel == null || _sampleList == null)
                return;

            _sampleList.Clear();

            if (_boundProperty == null)
            {
                _countLabel.text = "(unbound)";
                if (_sampleFoldout != null) _sampleFoldout.style.display = DisplayStyle.None;
                return;
            }

            if (_boundProperty.propertyType == SerializedPropertyType.String || !_boundProperty.isArray)
            {
                _countLabel.text = "(not array/list)";
                if (_sampleFoldout != null) _sampleFoldout.style.display = DisplayStyle.None;
                return;
            }

            int count = _boundProperty.arraySize;
            _countLabel.text = $"{countPrefix}{count}";

            if (_sampleFoldout != null)
                _sampleFoldout.style.display = DisplayStyle.Flex;

            if (count == 0)
            {
                _sampleList.Add(CreateValueOnlyRow(emptyText, "array-summary-empty-row"));
                return;
            }

            foreach (var entry in EnumerateSampleEntries(count))
            {
                switch (entry.kind)
                {
                    case SampleEntryKind.Item:
                        _sampleList.Add(CreateItemRow(entry.index, entry.text));
                        break;
                    case SampleEntryKind.Ellipsis:
                        _sampleList.Add(CreateEllipsisRow(ellipsisText));
                        break;
                }
            }
        }

        private IEnumerable<SampleEntry> EnumerateSampleEntries(int count)
        {
            int safeHead = Mathf.Max(0, headCount);
            int safeTail = Mathf.Max(0, tailCount);

            if (count <= safeHead + safeTail)
            {
                for (int i = 0; i < count; i++)
                {
                    yield return SampleEntry.Item(i, GetElementDisplayText(_boundProperty.GetArrayElementAtIndex(i)));
                }

                yield break;
            }

            for (int i = 0; i < safeHead; i++)
            {
                yield return SampleEntry.Item(i, GetElementDisplayText(_boundProperty.GetArrayElementAtIndex(i)));
            }

            yield return SampleEntry.Ellipsis();

            for (int i = count - safeTail; i < count; i++)
            {
                yield return SampleEntry.Item(i, GetElementDisplayText(_boundProperty.GetArrayElementAtIndex(i)));
            }
        }

        private static VisualElement CreateItemRow(int index, string value)
        {
            var row = new VisualElement();
            row.AddToClassList("array-summary-item-row");

            var indexLabel = new Label($"[{index}]");
            indexLabel.AddToClassList("array-summary-item-index");

            var valueLabel = new Label(value);
            valueLabel.AddToClassList("array-summary-item-value");
            valueLabel.tooltip = value;

            row.Add(indexLabel);
            row.Add(valueLabel);
            return row;
        }

        private static VisualElement CreateEllipsisRow(string text)
        {
            var row = new VisualElement();
            row.AddToClassList("array-summary-ellipsis-row");

            var label = new Label(text);
            row.Add(label);

            return row;
        }

        private static VisualElement CreateValueOnlyRow(string value, string className)
        {
            var row = new VisualElement();
            row.AddToClassList(className);

            var valueLabel = new Label(value);
            valueLabel.AddToClassList("array-summary-item-value");

            row.Add(valueLabel);
            return row;
        }

        private static string GetElementDisplayText(SerializedProperty property)
        {
            if (property == null)
                return "null";

            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    return property.stringValue ?? string.Empty;
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return property.boolValue ? "true" : "false";
                case SerializedPropertyType.Float:
                    return property.floatValue.ToString();
                case SerializedPropertyType.Enum:
                    return property.enumDisplayNames[property.enumValueIndex];
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue != null ? property.objectReferenceValue.name : "null";
                default:
                    return property.displayName;
            }
        }

        private enum SampleEntryKind
        {
            Item,
            Ellipsis
        }

        private readonly struct SampleEntry
        {
            public readonly SampleEntryKind kind;
            public readonly int index;
            public readonly string text;

            private SampleEntry(SampleEntryKind kind, int index, string text)
            {
                this.kind = kind;
                this.index = index;
                this.text = text;
            }

            public static SampleEntry Item(int index, string text) =>
                new SampleEntry(SampleEntryKind.Item, index, text);

            public static SampleEntry Ellipsis() =>
                new SampleEntry(SampleEntryKind.Ellipsis, -1, null);
        }
    }
}

#endif
