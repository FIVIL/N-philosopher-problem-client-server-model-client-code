using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client
{
    class Program
    {
        static void Main(string[] args)
        {
            Phphilosopher ph = new Phphilosopher("127.0.0.1", 5050);
            ph.OnSetCond += an;
        }
        static void an(int cond)
        {
            Console.WriteLine(cond);
        }
    }
}
