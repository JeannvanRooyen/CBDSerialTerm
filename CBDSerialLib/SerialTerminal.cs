using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace CBDSerialLib
{
    public class DataReceivedEventArgs : EventArgs
    {
        public bool CanGrid { get; private set; }

        public bool CanGraph { get; private set; }

        public string Data { get; set; }

        public DataReceivedEventArgs(string data, bool canGrid, bool canGraph)
        {
            CanGraph = canGraph;
            Data = data;
            CanGrid = canGrid;
        }
    }

    public class SerialTerminal : IDisposable
    {
        private SerialPort _serialPort;
        private Thread _readThread;
        private bool _continue;

        public static List<int> BaudRates { get; } = new List<int> { 9600, 19200, 38400, 57600, 115200 };

        public static string[] PortNames => SerialPort.GetPortNames();

        public event EventHandler<DataReceivedEventArgs>? DataReceived;

        public event EventHandler<string>? DataSent;

        public event EventHandler<Exception>? Exceptioned;

        public void Flush()
        {
            _serialPort.DiscardInBuffer();
        }

        public SerialTerminal(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake, int readTimeout, int writeTimeout, bool rtsEnabled, bool dtrEnabled)
        {
            _serialPort = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate,
                Parity = parity,
                DataBits = dataBits,
                StopBits = stopBits,
                Handshake = handshake,
                ReadTimeout = readTimeout,
                WriteTimeout = writeTimeout,
                RtsEnable = rtsEnabled,
                DtrEnable = dtrEnabled
            };

            _readThread = new Thread(Read);
        }

        public void Open()
        {
            try
            {
                if (_readThread.ThreadState == ThreadState.Stopped)
                {
                    _readThread = new Thread(Read);
                }

                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                    _continue = true;

                    if (_readThread.ThreadState == ThreadState.Unstarted && !_readThread.IsAlive)
                    {
                        _readThread.Start();
                    }
                }

                if (!_serialPort.IsOpen)
                {
                    throw new Exception("Failed to open serial port.");
                }
            }
            catch (Exception err)
            {
                OnExceptioned(err);
                throw; // Rethrow the exception to preserve stack trace
            }
        }

        public bool IsOpen => _serialPort.IsOpen;

        public void Close()
        {
            try
            {
                _continue = false;

                if (_readThread.ThreadState != ThreadState.Stopped && _readThread.IsAlive)
                {
                    _readThread.Join();
                }

                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
            }
            catch (Exception err)
            {
                OnExceptioned(err);
                throw; // Rethrow the exception to preserve stack trace
            }
        }

        public void Write(string message)
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.WriteLine(message);
                    OnDataSent(message);
                }
                else
                {
                    throw new Exception("Trying to write to closed serial port.");
                }
            }
            catch (Exception err)
            {
                OnExceptioned(err);
                throw; // Rethrow the exception to preserve stack trace
            }
        }

        private void Read()
        {
            try
            {
                while (_continue)
                {
                    if (_serialPort.IsOpen)
                    {
                        if (_serialPort.BytesToRead > 0)
                        {
                            try
                            {
                                var message = _serialPort.ReadLine();
                                OnDataReceived(message);
                            }
                            catch (TimeoutException) { }
                        }
                        else
                        {
                            Thread.Sleep(50);
                        }
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
            }
            catch (Exception err)
            {
                OnExceptioned(err);
                throw; // Rethrow the exception to preserve stack trace
            }
        }

        protected virtual void OnDataReceived(string message)
        {
            var canGrid = message.Split('.').Length > 1;
            var canGraph = message.Split(':').Length > 1;

            DataReceived?.Invoke(this, new DataReceivedEventArgs(message,canGrid,canGraph));
        }

        protected virtual void OnDataSent(string message)
        {
            DataSent?.Invoke(this, message);
        }

        protected virtual void OnExceptioned(Exception e)
        {
            Exceptioned?.Invoke(this, e);
        }

        public void Dispose()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            _serialPort.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}