using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
	[TestFixture]
	public class IdentityHelperTest
	{
		private const string toyStory = "ToyStory";
		private const string buzzLightyear = "BuzzLightyear";

		[TestCase(toyStory, buzzLightyear, toyStory + "\\" + buzzLightyear)]
		[TestCase(" " + toyStory + " ", " " + buzzLightyear + " ", toyStory + "\\" + buzzLightyear)]
		[TestCase("", buzzLightyear, buzzLightyear)]
		[TestCase("   ", buzzLightyear, buzzLightyear)]
		[TestCase(null, buzzLightyear, buzzLightyear)]
		[TestCase(null, null, "")]
		public void ShouldMergeIdentity(string domainName, string userName, string identity)
		{
			var userIdentity = IdentityHelper.Merge(domainName, userName);
			userIdentity.Should().Be.EqualTo(identity);
		}

		[TestCase(toyStory + "\\" + buzzLightyear, toyStory, buzzLightyear)]
		[TestCase(toyStory + "\\" + buzzLightyear + "\\AndWoody", toyStory, buzzLightyear)]
		[TestCase(toyStory + "\\", toyStory, "")]
		[TestCase(" " + toyStory + " \\ " + buzzLightyear + " ", toyStory, buzzLightyear)]
		[TestCase(buzzLightyear, "", buzzLightyear)]
		public void ShouldSplitIdentity(string identity, string domainName, string userName)
		{
			var winLogon = IdentityHelper.Split(identity);
			winLogon.Item1.Should().Be.EqualTo(domainName);
			winLogon.Item2.Should().Be.EqualTo(userName);
		}
	}
}
