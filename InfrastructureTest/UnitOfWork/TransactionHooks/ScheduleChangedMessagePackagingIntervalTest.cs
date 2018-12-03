using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.TransactionHooks
{
	[TestFixture]
	[PrincipalAndStateTest]
	[Setting("ScheduleChangedMessagePackagingSendOnIdleTimeSeconds", 2)]
	[Setting("ScheduleChangedMessagePackagingSendOnIntervalSeconds", 10)]
	public class ScheduleChangedMessagePackagingIntervalTest
	{
		public FakeMessageSender MessageSender;
		public FakeTime Time;
		public Database Database;

		[Test]
		public void ShouldSendWhenNotReceivedAnyMessageDuringIdleTime()
		{
			Database
				.WithPerson()
				.WithAssignment("2018-11-30");
			Time.Passes("3".Seconds());

			MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotSendWhileCommitingTransactions()
		{
			Database
				.WithPerson()
				.WithAssignment("2018-11-30");
			Time.Passes("1".Seconds());
			Database.WithAssignment("2018-12-01");
			Time.Passes("1".Seconds());

			MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Should().Have.Count.EqualTo(0);
		}
		
		[Test]
		public void ShouldAlwaysSendAfterSpecifiedTime()
		{
			Database
				.WithPerson();			
			10.Times(i =>
			{
				Database.WithAssignment("2018-12-" + (10 + i));
				Time.Passes("1".Seconds());
			}); 
			
			MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Should().Have.Count.EqualTo(1);
		}
		
		[Test]
		public void ShouldAlwaysSendAfterSpecifiedTime2()
		{
			Database
				.WithPerson();			
			30.Times(i =>
			{
				Database.WithAssignment($"2018-12-{i+1:D2}");
				Time.Passes("1".Seconds());
			}); 
			
			MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Should().Have.Count.EqualTo(3);
		}
	}
}