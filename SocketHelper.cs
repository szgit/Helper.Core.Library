/*
 * 作用：通过 Socket 实现客户端/服务端通信。
 * */
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Helper.Core.Library
{
    public class SocketServerHelper
    {
        #region 私有属性常量
        private static object lockItem = new object();
        private Socket socket;
        private Dictionary<string, SocketThreadHelper> SocketDict = new Dictionary<string, SocketThreadHelper>();
        private ISocketProcess process;
        #endregion

        #region 构造函数
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip">服务器 IP</param>
        /// <param name="port">端口</param>
        /// <param name="process">数据流处理接口</param>
        /// <param name="maxClientCount">客户端最大连接数</param>
        public SocketServerHelper(string ip, int port, ISocketProcess process, int maxClientCount = int.MaxValue)
        {
            try
            {
                this.process = process;

                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                this.socket.Listen(maxClientCount);
            }
            catch
            {
                SocketHelper.Close(this.socket);
                throw;
            }
        }
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 启动侦听
        /// </summary>
        public void Listen()
        {
            this.socket.BeginAccept((IAsyncResult asyncResult) =>
            {
                try
                {
                    Socket clientSocket = this.socket.EndAccept(asyncResult);
                    IPEndPoint clientIPEndPoint = (IPEndPoint)clientSocket.RemoteEndPoint;

                    SocketThreadHelper threadHelper = new SocketThreadHelper(this, clientIPEndPoint.ToString(), clientSocket, this.process, (Socket socket) =>
                    {
                        this.RemoveSocket(socket);
                    });
                    SocketDict.Add(clientIPEndPoint.ToString(), threadHelper);

                    System.Threading.WaitCallback waitCallback = new WaitCallback(threadHelper.Receive);
                    ThreadPool.QueueUserWorkItem(waitCallback);

                    this.Listen();
                }
                catch
                {
                    throw;
                }
            }, null);
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="key">客户端 IP 和 端口组合</param>
        /// <param name="bytes">数据流</param>
        /// <param name="callback">回调函数</param>
        public void Send(string key, byte[] bytes, Action<bool> callback)
        {
            if (this.SocketDict == null || !this.SocketDict.ContainsKey(key)) return;

            this.SocketDict[key].Send(bytes, callback);
        }
        /// <summary>
        /// 广播数据
        /// </summary>
        /// <param name="bytes">数据流</param>
        /// <param name="callback">发送回调，成功数，失败数</param>
        public void Broadcast(byte[] bytes, Action<int, int> callback)
        {
            int success = 0;
            int error = 0;
            foreach (KeyValuePair<string, SocketThreadHelper> keyValueItem in SocketDict)
            {
                keyValueItem.Value.Send(bytes, (bool status) =>
                {
                    if (status) success++;
                    if (!status) error++;
                });
            }
            if (callback != null) callback(success, error);
        }
        /// <summary>
        /// 删除 Socket
        /// </summary>
        /// <param name="key">客户端 IP 和 端口组合</param>
        public void Remove(string key)
        {
            if (!SocketDict.ContainsKey(key)) return;
            lock(lockItem)
            {
                if (SocketDict.ContainsKey(key))
                {
                    SocketDict.Remove(key);
                }
            }
        }
        #endregion

        #region 逻辑处理私有函数
        private void RemoveSocket(Socket socket)
        {
            IPEndPoint clientIPEndPoint = (IPEndPoint)socket.RemoteEndPoint;
            if (this.SocketDict.ContainsKey(clientIPEndPoint.ToString()))
            {
                this.SocketDict.Remove(clientIPEndPoint.ToString());
            }
        }
        #endregion
    }
    public class SocketClientHelper
    {
        #region 私有属性常量
        private Socket socket;
        private SocketReceiveHelper helper;
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ip">服务器 IP</param>
        /// <param name="port">端口</param>
        /// <param name="process">数据流处理接口</param>
        public void Connect(string ip, int port, ISocketProcess process)
        {
            try
            {
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), (IAsyncResult asyncResult) =>
                {
                    try
                    {
                        this.socket.EndConnect(asyncResult);
                    }
                    catch
                    {
                        throw;
                    }
                }, null);


                this.helper = new SocketReceiveHelper(null, this, this.socket, null, process, () =>
                {
                    SocketHelper.Close(this.socket);
                });
                this.helper.Receive();
            }
            catch
            {
                SocketHelper.Close(this.socket);
                throw;
            }
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bytes">数据流</param>
        /// <param name="callback">回调函数</param>
        public void Send(byte[] bytes, Action<bool> callback)
        {
            new SocketSendHelper(this.socket).Send(bytes, callback);
        }
        #endregion
    }

    #region 逻辑处理辅助类
    internal class SocketHelper
    {
        public static void Close(Socket socket)
        {
            if (socket != null)
            {
                socket.Disconnect(false);
                socket.Close();
                socket.Dispose();
            }
        }
    }
    internal class SocketThreadHelper
    {
        private Socket socket;
        private SocketReceiveHelper helper;

        public SocketThreadHelper(SocketServerHelper helper, string key, Socket socket, ISocketProcess process, Action<Socket> closeCallback)
        {
            this.socket = socket;
            this.helper = new SocketReceiveHelper(helper, null, socket, key, process, () =>
            {
                if (closeCallback != null) closeCallback(socket);
            });
        }

        public void Receive(object obj)
        {
            this.helper.Receive();
        }

        public void Send(byte[] bytes, Action<bool> callback)
        {
            new SocketSendHelper(this.socket).Send(bytes, callback);
        }
    }
    internal class SocketReceiveHelper
    {
        private Socket socket;
        private ISocketProcess process;
        private Action closeCallback;
        private SocketServerHelper serverHelper;
        private SocketClientHelper clientHelper;
        private string key;

        public SocketReceiveHelper(SocketServerHelper serverHelper, SocketClientHelper clientHelper, Socket socket, string key, ISocketProcess process, Action closeCallback)
        {
            this.serverHelper = serverHelper;
            this.clientHelper = clientHelper;
            this.socket = socket;
            this.process = process;
            this.key = key;
            this.closeCallback = closeCallback;
        }
        public void Receive()
        {
            byte[] buffers = new byte[1024 * 1024];
            this.socket.BeginReceive(buffers, 0, buffers.Length, SocketFlags.None, (IAsyncResult asyncResult) =>
            {
                try
                {
                    int length = this.socket.EndReceive(asyncResult);
                    if (length > 0)
                    {
                        byte[] bytes = new byte[length];
                        Array.Copy(buffers, 0, bytes, 0, length);

                        // 消息交给外部处理
                        if (process != null) process.NewItem().Process(this.serverHelper, this.clientHelper, this.key, bytes);

                        this.Receive();
                    }
                    else
                    {
                        if (this.closeCallback != null) this.closeCallback();
                    }
                }
                catch
                {
                    throw;
                }
            }, null);
        }
    }
    internal class SocketSendHelper
    {
        private Socket socket;
        public SocketSendHelper(Socket socket)
        {
            this.socket = socket;
        }
        public void Send(byte[] bytes, Action<bool> callback)
        {
            if (!socket.Connected) return;

            this.socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, (IAsyncResult asyncResult) =>
            {
                try
                {
                    int length = this.socket.EndSend(asyncResult);
                    if (callback != null) callback(length == bytes.Length);
                }
                catch
                {
                    if (callback != null) callback(false);
                    throw;
                }
            }, null);
        }
    }
    #endregion

    #region 逻辑处理辅助接口
    public interface ISocketProcess
    {
        /// <summary>
        /// 数据流处理
        /// </summary>
        /// <param name="server">SocketServerHelper</param>
        /// <param name="client">SocketClientHelper</param>
        /// <param name="key">IP 和 端口组合标识</param>
        /// <param name="bytes">数据流</param>
        void Process(SocketServerHelper server, SocketClientHelper client, string key, byte[] bytes);

        /// <summary>
        /// 对象克隆
        /// </summary>
        /// <returns></returns>
        ISocketProcess NewItem();
    }
    #endregion
}
