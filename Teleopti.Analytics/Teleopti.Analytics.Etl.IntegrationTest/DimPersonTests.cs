using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.IntegrationTest.TestData;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
    [TestFixture]
    public class DimPersonTests
    {
        [SetUp]
        public void Setup()
        {
            SetupFixtureForAssembly.BeginTest();
        }

        [TearDown]
        public void TearDown()
        {
            SetupFixtureForAssembly.EndTest();
        }

        private const string datasourceName = "Teleopti CCC Agg: Default log object";

        [Test]
        public void AddNewPersonInApp_AfterIntraday_CorrectPersonPeriodInAnalytics()
        {
            var analyticsDataFactory = new AnalyticsDataFactory();
            var dates = new CurrentWeekDates();

            analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
            analyticsDataFactory.Setup(dates);
            analyticsDataFactory.Persist();

            const string timeZoneId = "W. Europe Standard Time";
            var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));

            var raptorRepository = new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, "");
            var jobParameters = new JobParameters(
                dateList, 1, "UTC", 15, "", "False",
                CultureInfo.CurrentCulture,
                new FakeContainerHolder(), false
                )
            {
                Helper = new JobHelperForTest(raptorRepository, null),
                DataSource = SqlCommands.DataSourceIdGet(datasourceName)
            };

            // When adding a person in app db
            var personName = "Test person";
            var step = new StagePersonJobStep(jobParameters);
            var site = new SiteConfigurable {BusinessUnit = TestState.BusinessUnit.Name, Name = "Västerhaninge"};
            var team = new TeamConfigurable {Name = "Yellow", Site = "Västerhaninge"};
            var contract = new ContractConfigurable {Name = "Kontrakt"};
            var contractSchedule = new ContractScheduleConfigurable {Name = "Kontraktsschema"};
            var partTimePercentage = new PartTimePercentageConfigurable {Name = "ppp"};

            Data.Apply(site);
            Data.Apply(team);
            Data.Apply(contract);
            Data.Apply(contractSchedule);
            Data.Apply(partTimePercentage);

            var personPeriodConfiguable = new PersonPeriodConfigurable
            {
                BudgetGroup = "",
                Contract = contract.Name,
                ContractSchedule = contractSchedule.ContractSchedule.Description.Name,
                PartTimePercentage = partTimePercentage.Name,
                Team = team.Name,
                StartDate = DateTime.Today,
                ExternalLogon = ""
            };

            var personApp = TestState.TestDataFactory.Person(personName).Person;
            Data.Person(personName).Apply(personPeriodConfiguable);

            // When Run an Intraday job (or event depending on toggle?)
            StepRunner.RunIntraday(jobParameters);

            // Question which other tables should have been affected as well. Skills? Teams? Sites? bridge_group_page_person?
            var repo = new AnalyticsPersonRepository();
            var analyticsPersonPeriods =
                repo.GetPersonPeriods(personApp.Id.Value == null ? Guid.Empty : personApp.Id.Value);

            Assert.AreEqual(1, analyticsPersonPeriods.Count(),
                "Not correct number of person period in analytics database.");

            var personPeriod = analyticsPersonPeriods.First();

            Assert.AreEqual(personName, personPeriod.PersonName, "Person name did not match in analytics");
            Assert.True(personName.StartsWith(personPeriod.FirstName), "First name did not match in analytics");
            Assert.True(personName.EndsWith(personPeriod.LastName), "Last name did not match in analytics");
            Assert.AreEqual(contract.Name, personPeriod.ContractName, "Contract name did not match in analytics");
            Assert.AreEqual(team.Name, personPeriod.TeamName, "Team name did not match in analytics");
            Assert.AreEqual(site.Name, personPeriod.SiteName, "Site name did not match in analytics");
            Assert.AreEqual(TestState.BusinessUnit.Name, personPeriod.BusinessUnitName,
                "BU name did not match in analytics");
            Assert.AreEqual(personApp.Email, personPeriod.Email, "Email did not match in analytics");
            Assert.AreEqual(personApp.Note, personPeriod.Note.Trim(), "Note did not match in analytics");

            Percent personPeriodPercentage;
            Percent.TryParse(personPeriod.ParttimePercentage, out personPeriodPercentage);
            Assert.AreEqual(partTimePercentage.PartTimePercentage.Percentage, personPeriodPercentage,
                "Parttime percentage did not match in analytics");

            // More data to check?
        }
    }
}