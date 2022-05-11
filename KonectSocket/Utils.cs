using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KonectSocket
{
    internal static class Functions
    {
        // ///<summary>
        // ///Asynchronously read a tcp stream and return an object of specified type.
        // ///</summary>
        // /// <param name="networkStream">The <see cref="NetworkStream"/> to read data from.</param>
        // /// <param name="includeObjectTypes">Indicates if object <see cref="Type"/> info should be added to the data to be sent.</param>
        // /// <returns>Returns an object of specified type.</returns>
        // /// <exception cref="ArgumentNullException"></exception>
        // /// <exception cref="ArgumentException"></exception>
        // /// <exception cref="ObjectDisposedException"></exception>
        // /// <exception cref="SocketException"></exception>
        // /// <exception cref="InvalidDataException"></exception>
        // /// <exception cref="JsonException"></exception>
        // /// <exception cref="TaskCanceledException"></exception>
        // internal static async Task<T> ReadNetworkStreamAsync<T>(NetworkStream networkStream, bool includeObjectTypes)
        // {
        //     return await ReadNetworkStreamAsync<T>(networkStream, includeObjectTypes,CancellationToken.None);
        // }

        // /// <summary>
        // /// Asynchronously reads a tcp stream and return an object of specified type.
        // /// </summary>
        // /// <param name="networkStream">The <see cref="NetworkStream"/> to read data from.</param>
        // /// <param name="includeObjectTypes">Indicates if object <see cref="Type"/> info should be added to the data to be sent.</param>
        // /// <param name="contentLength">The size of the request body.</param>
        // /// <returns>Returns an object of specified type.</returns>
        // /// <exception cref="ArgumentNullException"></exception>
        // /// <exception cref="ArgumentException"></exception>
        // /// <exception cref="ObjectDisposedException"></exception>
        // /// <exception cref="SocketException"></exception>
        // /// <exception cref="InvalidDataException"></exception>
        // /// <exception cref="JsonException"></exception>
        // /// <exception cref="TaskCanceledException"></exception>
        // internal static async Task<T> ReadNetworkStreamAsync<T>(NetworkStream networkStream, uint contentLength, bool includeObjectTypes)
        // {
        //     return await ReadNetworkStreamAsync<T>(networkStream, includeObjectTypes, CancellationToken.None, contentLength);
        // }

        // ///<summary>
        // ///Asynchronously read a tcp stream and return an object of specified type.
        // ///</summary>
        // /// <param name="networkStream">The <see cref="NetworkStream"/> to read data from.</param>
        // /// <param name="includeObjectTypes">Indicates if object <see cref="Type"/> info should be added to the data to be sent.</param>
        // /// <param name="ct">The token to monitor for cancellation requests.</param>
        // /// <returns>Returns an object of specified type.</returns>
        // /// <exception cref="ArgumentNullException"></exception>
        // /// <exception cref="ArgumentException"></exception>
        // /// <exception cref="ObjectDisposedException"></exception>
        // /// <exception cref="SocketException"></exception>
        // /// <exception cref="InvalidDataException"></exception>
        // /// <exception cref="JsonException"></exception>
        // /// <exception cref="TaskCanceledException"></exception>
        // internal static async Task<T> ReadNetworkStreamAsync<T>(NetworkStream networkStream, bool includeObjectTypes, CancellationToken ct)
        // {
        //     return await ReadNetworkStreamAsync<T>(networkStream,includeObjectTypes, ct, null);
        // }

        // ///<summary>
        // ///Asynchronously read a tcp stream and return an object of specified type.
        // ///</summary>
        // /// <param name="networkStream">The <see cref="NetworkStream"/> to read data from.</param>
        // /// <param name="contentLength">The size of the request body.</param>
        // /// <param name="includeObjectTypes">Indicates if object <see cref="Type"/> info should be added to the data to be sent.</param>
        // /// <param name="ct">The token to monitor for cancellation requests.</param>
        // /// <returns>Returns an object of specified type.</returns>
        // /// <exception cref="ArgumentNullException"></exception>
        // /// <exception cref="ArgumentException"></exception>
        // /// <exception cref="ObjectDisposedException"></exception>
        // /// <exception cref="SocketException"></exception>
        // /// <exception cref="InvalidDataException"></exception>
        // /// <exception cref="JsonException"></exception>
        // /// <exception cref="TaskCanceledException"></exception>
        // internal static async Task<T> ReadNetworkStreamAsync<T>(NetworkStream networkStream, uint contentLength, bool includeObjectTypes, CancellationToken ct)
        // {
        //     return await ReadNetworkStreamAsync<T>(networkStream, includeObjectTypes,ct, contentLength);
        // }

        // ///<summary>
        // ///Asynchronously read a tcp stream and return an object of specified type.
        // ///</summary>
        // /// <param name="networkStream">The <see cref="NetworkStream"/> to read data from.</param>
        // /// <param name="includeObjectTypes">Indicates if object <see cref="Type"/> info should be added to the data to be sent.</param>
        // /// <param name="contentLength">The size of the request body.</param>
        // /// /// <param name="ct">The token to monitor for cancellation requests.</param>
        // /// <returns>Returns an object of specified type.</returns>
        // /// <exception cref="ArgumentNullException"></exception>
        // /// <exception cref="ArgumentException"></exception>
        // /// <exception cref="ObjectDisposedException"></exception>
        // /// <exception cref="SocketException"></exception>
        // /// <exception cref="InvalidDataException"></exception>
        // /// <exception cref="JsonException"></exception>
        // /// <exception cref="TaskCanceledException"></exception>
        // private static async Task<T> ReadNetworkStreamAsync<T>(NetworkStream networkStream, CancellationToken ct, uint? contentLength)
        // {
        //     if (networkStream == null)
        //         throw new ArgumentNullException(nameof(networkStream));
        //     if (ct == null)
        //         throw new ArgumentNullException(nameof(ct));
        //     //get the size of the request content.
        //     uint messageLength;
        //     if (contentLength.HasValue)
        //         //message length is assigned when the caller provides the content length.
        //         messageLength = contentLength.Value;
        //     else
        //         //if content length is not provided the message length is retrieved from the request.
        //         messageLength = await GetRequestContentLength(networkStream, ct);
        //     //return default or null if messageLength is not specified in the header.
        //     if (messageLength == 0)
        //         return default;
        //     var buffer = new byte[messageLength];
        //     //read the rest of the data from the network stream.
        //     int byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length, ct);
        //     if (byteCount <= 0)
        //         throw new SocketException(10054);
        //     //if T is of type byte[] return the raw bytes.
        //     if (typeof(T) == typeof(byte[]))
        //         return (T)(dynamic)buffer;
        //     //message body should be in the json format
        //     string messageBodyJson = Encoding.UTF8.GetString(buffer);
        //     //serialize object to json string and convert to bytes.
        //     if (includeObjectTypes)
        //     {
        //         var jsonSerializerSettings = new JsonSerializerSettings();
        //         jsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;
        //         return (T)JsonConvert.DeserializeObject(messageBodyJson, jsonSerializerSettings);
        //     }
        //     return JsonConvert.DeserializeObject<T>(messageBodyJson);
        // }

        /// <summary>
        /// Asynchronously gets the size of the current message body.
        /// </summary>
        /// <param name="networkStream">The <see cref="NetworkStream"/> to read data from.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="TaskCanceledException"></exception>
        /// <returns></returns>
        internal static async Task<uint> GetRequestContentLength(NetworkStream networkStream)
        {
            return await GetRequestContentLength(networkStream, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously gets the size of the current message body.
        /// </summary>
        /// <param name="networkStream">The <see cref="NetworkStream"/> to read data from.</param>
        /// <param name="ct">The token to monitor for cancellation requests.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="TaskCanceledException"></exception>
        /// <returns></returns>
        internal static async Task<uint> GetRequestContentLength(NetworkStream networkStream, CancellationToken ct)
        {
            if (networkStream == null)
                throw new ArgumentNullException(nameof(networkStream));
            if (ct == null)
                throw new ArgumentNullException(nameof(ct));
            //read the 1st byte from the network stream.
            //the 1st byte represents the max size of the data been sent.           
            var buffer = new byte[1];
            var byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length, ct);
            //if byteCount returns 0 throw exception 
            if (byteCount <= 0)
                throw new Exception("An existing connection was forcibly closed by the remote host.");
            uint messageLength;
            byte header = buffer[0];
            //if the stream header = 1 then the message body of <= 255 bytes is read from the network stream.
            switch (header)
            {
                case 0:
                    messageLength = 0;
                    break;
                case 1:
                    buffer = new byte[header];
                    byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length, ct);
                    if (byteCount <= 0)
                        throw new SocketException(10054);
                    messageLength = buffer[0];
                    //message length must be greater than 0 per Ktp protocols.
                    //throw exception if message length is = 0
                    if (messageLength == 0)
                        throw new InvalidDataException();
                    break;
                //if the stream header = 2 then the message body of <= 65535 bytes is read from the network stream.
                case 2:
                    //read the next 2bytes of the network stream which represents the size of the message body in 16bits.
                    buffer = new byte[header];
                    byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length, ct);
                    if (byteCount <= 0)
                        throw new SocketException(10054);
                    //convert data from big-endian order to little-endian order.
                    //messageLength = BitConverter.ToUInt16(buffer, 0);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(buffer);
                    messageLength = BitConverter.ToUInt16(buffer, 0);
                    if (messageLength <= byte.MaxValue)
                        throw new InvalidDataException();
                    break;
                //if the stream header = 4 then the message body of <= 4294967295 bytes is read from the network stream.
                case 4:
                    //read the next 4bytes of the network stream which represents the size of the message body in 32bits.
                    buffer = new byte[header];
                    byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length, ct);
                    if (byteCount <= 0)
                        throw new SocketException(10054);
                    //convert data from big-endian order to little-endian order.
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(buffer);
                    messageLength = BitConverter.ToUInt32(buffer, 0);
                    if (messageLength <= ushort.MaxValue)
                        throw new InvalidDataException();
                    break;
                default:
                    throw new InvalidDataException();
            }

            return messageLength;
        }

        /// <summary>
        /// Gets the <see cref="KtpClient"/> formatted data to be sent to a <see cref="NetworkStream"/>.
        /// </summary>
        /// <param name="dataBuffer">The data to be formatted.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static byte[] GetWriteData(byte[] dataBuffer)
        {
            if (dataBuffer == null)
                throw new ArgumentNullException(nameof(dataBuffer));
            if (dataBuffer.LongLength > uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(dataBuffer), dataBuffer.LongLength,
                    $"Data size must be less than or equals to {uint.MaxValue} bytes.");
            //if data buffer content is 0 only, then the writer is sending an empty message. 
            if (dataBuffer.Length == 1 && dataBuffer[0] == 0)
                return dataBuffer;
            //a header Buffer indicates the type of message been sent.
            byte[] headerBuffer;
            //contentLengthBuffer indicates the size of the message.
            byte[] contentLengthBuffer;
            //dataBuffer contains the data body of the response.
            //the message is the size of the message content to be sent over the network.
            uint messageSize = (uint) dataBuffer.Length;
            if (messageSize <= byte.MaxValue)
            {
                headerBuffer = new[] {(byte) 1};
                contentLengthBuffer = new[] {(byte) messageSize};
            }
            else if (messageSize <= ushort.MaxValue)
            {
                headerBuffer = new[] {(byte) 2};
                contentLengthBuffer = BitConverter.GetBytes((ushort) messageSize);
            }
            else
            {
                headerBuffer = new[] {(byte) 4};
                contentLengthBuffer = BitConverter.GetBytes(messageSize);
            }

            //convert data to big-endian order.
            if (BitConverter.IsLittleEndian)
                Array.Reverse(contentLengthBuffer);
            //the messageSize is appended to the headerBuffer.
            headerBuffer = headerBuffer.Concat(contentLengthBuffer).ToArray();
            var responseBuffer =
                new byte[headerBuffer.Length + dataBuffer.Length]; //headerBuffer.Concat(dataBuffer).ToArray();
            //copy header and data buffer to the response buffer.
            Array.Copy(headerBuffer, responseBuffer, headerBuffer.Length);
            Array.Copy(dataBuffer, 0, responseBuffer, headerBuffer.Length, dataBuffer.Length);
            return responseBuffer;
        }

        /// <summary>
        /// Reads all the available data from a <see cref="TcpClient"/>`s <see cref="NetworkStream"/>.
        /// </summary>
        /// <param name="tcpClient">The <see cref="TcpClient"/> to read the data from.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        internal static async Task<byte[]> ReadTcpClientStreamAsync(TcpClient tcpClient)
        {
            if (tcpClient == null)
                throw new ArgumentNullException(nameof(tcpClient));
            var buffer = new byte[tcpClient.Available];
            await tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length);
            return buffer;
        }

        public static async Task<byte[]> ReadInputStreamAsync(NetworkStream networkStream, int length,
            CancellationToken ct)
        {
            byte[] buffer = new byte[length];
            int totalBytesRead = 0;
            while (totalBytesRead != length)
            {
                int byteRead = await networkStream.ReadAsync(buffer, totalBytesRead, length - totalBytesRead, ct);
                if (byteRead == -1)
                {
                    throw new SocketException();
                }

                totalBytesRead += byteRead;
            }

            return buffer;
        }
    }
}