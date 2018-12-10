using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
	[TestFixture]
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

			person.PopAllEvents().OfType<PersonEmploymentChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnEmploymentChangedEventWhenContractScheduleIsChanged()
		{
			DateOnly dateOnly = new DateOnly(2018, 03, 28);
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1));
			var target = new PersonPeriodModel(dateOnly, person, new List<IPersonSkill>(), new List<IExternalLogOn>(), new List<SiteTeamModel>(), new CommonNameDescriptionSetting());

			person.PopAllEvents();

			target.ContractSchedule = ContractScheduleFactory.CreateContractSchedule("new name");

			person.PopAllEvents().OfType<PersonEmploymentChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnEmploymentChangedEventWhenPartTimePercentageIsChanged()
		{
			DateOnly dateOnly = new DateOnly(2018, 03, 28);
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1));
			var target = new PersonPeriodModel(dateOnly, person, new List<IPersonSkill>(), new List<IExternalLogOn>(), new List<SiteTeamModel>(), new CommonNameDescriptionSetting());

			person.PopAllEvents();

			target.PartTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("new name");

			person.PopAllEvents().OfType<PersonEmploymentChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnEmploymentChangedEventWhenStartDateIsChanged()
		{
			DateOnly dateOnly = new DateOnly(2018, 03, 28);
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1));
			var target = new PersonPeriodModel(dateOnly, person, new List<IPersonSkill>(), new List<IExternalLogOn>(), new List<SiteTeamModel>(), new CommonNameDescriptionSetting());

			person.PopAllEvents();

			target.PeriodDate = new DateOnly(2018,04,01);

			person.PopAllEvents().OfType<PersonEmploymentChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnEventDetailsWhenContractIsChanged()
		{
			DateOnly dateOnly = new DateOnly(2018, 03, 28);
			var fromDate = new DateOnly(2018, 1, 1);
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(fromDate).WithId();
			var target = new PersonPeriodModel(dateOnly, person, new List<IPersonSkill>(), new List<IExternalLogOn>(), new List<SiteTeamModel>(), new CommonNameDescriptionSetting());

			person.PopAllEvents();

			target.Contract = ContractFactory.CreateContract("new name");
			var @event = person.PopAllEvents().OfType<PersonEmploymentChangedEvent>().First();
			@event.PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			@event.FromDate.Should().Be.EqualTo(fromDate);
		}

		[Test]
		public void ShouldReturnPreviousStartDateIfCurrentPeriodStartDateIsChanged()
		{
			DateOnly dateOnly = new DateOnly(2018, 03, 28);
			var previousStartDate = new DateOnly(2017, 12, 15);
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1)).WithId();
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2017,12,15)));

			var target = new PersonPeriodModel(dateOnly, person, new List<IPersonSkill>(), new List<IExternalLogOn>(), new List<SiteTeamModel>(), new CommonNameDescriptionSetting());

			person.PopAllEvents();

			target.PeriodDate = new DateOnly(2018, 04, 01);
			var @event = person.PopAllEvents().OfType<PersonEmploymentChangedEvent>().First();
			@event.PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			@event.FromDate.Should().Be.EqualTo(previousStartDate);
		}

		[Test]
		public void ShouldReturnPreviousStartDateIfCurrentPeriodStartDateIsChangedChild()
		{
			//DateOnly dateOnly = new DateOnly(2018, 03, 28);
			var previousStartDate = new DateOnly(2017, 12, 15);
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1)).WithId();
			var latestPeriod = person.PersonPeriodCollection.First();
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2017, 12, 15)));

			var childTarget = EntityConverter.ConvertToOther<IPersonPeriod, PersonPeriodChildModel>(latestPeriod);

			person.PopAllEvents();

			childTarget.PeriodDate = new DateOnly(2018, 04, 01);
			var @event = person.PopAllEvents().OfType<PersonEmploymentChangedEvent>().First();
			@event.PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			@event.FromDate.Should().Be.EqualTo(previousStartDate);
		}

		[Test]
		public void ShouldReturnEmploymentChangedEventWhenContractIsChangedOnChild()
		{
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1));
			var childTarget = EntityConverter.ConvertToOther<IPersonPeriod, PersonPeriodChildModel>(person.PersonPeriodCollection.First());

			person.PopAllEvents();

			childTarget.Contract = ContractFactory.CreateContract("new name");

			person.PopAllEvents().OfType<PersonEmploymentChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnEmploymentChangedEventWhenContractScheduleIsChangedOnChild()
		{
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1));
			var childTarget = EntityConverter.ConvertToOther<IPersonPeriod, PersonPeriodChildModel>(person.PersonPeriodCollection.First());

			person.PopAllEvents();

			childTarget.ContractSchedule = ContractScheduleFactory.CreateContractSchedule("new name");

			person.PopAllEvents().OfType<PersonEmploymentChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnEmploymentChangedEventWhenPartTimePercentageIsChangedOnChild()
		{
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1));

			var childTarget = EntityConverter.ConvertToOther<IPersonPeriod, PersonPeriodChildModel>(person.PersonPeriodCollection.First());

			person.PopAllEvents();

			childTarget.PartTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("new name");

			person.PopAllEvents().OfType<PersonEmploymentChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnEmploymentChangedEventWhenStartDateIsChangedOnChild()
		{
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2018, 1, 1));

			var childTarget = EntityConverter.ConvertToOther<IPersonPeriod, PersonPeriodChildModel>(person.PersonPeriodCollection.First());

			person.PopAllEvents();

			childTarget.PeriodDate = new DateOnly(2018,04,01);

			person.PopAllEvents().OfType<PersonEmploymentChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnEventDetailsWhenContractIsChangedOnChild()
		{
			var fromDate = new DateOnly(2018, 1, 1);
			Person person = (Person)PersonFactory.CreatePersonWithPersonPeriod(fromDate);
			var childTarget = EntityConverter.ConvertToOther<IPersonPeriod, PersonPeriodChildModel>(person.PersonPeriodCollection.First());

			person.PopAllEvents();

			childTarget.Contract = ContractFactory.CreateContract("new name");

			var @event = person.PopAllEvents().OfType<PersonEmploymentChangedEvent>().First();
			@event.PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			
			@event.FromDate.Should().Be.EqualTo(fromDate);
		}
	}
}