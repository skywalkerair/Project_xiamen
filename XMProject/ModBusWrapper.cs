using System;
using System.Collections.Generic;
using System.Text;


namespace SimpleImageDisplaySample
{
    public enum Protocol
    {
        TCPIP
    }

    public abstract class ModBusWrapper : IDisposable
    {
        private static ModBusWrapper _Instance;

        public static ModBusWrapper CreateInstance(Protocol protocol)
        {
            if (_Instance == null)
            {
                switch (protocol)
                {
                    case Protocol.TCPIP:
                        _Instance = new ModBusTCPIPWrapper();
                        break;
                    default:
                        break;
                }
            }
            return _Instance;
        }


        #region Transaction Identifier
        /// <summary>
        /// 数据序号标识
        /// </summary>
        private byte dataIndex = 0;

        protected byte CurrentDataIndex
        {
            get { return this.dataIndex; }
        }

        protected byte NextDataIndex()
        {
            return ++this.dataIndex;
        }
        #endregion

        public ILog Logger { get; set; }

        public abstract void Connect();

        public abstract byte[] Receive();

        public abstract void Send(byte[] data);

        #region IDisposable 成员
        public virtual void Dispose()
        {
        }
        #endregion
    }
}
