﻿using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture, RequestsTest]
	public class OvertimeRequestPersisterTest : ISetup
	{
		public IOvertimeRequestPersister Target;
		public IMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRepository;
		private IPerson _person;
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			_person = PersonFactory.CreatePerson();

			system.UseTestDouble(new FakeLoggedOnUser(_person)).For<ILoggedOnUser>();
			system.UseTestDouble<FakeMultiplicatorDefinitionSetRepository>().For<IMultiplicatorDefinitionSetRepository>();
			system.UseTestDouble(new FakeLinkProvider()).For<ILinkProvider>();
		}

		[Test]
		public void ShouldChangeStatusToPendingWhenPersistOvertimeRequest()
		{
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime).WithId();
			MultiplicatorDefinitionSetRepository.Add(multiplicatorDefinitionSet);

			var overtimeRequestForm = new OvertimeRequestForm {MultiplicatorDefinitionSet = multiplicatorDefinitionSet.Id.Value};

			var result = Target.Persist(overtimeRequestForm);
			Assert.AreEqual(result.IsPending, true);
		}
	}
}