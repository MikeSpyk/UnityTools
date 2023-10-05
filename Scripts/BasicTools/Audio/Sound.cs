using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicTools
{
    namespace Audio
    {
        [RequireComponent(typeof(AudioSource))]
        public class Sound : MonoBehaviour
        {
            public enum SoundPlaystyle { Once, loop }
            private enum FadingDirection { Louder, Quieter }

            private float m_defaultVolume = 1f;
            private float m_volumeMultiplier = 1f;
            private float m_defaultLifeTime = 0.0f;
            private float m_lifetime = 0.0f;
            private float m_soundStartTime = 0.0f;
            private AudioSource m_myAudioSource;
            private SoundPlaystyle m_mySoundPlaystyle;
            public float m_volumeFadeSpeed = .01f;
            public bool m_isVolumeFading = false;
            private float m_targetVolumeFade = 0f;
            private FadingDirection m_fadingDirection; // is the sound fading to a lower volume or a higher volume
            private int m_soundIndex = -1;
            private AnimationCurve m_defaultFalloffCurve;
            public bool m_isWarmUpSound = false;

            public int SoundIndex { get { return m_soundIndex; } }
            public float defaultVolumne { get { return m_defaultVolume; } }
            public float pitch
            {
                get { return m_myAudioSource.pitch; }
                set { m_myAudioSource.pitch = value; }
            }

            public float volume { get { return m_myAudioSource.volume; } }

            public event System.EventHandler<Sound> soundEnded;

            private void fireSoundEnded(Sound sound)
            {
                soundEnded?.Invoke(this, sound);
            }

            private void Awake()
            {
                m_myAudioSource = GetComponent<AudioSource>();
                m_defaultFalloffCurve = m_myAudioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
                m_isVolumeFading = false;
            }

            private void Update()
            {
                if (m_isVolumeFading)
                {
                    if (m_fadingDirection == FadingDirection.Louder)
                    {
                        m_myAudioSource.volume += m_volumeFadeSpeed * m_volumeMultiplier * Time.deltaTime;
                        if (m_myAudioSource.volume > m_targetVolumeFade * m_volumeMultiplier)
                        {
                            m_myAudioSource.volume = m_targetVolumeFade * m_volumeMultiplier;
                            m_isVolumeFading = false;
                        }
                    }
                    else // fading to lower volume
                    {
                        m_myAudioSource.volume -= m_volumeFadeSpeed * m_volumeMultiplier * Time.deltaTime;
                        if (m_myAudioSource.volume < m_targetVolumeFade * m_volumeMultiplier)
                        {
                            m_myAudioSource.volume = m_targetVolumeFade * m_volumeMultiplier;
                            m_isVolumeFading = false;
                        }
                    }
                }
            }

            private void FixedUpdate()
            {
                if (m_mySoundPlaystyle == SoundPlaystyle.Once)
                {
                    if (Time.time > m_soundStartTime + m_lifetime)
                    {
                        fireSoundEnded(this);
                        m_myAudioSource.Stop();
                        SoundManager.singleton.recyleSound(this);
                    }
                }
            }

            public void initialize(int soundIndex, AudioClip audioClip, float defaultVolume, bool globalSound, AnimationCurve volumeFalloff, int priority)
            {
                initialize(soundIndex, audioClip, defaultVolume, Vector3.zero, audioClip.length, globalSound, volumeFalloff, priority);
            }
            public void initialize(int soundIndex, AudioClip audioClip, float defaultVolume, Vector3 soundPosition, float defaultSoundLifeTime, bool globalSound, int priority)
            {
                initialize(soundIndex, audioClip, defaultVolume, soundPosition, defaultSoundLifeTime, globalSound, m_defaultFalloffCurve, priority);
            }
            public void initialize(int soundIndex, AudioClip audioClip, float defaultVolume, Vector3 soundPosition, float defaultSoundLifeTime, bool globalSound, AnimationCurve volumeFalloff, int priority)
            {
                m_soundIndex = soundIndex;
                m_myAudioSource.clip = audioClip;
                transform.position = soundPosition;

                if (globalSound)
                {
                    m_myAudioSource.spatialBlend = 0.0f;
                }
                else
                {
                    m_myAudioSource.spatialBlend = 1.0f;
                }

                if (defaultSoundLifeTime < 0)
                {
                    m_defaultLifeTime = Mathf.Infinity;
                }
                else
                {
                    m_defaultLifeTime = defaultSoundLifeTime;
                }

                m_lifetime = m_defaultLifeTime;

                m_defaultVolume = defaultVolume;
                m_myAudioSource.volume = m_defaultVolume * m_volumeMultiplier;
                m_myAudioSource.rolloffMode = AudioRolloffMode.Custom;
                m_myAudioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, volumeFalloff);
                m_myAudioSource.maxDistance = SoundManager.singleton.getMaxHearableDistance(soundIndex);
                m_myAudioSource.priority = priority;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="global">if true => global sound. if false => local sound</param>
            public void setGlobalLocal(bool global)
            {
                if (global)
                {
                    m_myAudioSource.spatialBlend = 0.0f;
                }
                else
                {
                    m_myAudioSource.spatialBlend = 1.0f;
                }
            }

            public void playOnce()
            {
                m_mySoundPlaystyle = SoundPlaystyle.Once;
                m_isVolumeFading = false;
                m_lifetime = m_defaultLifeTime;
                m_soundStartTime = Time.time;
                m_myAudioSource.loop = false;
                m_myAudioSource.PlayOneShot(m_myAudioSource.clip);
            }

            public void playLooping()
            {
                m_mySoundPlaystyle = SoundPlaystyle.loop;
                m_isVolumeFading = false;
                m_lifetime = Mathf.Infinity;
                m_myAudioSource.loop = true;
                m_myAudioSource.Play();
            }

            public void setDefaultVolume()
            {
                if (m_myAudioSource != null)
                {
                    m_myAudioSource.volume = m_defaultVolume * m_volumeMultiplier;
                }
            }

            public void setVolumeMultiplier(float multiplier)
            {
                m_volumeMultiplier = multiplier;
            }

            public void stopPlaying()
            {
                m_myAudioSource.Stop();
            }

            public void fadeVolumeTo(float targetVolume)
            {
                if (targetVolume < 0.0f || targetVolume > 1.0f)
                {
                    Debug.LogError("fadeVolumeTo: \"targetVolume\" out of bounds with value: " + targetVolume);
                }
                else
                {
                    m_isVolumeFading = true;
                    m_targetVolumeFade = targetVolume;
                    if (m_targetVolumeFade > m_myAudioSource.volume / m_volumeMultiplier)
                    {
                        m_fadingDirection = FadingDirection.Louder;
                    }
                    else
                    {
                        m_fadingDirection = FadingDirection.Quieter;
                    }
                }
            }

            public void setVolume(float volume)
            {
                m_myAudioSource.volume = volume * m_volumeMultiplier;
                m_isVolumeFading = false;
            }

            public void pause()
            {
                m_myAudioSource.Pause();
            }

            public void resume()
            {
                m_myAudioSource.UnPause();
            }
        }
    }
}