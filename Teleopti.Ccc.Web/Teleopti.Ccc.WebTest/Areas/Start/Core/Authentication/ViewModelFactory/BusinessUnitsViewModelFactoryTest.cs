using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.WebTest.Areas.Start.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Start.Core.Authentication.ViewModelFactory
{
	[TestFixture]
	public class BusinessUnitsViewModelFactoryTest
	{
		[Test]
		public void ShouldRetrieveBusinessUnitsForPerson()
		{
			var businessUnitProvider = MockRepository.GenerateMock<IBusinessUnitProvider>();
			var target = new BusinessUnitsViewModelFactory(businessUnitProvider);
			var dataSource = new FakeDataSource();
			var person = new Person();
			var businessUnit = new BusinessUnit("bu");
			businessUnit.SetId(Guid.NewGuid());
			businessUnitProvider.Stub(x => x.RetrieveBusinessUnitsForPerson(dataSource, person)).Return(new[] { businessUnit });

			var result = target.BusinessUnits(dataSource, person);

			result.Single().Id.Should().Be(businessUnit.Id);
			result.Single().Name.Should().Be(businessUnit.Name);
		}
	}
}