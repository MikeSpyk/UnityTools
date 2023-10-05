using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Runtime.InteropServices;
using AOT;

namespace BasicTools.Networking
{
    /// <summary>
    /// needs the websocket.jslib plugin
    /// </summary>
    public class WebSocketConnection : NetworkConnection
    {
        private readonly IPEndPoint m_destination;
        private bool m_useSsl = false;
        private bool m_isInitialized = false;
        private static object m_messageBufferLock = new object();
        private static byte[] m_messageBuffer = null;

        public WebSocketConnection(IPEndPoint destination)
        {
#if !UNITY_WEBGL
        Debug.LogError("The javascript-code for websockets can only be executed on a WEBGL-Build");
#endif
            m_destination = destination;
        }

        ~WebSocketConnection()
        {
            Dispose();
        }

        [DllImport("__Internal")]
        private static extern void WebSocketInit(string url);

        [DllImport("__Internal")]
        private static extern void WebSocketSend(string message);

        [DllImport("__Internal")]
        private static extern ushort WebSocketReadyState();

        [DllImport("__Internal")]
        private static extern void WebSocketClose();

        [DllImport("__Internal")]
        private static extern void WebSocketAddMessageListener(Action<string> action);

        [MonoPInvokeCallback(typeof(Action))]
        public static void OnMessageReceivedCallback(string data)
        {
            lock (m_messageBufferLock)
            {
                byte[] newBytes = System.Text.Encoding.ASCII.GetBytes(data); // might use utf8 here since websockets forces it on messages

                if (m_messageBuffer == null || m_messageBuffer.Length < 1)
                {
                    m_messageBuffer = newBytes;
                }
                else
                {
                    byte[] oldBuffer = m_messageBuffer;
                    m_messageBuffer = new byte[newBytes.Length + oldBuffer.Length];

                    Array.Copy(oldBuffer, 0, m_messageBuffer, 0, oldBuffer.Length);
                    Array.Copy(newBytes, 0, m_messageBuffer, oldBuffer.Length, newBytes.Length);
                }
            }

            //Debug.Log("Callback called: " +data);
        }

        public override bool isConnected()
        {
            try
            {
                return WebSocketReadyState() == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override bool isConnecting()
        {
            try
            {
                return WebSocketReadyState() == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void setSsl(bool state)
        {
            if (m_isInitialized)
            {
                throw new NotSupportedException("SSL can't get set after the socket has been initialized. Call setSsl before requestConnect");
            }
            else
            {
                m_useSsl = state;
            }
        }

        public override void requestConnect(int waitMillis = 0)
        {
            try
            {
                string protocol;
                if (m_useSsl)
                {
                    protocol = "wss";
                }
                else
                {
                    protocol = "ws";
                }

                WebSocketInit(string.Format("{0}://{1}:{2}", protocol, m_destination.Address.ToString(), m_destination.Port.ToString()));
                m_isInitialized = true;

                if (waitMillis > 0)
                {
                    DateTime startTime = DateTime.Now;

                    while ((DateTime.Now - startTime).TotalMilliseconds < waitMillis && !isConnected())
                    {
                        System.Threading.Thread.Sleep(1);
                    }
                }

                WebSocketAddMessageListener(OnMessageReceivedCallback);
            }
            catch (Exception) { }
        }

        public override byte[] readFromStream()
        {
            byte[] returnValue;

            lock (m_messageBufferLock)
            {
                returnValue = m_messageBuffer;
                m_messageBuffer = null;
            }

            return returnValue;
        }

        public override void sendMessage(string message)
        {
            if (isConnected())
            {
                //Debug.Log("sendMessage: " + message);
                WebSocketSend(message);
            }
            else
            {
                throw new System.NotSupportedException("not connected");
            }
        }

        public override void reconnect()
        {
            if (m_isInitialized)
            {
                WebSocketClose();
            }
            requestConnect();
        }

        public override void Dispose()
        {
            if (m_isInitialized)
            {
                WebSocketClose();
            }
        }
    }
}