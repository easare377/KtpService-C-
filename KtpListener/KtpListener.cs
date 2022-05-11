using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using KonectSocket;

namespace KtpListener
{
    /// <summary>
    /// Provides client connections for Ktp network services.
    /// </summary>
    public class KtpListener
    {
        private readonly TcpListener _tcpListener;
        /// <summary>Initializes a new instance of the <see cref="KtpListener" /> class with the specified local endpoint.</summary>
        /// <param name="localEp">An <see cref="T:System.Net.IPEndPoint" /> that represents the local endpoint to which to bind the listener <see cref="T:System.Net.Sockets.Socket" />. </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="localEp" /> is <see langword="null" />. </exception>
        public KtpListener(IPEndPoint localEp)
        {
            if (localEp == null)
                throw new ArgumentNullException(nameof(localEp));
            _tcpListener = new TcpListener(localEp);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KtpListener"/> class with the specified local IP address and port number.
        /// </summary>
        /// <param name="localAddress">An <see cref="T:System.Net.IPAddress" /> that represents the local IP address.</param>
        /// <param name="port">The port on which to listen for incoming connection attempt.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="localAddress" /> is <see langword="null" />. </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="port" /> is not between <see cref="F:System.Net.IPEndPoint.MinPort" /> and <see cref="F:System.Net.IPEndPoint.MaxPort" />. </exception>
        public KtpListener(IPAddress localAddress, ushort port)
        {
            if (localAddress == null)
                throw new ArgumentNullException(nameof(localAddress));
            _tcpListener = new TcpListener(localAddress, port);
        }

        /// <summary>
        /// Starts listening for incoming requests.
        /// </summary>
        /// <exception cref="SocketException"></exception>
        public void Start()
        {
            _tcpListener.Start();
        }

        /// <summary>
        /// Starts listening for incoming requests with a maximum number of pending connection.
        /// </summary>
        /// <param name="backLog"></param>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start(int backLog)
        {
            if (backLog < 0)
                throw new ArgumentOutOfRangeException(nameof(backLog));
            _tcpListener.Start(backLog);
        }

        /// <summary>
        /// Closes the listener.
        /// </summary>
        /// <exception cref="SocketException"></exception>
        public void Stop()
        {
            _tcpListener.Stop();
        }

        /// <summary>Accepts a pending connection request as an asynchronous operation. </summary>
        /// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="KtpClient" /> used to send and receive data.</returns>
        /// <exception cref="InvalidOperationException">The listener has not been started with a call to <see cref="KtpListener.Start()" />. </exception>
        /// <exception cref="SocketException">Use the <see cref="P:System.Net.Sockets.SocketException.ErrorCode" />
        ///  property to obtain the specific error code. When you have obtained this code, you can refer to the Windows Sockets version 2 API error code documentation in MSDN for a detailed description of the error.
        /// </exception>
        public async Task<KtpClient> AcceptKtpClientAsync()
        {
            return await AcceptKtpClientAsync(CancellationToken.None);
        }

        /// <summary>Accepts a pending connection request as an asynchronous operation. </summary>
        /// <param name="ct">The token to monitor for cancellation request.</param>
        /// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="KtpClient" /> used to send and receive data.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException">The listener has not been started with a call to <see cref="KtpListener.Start()" />. </exception>
        /// <exception cref="SocketException">Use the <see cref="P:System.Net.Sockets.SocketException.ErrorCode" />
        ///  property to obtain the specific error code. When you have obtained this code, you can refer to the Windows Sockets version 2 API error code documentation in MSDN for a detailed description of the error.
        /// </exception>
        /// <exception cref="OperationCanceledException"></exception>
        public async Task<KtpClient> AcceptKtpClientAsync(CancellationToken ct)
        {
            if(ct == null)
                throw new ArgumentNullException(nameof(ct));
            //assign CancellationToken 'ct' to TcpListener Stop method.
            //When cancellation is requested the TcpListener.Stop() method will be called.
            ct.Register(() => _tcpListener.Stop());
            try
            {
                TcpClient client = await _tcpListener.AcceptTcpClientAsync();
                return new KtpClient(client);
            }
            catch
            {
                //throw OperationCanceledException if task cancellation is requested.
                ct.ThrowIfCancellationRequested();
                //or else throw the exception that caused the tcp listener to fail.
                throw;
            }
        }
    }
}
