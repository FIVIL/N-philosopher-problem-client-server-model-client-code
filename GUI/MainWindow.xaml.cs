using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using client;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Phphilosopher ph;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void conect_Click(object sender, RoutedEventArgs e)
        {
            if (ipbox.Text == "loop back") ipbox.Text = "127.0.0.1";
            //ph = new Phphilosopher(ipbox.Text, int.Parse(portbox.Text));
            //ph.startingo += go;
            //ph.OnSetCond += setcond;
            //Thread th = new Thread(() => Phphilosopher(ipbox.Text, int.Parse(portbox.Text)));
            //th.SetApartmentState(ApartmentState.STA);
            //th.Start();
            //th.IsBackground = false;
            Phphilosopher(ipbox.Text, int.Parse(portbox.Text));
            //startingo += go;
            //OnSetCond += setcond;
            status.Text = "connecting...";
            ipbox.IsEnabled = false;
            ipbox.Visibility = Visibility.Hidden;
            portbox.IsEnabled = false;
            portbox.Visibility = Visibility.Hidden;
            iplbl.IsEnabled = false;
            iplbl.Visibility = Visibility.Hidden;
            portlbl.IsEnabled = false;
            portlbl.Visibility = Visibility.Hidden;
            conect.IsEnabled = false;
            conect.Visibility = Visibility.Hidden;
            status.IsEnabled = true;
            status.Visibility = Visibility.Visible;
            fok.Visibility = Visibility.Visible;
            fok.IsEnabled = true;
            namelbl.IsEnabled = true;
            namelbl.Visibility = Visibility.Visible;
        }
        private void go(int i)
        {
            this.Dispatcher.Invoke((Action)(() =>
           {
               namelbl.Text = i + "";
           }));
        }
        private void ForkCond(int i)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (i == 3)
                {
                    fok.Text = "both";
                }
                if (i == 2)
                {
                    fok.Text = "right";
                }
                if (i == 1)
                {
                    fok.Text = "none";
                }
            }));
        }
        private void setcond(int i)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (i == 0)
                {
                    status.Text = "status!!!";
                    C.Background = Brushes.LightCyan;
                }
                if (i == 1)
                {
                    status.Text = "status: Thinking";
                    C.Background = Brushes.LightCyan;
                }
                if (i == 2)
                {
                    status.Text = "status: Waiting";
                    C.Background = Brushes.Pink;
                }
                if (i == 3)
                {
                    status.Text = "status: Eating";
                    C.Background = Brushes.LightGreen;
                }
            }));
        }
        #region ph
        public Socket SocClient;
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
            set { if (_condition != value) setcond(value); _condition = value; }
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

                if (_left && _right) ForkCond(3);
                if (!_left && _right) ForkCond(2);
                if (!_left && !_right) ForkCond(1);

            }
        }
        public bool right
        {
            get { return _right; }
            set
            {
                _right = value;

                if (_left && _right) ForkCond(3);
                if (!_left && _right) ForkCond(2);
                if (!_left && !_right) ForkCond(1);

            }
        }
        public int MyProperty { get; set; }
        private int attempt;
        private int maxattempt;
        private string mesg;
        public void Phphilosopher(string ip, int port)
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
            conectfun(ip, port);
        }
        private void conectfun(string ip, int port)
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
                        if (mesg == "close") close();
                        //Console.WriteLine(mesg);
                        if (mesg == "start")
                        {
                            Thread th = new Thread(run);
                            th.SetApartmentState(ApartmentState.STA);
                            th.Start();
                            mesg = "";
                            setcond(0);
                        }
                        if (mesg.Contains("ran:"))
                        {
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
            this.Dispatcher.Invoke((Action)(() =>
            {
                namelbl.Text = str;
            }));
            mesg = "";
        }
        private void run()
        {
            while (true)
            {
                if (Condition == 0)
                {
                    //Condition = 1;
                    send("status:thinking");
                    //mesg = "";
                    while (mesg!= "think") ;
                    check();
                }
                else if (Condition == 1)
                {
                    Thread.Sleep(thtime);
                    //Condition = 2;
                    send("status:waiting");
                    while (mesg != "w8") ;
                    check();
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
                            //Condition = 3;
                            send("status:eating");
                            while (mesg != "eat") ;
                            check();
                            Thread.Sleep(eatime);
                            left = false;
                            right = false;
                            send("done");
                            while (mesg !="ok") ;
                            check();
                            //Condition = 1;
                            send("status:thinking");
                            while (mesg != "think") ;
                            check();
                        }
                        else if (mesg == "permission denied")
                        {
                            mesg = "";
                            if (attempt >= maxattempt)
                            {
                                attempt = 0;
                                right = false;
                                send("gave up");
                                while (mesg != "ok") ;
                                check();
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
        private void check()
        {
            if (mesg == "put the fork on table")
            {
                right = false;
                mesg = "";
            }
            if (mesg == "eat")
            {
                Condition = 3;
                send("okea");
                mesg = "";
            }
            if (mesg == "w8")
            {
                Condition = 2;
                send("okw8");
                mesg = "";
            }
            if (mesg == "think")
            {
                Condition = 1;
                send("okth");
                mesg = "";
            }
            if (mesg == "ok") mesg = "";
        }
        public void close()
        {
            if (SocClient != null)
            {
                SocClient.Shutdown(SocketShutdown.Both);
                //Application.ExitThread();
                Environment.Exit(Environment.ExitCode);
                this.Close();
            }
        }
        #endregion

    }
}
