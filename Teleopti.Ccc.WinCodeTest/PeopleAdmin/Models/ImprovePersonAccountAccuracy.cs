using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using SharpTestsEx;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
	[TestFixture]
	[Toggle(Toggles.People_ImprovePersonAccountAccuracy_74914)]
	public class ImprovePersonAccountAccuracy
	{
		private IPersonPeriodModel _target;



		[Test]
		public void ShouldReturnEmploymentChangedEventWhenContractIsChanged()
		{
			DateOnly dateOnly = new DateOnly(2018,03,28);
			Person person =(Person) PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018,1,1));
			_target = new PersonPeriodModel(dateOnly,person,new List<IPersonSkill>(), new List<IExternalLogOn>(), new List<SiteTeamModel>(),new CommonNameDescriptionSetting());

			person.PopAllEvents();

			_target.Contract = ContractFactory.CreateContract("new name");

			person.PopAllEvents().Should().Not.Be.Empty();


		}
	}

	
}