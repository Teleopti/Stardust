using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	[TestFixture, DomainTest]
	public class ScheduleValidationProviderTest:ISetup
	{
		public FakeCurrentScenario CurrentScenario;
		public FakeScheduleStorage ScheduleStorage;
		public FakePersonRepository PersonRepository;
		public ScheduleValidationProvider Target;

		public void Setup(ISystem system,IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<ScheduleValidationProvider>().For<IScheduleValidationProvider>();
		}

		[Test]
		[Ignore("Ignore until finished.")]
		public void ShouldGetResultForNightlyRestRuleCheckBetweenYesterdayAndToday()
		{
			var scenario = CurrentScenario.Current();
			var team = TeamFactory.CreateSimpleTeam();

			var contract = PersonContractFactory.CreatePersonContract();
			contract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0),TimeSpan.FromHours(40),TimeSpan.FromHours(8),TimeSpan.FromHours(40));

			var person = PersonFactory.CreatePersonWithGuid("Peter","peter");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(2015,12,30), contract, team));
			PersonRepository.Has(person);

			var dateTimePeriodToday = new DateTimePeriod(2016,1,2,1,2016,1,2,23);
			var dateTimePeriodYesterday = new DateTimePeriod(2016,1,1,1,2016,1,1,23);
			var personAssignmentToday = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario,person,dateTimePeriodToday);
			var personAssignmentYesterday = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario,person,dateTimePeriodYesterday);
			ScheduleStorage.Add(personAssignmentToday);
			ScheduleStorage.Add(personAssignmentYesterday);
			var results = Target.GetBusineeRuleValidationResults(new FetchRuleValidationResultFormData
			{
				Date = new DateTime(2016,1,2),
				PersonIds = new List<Guid>
				{
					person.Id.GetValueOrDefault()
				}
			});
			results.First().PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			results.First().Warnings.Contains(string.Format(Resources.BusinessRuleNightlyRestRuleErrorMessage,8,"2016-01-01","2016-01-02",2)).Should().Be.EqualTo(true);
		}
	}
}
