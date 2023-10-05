using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BasicTools
{
    namespace UI
    {
        /// <summary>
        /// a custom implementation of a button with additional events. serves as a base for other classes. depends on guimanager to send cursor enter
        /// </summary>
        [RequireComponent(typeof(UnityEngine.UI.Image))]
        public class ExtendedButton : MonoBehaviour
        {
            protected enum ColorFadeTarget { Normal, Highlighted, selected }

            [SerializeField] private Color m_highlightedColor = Color.gray;
            [SerializeField] private Color m_selectedColor = Color.black;
            [SerializeField] private float m_colorFadeTime = 0.1f;

            private UnityEngine.UI.Image m_image;
            protected int m_lastFrameCursor = int.MinValue;
            protected bool m_isCursorOnControl = false;
            private Color m_imageDefaultColor;
            private float m_colorFadeStartTime = 0f;
            private bool m_isColorFading = false;
            private ColorFadeTarget m_colorFadeTarget = ColorFadeTarget.Normal;
            private Color m_colorFadingStartColor;

            public event EventHandler<object> mouseZeroDown;
            public event EventHandler<object> mouseZeroUp;
            public event EventHandler<object> mouseOneDown;
            public event EventHandler<object> mouseOneUp;
            public event EventHandler<object> cursorLeft;

            protected void fireMouseZeroDown(object obj)
            {
                mouseZeroDown?.Invoke(this, obj);
            }

            protected void fireMouseZeroUp(object obj)
            {
                mouseZeroUp?.Invoke(this, obj);
            }

            protected void fireMouseOneDown(object obj)
            {
                mouseOneDown?.Invoke(this, obj);
            }

            protected void fireMouseOneUp(object obj)
            {
                mouseOneUp?.Invoke(this, obj);
            }

            protected void fireCursorLeft(object obj)
            {
                cursorLeft?.Invoke(this, obj);
            }

            private void Awake()
            {
                m_image = GetComponent<UnityEngine.UI.Image>();
                m_imageDefaultColor = m_image.color;
            }

            private void Update()
            {
                colorUpdate();
            }

            private void LateUpdate()
            {
                if (m_isCursorOnControl)
                {
                    if (Time.frameCount > m_lastFrameCursor)
                    {
                        onCursorLeft();
                    }
                    else
                    {
                        buttonsUpdate();
                    }
                }
            }

            protected virtual void onCursorLeft()
            {
                m_isCursorOnControl = false;
                fireCursorLeft(this);
                setColorFadeTarget(ColorFadeTarget.Normal);
            }

            private void colorUpdate()
            {
                if (m_isColorFading)
                {
                    Color targetColor;

                    switch (m_colorFadeTarget)
                    {
                        case ColorFadeTarget.Normal:
                            targetColor = m_imageDefaultColor;
                            break;
                        case ColorFadeTarget.Highlighted:
                            targetColor = m_imageDefaultColor * m_highlightedColor;
                            break;
                        case ColorFadeTarget.selected:
                            targetColor = m_imageDefaultColor * m_selectedColor;
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    if (Time.time > m_colorFadeStartTime + m_colorFadeTime)
                    {
                        m_image.color = targetColor;
                        m_isColorFading = false;
                    }
                    else
                    {
                        m_image.color = Color.Lerp(m_colorFadingStartColor, targetColor, (Time.time - m_colorFadeStartTime) / m_colorFadeTime);
                    }
                }
            }

            protected virtual void buttonsUpdate()
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    fireMouseZeroDown(this);
                    setColorFadeTarget(ColorFadeTarget.selected);
                }
                else if (Input.GetKeyUp(KeyCode.Mouse0))
                {
                    fireMouseZeroUp(this);
                    setColorFadeTarget(ColorFadeTarget.Highlighted);
                }

                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    fireMouseOneDown(this);
                    setColorFadeTarget(ColorFadeTarget.selected);
                }
                else if (Input.GetKeyUp(KeyCode.Mouse1))
                {
                    fireMouseOneUp(this);
                    setColorFadeTarget(ColorFadeTarget.Highlighted);
                }
            }

            protected void setColorFadeTarget(ColorFadeTarget target)
            {
                m_colorFadeTarget = target;
                m_colorFadeStartTime = Time.time;
                m_isColorFading = true;
                m_colorFadingStartColor = m_image.color;
            }

            public virtual void setCursorOnControl()
            {
                m_lastFrameCursor = Time.frameCount;

                if (m_isCursorOnControl == false)
                {
                    m_isCursorOnControl = true;
                    setColorFadeTarget(ColorFadeTarget.Highlighted);
                }
            }
        }
    }
}