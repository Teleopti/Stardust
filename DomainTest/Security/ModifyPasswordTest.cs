using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security
{
	[TestFixture]
	public class ModifyPasswordTest
	{
		private IModifyPassword target;
		private IUserDetailRepository userDetailRepository;
		private ILoadPasswordPolicyService loadPasswordPolicyService;
		private IPerson person; 

		[SetUp]
		public void Setup()
		{
			userDetailRepository = MockRepository.GenerateStub<IUserDetailRepository>();
			loadPasswordPolicyService = MockRepository.GenerateStub<ILoadPasswordPolicyService>();
			person = MockRepository.GenerateStub<IPerson>();
			target = new ModifyPassword(loadPasswordPolicyService, userDetailRepository);
		}

		[Test]
		public void ShouldSucceed()
		{
			var uDetail = new UserDetail(null);
			userDetailRepository.Expect(r => r.FindByUser(person)).Return(uDetail);
			person.Expect(p => p.ChangePassword("old", "new", loadPasswordPolicyService, uDetail)).Return(true);
			target.Change(person, "old", "new")
				.Should().Be.True();
		}

		[Test]
		public void ShouldFail()
		{
			var uDetail = new UserDetail(null);
			userDetailRepository.Expect(r => r.FindByUser(person)).Return(uDetail);
			person.Expect(p => p.ChangePassword("old", "new", loadPasswordPolicyService, uDetail)).Return(false);
			target.Change(person, "old", "new")
				.Should().Be.False();
		}
	}
}