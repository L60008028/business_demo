using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace business_demo
{
    public delegate void RecvDelegate(string xml);
    /// <summary>
    /// 接收状态回执及上行短信
    /// </summary>
    public class HttpServer
    {

         private int _port;//端口
        private IPAddress _ip;//IP
        private Socket _asyncsocketlisten;//异步socket
        private Socket _RemoteSocket;//接收到的socket
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public bool iss = false;
        private Thread _threadListen;
        public event RecvDelegate DateProcessEvent;
        public event RecvDelegate WriteMsgEvent;
        public HttpServer(int port)
        {
            this._port = port;
            DateProcessEvent += (s) => { };
            WriteMsgEvent += (x) => { };
        }


        /// <summary>
        /// 启动监听
        /// </summary>
        public void StartListen()
        {
            try
            {

                iss = true;
                _threadListen = new Thread(new ThreadStart(AsyncListen));
                _threadListen.Start();
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }


        /// <summary>
        /// 停止监听
        /// </summary>
        public void StopListen()
        {

            string msg = string.Format("已停止监听>>>--->>");
            iss = false;

            WriteMsgEvent(msg);
            
            try
            {
                if (_threadListen != null)
                {
                    _threadListen.Abort();
                    _threadListen.Join();
                    _threadListen = null;
                }
            }
            catch (Exception ex) { }
            try
            {

                if (_asyncsocketlisten != null)
                {
                    allDone = new ManualResetEvent(false);
                    if (_asyncsocketlisten.Connected)
                    {
                        _asyncsocketlisten.Shutdown(SocketShutdown.Both);
                        _asyncsocketlisten.Close(1000);
                    }
                }

            }
            catch (Exception ex)
            {

            }

            try
            {
                if (_asyncsocketlisten != null)
                {
                    _asyncsocketlisten.Close(1000);
                }

            }
            catch (Exception ex)
            {

            }

        }




        /// <summary>
        /// 异步监听
        /// </summary>
        private void AsyncListen()
        {
            try
            {
                string msg = string.Format("已启动监听在端口[{0}]上>>>--->>", _port);
                WriteMsgEvent(msg); 
                _asyncsocketlisten = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _asyncsocketlisten.Bind(new IPEndPoint(IPAddress.Any, _port));
                _asyncsocketlisten.Listen(10000);
                _asyncsocketlisten.BeginAccept(new AsyncCallback(AcceptCallback), _asyncsocketlisten);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        /// <summary>
        /// 处理连接
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallback(IAsyncResult ar)
        {

            try
            {
                if (iss)
                {
                    allDone.Set();
                    // 获取客户端连接的socket
                    Socket listener = (Socket)ar.AsyncState;
                    Socket handler = listener.EndAccept(ar);
                    StateObject state = new StateObject();
                    state.workSocket = handler;
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);//处理接收的数据
                }

            }
            catch (Exception ex)
            {
                WriteMsgEvent("Error:AcceptCallback--handler:" + ex.ToString());
            }
            finally
            {
                try
                {
                    if (_asyncsocketlisten != null)
                    {
                        _asyncsocketlisten.BeginAccept(new AsyncCallback(AcceptCallback), _asyncsocketlisten);//处于监听状态
                    }
                }
                catch (Exception ex)
                {
                     
                }
            }
        }



        StringBuilder sb = new StringBuilder();
        bool IsExpect100Continue = false;
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="ar"></param>
        private void ReadCallback(IAsyncResult ar)
        {
            Socket handler = null;

            try
            {
                if (ar.IsCompleted)
                {
                    //string content = string.Empty;
                    StateObject state = (StateObject)ar.AsyncState;
                    handler = state.workSocket;
                    _RemoteSocket = handler;
                    WriteMsgEvent("接收到请求RemoteEndPoint:" + handler.RemoteEndPoint);
                    SocketError se;
                    // 结束挂起的异步读取     
                    int bytesRead = handler.EndReceive(ar, out se);
                    if (bytesRead > 0 && se == SocketError.Success)
                    {

                        string rexml = Encoding.UTF8.GetString(state.buffer, 0, bytesRead);//接收到的数据
                        WriteMsgEvent("接收到数据:\n" + rexml);
                        string[] aa = rexml.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        string cl = aa[aa.Length - 1].Split(':')[0];
                        if (!string.IsNullOrEmpty(rexml) && rexml.StartsWith("POST") && cl.Equals("Content-Length"))
                        {
                            sb.Append(rexml);
                            IsExpect100Continue = true;
                            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);//继续接收数据
                        }
                        else
                        {
                            sb.Append(rexml);
                            IsExpect100Continue = false;
                        }
                        string data = "";
                        if (!IsExpect100Continue)
                        {

                            string tmp = sb.ToString();
                            if (tmp.IndexOf("POST") >= 0)
                            {
                                try
                                {
                                    string[] httpdata = rexml.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    data = httpdata[httpdata.Length - 1];
                                }
                                catch (Exception ex)
                                {
                                    WriteMsgEvent("接收到POST数据,exception:\n" + ex.ToString());
                                }
                                sb = new StringBuilder();
                                WriteMsgEvent("接收到POST数据:\r\n" + tmp);

                            }
                            else if (tmp.IndexOf("GET") >= 0)
                            {

                                WriteMsgEvent("接收到GET数据:\r\n" + tmp);
                                string[] recv = tmp.Split(new char[] { '?' });
                                if (recv != null && recv.Length > 1)
                                {
                                    data = recv[1];
                                    int ix = data.IndexOf("HTTP/1.1");
                                    if (ix > 0)
                                    {
                                        data = data.Substring(0, ix);
                                    }
                                }

                            }
                            else
                            {
                                SendReturnMessage("hi ,this is Callback", handler);
                            }

                            WriteMsgEvent("DATA::::" + data);
                            if (!string.IsNullOrEmpty(data))
                            {
                                DateProcessEvent(data);
                                string result = "100";
                                SendReturnMessage(result, handler);

                            }
                            else
                            {
                                SendReturnMessage("hi ,this is Callback", handler);

                            }
                           // handler.Shutdown(SocketShutdown.Both);

                        }
 
                    }


                }
 
            }
            catch (Exception ex)
            {
                sb = new StringBuilder();
                handler.Shutdown(SocketShutdown.Both);
                //throw new Exception("处理数据异常：" + ex.Message);
                WriteMsgEvent("处理数据异常：" + ex.Message);
            }
 


        }




        private byte[] GetSubBytes(byte[] buffer, int index, int count)
        {
            try
            {
                if (count + index > buffer.Length)
                {
                    count = buffer.Length - index;
                }
                using (MemoryStream memoryStream = new MemoryStream(buffer, index, count))
                {
                    return memoryStream.ToArray();
                }
            }
            catch (Exception)
            {
 
            }
            return null;
        }



       

        private void SendReturnMessage(string txtxml, Socket socket)
        {
            try
            {
                StringBuilder RequestHeaders = new StringBuilder();
                RequestHeaders.AppendFormat("HTTP/1.1 200 OK\r\n");
                RequestHeaders.AppendFormat("User-Agent:Mozilla/4.0(compatible;MSIE 7.0;Windows NT 5.2;.NET CLR 1.1.4322;.NET CLR 2.0.50727)\r\n");
                RequestHeaders.AppendFormat("Content-Type:application/x-www-form-urlencoded\r\n");
                RequestHeaders.AppendFormat("Host:{0}\r\n", socket.RemoteEndPoint);
                RequestHeaders.AppendFormat("Cache-Control: no-cache\r\n");
                RequestHeaders.AppendFormat("Pragma: no-cache\r\n");
                RequestHeaders.AppendFormat("Content-Type:text/html;UTF-8\r\n");
                RequestHeaders.AppendFormat("Content-Length:{0}\r\n", Encoding.UTF8.GetBytes(txtxml).Length);
                RequestHeaders.AppendFormat("Connection:Keep-Alive\r\n\r\n");//必须多加一个\r\n
                RequestHeaders.AppendFormat("{0}\r\n\r\n", txtxml);
                this.Send(socket, RequestHeaders.ToString());
            }
            catch (Exception )
            {
 
            }

        }
        private void Send(Socket handler, string data)
        {
            try
            {
                byte[] byteData = Encoding.UTF8.GetBytes(data);

                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
            }
            catch (Exception)
            {
                
 
            }
        }

        public void Send(string data)
        {
            try
            {
                byte[] byteData = Encoding.UTF8.GetBytes(data);
                this._RemoteSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), _RemoteSocket);
            }
            catch (Exception)
            {
 
            }
        }


        private void SendCallback(IAsyncResult ar)
        {
            Socket handler = null;
            try
            {
                  handler = (Socket)ar.AsyncState;
                SocketError errorCode;
                int bytesSent = handler.EndSend(ar, out errorCode);
                WriteMsgEvent("SendCallback==>" + errorCode.ToString());

            }
            catch (Exception)
            {

            }
            finally
            {
                try
                {
                    if (handler != null)
                    {
                        handler.Shutdown(SocketShutdown.Both);
                    }
                }
                catch { }
            }

        }
        
 

        private void WriteMessage(string message, Socket skt)
        {
            try
            {
                NetworkStream ns = new NetworkStream(skt, FileAccess.ReadWrite);
                StreamWriter sw = new StreamWriter(ns);
                char[] rep = message.ToCharArray();
                sw.Write("HTTP/1.1 200 OK\r\n");
                sw.Write("Content-Length:" + rep.Length + "\r\n");
                sw.Write("Content-Type:text/html;UTF-8\r\n");
                sw.Write("Connection: Keep-Alive\r\n");
                sw.Write("\r\n");
                sw.Write(rep);
                sw.Flush();
                sw.Close();


            }
            catch (Exception ex)
            {
                WriteMsgEvent("WriteMessage=>exception:" + ex.ToString());
            }
        }

        private void ReturnMsg(string txtxml, Socket socket)
        {
            try
            {
                StringBuilder sss = new StringBuilder();
                sss.Append("HTTP/1.1 200 OK\r\n");
                sss.Append("Content-Length:" + txtxml.Length + "\r\n");
                sss.Append("Content-Type:text/html;UTF-8\r\n");
                sss.Append("Connection: Keep-Alive\r\n");
                sss.Append("\r\n");
                sss.Append(txtxml);
                socket.Send(Encoding.UTF8.GetBytes(sss.ToString()));
            }
            catch (Exception)
            {
 
            }
        }


    }//end

    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024*10;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    
}//end
