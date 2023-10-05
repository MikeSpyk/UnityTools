using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BasicTools
{
    namespace Audio
    {
        [CustomPropertyDrawer(typeof(SoundAsset))]
        public class SoundAssetDrawer : PropertyDrawer
        {
            public override VisualElement CreatePropertyGUI(SerializedProperty property)
            {
                // Create property container element.
                var container = new VisualElement();

                Foldout foldout = new Foldout() { text = "Settings" };

                // Create property fields.
                var audioClipField = new PropertyField(property.FindPropertyRelative("m_audioClip")) { label = string.Format("{0}: AudioClip", property.displayName) };
                var volumeField = new PropertyField(property.FindPropertyRelative("m_volume"));
                var rangeField = new PropertyField(property.FindPropertyRelative("m_range"));
                var priorityField = new PropertyField(property.FindPropertyRelative("m_priority"));
                var falloffField = new PropertyField(property.FindPropertyRelative("m_falloff"));
                var warmUpCountField = new PropertyField(property.FindPropertyRelative("m_warmUpCount"));
                var soundCategoryField = new PropertyField(property.FindPropertyRelative("m_soundCategory"));

                // Add fields to the container.
                container.Add(audioClipField);
                container.Add(foldout);
                foldout.Add(volumeField);
                foldout.Add(rangeField);
                foldout.Add(priorityField);
                foldout.Add(falloffField);
                foldout.Add(warmUpCountField);
                foldout.Add(soundCategoryField);

                return container;
            }
        }
    }
}