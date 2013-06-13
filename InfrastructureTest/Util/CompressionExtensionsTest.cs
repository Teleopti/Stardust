using System;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Ccc.InfrastructureTest.Util
{
    [TestFixture]
    public class CompressionExtensionsTest
    {
        [Test]
        public void ShouldCompressAndUncompress()
        {
            var toCompress = randomString(100);
            var result = toCompress.ToCompressedBase64String();
            Assert.AreEqual(toCompress, result.ToUncompressedString());
        }

        [Test]
        public void ShouldStressCompressAndUncompress()
        {
            for (var i = 0; i < 10000; i++)
                ShouldCompressAndUncompress();
        }

        private static readonly Random Random = new Random((int)DateTime.Now.Ticks);
        private static string randomString(int size)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < size; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * Random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }
    }
}