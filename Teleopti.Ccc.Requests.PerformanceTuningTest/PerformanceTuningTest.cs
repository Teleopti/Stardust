using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Requests.PerformanceTuningTest
{
	[TestFixture]
	public class PerformanceTuningTest
    {
        public IAbsenceRepository AbsenceRepository;
        public AsSystem AsSystem;
        public IDataSourceScope DataSource;
        public IPersonRepository PersonRepository;
        public IPersonRequestRepository PersonRequestRepository;
        public MultiAbsenceRequestsHandler Target;
        public WithUnitOfWork WithUnitOfWork;
        public IWorkflowControlSetRepository WorkflowControlSetRepository;
        public IBudgetGroupRepository BudgetGroupRepository;
        public IBudgetDayRepository BudgetDayRepository;
        public IScenarioRepository ScenarioRepository;
        public IBusinessUnitRepository BusinessUnitRepository;
        public IQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
        public ICurrentUnitOfWork CurrentUnitOfWork;

        [Test]
	    public void ThisIsADummyTest()
	    {
		    Assert.Pass();
	    }

        [Test]
        public void ShouldHandle200AbsenceRequestsFast()
        {
            using (DataSource.OnThisThreadUse("Teleopti WFM"))
                AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

            WithUnitOfWork.Do(() =>
            {
                var personRequests =
                    PersonRequestRepository.FindPersonRequestWithinPeriod(
                        new DateTimePeriod(new DateTime(2016, 2, 29, 23, 0, 0, DateTimeKind.Utc),
                            new DateTime(2016, 3, 2, 23, 0, 0, DateTimeKind.Utc))).Where(x => x.Request is AbsenceRequest);

                var absenceRequestIds = personRequests.Select(x => x.Id.GetValueOrDefault()).ToList();

                var newMultiAbsenceRequestsCreatedEvent = new NewMultiAbsenceRequestsCreatedEvent()
                {
                    PersonRequestIds = absenceRequestIds,
                    InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
                    LogOnBusinessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"),
                    LogOnDatasource = "Teleopti WFM",
                    Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
                    Sent = DateTime.UtcNow
                };

                Target.Handle(newMultiAbsenceRequestsCreatedEvent);

            });

        }

    }
}
