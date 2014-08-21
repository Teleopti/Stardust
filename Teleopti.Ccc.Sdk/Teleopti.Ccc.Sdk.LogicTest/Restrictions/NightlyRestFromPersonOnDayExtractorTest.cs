using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.Restrictions
{
	[TestFixture]
	public class NightlyRestFromPersonOnDayExtractorTest
	{
		private MockRepository _mocks;
		private IPerson _person;
		private IPersonPeriod _period;
		private IPersonContract _personContract;
		private IContract _contract;
		private WorkTimeDirective _workTimeDirective;
		private INightlyRestFromPersonOnDayExtractor _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_person = _mocks.StrictMock<IPerson>();
			_period = _mocks.StrictMock<IPersonPeriod>();
			_personContract = _mocks.StrictMock<IPersonContract>();
			_contract = _mocks.StrictMock<IContract>();
			_workTimeDirective = new WorkTimeDirective(TimeSpan.FromDays(0), TimeSpan.FromDays(5), TimeSpan.FromHours(12), TimeSpan.FromDays(2));
			
			_target = new NightlyRestFromPersonOnDayExtractor(_person);
		}

		[Test]
		public void ShouldReturnNightlyRestFromWorkTimeDirective()
		{
			var dateOnly = new DateOnly(2011, 9, 1);
			var dateOnlyDto = new DateOnlyDto { DateTime = dateOnly };
			Expect.Call(_person.Period(dateOnly)).Return(_period);
			Expect.Call(_period.PersonContract).Return(_personContract);
			Expect.Call(_personContract.Contract).Return(_contract);
			Expect.Call(_contract.WorkTimeDirective).Return(_workTimeDirective);
			_mocks.ReplayAll();
			Assert.That(_target.NightlyRestOnDay(dateOnlyDto), Is.EqualTo(TimeSpan.FromHours(12)));
		}
	}
	

}