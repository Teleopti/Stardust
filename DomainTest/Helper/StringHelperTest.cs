using System;
using System.Diagnostics;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.DomainTest.Helper
{
    /// <summary>
    /// Test class for StringHelper
    /// </summary>
    /// <remarks>
    /// Created by: henryg
    /// Created date: 2007-12-13
    /// </remarks>
    [TestFixture]
    public class StringHelperTest
    {
        private string myName = "henry matias greijer";

        [Test]
        public void CapitalizeWorksJustFine()
        {
            Assert.IsTrue("Henry Matias Greijer" == StringHelper.Capitalize(myName));
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
			StringHelper.DisplayString(input, 20)
							 .Should().Be.EqualTo("roger heter jag     ");

		}

		[Test]
		public void RunPerformance20()
		{
			const string input = "roger heter jag tral";
			StringHelper.DisplayString(input, 20)
											 .Should().Be.EqualTo("roger heter jag tral");
		}

		[Test]
		public void RunPerformanceLong()
		{
			const string input = "roger heter jag tralla la la";
			StringHelper.DisplayString(input, 20)
											 .Should().Be.EqualTo("roger heter jag t...");
		}

		[Test]
		public void TrimLongWord()
		{
			const string input = "roger heter                                      ";
			StringHelper.DisplayString(input, 20)
											 .Should().Be.EqualTo("roger heter         ");
		}

		[Test]
		public void EmptyThanAChar()
		{
			const string input = "                                                                   roger heter                                      ";
			StringHelper.DisplayString(input, 20)
											 .Should().Be.EqualTo("                 ...");
		}

		[Test]
		public void ShouldCreateGuidFromString()
		{
			StringHelper.GenerateGuid("hello").ToString().Should().Be.EqualTo("2a40415d-4bbc-762a-b971-9d911017c592");
		}
    }
}
