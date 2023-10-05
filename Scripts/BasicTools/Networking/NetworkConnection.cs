using System;

namespace BasicTools.Networking
{
    public abstract class NetworkConnection : System.IDisposable
    {
        protected object m_lastTimeConnectionEstablisedLock = new Object();
        protected DateTime m_lastTimeConnectionEstablised = DateTime.MinValue;

        public DateTime getLastTimeConnectionEstablised()
        {
            lock (m_lastTimeConnectionEstablisedLock)
            {
                return m_lastTimeConnectionEstablised;
            }
        }
        public abstract void requestConnect(int waitMillis = 0);
        public abstract void reconnect();
        public abstract bool isConnected();
        public abstract bool isConnecting();
        public abstract void sendMessage(string message);
        public abstract void setSsl(bool state);
        public abstract byte[] readFromStream();
        public abstract void Dispose();
    }
}