using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class PersonScheduleDayReadModelHandler : ConsumerOf<DenormalizedSchedule>, ConsumerOf<DenormalizedScheduleForPersonScheduleDay>
	{
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPersonScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IPersonScheduleDayReadModelRepository _scheduleDayReadModelRepository;

		public PersonScheduleDayReadModelHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory,
									IPersonScheduleDayReadModelsCreator scheduleDayReadModelsCreator,
									IPersonScheduleDayReadModelRepository scheduleDayReadModelRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(DenormalizedSchedule message)
		{
			createReadModel(message);
		}

		private void createReadModel(DenormalizedScheduleBase message)
		{
			if (!message.IsDefaultScenario) return;
			if (message.ScheduleDays == null || message.ScheduleDays.Count == 0) return;

			using (var uow = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(message.ScheduleDays.Min(s => s.Date.Date)),
				                                        new DateOnly(message.ScheduleDays.Max(s => s.Date.Date)));

				if (!message.IsInitialLoad)
				{
					_scheduleDayReadModelRepository.ClearPeriodForPerson(dateOnlyPeriod, message.PersonId);
				}

				var readModels = _scheduleDayReadModelsCreator.GetReadModels(message);
				foreach (var readModel in readModels)
				{
					_scheduleDayReadModelRepository.SaveReadModel(readModel);
				}
				uow.PersistAll();
			}
		}

		public void Consume(DenormalizedScheduleForPersonScheduleDay message)
		{
			createReadModel(message);
		}
	}
}