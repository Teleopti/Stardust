using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("LongRunning")]
	public class StatisticRepositoryEmptyTest
	{
		private IStatisticRepository target;
		private ISkill skill;
		
		[SetUp]
		public void Setup()
		{
			MockRepository mocks = new MockRepository();
			IState stateMock = mocks.StrictMock<IState>();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			skill = mocks.StrictMock<ISkill>();

		    var applicationData = new ApplicationData(new Dictionary<string, string>(), dataSource, null);
			StateHolderProxyHelper.ClearAndSetStateHolder(mocks, ((IUnsafePerson)TeleoptiPrincipal.Current).Person, BusinessUnitFactory.BusinessUnitUsedInTest, applicationData, stateMock);
			dataSource.Stub(x => x.Statistic).Return(null).Repeat.Any();
			mocks.ReplayAll();
			target = StatisticRepositoryFactory.Create();
		}

		[Test]
		public void HasNoPublicConstructor()
		{
			Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof(StatisticRepositoryEmpty)));
		}

		[Test]
		public void VerifyCorrectRepository()
		{
			Assert.IsNotNull(target);
			Assert.IsInstanceOf<StatisticRepositoryEmpty>(target);
		}

		[Test]
		public void VerifyEmptyLists()
		{
			Assert.AreEqual(0, target.LoadSpecificDates(new List<IQueueSource>(),new DateTimePeriod()).Count);
			Assert.AreEqual(0, target.LoadQueues().Count);
			Assert.AreEqual(0, target.LoadActiveAgentCount(skill,new DateTimePeriod()).Count);
			
			//Does nothing!
			using(var dt = new DataTable())
			{
				target.PersistFactQueues(dt);                
			}
			target.DeleteStgQueues();
			target.LoadFactQueues();
			target.LoadDimQueues();
		}

		[Test]
		public void VerifyNullLists()
		{
			Assert.IsNull(target.LoadReports());
		}

		[Test]
		public void VerifyRtaMethods()
		{
			target.LoadAdherenceData(new DateTime(), "", Guid.Empty, Guid.Empty, 1, 1).Count.Should().Be.EqualTo(0);
			target.LoadAgentStat(Guid.Empty, new DateTime(), new DateTime(), "", Guid.Empty).Count.Should().Be.EqualTo(0);
			target.LoadAgentQueueStat(new DateTime(), new DateTime(), "", Guid.Empty).Count.Should().Be.EqualTo(0);
			target.LoadActualAgentState(new List<IPerson>()).Count.Should().Be.EqualTo(0);
			target.LoadOneActualAgentState(Guid.Empty);
			target.AddOrUpdateActualAgentState(new ActualAgentState());
		}
	}
}
