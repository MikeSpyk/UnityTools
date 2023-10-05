using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicTools.UI
{
    /// <summary>
    /// moves a vertical line of color horizontaly through the image. BE ARWARE: special shader need to get used "UiColorFading"
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public class MovingColorImage : MonoBehaviour
    {
        private enum MoveDirection { Up, Down }

        [SerializeField] private Color m_movingColor;
        [SerializeField] private float m_speed = 1f;
        [SerializeField] private bool m_isMoving = true;

        private float m_currentFadePosition = 0f;
        private Material m_material; // shader Unlit/UiColorFading is needed
        private MoveDirection m_moveDirection = MoveDirection.Up;
        private UnityEngine.UI.Image m_image;

        private void Awake()
        {
            m_image = GetComponent<UnityEngine.UI.Image>();
            m_material = m_image.material;
        }

        private void Update()
        {
            if (m_isMoving)
            {
                if (m_moveDirection == MoveDirection.Up)
                {
                    m_currentFadePosition += Time.deltaTime * m_speed;

                    if (m_currentFadePosition > 1f)
                    {
                        m_currentFadePosition = 1f;
                        m_moveDirection = MoveDirection.Down;
                    }
                }
                else if (m_moveDirection == MoveDirection.Down)
                {
                    m_currentFadePosition -= Time.deltaTime * m_speed;

                    if (m_currentFadePosition < 0f)
                    {
                        m_currentFadePosition = 0f;
                        m_moveDirection = MoveDirection.Up;
                    }
                }

                m_material.SetFloat("_FadePosition", m_currentFadePosition);
#if UNITY_EDITOR // can change color in editor but not in build
                m_material.SetColor("_Color1", m_movingColor);
#endif
            }
        }

        public void startEffect()
        {
            m_currentFadePosition = 0f;
            m_moveDirection = MoveDirection.Up;
            m_isMoving = true;

            if (m_material != null)
            {
                m_material.SetColor("_Color1", m_movingColor);
            }
        }

        public void stopEffect()
        {
            m_isMoving = false;

            if (m_material != null)
            {
                m_material.SetColor("_Color1", m_image.color);
            }
        }
    }
}