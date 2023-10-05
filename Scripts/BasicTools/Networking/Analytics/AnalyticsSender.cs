using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

namespace BasicTools
{
    namespace Networking
    {
        namespace Analytics
        {
            public class AnalyticsSender : IDisposable
            {
                public enum ContextId { LastStandTowerDefense = 0, }

                private const string SERVER_IP = "127.0.0.1";
                private const int SERVER_PORT_TCP_WS = 3031;
                private const int SERVER_PORT_WSS = 3032;
                private const int MAX_QUEUE_MESSAGES = 100;
                private const int SECOUNDS_TILL_RECONNECT = 60;
                private const int MILLIS_WAIT_FOR_CONNECTION = 300; // on first connection
                private const int SECOUNDS_TILL_TIMEOUT = 5; // keep this above server ping interval
                private readonly IPEndPoint m_serverAddress;
                private static AnalyticsSender m_analyticsSender = null;
                private NetworkConnection m_netConnection = null;
                private Queue<Tuple<string, DateTime>> m_delayedMessages = new Queue<Tuple<string, DateTime>>();
                private DateTime m_lastTimeTryConnecting = DateTime.MinValue;
                private DateTime m_lastTimeReceivedFromServer = DateTime.MinValue;
                private bool isConnectionTimeout { get { return (DateTime.Now - m_lastTimeReceivedFromServer).TotalSeconds > SECOUNDS_TILL_TIMEOUT; } }
                /// <summary>
                /// send data when application is running in unity editor
                /// </summary>
                public static bool sendInEditor = true;

                private AnalyticsSender(IPEndPoint serverAddress)
                {
                    m_serverAddress = serverAddress;

                    if (m_serverAddress.Address.ToString() == "127.0.0.1")
                    {
                        Debug.LogWarning("Analytics server address is 127.0.0.1");
                    }

#if UNITY_WEBGL && !UNITY_EDITOR
        m_netConnection = new WebSocketConnection(m_serverAddress); // webgl doesn't allow TCP

        if(serverAddress.Port == SERVER_PORT_WSS)
        {
            m_netConnection.setSsl(true); // use wss instead of ws
        }
#else
                    m_netConnection = new TCPConnection(m_serverAddress);
#endif

                    m_netConnection.requestConnect(MILLIS_WAIT_FOR_CONNECTION);
                }

                ~AnalyticsSender()
                {
                    Dispose();
                }

                private bool tryConnectTcp()
                {
                    try
                    {
                        if ((DateTime.Now - m_lastTimeTryConnecting).TotalSeconds > SECOUNDS_TILL_RECONNECT)
                        {
                            m_lastTimeTryConnecting = DateTime.Now;

                            if (!m_netConnection.isConnected() && !m_netConnection.isConnecting())
                            {
                                m_netConnection.requestConnect();
                            }

                            if (m_netConnection.isConnected())
                            {
                                sendDelayedMessages();
                            }

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("Could not connect to analytics server: " + ex);
                        return false;
                    }
                }

                private void sendDelayedMessages()
                {
                    int loopOutCounter = 0;

                    while (loopOutCounter < 1000 && m_delayedMessages.Count > 0)
                    {
                        loopOutCounter++;

                        Tuple<string, DateTime> message = m_delayedMessages.Dequeue();

                        if (!sendDelayedData(string.Format("{0};D:{1}", message.Item1, (DateTime.Now - message.Item2).TotalSeconds.ToString("0"))))
                        {
                            m_delayedMessages.Enqueue(message);
                            break;
                        }
                    }
                }

                private static AnalyticsSender singleton
                {
                    get
                    {
                        if (m_analyticsSender == null)
                        {
#if UNITY_EDITOR
                            if (sendInEditor)
                            {
#endif
                                int port;

#if UNITY_WEBGL && !UNITY_EDITOR
                    port = SERVER_PORT_WSS; // onyl wss supported on https websites. There are probably no more http website so this should be right
#else
                                port = SERVER_PORT_TCP_WS;
#endif
                                m_analyticsSender = new AnalyticsSender(new IPEndPoint(IPAddress.Parse(SERVER_IP), port));
#if UNITY_EDITOR
                            }
#endif
                        }

                        return m_analyticsSender;
                    }
                }

                public static bool sendData(ContextId gameId, string textAscii)
                {
#if UNITY_EDITOR
                    if (sendInEditor)
                    {
#endif
                        string textToSend = string.Format("{0};{1}", (int)gameId, textAscii);

                        try
                        {
                            if (!singleton.m_netConnection.isConnected())
                            {
                                singleton.tryConnectTcp();
                            }

                            if (singleton.m_netConnection.isConnected())
                            {
                                singleton.updateTimeout();

                                if (!singleton.isConnectionTimeout)
                                {
                                    singleton.sendDelayedMessages();
                                    singleton.m_netConnection.sendMessage(textToSend);
                                    return true;
                                }
                                else
                                {
                                    Debug.LogWarning("Could not send analytics data: connection timeout");
                                    singleton.addDelayedMessage(textToSend);
                                    singleton.m_netConnection.reconnect();
                                    return false;
                                }
                            }
                            else
                            {
                                Debug.LogWarning("Could not send analytics data: not connected");
                                singleton.addDelayedMessage(textToSend);
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning("Could not send analytics data: error while sending: " + ex);

                            singleton.addDelayedMessage(textToSend);
                            return false;
                        }
#if UNITY_EDITOR
                    }
                    else
                    {
                        return false;
                    }
#endif
                }

                private void addDelayedMessage(string fullMessage)
                {
                    if (singleton.m_delayedMessages.Count < MAX_QUEUE_MESSAGES)
                    {
                        singleton.m_delayedMessages.Enqueue(new Tuple<string, DateTime>(fullMessage, DateTime.Now));
                    }
                    else
                    {
                        Debug.LogWarning("analytics data delayed messages overflow");
                    }
                }

                private static bool sendDelayedData(string fullText)
                {
                    try
                    {
                        singleton.m_netConnection.sendMessage(fullText);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }

                private void updateTimeout()
                {
                    if (m_netConnection.getLastTimeConnectionEstablised() > m_lastTimeReceivedFromServer)
                    {
                        m_lastTimeReceivedFromServer = m_netConnection.getLastTimeConnectionEstablised();
                    }

                    if (m_netConnection.isConnected())
                    {
                        try
                        {
                            byte[] received = m_netConnection.readFromStream();

                            if (received != null)
                            {
                                m_lastTimeReceivedFromServer = DateTime.Now;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning("Error while receiving form server: " + ex);
                        }
                    }
                }

                public static void OnApplicationQuit()
                {
                    if (m_analyticsSender != null)
                    {
                        singleton.Dispose();
                    }
                }

                public void Dispose()
                {
                    if (m_netConnection != null)
                    {
                        try
                        {
                            m_netConnection.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.Log("error while shutting down connection: " + ex);
                        }
                    }
                }
            }
        }
    }
}
