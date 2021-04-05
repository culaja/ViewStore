using System;
using System.Threading;

namespace WriteBehindCacheTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var queueingSystemGenerator = new QueueingSystemGenerator(1, 2, 1, DateTime.Now);
            foreach (var e in queueingSystemGenerator.Generate())
            {
                Console.WriteLine(e);
                Thread.Sleep(1000);
            }
        }
    }
}