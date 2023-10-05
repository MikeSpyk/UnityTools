using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace BasicTools.DevTools
{
    public class DevConsole : MonoBehaviour
    {
        [SerializeField] private GameObject m_textLayoutGroup;
        [SerializeField] private GameObject m_textPrefab;
        [SerializeField] private TMPro.TMP_InputField m_userInputField;
        /// <summary>
        /// is optinal. if available will scroll to last message on send
        /// </summary>
        [SerializeField] private ScrollRect m_scrollview;
        [SerializeField] private int m_maxCharsPerMessage = 120; // until messages gets split
        [SerializeField] private int m_maxActiveMessages = 100;

        private Queue<Tuple<string, string, LogType>> m_queuedUnityLogMessages = new Queue<Tuple<string, string, LogType>>();
        private int m_inputFieldLastFrameFocused = 0;
        private Stack<string> m_lastInputMessages = new Stack<string>();
        private Stack<string> m_usedInputMessages = new Stack<string>();
        private Queue<TMPro.TMP_Text> m_activeMessageObjects = new Queue<TMPro.TMP_Text>();
        private Queue<TMPro.TMP_Text> m_messageObjectsCache = new Queue<TMPro.TMP_Text>();
        private int m_lastFrameEnabled = 0;

        public event EventHandler gotEnabled;
        public event EventHandler gotDisabled;

        private void fireGotEnabled()
        {
            gotEnabled?.Invoke(this, null);
        }

        private void fireGotDisabled()
        {
            gotDisabled?.Invoke(this, null);
        }

        private void Awake()
        {
#if UNITY_WEBGL
            Application.logMessageReceived += Application_logMessageReceived; // WebGl is single threaded only
#else
            Application.logMessageReceivedThreaded += Application_logMessageReceivedThreaded;
#endif
        }

        private void OnEnable()
        {
            m_lastFrameEnabled = Time.frameCount;
            fireGotEnabled();
        }

        private void OnDisable()
        {
            fireGotDisabled();
        }

        private void Update()
        {
            if (m_queuedUnityLogMessages.Count > 0)
            {
                lock (m_queuedUnityLogMessages)
                {
                    while (m_queuedUnityLogMessages.Count > 0)
                    {
                        printUnityLogMessage(m_queuedUnityLogMessages.Dequeue());
                    }
                }
            }

            if (Time.frameCount < m_lastFrameEnabled + 2)
            {
                m_userInputField.Select();
                m_userInputField.ActivateInputField();
            }

            inputUpdate();
        }

        private void inputUpdate()
        {
            if (m_userInputField.isFocused)
            {
                m_inputFieldLastFrameFocused = Time.frameCount;
            }

            if (Time.frameCount < m_inputFieldLastFrameFocused + 2) // is focused
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    submitInputFieldText();
                }

                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (m_lastInputMessages.Count != 0 || m_usedInputMessages.Count != 0)
                    {
                        if (m_lastInputMessages.Count < 1)
                        {
                            // reset
                            while (m_usedInputMessages.Count > 0)
                            {
                                m_lastInputMessages.Push(m_usedInputMessages.Pop());
                            }
                        }

                        string currentMessage = m_lastInputMessages.Pop();
                        m_userInputField.text = currentMessage;
                        m_usedInputMessages.Push(currentMessage);
                    }
                }

                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (m_lastInputMessages.Count != 0 || m_usedInputMessages.Count != 0)
                    {
                        if (m_usedInputMessages.Count < 1)
                        {
                            // reset
                            while (m_lastInputMessages.Count > 0)
                            {
                                m_usedInputMessages.Push(m_lastInputMessages.Pop());
                            }
                        }

                        string currentMessage = m_usedInputMessages.Pop();
                        m_userInputField.text = currentMessage;
                        m_lastInputMessages.Push(currentMessage);
                    }
                }

                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    m_userInputField.text = "";
                }
            }
        }

        private void applyCommand(string userInput)
        {
            //Debug.Log("Console Command: \"" + command + "\"");
            printText(userInput);

            // expand last messages stack
            while (m_usedInputMessages.Count > 0)
            {
                m_lastInputMessages.Push(m_usedInputMessages.Pop());
            }
            m_lastInputMessages.Push(userInput);
            onCommand(userInput);
        }

        protected virtual void onCommand(string command)
        {
            throw new NotImplementedException("override this");
        }

        private void Application_logMessageReceivedThreaded(string condition, string stackTrace, LogType type)
        {
            lock (m_queuedUnityLogMessages)
            {
                m_queuedUnityLogMessages.Enqueue(new Tuple<string, string, LogType>(condition, stackTrace, type));
            }
        }

        private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            printUnityLogMessage(condition, stackTrace, type);
        }

        private void printUnityLogMessage(Tuple<string, string, LogType> message)
        {
            printUnityLogMessage(message.Item1, message.Item2, message.Item3);
        }
        private void printUnityLogMessage(string condition, string stackTrace, LogType type)
        {
            #if UNITY_WEBGL // unity will spam the log with this message on web build see https://forum.unity.com/threads/fmod-returns-error-code-36-fmod_err_invalid_handle-executing-setrelativeaudibility.1377675/
            if(condition.Contains("Error executing dspHead->setRelativeAudibility"))
            {
                return;
            }
            #endif

            string messageType = "UnityLog (";

            switch (type)
            {
                case LogType.Assert:
                    {
                        messageType += "Assert)";
                        break;
                    }
                case LogType.Error:
                    {
                        messageType += "Error)";
                        break;
                    }
                case LogType.Exception:
                    {
                        messageType += "Exception)";
                        break;
                    }
                case LogType.Log:
                    {
                        messageType += "Log)";
                        break;
                    }
                case LogType.Warning:
                    {
                        messageType += "Warning)";
                        break;
                    }
                default:
                    {
                        messageType += "Assert)";
                        break;
                    }
            }

            if (type == LogType.Log)
            {
                printText(string.Format("{0}: {1}", messageType, condition.Trim().Replace(System.Environment.NewLine, " ")));
            }
            else
            {
                printText(string.Format("{0}: {1}: {2}", messageType, condition.Trim().Replace(System.Environment.NewLine, " "), stackTrace.Trim().Replace(System.Environment.NewLine, " ")));
            }
        }

        protected void printText(string text)
        {
            text = string.Format("[{0}] {1}", System.DateTime.Now.ToString("hh:mm:ss"), text);

            List<string> subStrings = new List<string>();

            for (int i = 0; i < text.Length; i += m_maxCharsPerMessage)
            {
                int length = Mathf.Min(m_maxCharsPerMessage, text.Length - i);

                subStrings.Add(text.Substring(i, length));
            }

            for (int i = 0; i < subStrings.Count; i++)
            {
                addTextObject(subStrings[i]);
            }

            if (m_scrollview != null)
            {
                m_scrollview.verticalNormalizedPosition = 0; // focus bottom
            }
        }

        private void addTextObject(string text)
        {
            TMPro.TMP_Text newText = getMessageObject();
            newText.transform.SetParent(m_textLayoutGroup.transform);
            newText.text = text;
            m_activeMessageObjects.Enqueue(newText);

            while (m_activeMessageObjects.Count > m_maxActiveMessages && m_maxActiveMessages > 0)
            {
                recyleMessageObject(m_activeMessageObjects.Dequeue());
            }
        }

        private TMPro.TMP_Text getMessageObject()
        {
            TMPro.TMP_Text returnValue;

            if (m_messageObjectsCache.Count > 0)
            {
                returnValue = m_messageObjectsCache.Dequeue();
                returnValue.gameObject.SetActive(true);
            }
            else
            {
                returnValue = Instantiate(m_textPrefab).GetComponent<TMPro.TMP_Text>();
            }

            return returnValue;
        }

        private void recyleMessageObject(TMPro.TMP_Text messageObject)
        {
            messageObject.gameObject.SetActive(false);
            messageObject.transform.SetParent(null);
            m_messageObjectsCache.Enqueue(messageObject);
        }

        public void submitInputFieldText()
        {
            if (m_userInputField.text != string.Empty)
            {
                applyCommand(m_userInputField.text);
                m_userInputField.text = "";
            }

            m_userInputField.Select();
            m_userInputField.ActivateInputField();
        }
    }
}