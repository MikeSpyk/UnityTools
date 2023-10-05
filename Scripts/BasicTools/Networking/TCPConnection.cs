using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BasicTools.Networking
{
    public class TCPConnection : NetworkConnection
    {
        private const char MESSAGE_SEPERATOR = (char)3; // ETX
        private readonly IPEndPoint m_destination;
        private long LOCKED_isConnecting = 0;
        private Thread m_connectingThread = null;
        private TcpClient m_tcpClient = null;

        public TCPConnection(IPEndPoint destination)
        {
            m_destination = destination;
        }

        ~TCPConnection()
        {
            Dispose();
        }

        public override void requestConnect(int waitMillis = 0)
        {
            if (!isConnecting())
            {
                if (m_connectingThread != null)
                {
                    //m_connectingThread.Abort();
                    m_connectingThread = null;
                }

                Interlocked.Exchange(ref LOCKED_isConnecting, 1);
                m_connectingThread = new Thread(new ThreadStart(connectingProcedure));
                m_connectingThread.Name = "Start TCP connection";
                m_connectingThread.Start();
            }

            DateTime startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalMilliseconds < waitMillis && isConnecting())
            {
                Thread.Sleep(1);
            }
        }

        public override void sendMessage(string messageAscii)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(string.Format("{0}{1}", messageAscii, MESSAGE_SEPERATOR));
            m_tcpClient.GetStream().Write(data, 0, data.Length);
        }

        public override byte[] readFromStream()
        {
            byte[] result = null;

            if (m_tcpClient.ReceiveBufferSize > 0 && m_tcpClient.GetStream().DataAvailable)
            {
                result = new byte[m_tcpClient.ReceiveBufferSize];
                m_tcpClient.GetStream().Read(result, 0, m_tcpClient.ReceiveBufferSize);
            }

            return result;
        }

        private void connectingProcedure()
        {
            if (m_tcpClient != null)
            {
                if (m_tcpClient.Connected)
                {
                    m_tcpClient.Close();
                }

                m_tcpClient.Dispose();
                m_tcpClient = null;
            }

            m_tcpClient = new TcpClient();

            try
            {
                m_tcpClient.Connect(m_destination);
                lock (m_lastTimeConnectionEstablisedLock)
                {
                    m_lastTimeConnectionEstablised = DateTime.Now;
                }
            }
            catch (Exception) { }

            Interlocked.Exchange(ref LOCKED_isConnecting, 0);
        }

        private void closeConnection()
        {
            if (m_tcpClient != null)
            {
                if (m_tcpClient.Connected)
                {
                    try
                    {
                        sendMessage(((char)4).ToString()); // send EOT: End of Transmission
                    }
                    catch (Exception) { }

                    try
                    {
                        m_tcpClient.Close();
                    }
                    catch (Exception) { }
                }

                m_tcpClient.Dispose();
                m_tcpClient = null;
            }
        }

        public override bool isConnected()
        {
            return !isConnecting() && m_tcpClient != null && m_tcpClient.Connected;
        }

        public override bool isConnecting()
        {
            return Interlocked.Read(ref LOCKED_isConnecting) == 1;
        }

        public override void reconnect()
        {
            closeConnection();
            requestConnect();
        }

        public override void Dispose()
        {
            closeConnection();
        }

        public override void setSsl(bool state)
        {
            throw new NotSupportedException("SSL is not supported"); // no need so far
        }
    }
}