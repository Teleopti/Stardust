using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class CreateAnywherePersonScheduleReadModelTest
	{
		[Test]
		public void ShouldConvertMessageToReadModel() {
			var fromDenormalizedScheduleToReadModel = MockRepository.GenerateMock<IAnywherePersonScheduleFromDenormalizedSchedule>();
			var target = new CreateAnywherePersonScheduleReadModel(fromDenormalizedScheduleToReadModel, MockRepository.GenerateMock<IAnywherePersonScheduleReadModelRepository>());
			var message = new DenormalizedSchedule();

			target.Consume(message);

			fromDenormalizedScheduleToReadModel.AssertWasCalled(x => x.Convert(message));
		}

		[Test]
		public void ShouldSaveReadModel()
		{
			var fromDenormalizedScheduleToReadModel = MockRepository.GenerateMock<IAnywherePersonScheduleFromDenormalizedSchedule>();
			var anywherePersonScheduleReadModelRepository = MockRepository.GenerateMock<IAnywherePersonScheduleReadModelRepository>();
			var target = new CreateAnywherePersonScheduleReadModel(fromDenormalizedScheduleToReadModel,anywherePersonScheduleReadModelRepository);
			var message = new DenormalizedSchedule();
			var anywherePersonScheduleReadModel = new AnywherePersonScheduleReadModel();
			fromDenormalizedScheduleToReadModel.Stub(x => x.Convert(message)).Return(anywherePersonScheduleReadModel);

			target.Consume(message);

			anywherePersonScheduleReadModelRepository.AssertWasCalled(x => x.SaveReadModel(anywherePersonScheduleReadModel));
		}
	}

	public interface IAnywherePersonScheduleReadModelRepository
	{
		void SaveReadModel(AnywherePersonScheduleReadModel anywherePersonScheduleReadModel);
	}

	public interface IAnywherePersonScheduleFromDenormalizedSchedule
	{
		AnywherePersonScheduleReadModel Convert(DenormalizedSchedule message);
	}

	public class AnywherePersonScheduleReadModel
	{
	}

	public class CreateAnywherePersonScheduleReadModel : ConsumerOf<DenormalizedSchedule>
	{
		private readonly IAnywherePersonScheduleFromDenormalizedSchedule _fromDenormalizedScheduleToReadModel;
		private readonly IAnywherePersonScheduleReadModelRepository _anywherePersonScheduleReadModelRepository;

		public CreateAnywherePersonScheduleReadModel(IAnywherePersonScheduleFromDenormalizedSchedule fromDenormalizedScheduleToReadModel, IAnywherePersonScheduleReadModelRepository anywherePersonScheduleReadModelRepository)
		{
			_fromDenormalizedScheduleToReadModel = fromDenormalizedScheduleToReadModel;
			_anywherePersonScheduleReadModelRepository = anywherePersonScheduleReadModelRepository;
		}

		public void Consume(DenormalizedSchedule message)
		{
			var readModel = _fromDenormalizedScheduleToReadModel.Convert(message);
			_anywherePersonScheduleReadModelRepository.SaveReadModel(readModel);
		}
	}
}
