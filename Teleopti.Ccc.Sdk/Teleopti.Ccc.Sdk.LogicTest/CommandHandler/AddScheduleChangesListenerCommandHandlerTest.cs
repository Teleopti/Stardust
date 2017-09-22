using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	public class AddScheduleChangesListenerCommandHandlerTest
	{
		[Test]
		public void ShouldAddNewListener()
		{
			var repository = new FakeGlobalSettingDataRepository();
			var handler = new AddScheduleChangesListenerCommandHandler(repository,new FakeCurrentUnitOfWorkFactory());

			var commandDto = new AddScheduleChangesListenerCommandDto
			{
				Listener =
					new ScheduleChangesListenerDto
					{
						Name = "Facebook",
						Url = "http://facebook/",
						DaysStartFromCurrentDate = -1,
						DaysEndFromCurrentDate = 1
					}
			};
			handler.Handle(commandDto);

			commandDto.Result.AffectedItems.Should().Be.EqualTo(1);
			repository.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions())
				.Subscriptions()
				.First()
				.Name.Should()
				.Be.EqualTo("Facebook");
		}

		[Test]
		public void ShouldRejectNewListenerWithoutName()
		{
			var repository = new FakeGlobalSettingDataRepository();
			var handler = new AddScheduleChangesListenerCommandHandler(repository, new FakeCurrentUnitOfWorkFactory());

			var commandDto = new AddScheduleChangesListenerCommandDto
			{
				Listener =
					new ScheduleChangesListenerDto
					{
						Name = "",
						Url = "http://facebook/",
						DaysStartFromCurrentDate = -1,
						DaysEndFromCurrentDate = 1
					}
			};
			Assert.Throws<FaultException>(() => handler.Handle(commandDto));
		}

		[Test]
		public void ShouldRejectNewListenerWithRelativeStartDateAfterEndDate()
		{
			var repository = new FakeGlobalSettingDataRepository();
			var handler = new AddScheduleChangesListenerCommandHandler(repository, new FakeCurrentUnitOfWorkFactory());

			var commandDto = new AddScheduleChangesListenerCommandDto
			{
				Listener =
					new ScheduleChangesListenerDto
					{
						Name = "Facebook",
						Url = "http://facebook/",
						DaysStartFromCurrentDate = 1,
						DaysEndFromCurrentDate = -1
					}
			};
			Assert.Throws<FaultException>(() => handler.Handle(commandDto));
		}

		[Test]
		public void ShouldRejectNewListenerWithInvalidUri()
		{
			var repository = new FakeGlobalSettingDataRepository();
			var handler = new AddScheduleChangesListenerCommandHandler(repository, new FakeCurrentUnitOfWorkFactory());

			var commandDto = new AddScheduleChangesListenerCommandDto
			{
				Listener =
					new ScheduleChangesListenerDto
					{
						Name = "Facebook",
						Url = "facebook",
						DaysStartFromCurrentDate = -1,
						DaysEndFromCurrentDate = 1
					}
			};
			Assert.Throws<FaultException>(() => handler.Handle(commandDto));
		}

		[Test]
		public void ShouldRejectNewListenerWithInsufficientPermissions()
		{
			var repository = new FakeGlobalSettingDataRepository();
			var handler = new AddScheduleChangesListenerCommandHandler(repository, new FakeCurrentUnitOfWorkFactory());

			var commandDto = new AddScheduleChangesListenerCommandDto
			{
				Listener =
					new ScheduleChangesListenerDto
					{
						Name = "Facebook",
						Url = "http://facebook/",
						DaysStartFromCurrentDate = -1,
						DaysEndFromCurrentDate = 1
					}
			};
			using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
			{
				Assert.Throws<FaultException>(() => handler.Handle(commandDto));
			}
		}
	}
}
