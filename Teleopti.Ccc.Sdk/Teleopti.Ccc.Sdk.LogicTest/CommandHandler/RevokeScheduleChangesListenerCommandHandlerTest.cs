using System.ServiceModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	public class RevokeScheduleChangesListenerCommandHandlerTest
	{
		[Test]
		public void ShouldRevokeListener()
		{
			var repository = new FakeGlobalSettingDataRepository();
			var handler = new RevokeScheduleChangesListenerCommandHandler(repository, new FakeCurrentUnitOfWorkFactory());
			var subscriptions = new ScheduleChangeSubscriptions();
			subscriptions.Add(new ScheduleChangeListener {Name = "Facebook"});
			repository.PersistSettingValue(ScheduleChangeSubscriptions.Key, subscriptions);

			var commandDto = new RevokeScheduleChangesListenerCommandDto
			{
				ListenerName = "Facebook"
			};
			handler.Handle(commandDto);

			commandDto.Result.AffectedItems.Should().Be.EqualTo(1);
			repository.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions())
				.Subscriptions()
				.Should()
				.Be.Empty();
		}

		[Test, ExpectedException(typeof(FaultException))]
		public void ShouldRejectRevokeListenerWhenNotPermitted()
		{
			var repository = new FakeGlobalSettingDataRepository();
			var handler = new RevokeScheduleChangesListenerCommandHandler(repository, new FakeCurrentUnitOfWorkFactory());
			var subscriptions = new ScheduleChangeSubscriptions();
			subscriptions.Add(new ScheduleChangeListener { Name = "Facebook" });
			repository.PersistSettingValue(ScheduleChangeSubscriptions.Key, subscriptions);

			var commandDto = new RevokeScheduleChangesListenerCommandDto
			{
				ListenerName = "Facebook"
			};
			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
			{
				handler.Handle(commandDto);
			}
		}
	}
}