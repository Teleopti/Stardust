using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.IoC;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
	[TestFixture]
	[Toggle(Toggles.People_ImprovePersonAccountAccuracy_74914)]
	public class ImprovePersonAccountAccuracyTest
	{

		[Test]
		public void ShouldReturnEmploymentChangedEventWhenContractIsChanged()
		{
			DateOnly dateOnly = new DateOnly(2018,03,28);
			Person person =(Person) PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018,1,1));
			var target = new PersonPeriodModel(dateOnly,person,new List<IPersonSkill>(), new List<IExternalLogOn>(), new List<SiteTeamModel>(),new CommonNameDescriptionSetting());

			person.PopAllEvents();

			target.Contract = ContractFactory.CreateContract("new name");

			person.PopAllEvents().OfType<PersonEmployementChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnEmploymentChangedEventWhenContractScheduleIsChanged()
		{
			DateOnly dateOnly = new DateOnly(2018, 03, 28);
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1));
			var target = new PersonPeriodModel(dateOnly, person, new List<IPersonSkill>(), new List<IExternalLogOn>(), new List<SiteTeamModel>(), new CommonNameDescriptionSetting());

			person.PopAllEvents();

			target.ContractSchedule = ContractScheduleFactory.CreateContractSchedule("new name");

			person.PopAllEvents().OfType<PersonEmployementChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnEmploymentChangedEventWhenPartTimePercentageIsChanged()
		{
			DateOnly dateOnly = new DateOnly(2018, 03, 28);
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1));
			var target = new PersonPeriodModel(dateOnly, person, new List<IPersonSkill>(), new List<IExternalLogOn>(), new List<SiteTeamModel>(), new CommonNameDescriptionSetting());

			person.PopAllEvents();

			target.PartTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("new name");

			person.PopAllEvents().OfType<PersonEmployementChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnEmploymentChangedEventWhenStartDateIsChanged()
		{
			DateOnly dateOnly = new DateOnly(2018, 03, 28);
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1));
			var target = new PersonPeriodModel(dateOnly, person, new List<IPersonSkill>(), new List<IExternalLogOn>(), new List<SiteTeamModel>(), new CommonNameDescriptionSetting());

			person.PopAllEvents();

			target.PeriodDate = new DateOnly(2018,04,01);

			person.PopAllEvents().OfType<PersonEmployementChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnEmploymentChangedEventWhenContractIsChangedOnChild()
		{
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1));
			var childTarget = EntityConverter.ConvertToOther<IPersonPeriod, PersonPeriodChildModel>(person.PersonPeriodCollection.First());

			person.PopAllEvents();

			childTarget.Contract = ContractFactory.CreateContract("new name");

			person.PopAllEvents().OfType<PersonEmployementChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnEmploymentChangedEventWhenContractScheduleIsChangedOnChild()
		{
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1));
			var childTarget = EntityConverter.ConvertToOther<IPersonPeriod, PersonPeriodChildModel>(person.PersonPeriodCollection.First());

			person.PopAllEvents();

			childTarget.ContractSchedule = ContractScheduleFactory.CreateContractSchedule("new name");

			person.PopAllEvents().OfType<PersonEmployementChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnEmploymentChangedEventWhenPartTimePercentageIsChangedOnChild()
		{
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1));

			var childTarget = EntityConverter.ConvertToOther<IPersonPeriod, PersonPeriodChildModel>(person.PersonPeriodCollection.First());

			person.PopAllEvents();

			childTarget.PartTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("new name");

			person.PopAllEvents().OfType<PersonEmployementChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnEmploymentChangedEventWhenStartDateIsChangedOnChild()
		{
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1));

			var childTarget = EntityConverter.ConvertToOther<IPersonPeriod, PersonPeriodChildModel>(person.PersonPeriodCollection.First());

			person.PopAllEvents();

			childTarget.PeriodDate = new DateOnly(2018,04,01);

			person.PopAllEvents().OfType<PersonEmployementChangedEvent>().Should().Not.Be.Empty();
		}
	}
}