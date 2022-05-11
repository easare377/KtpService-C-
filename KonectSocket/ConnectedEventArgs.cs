using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonectSocket
{
    /// <summary>
    /// Contains arguments associated with <see cref="KtpClient.Connected"/> event.
    /// </summary>
    public class ConnectedEventArgs
    {
        /// <summary>
        /// Gets a value which indicates the error that occurs in a <see cref="KtpClient"/> persistent connection.
        /// </summary>
        public Exception Error { get; }

        /// <summary>
        /// Gets the current connection state of the <see cref="KtpClient"/> connection.
        /// </summary>
        public KtpClient.ConnectionState ConnectionState { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="ConnectedEventArgs"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> that occurs in a <see cref="KtpClient"/> persistent connection.</param>
        /// <param name="connectionState">The connection state of the <see cref="KtpClient"/> persistent connection.</param>
        public ConnectedEventArgs(Exception exception, KtpClient.ConnectionState connectionState)
        {
            Error = exception;
            ConnectionState = connectionState;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ConnectedEventArgs"/>.
        /// </summary>
        /// <param name="connectionState">The connection state of the <see cref="KtpClient"/> persistent connection.</param>
        public ConnectedEventArgs(KtpClient.ConnectionState connectionState)
        {
            ConnectionState = connectionState;
        }
    }
}
