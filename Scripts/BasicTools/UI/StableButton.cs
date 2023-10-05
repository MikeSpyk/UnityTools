using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BasicTools
{
    namespace UI
    {
        /// <summary>
        /// a button that can be toggled by mouse0 or temporarily pressed with mouse1. depends on guimanager to send cursor enter
        /// </summary>
        public class StableButton : ExtendedButton
        {
            [SerializeField] private KeyCode m_switchKey = KeyCode.Mouse0;
            [SerializeField] private KeyCode m_holdKey = KeyCode.Mouse1;
            private int m_lastFrameTouch = 0;
            private bool m_isSwitchedOn = false;
            public event EventHandler<object> switchOn;
            public event EventHandler<object> switchOff;

            private void fireSwitchOn(object obj)
            {
                switchOn?.Invoke(this, obj);
            }

            private void fireSwitchOff(object obj)
            {
                switchOff?.Invoke(this, obj);
            }

            private void setSwitchState(bool newState)
            {
                if (newState != m_isSwitchedOn)
                {
                    m_isSwitchedOn = newState;

                    if (newState == true)
                    {
                        fireSwitchOn(this);
                    }
                    else
                    {
                        fireSwitchOff(this);
                    }
                }
            }

            protected override void buttonsUpdate()
            {
                if (Input.GetKeyDown(m_holdKey))
                {
                    fireMouseZeroDown(this);
                    setSwitchState(true);
                    setColorFadeTarget(ColorFadeTarget.selected);
                }
                else if (Input.GetKeyUp(m_holdKey))
                {
                    fireMouseZeroUp(this);
                    setSwitchState(false);
                    setColorFadeTarget(ColorFadeTarget.Highlighted);
                }

                if (Input.GetKeyDown(m_switchKey) || (Input.touchCount > 0 && m_lastFrameTouch != Time.frameCount - 1))
                {
                    fireMouseOneDown(this);

                    setSwitchState(!m_isSwitchedOn);

                    if (m_isSwitchedOn)
                    {
                        setColorFadeTarget(ColorFadeTarget.selected);
                    }
                    else
                    {
                        setColorFadeTarget(ColorFadeTarget.Highlighted);
                    }
                }
                else if (Input.GetKeyUp(m_switchKey))
                {
                    fireMouseOneUp(this);
                }

                if (Input.touchCount > 0)
                {
                    m_lastFrameTouch = Time.frameCount;
                }
            }

            protected override void onCursorLeft()
            {
                m_isCursorOnControl = false;
                fireCursorLeft(this);

                if (Input.GetKey(m_holdKey))
                {
                    setSwitchState(false);
                }

                if (!m_isSwitchedOn)
                {
                    setColorFadeTarget(ColorFadeTarget.Normal);
                }
            }

            public override void setCursorOnControl()
            {
                m_lastFrameCursor = Time.frameCount;

                if (m_isCursorOnControl == false)
                {
                    m_isCursorOnControl = true;
                    if (!m_isSwitchedOn)
                    {
                        setColorFadeTarget(ColorFadeTarget.Highlighted);
                    }
                }
            }
        }
    }
}