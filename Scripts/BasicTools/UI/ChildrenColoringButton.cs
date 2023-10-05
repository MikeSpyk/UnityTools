using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BasicTools.UI
{
    /// <summary>
    /// a button, that doesn't only change its own color but the color of all of its children too
    /// </summary>
    public class ChildrenColoringButton : Button
    {
        private Component[] m_coloringTargets;

        protected override void Awake()
        {
            base.Awake();

            List<Component> colorChildren = new List<Component>();
            findColoringTargetsInChildren(transform, ref colorChildren);

            Image buttonImage = GetComponent<Image>();

            if (buttonImage == null)
            {
                m_coloringTargets = new Component[colorChildren.Count];
            }
            else
            {
                m_coloringTargets = new Component[colorChildren.Count + 1];
            }

            for (int i = 0; i < colorChildren.Count; i++)
            {
                m_coloringTargets[i] = colorChildren[i];
            }

            if (buttonImage != null)
            {
                m_coloringTargets[m_coloringTargets.Length - 1] = buttonImage;
            }
        }

        private static void findColoringTargetsInChildren(Transform inputTransform, ref List<Component> coloringTargets)
        {
            for (int i = 0; i < inputTransform.childCount; i++)
            {
                findColoringTargetsInChildren(inputTransform.GetChild(i), ref coloringTargets);

                Component component = inputTransform.GetChild(i).GetComponent<Image>();
                if (component != null)
                {
                    coloringTargets.Add(component);
                }

                component = inputTransform.GetChild(i).GetComponent<TMPro.TMP_Text>();
                if (component != null)
                {
                    coloringTargets.Add(component);
                }
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            Color targetColor =
                state == SelectionState.Disabled ? colors.disabledColor :
                state == SelectionState.Highlighted ? colors.highlightedColor :
                state == SelectionState.Normal ? colors.normalColor :
                state == SelectionState.Pressed ? colors.pressedColor :
                state == SelectionState.Selected ? colors.selectedColor : Color.white;

            for (int i = 0; i < m_coloringTargets.Length; i++)
            {
                if (m_coloringTargets[i] is Image)
                {
                    ((Image)m_coloringTargets[i]).CrossFadeColor(targetColor, instant ? 0 : colors.fadeDuration, true, true);
                }
                if (m_coloringTargets[i] is TMPro.TMP_Text)
                {
                    ((TMPro.TMP_Text)m_coloringTargets[i]).CrossFadeColor(targetColor, instant ? 0 : colors.fadeDuration, true, true);
                }
            }
        }
    }
}