using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicTools.TimeTools
{
    /// <summary>
    /// make sure this script is executed first: move it to the top of the hierarchy
    /// </summary>
    public class WorldTime : MonoBehaviour
    {
        private static WorldTime singleton;

        private int m_frameCount = 0;
        private float m_time = 0f;
        private float m_deltaTime = 0f;
        private bool m_isPaused = false;
        private float m_timeScale = 1f;
        private int m_lastFixedUpdateFrame = -1;
        private bool m_isFrameAfterPhysics = false;
        private HashSet<int> m_pausedReasons = new HashSet<int>();

        public static event System.EventHandler pauseStarted;
        public static event System.EventHandler pauseEnded;

        private void firePauseStarted()
        {
            pauseStarted?.Invoke(this, null);
        }

        private void firePauseEnded()
        {
            pauseEnded?.Invoke(this, null);
        }

        private void Awake()
        {
            singleton = this;
        }

        private void Update()
        {
            if (!m_isPaused)
            {
                m_deltaTime = UnityEngine.Time.deltaTime * m_timeScale;
                m_time += m_deltaTime;
                m_frameCount++;
            }

            if (m_isFrameAfterPhysics && UnityEngine.Time.frameCount > m_lastFixedUpdateFrame)
            {
                m_isFrameAfterPhysics = false;
            }
        }

        private void FixedUpdate()
        {
            m_lastFixedUpdateFrame = UnityEngine.Time.frameCount;
            m_isFrameAfterPhysics = true;
        }

        public static bool isFrameAfterPhysics
        {
            get { return singleton.m_isFrameAfterPhysics; }
        }

        public static float timeScale
        {
            get { return singleton.m_timeScale; }
            set { singleton.m_timeScale = value; }
        }

        public static int frameCount
        {
            get { return singleton.m_frameCount; }
        }

        public static float deltaTime
        {
            get
            {
                if (singleton.m_isPaused)
                {
                    return 0f;
                }
                else
                {
                    return singleton.m_deltaTime;
                }
            }
        }

        public static float time
        {
            get { return singleton.m_time; }
        }

        public static bool paused
        {
            get { return singleton.m_isPaused; }
        }

        public static void addPausedReason(int uid)
        {
            if (singleton.m_pausedReasons.Contains(uid))
            {
                Debug.LogWarning("Paused reason added multiple times: " + uid);
            }
            else
            {
                singleton.m_pausedReasons.Add(uid);

                if (singleton.m_pausedReasons.Count == 1)
                {
                    singleton.m_isPaused = true;
                    singleton.firePauseStarted();
                }
            }
        }

        public static void removePausedReason(int uid)
        {
            if (singleton.m_pausedReasons.Contains(uid))
            {
                singleton.m_pausedReasons.Remove(uid);

                if (singleton.m_pausedReasons.Count == 0)
                {
                    singleton.m_isPaused = false;
                    singleton.firePauseEnded();
                }
            }
            else
            {
                Debug.LogWarning("tried to remove paused reason that wasn't active: " + uid);
            }
        }
    }
}