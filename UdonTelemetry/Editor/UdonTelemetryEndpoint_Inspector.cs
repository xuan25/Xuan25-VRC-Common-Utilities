#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Xuan25.UdonTelemetry
{
    [CustomEditor(typeof(UdonTelemetryEndpoint))]
    public class UdonTelemetryEndpoint_Inspector : Editor
    {
        public VisualTreeAsset m_InspectorXML;
        public VisualTreeAsset m_ArraySummaryDisplayXML;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            m_InspectorXML.CloneTree(root);

            BindArraySummaryDisplays(root);

            return root;
        }

        private void BindArraySummaryDisplays(VisualElement root)
        {
            var displays = root.Query<ArraySummaryDisplay>().ToList();

            foreach (var display in displays)
            {
                display.SetTemplate(m_ArraySummaryDisplayXML);

                if (string.IsNullOrEmpty(display.bindingPath))
                    continue;

                var property = serializedObject.FindProperty(display.bindingPath);
                display.BindProperty(property);

                if (property != null)
                {
                    root.TrackPropertyValue(property, _ => display.Refresh());
                }
            }
        }
    }
}

#endif
