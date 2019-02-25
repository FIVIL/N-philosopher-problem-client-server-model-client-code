using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace client
{
    public delegate void SetCond(int cond);
    public class Phphilosopher
    {
        public Socket SocClient;
        public event SetCond OnSetCond;
        public event SetCond ForkCond;
        public event SetCond startingo;
        private int _condition;
        /// <summary>
        /// 0 start
        /// 1 think
        /// 2 w8
        /// 3 eat
        /// </summary>
        public int Condition
        {
            get { return _condition; }
            set { if (OnSetCond != null && _condition != value) OnSetCond(value); _condition = value; }
        }
        private int thtime;
        private int eatime;
        private bool _left;
        private bool _right;
        public bool left
        {
            get { return _left; }
            set
            {
                _left = value;
                if (ForkCond != null)
                {
                    if (_left && _right) ForkCond(3);
                    if (!_left && _right) ForkCond(2);
                    if (!_left && _right) ForkCond(1);
                }
            }
        }
        public bool right
        {
            get { return _right; }
            set
            {
                _right = value;
                if (ForkCond != null)
                {
                    if (_left && _right) ForkCond(3);
                    if (!_left && _right) ForkCond(2);
                    if (!_left && _right) ForkCond(1);
                }
            }
        }
        public int MyProperty { get; set; }
        private int attempt;
        private int maxattempt;
        private string mesg;
        private string _name;
        public string name
        {
            get { return _name; }
            set
            {
                _name = value;
                if (startingo != null) startingo(int.Parse(value));
            }
        }

        public Phphilosopher(string ip, int port)
        {
            mesg = "";
            thtime = 0;
            eatime = 0;
            left = false;
            right = false;
            attempt = 0;
            maxattempt = 0;
            Condition = 0;
            SocClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            conect(ip, port);
        }
        private void conect(string ip, int port)
        {
            SocClient.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
            Thread tr = new Thread(GetData);
            tr.SetApartmentState(ApartmentState.STA);
            tr.Start();
        }
        private void GetData()
        {
            while (true)
            {
                try
                {
                    byte[] b = new byte[1024];
                    int r = SocClient.Receive(b);
                    if (r > 0)
                    {
                        mesg = (Encoding.Unicode.GetString(b, 0, r));
                        //Console.WriteLine(mesg);
                        if (mesg == "start")
                        {
                            Thread th = new Thread(run);
                            th.SetApartmentState(ApartmentState.STA);
                            th.Start();
                            mesg = "";
                            if (startingo != null) startingo(0);
                        }
                        if (mesg.Contains("ran:"))
                        {
                            send("hi");
                            set(mesg);
                        }
                    }
                }
                catch
                {
                    ;
                }
            }
        }
        private void send(string msg)
        {
            byte[] b = Encoding.Unicode.GetBytes(msg);
            SocClient.Send(b);
        }
        private void set(string str)
        {
            str = str.Remove(0, 4);
            thtime = int.Parse(str.Substring(0, str.IndexOf(':')));
            str = str.Remove(0, str.IndexOf(':') + 1);
            eatime = int.Parse(str.Substring(0, str.IndexOf(':')));
            str = str.Remove(0, str.IndexOf(':') + 1);
            maxattempt = int.Parse(str.Substring(0, str.IndexOf(':')));
            str = str.Remove(0, str.IndexOf(':') + 1);
            name = str;
            mesg = "";
        }
        private void run()
        {
            while (true)
            {
                if (Condition == 0)
                {
                    Condition = 1;
                    send("status:thinking");
                }
                else if (Condition == 1)
                {
                    Thread.Sleep(thtime);
                    Condition = 2;
                    send("status:waiting");
                }
                else if (Condition == 2)
                {
                    if (!right)
                    {
                        send("request right");
                        while (mesg.Length == 0) ;
                        if (mesg == "permission granted")
                        {
                            right = true;
                            mesg = "";
                        }
                        else if (mesg == "permission denied")
                        {
                            mesg = "";
                        }
                        else
                        {
                            Console.WriteLine("OMGR");
                        }
                    }
                    if (!left && right)
                    {
                        attempt++;
                        send("request left");
                        while (mesg.Length == 0) ;
                        if (mesg == "permission granted")
                        {
                            left = true;
                            mesg = "";
                            Condition = 3;
                            send("status:eating");
                            Thread.Sleep(eatime);
                            left = false;
                            right = false;
                            send("done");
                            Condition = 1;
                            send("status:thinking");
                        }
                        else if (mesg == "permission denied")
                        {
                            mesg = "";
                            if (attempt >= maxattempt)
                            {
                                attempt = 0;
                                right = false;
                                send("gave up");
                            }
                        }
                        else
                        {
                            Console.WriteLine("OMGL");
                        }
                    }
                }
            }
        }
    }
}
