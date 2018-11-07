using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[DomainTest]
	public class AddScheduleChangesListenerCommandHandlerTest : IExtendSystem
	{
		public FakeGlobalSettingDataRepository GlobalSettingDataRepository;
		public AddScheduleChangesListenerCommandHandler Target;

		[Test]
		public void ShouldAddNewListener()
		{
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
			Target.Handle(commandDto);

			commandDto.Result.AffectedItems.Should().Be.EqualTo(1);
			GlobalSettingDataRepository.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions())
				.Subscriptions()
				.First()
				.Name.Should()
				.Be.EqualTo("Facebook");
		}

		[Test]
		public void ShouldRejectNewListenerWithoutName()
		{
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
			Assert.Throws<FaultException>(() => Target.Handle(commandDto));
		}

		[Test]
		public void ShouldRejectNewListenerWithRelativeStartDateAfterEndDate()
		{
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
			Assert.Throws<FaultException>(() => Target.Handle(commandDto));
		}

		[Test]
		public void ShouldRejectNewListenerWithInvalidUri()
		{
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
			Assert.Throws<FaultException>(() => Target.Handle(commandDto));
		}

		[Test]
		public void ShouldRejectNewListenerWithInsufficientPermissions()
		{
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
				Assert.Throws<FaultException>(() => Target.Handle(commandDto));
			}
		}

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<AddScheduleChangesListenerCommandHandler>();
		}
	}
}
