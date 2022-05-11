using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KonectSocket
{
    #region Variables

    /// <summary>
    /// Provides client connections for Ktp network services.
    /// </summary>
    public class KtpClient : IDisposable, IAsyncDisposable
    {
        private NetworkStream _tcpClientStream;
        private string _host;

        private ushort _port;

        //private object _requestData;
        private bool _isPersistentConnection;
        private TimeSpan _reConnectDelay = TimeSpan.FromSeconds(10);
        private Task _reconnectTask;
        private uint? _contentLength;
        private bool? _dataRead;
        private bool _includeObjectTypes;

        /// <summary>
        /// Gets or Sets the data sent to the host when the <see cref="KtpClient"/> connects or reconnects to a host. 
        /// </summary>
        private object ConnectionData { get; set; }

        private bool _isConnected;

        /// <summary>
        /// Gets a value indicating whether the <see cref="KtpClient" /> is connected to a remote host.
        /// </summary>
        /// <returns>
        /// <see langword="true" /> if the <see cref="KtpClient" /> is connected to a remote host; otherwise, <see langword="false" />.
        /// </returns>
        public bool IsConnected
        {
            get
            {
                if (!_isPersistentConnection)
                    return Client.Connected;
                return _isConnected;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether data is available on the <see cref="KtpClient"/> to be read.
        /// </summary>
        public bool DataAvailable
        {
            get
            {
                try
                {
                    return _tcpClientStream.DataAvailable;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the maximum amount of time for a read operation when it receives new data.
        /// The default is 15 seconds.
        /// </summary>
        public TimeSpan ReadTimeout { get; set; } = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Gets or Sets the maximum amount amount of time a write operation tries to write data.
        /// The default is 15 seconds.
        /// </summary>
        public TimeSpan WriteTimeout { get; set; } = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Gets the underlying <see cref="TcpClient"/> of the <see cref="KtpClient"/> connection.
        /// </summary>
        public TcpClient Client { get; private set; }

        private EndPoint _remoteEndPoint;

        /// <summary>
        /// Gets the remote endpoint.
        /// </summary>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public EndPoint RemoteEndPoint
        {
            get
            {
                if (_remoteEndPoint == null)
                    _remoteEndPoint = Client.Client.RemoteEndPoint;
                return _remoteEndPoint;
            }
        }

        /// <summary>
        /// Gets or Sets the maximum time to wait for a keep-alive message.
        /// </summary>
        public TimeSpan KeepAliveTimeout { get; set; } = TimeSpan.FromSeconds(60);

        #endregion


        /// <summary>
        /// Create a new instance of KtpClient.
        /// </summary>
        public KtpClient()
        {
            //empty
        }

        /// <summary>
        /// Create a new instance of KtpClient.
        /// </summary>
        /// <param name="client"></param>
        public KtpClient(TcpClient client)
        {
            Client = client;
            //Client = tcpClient;
            _tcpClientStream = Client.GetStream();
        }

        #region Connect to Server

        /// <summary>
        /// Asynchronously connects the client to the specified host.
        /// </summary>
        /// <param name="host">The DNS or Ip Address of the remote host in which you intend to connect.</param>
        /// <param name="port">The port of the remote host in which you intend to connect.</param>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="KtpException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task ConnectAsync(string host, ushort port)
        {
            await ConnectAsync(host, port, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously connects the client to the specified host.
        /// </summary>
        /// <param name="host">The DNS or Ip Address of the remote host in which you intend to connect.</param>
        /// <param name="port">The port of the remote host in which you intend to connect.</param>
        /// <param name="ct">The token to monitor for cancellation request.</param>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="KtpException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task ConnectAsync(string host, ushort port, CancellationToken ct)
        {
            switch (host)
            {
                //check for invalid arguments.
                case null:
                    throw new ArgumentNullException(nameof(host));
                case "":
                    throw new ArgumentException(nameof(host));
            }

            try
            {
                ct.Register((client) =>
                {
                    //assign the current client to a local variable so that the task cancellation
                    //only affects the TcpClient when connect was called.
                    var currentClient = (TcpClient)client;
                    if (!IsConnected)
                        currentClient.Close();
                }, Client);
                Client = new TcpClient();
                ct.ThrowIfCancellationRequested();
                await Client.ConnectAsync(host, port);
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ct.ThrowIfCancellationRequested();
                var kex = new KtpException(ex.Message, KtpException.ExceptionStatus.FailedToConnectToRemoteHost, ex);
                //if connection fails Raise the Connected event to false
                RaiseConnectedEvent(kex, ConnectionState.Disconnected);
                throw kex;
            }
        }


        // /// <summary>
        // /// Connects the client to the specified Host.
        // /// </summary>
        // /// <param name="host">The DNS or Ip Address of the remote host in which you intend to connect.</param>
        // /// <param name="port">The port of the remote host in which you intend to connect.</param>
        // /// <param name="connectionData">The object that is sent to the remote host when you connect for the first time.</param>
        // /// <param name="isPersistentConnection">True if you want to keep the connection alive after the remote host
        // /// responds to the request.</param>
        // /// <param name="reConnectDelay">The interval in which to re-connect to the remote host when the connection is lost.</param>
        // /// <returns>Task</returns>
        // /// <exception cref="ArgumentNullException"></exception>
        // /// <exception cref="ArgumentException"></exception>
        // /// <exception cref="KtpException"></exception>
        // /// <exception cref="ObjectDisposedException"></exception>
        // public void Connect(string host, ushort port, object connectionData, bool isPersistentConnection,
        //     TimeSpan reConnectDelay)
        // {
        //     _reConnectDelay = reConnectDelay;
        //     Connect(host, port, connectionData, isPersistentConnection);
        // }

        // /// <summary>
        // /// Connects the client to the specified Host.
        // /// </summary>
        // /// <param name="host">The DNS or Ip Address of the remote host in which you intend to connect.</param>
        // /// <param name="port">The port of the remote host in which you intend to connect.</param>
        // /// <param name="connectionData">The object that is sent to the remote host when you connect for the first time.</param>
        // /// <param name="isPersistentConnection">True if you want to keep the connection alive after the remote host
        // /// responds to the request.</param>
        // /// <returns>Task</returns>
        // /// <exception cref="ArgumentNullException"></exception>
        // /// <exception cref="ArgumentException"></exception>
        // /// <exception cref="KtpException"></exception>
        // /// <exception cref="ObjectDisposedException"></exception>
        // public void Connect(string host, ushort port, object connectionData, bool isPersistentConnection)
        // {
        //     _isPersistentConnection = isPersistentConnection;
        //     Connect(host, port, connectionData);
        // }

        // /// <summary>
        // /// Connects the client to the specified Host.
        // /// </summary>
        // /// <param name="host">The DNS or Ip Address of the remote host in which you intend to connect.</param>
        // /// <param name="port">The port of the remote host in which you intend to connect.</param>
        // /// <param name="connectionData">The object that is sent to the remote host when you connect for the first time.</param>
        // /// <returns>Task</returns>
        // /// <exception cref="ArgumentNullException"></exception>
        // /// <exception cref="ArgumentException"></exception>
        // /// <exception cref="KtpException"></exception>
        // /// <exception cref="ObjectDisposedException"></exception>
        // public void Connect(string host, ushort port, object connectionData)
        // {
        //     //check for invalid arguments.
        //     switch (host)
        //     {
        //         case null:
        //             throw new ArgumentNullException(nameof(host));
        //         case "":
        //             throw new ArgumentException(nameof(host));
        //     }
        //
        //     if (connectionData == null)
        //         throw new ArgumentNullException(nameof(connectionData));
        //     _host = host;
        //     _port = port;
        //     ConnectionData = connectionData;
        //     Client = new TcpClient();
        //     try
        //     {
        //         RaiseConnectedEvent(ConnectionState.Connecting);
        //         Client.Connect(host, port);
        //         _tcpClientStream = Client.GetStream();
        //         RaiseConnectedEvent(ConnectionState.Connected);
        //         SendData(connectionData);
        //     }
        //     catch (Exception ex)
        //     {
        //         RaiseConnectedEvent(ConnectionState.Disconnected);
        //         throw new KtpException(ex.Message, KtpException.ExceptionStatus.FailedToConnectToRemoteHost);
        //     }
        // }

        #endregion


        #region Write Data

        ///<summary>
        ///Asynchronously writes a sequence of bytes to a network stream
        ///</summary>
        ///<param name="buffer">The object to write to the stream</param>
        ///<exception cref="ArgumentNullException"></exception>
        ///<exception cref="ArgumentOutOfRangeException"></exception>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="KtpException"></exception>
        ///<exception cref="ObjectDisposedException"></exception>
        ///<exception cref="InvalidOperationException"></exception>
        public async Task WriteAsync(byte[] buffer)
        {
            await WriteAsync(buffer, CancellationToken.None);
        }

        ///<summary>
        ///Asynchronously writes a sequence of bytes to a network stream.
        ///</summary>
        ///<param name="buffer">The object to write to the stream</param>
        ///<param name = "ct" >The token to monitor for cancellation request.</param>
        ///<exception cref="ArgumentNullException"></exception>
        ///<exception cref="ArgumentOutOfRangeException"></exception>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="KtpException"></exception>
        ///<exception cref="ObjectDisposedException"></exception>
        ///<exception cref="InvalidOperationException"></exception>
        public async Task WriteAsync(byte[] buffer, CancellationToken ct)
        {
            //await SendDataAsync(buffer, false, ct);

            //check if object is null
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            //if (!IsConnected && _isPersistentConnection)
            //    await ReConnectAsync();
            var responseBuffer = Functions.GetWriteData(buffer);
            await _tcpClientStream.WriteAsync(responseBuffer, 0, responseBuffer.Length, ct);
            await _tcpClientStream.FlushAsync(ct);
        }

        ///<summary>
        ///Asynchronously writes an object to a network stream
        ///</summary>
        /// <param name="value">The object to write to the stream</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="KtpException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SendDataAsync(object value)
        {
            await SendDataAsync(value, CancellationToken.None);
        }

        ///<summary>
        ///Asynchronously writes an object to a network stream.
        ///</summary>
        /// <param name="value">The object to write to the stream.</param>
        /// <param name="ct">The token to monitor for cancellation request.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="KtpException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TaskCanceledException"></exception>
        public async Task SendDataAsync(object value, CancellationToken ct)
        {
            // null buffer or simply writing 0 to the stream can be used to send
            // keep-alive messages to the client.
            if (value == null)
            {
                await WriteAsync(new byte[] { 0 }, ct);
                return;
            }
            //serialize object to json string and convert to bytes.
            var jsonSerializerSettings = new JsonSerializerSettings();
            string messageBodyJson = JsonConvert.SerializeObject(value, jsonSerializerSettings);
            //dataBuffer contains the data body of the response.
            byte[] bodyBuffer = Encoding.UTF8.GetBytes(messageBodyJson);
            await WriteAsync(bodyBuffer, ct);
        }

        // ///<summary>
        // ///Writes an object to a network stream.
        // ///</summary>
        // /// <param name="value">The object to write to the stream.</param>
        // /// <exception cref="ArgumentNullException"></exception>
        // /// <exception cref="ArgumentException"></exception>
        // /// <exception cref="KtpException"></exception>
        // /// <exception cref="ObjectDisposedException"></exception>
        // /// <exception cref="System.IO.IOException"></exception>
        // public void SendData(object value)
        // {
        //     SendData(value, false);
        // }

        // ///<summary>
        // ///Writes an object to a network stream.
        // ///</summary>
        // /// <param name="value">The object to write to the stream.</param>
        // /// <param name="includeObjectTypes">Indicates if object <see cref="Type"/> info should be added to the data to be sent.</param>
        // /// <exception cref="ArgumentNullException"></exception>
        // /// <exception cref="ArgumentException"></exception>
        // /// <exception cref="KtpException"></exception>
        // /// <exception cref="ObjectDisposedException"></exception>
        // /// <exception cref="System.IO.IOException"></exception>
        // public void SendData(object value, bool includeObjectTypes)
        // {
        //     //check if object is null.
        //     if (value == null)
        //         throw new ArgumentNullException(nameof(value));
        //     if (!IsConnected && _isPersistentConnection)
        //         ReConnectAsync().Wait();
        //     //serialize object to json string and convert to bytes.
        //     var jsonSerializerSettings = new JsonSerializerSettings();
        //     if (includeObjectTypes)
        //         jsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;
        //     string messageBodyJson = JsonConvert.SerializeObject(value);
        //     //dataBuffer contains the data body of the response.
        //     byte[] dataBuffer = Encoding.UTF8.GetBytes(messageBodyJson);
        //     var responseBuffer = Functions.GetWriteData(dataBuffer);
        //     //write the responseBuffer to the network stream.
        //     _tcpClientStream.Write(responseBuffer, 0, responseBuffer.Length);
        //     _tcpClientStream.Flush();
        // }


        // ///<summary>
        // ///Writes data to a network stream.
        // ///</summary>
        // ///<param name="buffer">The object to write to the stream</param>
        // ///<exception cref="ArgumentNullException"></exception>
        // ///<exception cref="ArgumentOutOfRangeException"></exception>
        // ///<exception cref="ArgumentException"></exception>
        // ///<exception cref="KtpException"></exception>
        // ///<exception cref="ObjectDisposedException"></exception>
        // ///<exception cref="InvalidOperationException"></exception>
        // public void Write(byte[] buffer)
        // {
        //     //check if object is null
        //     if (buffer == null)
        //         throw new ArgumentNullException(nameof(buffer));
        //     if (!IsConnected && _isPersistentConnection)
        //         ReConnectAsync().Wait();
        //     var responseBuffer = Functions.GetWriteData(buffer);
        //     _tcpClientStream.Write(responseBuffer, 0, responseBuffer.Length);
        //     _tcpClientStream.Flush();
        // }

        #endregion

        #region Receive Data

        // /// <summary>
        // /// Asynchronously gets the size of the current message body.
        // /// </summary>
        // /// <exception cref="ArgumentNullException"></exception>
        // /// <exception cref="ArgumentException"></exception>
        // /// <exception cref="ObjectDisposedException"></exception>
        // /// <exception cref="SocketException"></exception>
        // /// <exception cref="InvalidOperationException"></exception>
        // /// <exception cref="TaskCanceledException"></exception>
        // /// <returns></returns>
        // public async Task<uint> GetContentLengthAsync()
        // {
        //     return await GetContentLengthAsync(CancellationToken.None);
        // }
        //
        // /// <summary>
        // /// Asynchronously gets the size of the current message body.
        // /// </summary>
        // /// <param name="ct">The token to monitor for cancellation requests.</param>
        // /// <exception cref="ArgumentNullException"></exception>
        // /// <exception cref="ArgumentException"></exception>
        // /// <exception cref="ObjectDisposedException"></exception>
        // /// <exception cref="SocketException"></exception>
        // /// <exception cref="InvalidOperationException"></exception>
        // /// <exception cref="TaskCanceledException"></exception>
        // /// <returns></returns>
        // private async Task<uint> GetContentLengthAsync(CancellationToken ct)
        // {
        //     if (ct == null)
        //         throw new ArgumentNullException(nameof(ct));
        //     if (_contentLength.HasValue)
        //         return _contentLength.Value;
        //     if (ct == CancellationToken.None)
        //     {
        //         using (var cts = new CancellationTokenSource(ReadTimeout))
        //             _contentLength = await Functions.GetRequestContentLength(_tcpClientStream, cts.Token);
        //     }
        //     else
        //     {
        //         _contentLength = await Functions.GetRequestContentLength(_tcpClientStream, ct);
        //     }
        //     if (_contentLength == null)
        //         throw new SocketException(10054);
        //     return _contentLength.Value;
        // }
        //
        // ///<summary>
        // ///Asynchronously reads an object from a <see cref="NetworkStream"/>.
        // ///</summary>
        // ///<returns>An object of specified Type</returns>
        // /// <exception cref="ArgumentNullException"></exception>
        // /// <exception cref="ObjectDisposedException"></exception>
        // /// <exception cref="InvalidOperationException"></exception>
        // /// <exception cref="SocketException"></exception>
        // /// <exception cref="TaskCanceledException"></exception>
        // public async Task<object> ReceiveDataAsync()
        // {
        //     return await ReceiveDataAsync<object>(CancellationToken.None,true);
        // }


        /// <summary>
        /// Asynchronously reads a sequence of bytes from a <see cref="NetworkStream"/>.
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> ReadAsync()
        {
            return await ReadAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously reads a sequence of bytes from a <see cref="NetworkStream"/>.
        /// </summary>
        /// <param name="ct">The token to monitor for cancellation request.</param>
        /// <returns></returns>
        public async Task<byte[]> ReadAsync(CancellationToken ct)
        {
            if (ct == null)
                throw new ArgumentNullException(nameof(ct));
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(KtpClient));
            //return await ReceiveDataAsync<byte[]>(ct, false);
            byte[] headerBuffer = new byte[1];
            await _tcpClientStream.ReadAsync(headerBuffer, 0, 1, ct);
            int headerValue = headerBuffer[0];
            uint contentLength;
            switch (headerValue)
            {
                case -1:
                    throw new KtpException("", KtpException.ExceptionStatus.FailedToConnectToRemoteHost);
                case 0:
                    return null;
                case 1:
                    byte[] contentLengthBuffer = await Functions.ReadInputStreamAsync(_tcpClientStream, 1, ct);
                    contentLength = contentLengthBuffer[0];
                    break;
                case 2:
                    contentLengthBuffer = await Functions.ReadInputStreamAsync(_tcpClientStream, 2, ct);
                    //convert data from big-endian order to little-endian order.
                    //messageLength = BitConverter.ToUInt16(buffer, 0);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(contentLengthBuffer);
                    contentLength = BitConverter.ToUInt16(contentLengthBuffer);
                    break;
                case 4:
                    contentLengthBuffer = await Functions.ReadInputStreamAsync(_tcpClientStream, 4, ct);
                    //convert data from big-endian order to little-endian order.
                    //messageLength = BitConverter.ToUInt16(buffer, 0);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(contentLengthBuffer);
                    contentLength = BitConverter.ToUInt32(contentLengthBuffer);
                    break;
                default:
                    throw new KtpException("", KtpException.ExceptionStatus.FailedToConnectToRemoteHost);
            }

            return await Functions.ReadInputStreamAsync(_tcpClientStream, (int)contentLength, ct);
        }


        ///<summary>
        ///Asynchronously reads an object from a <see cref="NetworkStream"/>.
        ///</summary>
        ///<param name = "ct">The token to monitor for cancellation request.</param>
        ///<returns>An object of specified Type</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="TaskCanceledException"></exception>
        public async Task<T> ReceiveDataAsync<T>(CancellationToken ct)
        {
            if (ct == null)
                throw new ArgumentNullException(nameof(ct));
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(KtpClient));
            //If the cancellation token is none, set a new cancellation token with Timeout
            CancellationTokenSource cts = null;
            //todo server response
            byte[] data = await ReadAsync(ct);
            if (data == null)
                return default(T);
            string json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(json);
        }


        // //This Task is used to persistently Connect to a remote host
        // //until it returns when connected.
        // private async Task ReConnectAsync()
        // {
        //     try
        //     {
        //         //check if reconnection is already in progress.
        //         if (_reconnectTask == null)
        //         {
        //             _reconnectTask = ConnectAsync(_host, _port, ConnectionData, _includeObjectTypes,
        //                 _isPersistentConnection);
        //         }
        //
        //         await _reconnectTask;
        //         _reconnectTask = null;
        //     }
        //     catch
        //     {
        //         _reconnectTask = null;
        //         throw;
        //     }
        // }

        public void CloseConnection()
        {
            // if (_isPersistentConnection)
            //     RaiseConnectedEvent(ConnectionState.Disconnected);
            _tcpClientStream?.Dispose();
            Client?.Close();
            _tcpClientStream = null;
            Client = null;
        }

        /// <summary>
        /// Indicates the state of the current <see cref="KtpClient"/> connection.
        /// </summary>
        public enum ConnectionState
        {
            /// <summary>
            /// Indicates that the client is disconnected from the host.
            /// </summary>
            Disconnected,

            /// <summary>
            /// Indicates that the client is connecting to the host.
            /// </summary>
            Connecting,

            /// <summary>
            /// Indicates that the client is connected to the host.
            /// </summary>
            Connected
        }

        /// <summary>
        /// Occurs when the KtpClient connects or disconnects from the Host.
        /// </summary>
        public event ConnectedEventHandler Connected;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ktpClient"></param>
        /// <param name="connectedEventArgs"></param>
        public delegate void ConnectedEventHandler(KtpClient ktpClient, ConnectedEventArgs connectedEventArgs);

        private void RaiseConnectedEvent(ConnectionState connectionState)
        {
            RaiseConnectedEvent(null, connectionState);
        }

        private void RaiseConnectedEvent(Exception ex, ConnectionState connectionState)
        {
            ConnectedEventArgs connectedEventArgs;
            if (ex == null)
                connectedEventArgs = new ConnectedEventArgs(connectionState);
            else
                connectedEventArgs = new ConnectedEventArgs(ex, connectionState);
            if (connectionState == ConnectionState.Connected)
                _isConnected = true;
            else
                _isConnected = false;
            var connected = Connected;
            connected?.Invoke(this, connectedEventArgs);
        }

        #endregion


        #region Exceptions

        /// <summary>
        /// An Exception that is thrown when an error occurs while working with KtpClient.
        /// </summary>
        public class KtpException : Exception
        {
            public enum ExceptionStatus
            {
                FailedToConnectToRemoteHost,
                MessageSizeTooLarge,
                SocketError
            }

            public override string Message { get; }

            /// <summary>
            /// Represents the type of exception Thrown
            /// </summary>
            public ExceptionStatus Status { get; }

            public new Exception InnerException { get; }

            public KtpException(string message, ExceptionStatus status)
            {
                Message = message;
                Status = status;
            }

            public KtpException(string message, ExceptionStatus status, Exception innerException)
            {
                Message = message;
                Status = status;
                InnerException = innerException;
            }
        }

        #endregion


        #region IDisposable Support

        private bool _disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases all resources used by the <see cref="KtpClient"/>.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _host = null;
                    ConnectionData = null;
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                _disposedValue = true;
                _tcpClientStream?.Dispose();
                Client?.Close();
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~KTPClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        /// Releases all resources used by the <see cref="KtpClient"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            try
            {
                Dispose();
                return default;
            }
            catch (Exception ex)
            {
                return new ValueTask(Task.FromException(ex));
            }
        }

        #endregion
    }
}