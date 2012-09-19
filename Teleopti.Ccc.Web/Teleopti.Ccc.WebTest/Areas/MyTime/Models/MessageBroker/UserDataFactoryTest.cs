using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Models.MessageBroker
{
	[TestFixture]
	public class UserDataFactoryTest
	{
		[Test]
		public void ShouldSetBusinessUnitId()
		{
			var buProvider = MockRepository.GenerateMock<ICurrentBusinessUnitProvider>();
			var factory = new UserDataFactory(buProvider);

			var expected = Guid.NewGuid();
			var bu = new BusinessUnit("dd");
			bu.SetId(expected);
			buProvider.Expect(mock => mock.CurrentBusinessUnit()).Return(bu);
			var result = factory.CreateViewModel();
			result.BusinessUnitId.Should().Be.EqualTo(expected);
		}
	}
}