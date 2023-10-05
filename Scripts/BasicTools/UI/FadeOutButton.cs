using UnityEngine;
using UnityEngine.UI;

namespace BasicTools.UI
{
    /// <summary>
    /// a button that will move towards an edge. every pixel over the edge will get hidden. the button will move over the edge and finally disappear
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class FadeOutButton : MonoBehaviour
    {
        private enum FadeDirection { Out, In }

        /// <summary>
        /// optional text child of the button
        /// </summary>
        [SerializeField] private TMPro.TMP_Text m_text;
        [SerializeField] private float m_moveSpeed = 1f;
        [SerializeField] private FadeDirection m_fadeDirection = FadeDirection.In;
        [SerializeField] private float m_buttonHeight = -1;

        private Button m_button;
        private Image m_image;
        private float m_fullyVisibleHeight;
        private Color m_textDefaultColor;

        private void Awake()
        {
            m_button = GetComponent<Button>();
            m_image = GetComponent<Image>();
            m_fullyVisibleHeight = transform.localPosition.y;

            if (m_buttonHeight <= 0)
            {
                m_buttonHeight = ((RectTransform)transform).sizeDelta.y;
            }

            if (m_text != null)
            {
                m_textDefaultColor = m_text.color;
            }
        }

        private void Update()
        {
            if (m_fadeDirection == FadeDirection.In)
            {
                if (transform.localPosition.y > m_fullyVisibleHeight)
                {
                    transform.localPosition = transform.localPosition + Vector3.down * Time.deltaTime * m_moveSpeed;

                    if (transform.localPosition.y < m_fullyVisibleHeight)
                    {
                        transform.localPosition = new Vector3(transform.localPosition.x, m_fullyVisibleHeight, transform.localPosition.z);
                    }

                    float fillAmount = 1f - (transform.localPosition.y - m_fullyVisibleHeight) / m_buttonHeight;

                    if (m_text != null)
                    {
                        m_text.color = new Color(m_textDefaultColor.r, m_textDefaultColor.g, m_textDefaultColor.b, m_textDefaultColor.a * fillAmount);
                    }

                    m_image.fillAmount = fillAmount;
                }
            }
            else if (m_fadeDirection == FadeDirection.Out)
            {
                if (transform.localPosition.y < m_fullyVisibleHeight + m_buttonHeight)
                {
                    transform.localPosition = transform.localPosition + Vector3.up * Time.deltaTime * m_moveSpeed;

                    if (transform.localPosition.y >= m_fullyVisibleHeight + m_buttonHeight)
                    {
                        transform.localPosition = new Vector3(transform.localPosition.x, m_fullyVisibleHeight + m_buttonHeight, transform.localPosition.z);
                        m_button.gameObject.SetActive(false);
                    }

                    float fillAmount = (m_fullyVisibleHeight + m_buttonHeight - transform.localPosition.y) / m_buttonHeight;

                    if (m_text != null)
                    {
                        m_text.color = new Color(m_textDefaultColor.r, m_textDefaultColor.g, m_textDefaultColor.b, m_textDefaultColor.a * fillAmount);
                    }

                    m_image.fillAmount = fillAmount;
                }
            }
        }

        public void fadeOut()
        {
            m_fadeDirection = FadeDirection.Out;

            if (m_button != null)
            {
                m_button.interactable = false;
            }
        }

        public void fadeIn()
        {
            m_fadeDirection = FadeDirection.In;

            if (m_button != null)
            {
                m_button.interactable = true;

                if (!m_button.gameObject.activeInHierarchy)
                {
                    m_button.gameObject.SetActive(true);
                }
            }
        }
    }
}