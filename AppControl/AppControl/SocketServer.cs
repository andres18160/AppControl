using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace AppControl
{
    internal class SocketServer
    {
        private readonly int _port;
        public int Port { get { return _port; } }
        private StreamSocketListener listener;
        private DataWriter _writer;
        public delegate void DataRecived(string data);
        public event DataRecived OnDataRecived;
        public delegate void Error(string message);
        public event Error OnError;
        public bool status { get; set; }
      //  DatagramSocket listenerSocket = null;
        public SocketServer(int port)
        {
            _port = port;
        }
        public async void Star()
        {
            try
            {
                if (listener != null)
                {
                    await listener.CancelIOAsync();
                    listener.Dispose();
                    listener = null;
                }
                listener = new StreamSocketListener();
                listener.ConnectionReceived += Listener_ConnectionReceived;

                await listener.BindServiceNameAsync(Port.ToString());
                status = true;
                Debug.WriteLine("SERVIDOR INICIADO");
            }
            catch (Exception ex)
            {

                if (OnError != null)
                    OnError("Error en el metodo Star= " + ex.Message);
                Debug.WriteLine("Error en el metodo Star= " + ex.Message);
                Star();
            }
        }
        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            var reader = new DataReader(args.Socket.InputStream);
            reader.InputStreamOptions = InputStreamOptions.Partial;
            reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            reader.ByteOrder = ByteOrder.LittleEndian;
            _writer = new DataWriter(args.Socket.OutputStream);
            uint sizeToReadEachTime = 43554432;

            try
            {
                while (true)
                {
                    uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
                    if (sizeFieldCount != sizeof(uint))
                        return;
                    uint stringLenght = reader.ReadUInt32();
                    uint actualStringLength = await reader.LoadAsync(sizeToReadEachTime);
                    if (OnDataRecived != null)
                    {
                        string data = reader.ReadString(actualStringLength);
                        Debug.WriteLine("Datos Recividos= " + data);
                        OnDataRecived(data);
                    }
                }
            }
            catch (Exception ex)
            {
                status = false;

                  if (OnError != null)
                     OnError("Error en el metodo Listener_ConnectionReceived= " + ex.Message);
                Debug.WriteLine("Error en el metodo Listener_ConnectionReceived= " + ex.Message);
                Star();
            }
        }
        public async void Send(string message)
        {
            if (_writer != null)
            {            
                //_writer.WriteUInt32(_writer.WriteString(message));
                //_writer.WriteString(message);
                _writer.WriteBytes(Encoding.UTF8.GetBytes(message));
                try
                {
                    await _writer.StoreAsync();
                    await _writer.FlushAsync();
                }
                catch (Exception ex)
                {
                    _writer = null;
                    status = false;
                    if (OnError != null)
                        OnError("Error en el metodo Send= " + ex.Message);
                    Debug.WriteLine("Error en el metodo Send= " + ex.Message);
                    Star();
                }
            }

        }

    }
}