using UnityEngine;

namespace BasicTools.UI
{
    /// <summary>
    /// a scale effect on an ui element. it scales the element up for m_time to m_maxSize. once m_maxSize is reached it reverts to its normal size
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIScaleEffect : MonoBehaviour
    {
        [SerializeField] private AnimationCurve m_effectCurve;
        [SerializeField] private float m_maxSize = 2f;
        [SerializeField] private float m_time = 1f;
        [SerializeField] private bool m_DEBUG_startEffect = false;

        private RectTransform m_rectTransform = null;
        private bool m_effectActive = false;
        private float m_lastTimeEffectStarted = float.MinValue;
        private Vector3 m_startSize = Vector3.one;

        private void Awake()
        {
            if (m_rectTransform == null)
            {
                m_rectTransform = GetComponent<RectTransform>();
            }
        }

        private void Update()
        {
            if (m_DEBUG_startEffect)
            {
                startEffect();
                m_DEBUG_startEffect = false;
            }

            if (m_effectActive)
            {
                if (Time.time > m_lastTimeEffectStarted + m_time)
                {
                    m_effectActive = false;
                    m_rectTransform.localScale = m_startSize;
                }
                else
                {
                    Vector3 endSize = m_startSize * m_maxSize;
                    float timeNormalized = m_effectCurve.Evaluate((Time.time - m_lastTimeEffectStarted) / m_time);

                    m_rectTransform.localScale = new Vector3(
                        Mathf.Lerp(m_startSize.x, endSize.x, timeNormalized),
                        Mathf.Lerp(m_startSize.y, endSize.y, timeNormalized),
                        Mathf.Lerp(m_startSize.z, endSize.z, timeNormalized)
                    );
                }
            }
        }

        public void startEffect()
        {
            if (!m_effectActive)
            {
                m_effectActive = true;
                m_lastTimeEffectStarted = Time.time;

                if (m_rectTransform == null)
                {
                    m_rectTransform = GetComponent<RectTransform>();
                }

                m_startSize = m_rectTransform.localScale;
            }
        }
    }
}