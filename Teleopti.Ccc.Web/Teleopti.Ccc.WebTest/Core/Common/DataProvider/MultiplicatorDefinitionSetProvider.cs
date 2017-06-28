using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	[TestFixture]
	public class MultiplicatorDefinitionSetProviderTest
	{
		private FakeMultiplicatorDefinitionSetRepository repository;
		private MultiplicatorDefinitionSetProvider provider;

		private MultiplicatorDefinitionSet overtimeSet1;
		private MultiplicatorDefinitionSet overtimeSet2;
		private MultiplicatorDefinitionSet obTimeSet;
		private IContract contract;
		private IPerson person;

		[SetUp]
		public void SetUp()
		{
			repository = new FakeMultiplicatorDefinitionSetRepository();

			overtimeSet1 = new MultiplicatorDefinitionSet("Overtime1", MultiplicatorType.Overtime);
			overtimeSet1.SetId(Guid.NewGuid());
			repository.Add(overtimeSet1);

			overtimeSet2 = new MultiplicatorDefinitionSet("Overtime2", MultiplicatorType.Overtime);
			overtimeSet2.SetId(Guid.NewGuid());
			repository.Add(overtimeSet2);

			obTimeSet = new MultiplicatorDefinitionSet("OBTime", MultiplicatorType.OBTime);
			obTimeSet.SetId(Guid.NewGuid());
			repository.Add(obTimeSet);

			provider = new MultiplicatorDefinitionSetProvider(repository);

			contract = ContractFactory.CreateContract("Test Contract");
			contract.AddMultiplicatorDefinitionSetCollection(overtimeSet1);
			contract.AddMultiplicatorDefinitionSetCollection(obTimeSet);

			var today = new DateOnly(DateTime.Now);
			person = PersonFactory.CreatePersonWithPersonPeriod(today.AddDays(-1));
			person.Period(today).PersonContract = PersonContractFactory.CreatePersonContract(contract);
		}

		[Test]
		public void ShouldLoadOvertimeMultiplicatorDefinitionSets()
		{
			var result = provider.GetAllOvertimeDefinitionSets();

			Assert.AreEqual(result.Count, 2);

			Assert.AreEqual(overtimeSet1.Id.GetValueOrDefault(), result.First().Id);
			Assert.AreEqual(overtimeSet1.Name, result.First().Name);

			Assert.AreEqual(overtimeSet2.Id.GetValueOrDefault(), result.Second().Id);
			Assert.AreEqual(overtimeSet2.Name, result.Second().Name);
		}

		[Test]
		public void ShouldLoadMultiplicatorDefinitionSetsForPerson()
		{
			var result = provider.GetDefinitionSets(person, new DateOnly(DateTime.Now));

			Assert.AreEqual(result.Count, 2);

			Assert.AreEqual(overtimeSet1.Id.GetValueOrDefault(), result.First().Id);
			Assert.AreEqual(overtimeSet1.Name, result.First().Name);

			Assert.AreEqual(obTimeSet.Id.GetValueOrDefault(), result.Second().Id);
			Assert.AreEqual(obTimeSet.Name, result.Second().Name);
		}
	}
}