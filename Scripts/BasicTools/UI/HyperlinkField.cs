using UnityEngine;

namespace BasicTools.UI
{
    /// <summary>
    /// a component, that can be put ontop of a ui elment. if it is clicked an url will be opened. also a text gets highlighted if the cursor is over the ui element
    /// </summary>
    public class HyperlinkField : MonoBehaviour
    {
        /// <summary>
        /// optional. a text that gets highlighted if the cursor is over the ui element
        /// </summary>
        [SerializeField] private TMPro.TMP_Text m_text;
        [SerializeField] private string m_url;
        [SerializeField] private Color m_hoverColor = Color.white;

        private int m_lastFramCursorOver = 0;
        private Color m_textDefaultColor;
        private bool m_isCursorOver = false;

        private void Awake()
        {
            if (m_text != null)
            {
                m_textDefaultColor = m_text.color;
            }
        }

        private void Update()
        {
            if (m_isCursorOver)
            {
                if (m_lastFramCursorOver > Time.frameCount - 2)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        Application.OpenURL(m_url);
                    }
                }
                else
                {
                    m_isCursorOver = false;

                    if (m_text != null)
                    {
                        m_text.color = m_textDefaultColor;
                    }
                }
            }
        }

        public void onCursorOver()
        {
            m_lastFramCursorOver = Time.frameCount;
            if (!m_isCursorOver)
            {
                if (m_text != null)
                {
                    m_text.color = m_hoverColor;
                }

                m_isCursorOver = true;
            }
        }
    }
}