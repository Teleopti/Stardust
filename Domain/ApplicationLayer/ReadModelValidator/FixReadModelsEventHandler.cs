using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class FixReadModelsEventHandler : IHandleEvent<FixReadModelsEvent>,
		IRunOnStardust
	{
		private readonly IReadModelValidationResultPersister _persister;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IReadModelPersonScheduleDayValidator _readModelPersonScheduleDayValidator;
		private readonly IReadModelScheduleProjectionReadOnlyValidator _readModelScheduleProjectionReadOnlyValidator;
		private readonly IReadModelScheduleDayValidator _readModelScheduleDayValidator;
		private readonly IReadModelFixer _readModelFixer;

		public FixReadModelsEventHandler(IReadModelValidationResultPersister persister, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IReadModelPersonScheduleDayValidator readModelPersonScheduleDayValidator, IReadModelScheduleProjectionReadOnlyValidator readModelScheduleProjectionReadOnlyValidator, IReadModelScheduleDayValidator readModelScheduleDayValidator, IReadModelFixer readModelFixer)
		{
			_persister = persister;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_readModelPersonScheduleDayValidator = readModelPersonScheduleDayValidator;
			_readModelScheduleProjectionReadOnlyValidator = readModelScheduleProjectionReadOnlyValidator;
			_readModelScheduleDayValidator = readModelScheduleDayValidator;
			_readModelFixer = readModelFixer;
		}

		public virtual void Handle(FixReadModelsEvent @event)
		{
			if (@event.Targets.HasFlag(ValidateReadModelType.ScheduleProjectionReadOnly))
			{
				IList<ReadModelValidationResult> invalidRecords;
				using(_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					invalidRecords = _persister.LoadAllInvalidScheduleProjectionReadOnly().ToList();
				}

				foreach(var records in invalidRecords.Batch(50))
				{
					using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
					{
						foreach (var record in records)
						{
							var date = new DateOnly(record.Date);
							var readModels = _readModelScheduleProjectionReadOnlyValidator.Build(record.PersonId,date).ToList();

							_readModelFixer.FixScheduleProjectionReadOnly(new ReadModelData
							{
								Date = date,
								PersonId = record.PersonId,
								ScheduleProjectionReadOnly = readModels
							});
						}
						uow.PersistAll();
					}
				}
			}

			if (@event.Targets.HasFlag(ValidateReadModelType.PersonScheduleDay))
			{				
				IList<ReadModelValidationResult> invalidRecords;
				using(_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					invalidRecords = _persister.LoadAllInvalidPersonScheduleDay().ToList();
				}

				foreach(var records in invalidRecords.Batch(50))
				{
					using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
					{
						foreach (var record in records)
						{
							var date = new DateOnly(record.Date);
							var readModel = _readModelPersonScheduleDayValidator.Build(record.PersonId, date);
							
							_readModelFixer.FixPersonScheduleDay(new ReadModelData
							{
								Date = date,
								PersonId = record.PersonId,
								PersonScheduleDay = readModel
							});
						}
						uow.PersistAll();
					}
				}
			}

			if(@event.Targets.HasFlag(ValidateReadModelType.ScheduleDay))
			{				
				IList<ReadModelValidationResult> invalidRecords;
				using(_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					invalidRecords = _persister.LoadAllInvalidScheduleDay().ToList();
				}

				foreach(var records in invalidRecords.Batch(50))
				{
					using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
					{
						foreach (var record in records)
						{
							var date = new DateOnly(record.Date);
							var readModel = _readModelScheduleDayValidator.Build(record.PersonId, date);
							
							_readModelFixer.FixScheduleDay(new ReadModelData
							{
								Date = date,
								PersonId = record.PersonId,
								ScheduleDay = readModel
							});
						}
						uow.PersistAll();
					}
				}
			}
		}
	}
}
