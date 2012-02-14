using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using Teleopti.Core;

namespace Teleopti.Messaging.Tests
{
    [TestFixture]
    public class ThreadPoolTest
    {
        private readonly ManualResetEvent resetEvent = new ManualResetEvent(false);

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void CustomThreadPoolTests()
        {
            CustomThreadPool pool = new CustomThreadPool(3, "CustomThreadPoolTests");
            // Instantiate the delegate using an anonymous method
            pool.QueueUserWorkItem(PrimeNumberCalculator, 100);
            pool.QueueUserWorkItem(PrimeNumberCalculator, 100);
            pool.QueueUserWorkItem(PrimeNumberCalculator, 100);
            resetEvent.WaitOne(2000, false);
            pool.Dispose();
        }

        [Test]
        public void CustomThreadPoolExceptionTests()
        {
            CustomThreadPool pool = new CustomThreadPool(5, "CustomThreadPoolTests");
            // Instantiate the delegate using an anonymous method
            EventHandler<UnhandledExceptionEventArgs> handler = PoolUnhandledException;
            pool.UnhandledException += handler;
            pool.QueueUserWorkItem(PrimeNumberCalculator, 100);
            pool.QueueUserWorkItem(PrimeNumberCalculator, 100);
            pool.QueueUserWorkItem(delegate { throw new ArithmeticException("testing handling of exceptions."); }, 100);
            resetEvent.WaitOne(2000, false);
            pool.UnhandledException -= handler;
            pool.Dispose();
        }

        public void PoolUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
        }

        #pragma warning disable 1692

        private static void PrimeNumberCalculator(object arg)
        {
            int max = Convert.ToInt32(arg, CultureInfo.InvariantCulture);
            IList<int> primeNumbers = new List<int>();
            for (int i = 2; i < max; i++)
            {
                bool divisible = false;
                foreach (int number in primeNumbers)
                    if (i % number == 0)
                        divisible = true;

                if (divisible == false)
                    primeNumbers.Add(i);
            }
        }

        #pragma warning restore 1692

        [TearDown]
        public void TearDown()
        {
        }

    }
}
