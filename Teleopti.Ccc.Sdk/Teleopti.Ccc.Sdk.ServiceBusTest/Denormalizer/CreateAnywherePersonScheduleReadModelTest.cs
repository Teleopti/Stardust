using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
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

		[Test]
		public void ShouldIgnoreScheduleChangesNotInDefaultScenario()
		{
			var fromDenormalizedScheduleToReadModel = MockRepository.GenerateMock<IAnywherePersonScheduleFromDenormalizedSchedule>();
			var target = new CreateAnywherePersonScheduleReadModel(fromDenormalizedScheduleToReadModel, MockRepository.GenerateMock<IAnywherePersonScheduleReadModelRepository>());
			var message = new DenormalizedSchedule {IsDefaultScenario = false};

			target.Consume(message);

			fromDenormalizedScheduleToReadModel.AssertWasNotCalled(x => x.Convert(message));
		}
	}

	[TestFixture]
	public class AnywherePersonScheduleFromDenormalizedScheduleTest
	{
		[Test]
		public void ShouldSetPersonIdToReadModel()
		{
			var personId = Guid.NewGuid();
			var target = new AnywherePersonScheduleFromDenormalizedSchedule();
			var result = target.Convert(new DenormalizedSchedule {PersonId = personId});

			result.PersonId.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldSetProjectedLayers() { 
			var layer = new DenormalizedScheduleProjectionLayer {StartDateTime = new DateTime(2012, 12, 12, 8, 0, 0, DateTimeKind.Utc), EndDateTime = new DateTime(2012, 12, 12, 15, 0, 0, DateTimeKind.Utc), DisplayColor = -3, Name = "Lunch"};
			var target = new AnywherePersonScheduleFromDenormalizedSchedule();
			var message = new DenormalizedSchedule();
			message.Layers = new Collection<DenormalizedScheduleProjectionLayer>{layer};

			var result = target.Convert(message);
			result.ProjectedLayers.Should().Not.Be.NullOrEmpty();
		}
	}

	public class AnywherePersonScheduleFromDenormalizedSchedule : IAnywherePersonScheduleFromDenormalizedSchedule
	{
		public AnywherePersonScheduleFromDenormalizedSchedule()
		{
		}

		public AnywherePersonScheduleReadModel Convert(DenormalizedSchedule message)
		{
			var model = new AnywherePersonScheduleReadModel
				{
					PersonId = message.PersonId,
					ProjectedLayers = " "
				};

			return model;
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
		public Guid PersonId { get; set; }
		public string ProjectedLayers { get; set; }
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
			if (!message.IsDefaultScenario) return;
			var readModel = _fromDenormalizedScheduleToReadModel.Convert(message);
			_anywherePersonScheduleReadModelRepository.SaveReadModel(readModel);
		}
	}
}
