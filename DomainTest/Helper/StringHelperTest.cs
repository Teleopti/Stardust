using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.DomainTest.Helper
{
    [TestFixture]
    public class StringHelperTest
    {
	    private const string myName = "henry matias greijer";

	    [Test]
        public void CapitalizeWorksJustFine()
        {
            Assert.IsTrue("Henry Matias Greijer" == myName.Capitalize());
        }

        [Test]
        public void VerifySplit()
        {
            string[] strings = myName.Split(" ");
            Assert.AreEqual(3, strings.Length);
        }

		[Test]
		public void RunPerformanceShort()
		{
			const string input = "roger heter jag";
			input.DisplayString(20).Should().Be.EqualTo("roger heter jag     ");
		}

		[Test]
		public void RunPerformance20()
		{
			const string input = "roger heter jag tral";
			input.DisplayString(20).Should().Be.EqualTo("roger heter jag tral");
		}

		[Test]
		public void RunPerformanceLong()
		{
			const string input = "roger heter jag tralla la la";
			input.DisplayString(20).Should().Be.EqualTo("roger heter jag t...");
		}

		[Test]
		public void TrimLongWord()
		{
			const string input = "roger heter                                      ";
			input.DisplayString(20).Should().Be.EqualTo("roger heter         ");
		}

		[Test]
		public void EmptyThanAChar()
		{
			const string input = "                                                                   roger heter                                      ";
			input.DisplayString(20).Should().Be.EqualTo("                 ...");
		}

		[Test]
		public void ShouldCreateGuidFromString()
		{
			"hello".GenerateGuid().ToString().Should().Be.EqualTo("2a40415d-4bbc-762a-b971-9d911017c592");
		}
    }
}
