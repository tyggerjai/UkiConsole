﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UkiConsole
{
    class TCPSender : Sender
    {
        private TcpClient _tcpClient;
        // private State state = new State();
        private ConcurrentQueue<RawMove> _moveOut = new();
        private NetworkStream _networkStream;
        private bool _run = true;
        private string _addr;
        private int _port;
        private bool _toggle_UDPdat = false;
        private short _prevVal = 0;
        public ConcurrentQueue<RawMove> MoveOut { get => _moveOut; }


        public TCPSender(TcpClient tcpclient)
        {

           
            try
            {

                _tcpClient = tcpclient;
                _tcpClient.Connect("192.168.1.110", 10001);


                _networkStream = _tcpClient.GetStream();
                    _networkStream.ReadTimeout = 2000;
               
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }



        public void Run()
        {
            try
            {


               
                _run = true;
                while (_run == true)
                {
                    while (!MoveOut.IsEmpty)
                    {
                        RawMove _mv;

                        MoveOut.TryDequeue(out _mv);

                        sendStatus(_mv);
                    }


                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        public void ShutDown()
        {
            _tcpClient.Close();
        }

        public void Enqueue(RawMove mv)
        {

            MoveOut.Enqueue(mv);
        }
        private void sendStatus(RawMove _mv)
        {


            // This should be a static singleton....
            byte[] data = new byte[6];
            byte[] _add = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(short.Parse(_mv.Addr)));
            byte[] _reg = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(_mv.Reg));
            byte[] _val = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(_mv.Val));
            // System.Diagnostics.Debug.WriteLine("UDP AXIS: " + _mv.Addr);
            //  System.Diagnostics.Debug.WriteLine("UDP REG: " + _mv.Reg.ToString());
            //  System.Diagnostics.Debug.WriteLine("UDP VAL: " + _mv.Val.ToString());
            data[0] = _add[1];
            data[1] = _add[0];


            // XXX Somewhere we are flipping network to host once too often.
            // This fixes that.
            data[2] = _reg[1];
            data[3] = _reg[0];
            data[4] = _val[1];
            data[5] = _val[0];


           /* if (_mv.Addr == "21" && _mv.Reg == 299 && _mv.Val != _prevVal )
            {
                _prevVal = _mv.Val;
                _toggle_UDPdat = true;
                DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;
                string formtime = String.Format("IN WRAPPER QUEUE {0}", now.ToString("mm:ss:fff"));
                System.Diagnostics.Debug.WriteLine(formtime);
            }
            else
            {
                _toggle_UDPdat = false;
                _prevVal = 0;
            }*/
            Send(data);

        }
        public void Send(byte[] message)
        {
           // System.Diagnostics.Debug.WriteLine("TCP trying");

            
            if (!_tcpClient.Connected)
            {
                System.Diagnostics.Debug.WriteLine("reconnecting");
                _tcpClient = new TcpClient();
                _tcpClient.Connect("192.168.1.110", 10001);
                _networkStream = _tcpClient.GetStream();
            }

            if (_networkStream is null)
            {
                System.Diagnostics.Debug.WriteLine("no stream");

                _networkStream = _tcpClient.GetStream();
            }
            if (_tcpClient.Connected)
                {
                    try
                {
                   /* if (_toggle_UDPdat)
                    {
                        DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;
                        string formtime = String.Format("IN UDP SOCKET{0}", now.ToString("mm:ss:fff"));
                        System.Diagnostics.Debug.WriteLine(formtime);
                    }*/
                    _networkStream.Write(message, 0, message.Length);
                    _networkStream.Flush();
                  //  System.Diagnostics.Debug.WriteLine("TCP sending");

                }
                catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("No TCP connection to send to");
                   // _tcpClient.Close();
                }
                }


            }
        
    }
}
