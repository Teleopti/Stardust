using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest, Toggle(Toggles.Wfm_Requests_ApprovingModifyRequests_41930)]
	[TestFixture]
	public class FilterRequestsWithDifferentVersionTest 
	{
		public IFilterRequestsWithDifferentVersion Target;
		public FakePersonRequestRepository PersonRequestRepository;

		[Test]
		public void ShouldNotFilterIfVersionIsSame()
		{
			var personReq1 = Guid.NewGuid();
			var personReq2 = Guid.NewGuid();
			var reqVersions = new Dictionary<Guid,int>();
			
			reqVersions.Add(personReq1,1);
			reqVersions.Add(personReq2,1);

			var potentialBulk = new List<IEnumerable<Guid>>();
			potentialBulk.Add(new List<Guid>() {personReq1});
			potentialBulk.Add(new List<Guid>() {personReq2});

			var filterRequests = Target.Filter(reqVersions, potentialBulk);
			CollectionAssert.AreEqual(potentialBulk, filterRequests);
		}

		[Test]
		public void ShouldFilterRequestIfVersionIsChanged()
		{
			IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");

			IRequest req = new AbsenceRequest(absence, new DateTimePeriod());
			var personReq1 = new PersonRequest(PersonFactory.CreatePerson(), req);
			personReq1.SetId(Guid.NewGuid());
			personReq1.SetVersion(1);
			PersonRequestRepository.Add(personReq1);

			var personReq2 = new PersonRequest(PersonFactory.CreatePerson(), req);
			personReq2.SetId(Guid.NewGuid());
			personReq2.SetVersion(2);
			PersonRequestRepository.Add(personReq2);

			var personReq3 = new PersonRequest(PersonFactory.CreatePerson(), req);
			personReq3.SetId(Guid.NewGuid());
			personReq3.SetVersion(2);
			PersonRequestRepository.Add(personReq3);

			var personReq4 = new PersonRequest(PersonFactory.CreatePerson(), req);
			personReq4.SetId(Guid.NewGuid());
			personReq4.SetVersion(1);
			PersonRequestRepository.Add(personReq4);

			var reqVersions = new Dictionary<Guid, int>();
			reqVersions.Add(personReq1.Id.GetValueOrDefault(), 1);
			reqVersions.Add(personReq2.Id.GetValueOrDefault(), 1);
			reqVersions.Add(personReq3.Id.GetValueOrDefault(), 1);
			reqVersions.Add(personReq4.Id.GetValueOrDefault(), 1);

			var potentialBulk = new List<IEnumerable<Guid>>();
			potentialBulk.Add(new List<Guid>() { personReq1.Id.GetValueOrDefault(), personReq2.Id.GetValueOrDefault() });
			potentialBulk.Add(new List<Guid>() { personReq3.Id.GetValueOrDefault(), personReq4.Id.GetValueOrDefault() });

			var filterRequests = Target.Filter(reqVersions, potentialBulk);

			var expectedResult = new List<IEnumerable<Guid>>();
			expectedResult.Add(new List<Guid>() { personReq1.Id.GetValueOrDefault()});
			expectedResult.Add(new List<Guid>() {  personReq4.Id.GetValueOrDefault() });

			CollectionAssert.AreEqual(expectedResult, filterRequests);
		}


		[Test]
		public void ShouldFilterRequestListIfVersionIsChanged()
		{
			IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");

			IRequest req = new AbsenceRequest(absence, new DateTimePeriod());
			var personReq1 = new PersonRequest(PersonFactory.CreatePerson(), req);
			personReq1.SetId(Guid.NewGuid());
			personReq1.SetVersion(1);
			PersonRequestRepository.Add(personReq1);

			var personReq2 = new PersonRequest(PersonFactory.CreatePerson(), req);
			personReq2.SetId(Guid.NewGuid());
			personReq2.SetVersion(2);
			PersonRequestRepository.Add(personReq2);

			var personReq3 = new PersonRequest(PersonFactory.CreatePerson(), req);
			personReq3.SetId(Guid.NewGuid());
			personReq3.SetVersion(2);
			PersonRequestRepository.Add(personReq3);

			var reqVersions = new Dictionary<Guid, int>();
			reqVersions.Add(personReq1.Id.GetValueOrDefault(), 1);
			reqVersions.Add(personReq2.Id.GetValueOrDefault(), 1);
			reqVersions.Add(personReq3.Id.GetValueOrDefault(), 1);

			var potentialBulk = new List<IEnumerable<Guid>>();
			potentialBulk.Add(new List<Guid>() { personReq1.Id.GetValueOrDefault(), personReq2.Id.GetValueOrDefault() });
			potentialBulk.Add(new List<Guid>() { personReq3.Id.GetValueOrDefault()});

			var filterRequests = Target.Filter(reqVersions, potentialBulk);

			var expectedResult = new List<IEnumerable<Guid>>();
			expectedResult.Add(new List<Guid>() { personReq1.Id.GetValueOrDefault() });

			CollectionAssert.AreEqual(expectedResult, filterRequests);
		}
	}
}