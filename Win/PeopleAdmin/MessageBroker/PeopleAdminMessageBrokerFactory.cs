using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.Win.PeopleAdmin.MessageBroker.MessageBrokerHandlers;
using Teleopti.Ccc.Win.PeopleAdmin.Views;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Win.PeopleAdmin.MessageBroker
{
	public static class PeopleAdminMessageBrokerFactory
	{
		public static IMessageBrokerHandler GetMessageBrokerHandler(ViewType view, EventMessageArgs e,
			FilteredPeopleHolder stateHolder)
		{
			switch (view)
			{
				case ViewType.PeoplePeriodView:
					{
						return GetMessageBrokerHandlerForPeoplePeriod(e, stateHolder);
					}
				case ViewType.GeneralView:
					{
						return new OptionalColumnMessageBrokerHandler(e, stateHolder);
					}
				case ViewType.PersonRotationView:
					{
						return GetMessageBrokerHandlerForRotationGrids(e, stateHolder);
					}
				case ViewType.PersonAvailabilityView:
					{
						return GetMessageBrokerHandlerForRotationGrids(e, stateHolder);
					}
				case ViewType.PersonalAccountGridView:
					{
						return new AbsenceMessageBrokerHandler(e, stateHolder);
					}
				default:
					{
						return null;
					}
			}
		}

		private static IMessageBrokerHandler GetMessageBrokerHandlerForPeoplePeriod(EventMessageArgs e,
			FilteredPeopleHolder stateHolder)
		{
			if (e.Message.InterfaceType.IsAssignableFrom(typeof(IContract)))
			{
				return new ContractMessageBrokerHandler(e, stateHolder);
			}

			if (e.Message.InterfaceType.IsAssignableFrom(typeof(ITeam)))
			{
				return new TeamMessageBrokerHandler(e, stateHolder);
			}

			if (e.Message.InterfaceType.IsAssignableFrom(typeof(IContractSchedule)))
			{
				return new ContractScheduleMessageBrokerHandler(e, stateHolder);
			}

			if (e.Message.InterfaceType.IsAssignableFrom(typeof(IPartTimePercentage)))
			{
				return new PartTimePercentageMessageBrokerHandler(e, stateHolder);
			}

			if (e.Message.InterfaceType.IsAssignableFrom(typeof(IRuleSetBag)))
			{
				return new RuleSetBagMessageBrokerHandler(e, stateHolder);
			}

			if (e.Message.InterfaceType.IsAssignableFrom(typeof(IBudgetGroup)))
			{
				return new BudgetGroupMessageBrokerHandler(e, stateHolder);
			}

			return null;
		}

		private static IMessageBrokerHandler GetMessageBrokerHandlerForRotationGrids(EventMessageArgs e, FilteredPeopleHolder stateHolder)
		{
			if (e.Message.InterfaceType.IsAssignableFrom(typeof(IRotation)))
			{
				return new PersonRotationMessageBrokerHandler(e, stateHolder);
			}

			if (e.Message.InterfaceType.IsAssignableFrom(typeof(IAvailabilityRotation)))
			{
				return new PersonAvailabilityMessageBrokerHandler(e, stateHolder);
			}

			return null;
		}
	}
}