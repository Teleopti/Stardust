using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Security;

namespace Teleopti.Ccc.Infrastructure.SecurityTest
{
	[TestFixture]
    public class OneWayEncryptionTest
    {
		[Test]
		public void HashedPasswordCanBeVerified()
		{
			const string password = "Correct battery horse staple";
			var target = new OneWayEncryption();
			
			var hash = target.CreateHash(password);

			target.Verify(password, hash).Should().Be.True();
		}

		[Test]
		public void WrongPasswordCanBeVerifiedAsWrong()
		{
			const string password = "Correct battery horse staple";
			const string incorrectPassword = "this is not the same";
			var target = new OneWayEncryption();

			var hash = target.CreateHash(password);

			target.Verify(incorrectPassword, hash).Should().Be.False();
		}

		[Test]
		public void HashedPasswordWithThisFunctionShouldBeRecognized()
		{
			const string password = "Incorrect battery horse staple";
			var target = new OneWayEncryption();

			var hash = target.CreateHash(password);

			target.IsGeneratedByThisFunction(hash).Should().Be.True();
		}

		[Test]
		public void NotHashedWithThisFunctionShouldBeRecognizedAsWrong()
		{
			const string password = "Incorrect battery horse staple";
			var target = new OneWayEncryption();


			target.IsGeneratedByThisFunction(password).Should().Be.False();
		}
	}
}
