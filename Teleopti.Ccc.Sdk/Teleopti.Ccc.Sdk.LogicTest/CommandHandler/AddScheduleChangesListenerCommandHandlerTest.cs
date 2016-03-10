using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
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

		[Test, ExpectedException(typeof(FaultException))]
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
			handler.Handle(commandDto);
		}

		[Test, ExpectedException(typeof(FaultException))]
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
			handler.Handle(commandDto);
		}

		[Test, ExpectedException(typeof(FaultException))]
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
			handler.Handle(commandDto);
		}

		[Test, ExpectedException(typeof(FaultException))]
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
			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
			{
				handler.Handle(commandDto);
			}
		}
	}
}
