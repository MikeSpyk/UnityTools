using UnityEngine;
using UnityEngine.UI;

namespace BasicTools.UI
{
    /// <summary>
    /// alternates an images alpha between [0-m_maxAlpha] if enabled
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class FadingImage : MonoBehaviour
    {
        private enum FadeDirection { Out, In }

        [SerializeField] private float m_fadeSpeed = 1;
        private FadeDirection m_fadeDirection = FadeDirection.Out;
        private float m_maxAlpha;
        private Image m_image;
        private bool m_isFading = false;

        private void Start()
        {
            m_image = GetComponent<Image>();
            m_maxAlpha = m_image.color.a;
        }

        private void Update()
        {
            if (m_isFading)
            {
                float alpha = m_image.color.a;

                if (m_fadeDirection == FadeDirection.Out)
                {
                    alpha -= m_fadeSpeed * Time.deltaTime;

                    if (alpha < 0)
                    {
                        alpha = 0;
                        m_fadeDirection = FadeDirection.In;
                    }
                }
                else if (m_fadeDirection == FadeDirection.In)
                {
                    alpha += m_fadeSpeed * Time.deltaTime;

                    if (alpha > m_maxAlpha)
                    {
                        alpha = m_maxAlpha;
                        m_fadeDirection = FadeDirection.Out;
                    }
                }

                m_image.color = new Color(m_image.color.r, m_image.color.g, m_image.color.b, alpha);
            }
        }

        public void setFading(bool state)
        {
            m_isFading = state;
            
            if(m_image != null)
            {
                m_image.color = new Color(m_image.color.r, m_image.color.g, m_image.color.b, m_maxAlpha);
            }
        }
    }
}
