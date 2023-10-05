using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;

namespace BasicTools.DevTools
{
    public class GameStatistics : MonoBehaviour
    {
        [SerializeField] TMPro.TMP_Text m_outputTarget;
        [SerializeField] int m_fpsCacheSize = 100;

        private List<float> m_timeBetweenFrames = new List<float>();
        private float m_timeLastFrame;
        private int m_currentFps = -1;
        private int m_averageFps = -1;
        private int m_minFps = -1;

        private ProfilerRecorder m_systemUsedMemoryRecorder;

        private void Start()
        {
            m_timeLastFrame = Time.time;
        }

        private void OnEnable()
        {
            m_timeLastFrame = Time.time;

            m_systemUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        }

        private void OnDisable()
        {
            m_systemUsedMemoryRecorder.Dispose();
        }

        private void Update()
        {
            updateFps();
            updateOutputTarget();
        }

        private void updateFps()
        {
            float currentTimeDif = Time.time - m_timeLastFrame;

            if (currentTimeDif > 0)
            {
                m_timeBetweenFrames.Add(currentTimeDif);
            }

            while (m_timeBetweenFrames.Count > m_fpsCacheSize && m_fpsCacheSize > 0)
            {
                m_timeBetweenFrames.RemoveAt(0);
            }

            if (m_timeBetweenFrames.Count > 0)
            {
                m_minFps = int.MaxValue;
                float m_timeBetweenFramesSum = 0;

                for (int i = 0; i < m_timeBetweenFrames.Count; i++)
                {
                    m_minFps = Mathf.Min(m_minFps, (int)(1f / m_timeBetweenFrames[i]));
                    m_timeBetweenFramesSum += m_timeBetweenFrames[i];
                }

                m_averageFps = (int)(1f / (m_timeBetweenFramesSum / m_timeBetweenFrames.Count));
            }

            if (currentTimeDif > 0)
            {
                m_currentFps = (int)(1f / currentTimeDif);
            }

            m_timeLastFrame = Time.time;
        }

        private void updateOutputTarget()
        {
            if (m_outputTarget != null)
            {
                m_outputTarget.text = string.Format("fps: now:{0}, avg:{1}, low:{2} | RAM:{3} MB", m_currentFps, m_averageFps, m_minFps, m_systemUsedMemoryRecorder.LastValue / (1024 * 1024));
            }
        }
    }
}