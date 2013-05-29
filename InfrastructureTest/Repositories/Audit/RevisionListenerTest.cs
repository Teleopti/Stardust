using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Audit
{
	[TestFixture]
	public class RevisionListenerTest
	{
		private RevisionListener target;
		private IUnsafePersonProvider personProvider;

		[SetUp]
		public void Setup()
		{
			personProvider = MockRepository.GenerateStrictMock<IUnsafePersonProvider>();
			target = new RevisionListener(personProvider);
		}

		[Test]
		public void ShouldSetProperties()
		{
			var person = new Person();
			personProvider.Expect(p => p.CurrentUser()).Return(person);

			var rev = new Revision();
			target.NewRevision(rev);

			rev.ModifiedBy.Should().Be.SameInstanceAs(person);
		}
	}
}