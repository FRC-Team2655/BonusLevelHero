using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace BonusLevel {
    public class Program {
        public const int TIMING = 20;

        public static void Main() {
            while (true) {
                System.Threading.Thread.Sleep(TIMING);
            }
        }
    }
}
