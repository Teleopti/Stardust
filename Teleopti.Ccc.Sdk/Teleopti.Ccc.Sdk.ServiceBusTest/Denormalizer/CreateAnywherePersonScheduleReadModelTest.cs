using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class CreateAnywherePersonScheduleReadModelTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldConvertMessageToReadModel() {
			var fromDenormalizedScheduleToReadModel = MockRepository.GenerateMock<IAnywherePersonScheduleFromDenormalizedSchedule>();
			var target = new CreateAnywherePersonScheduleReadModel(fromDenormalizedScheduleToReadModel, MockRepository.GenerateMock<IAnywherePersonScheduleReadModelRepository>());
			var message = new DenormalizedSchedule();

			target.Consume(message);

			fromDenormalizedScheduleToReadModel.AssertWasCalled(x => x.Convert(message));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldSetPersonIdToReadModel()
		{
			var personId = Guid.NewGuid();
			var target = new AnywherePersonScheduleFromDenormalizedSchedule();
			var result = target.Convert(new DenormalizedSchedule {PersonId = personId});

			result.PersonId.Should().Be.EqualTo(personId);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldSetProjectedLayers() { 
			var layer = new DenormalizedScheduleProjectionLayer {StartDateTime = new DateTime(2012, 12, 12, 8, 0, 0, DateTimeKind.Utc), EndDateTime = new DateTime(2012, 12, 12, 15, 0, 0, DateTimeKind.Utc), DisplayColor = -3, Name = "Lunch"};
			var target = new AnywherePersonScheduleFromDenormalizedSchedule();
			var message = new DenormalizedSchedule
			              	{
			              		ScheduleDays = new []
			              		               	{
			              		               		new DenormalizedScheduleDay
			              		               			{
			              		               				Layers = new Collection<DenormalizedScheduleProjectionLayer>{layer}
			              		               			}
			              		               	}
			              	};
			
			var result = target.Convert(message);
			result.ProjectedLayers.Should().Not.Be.NullOrEmpty();
		}
	}

	public class AnywherePersonScheduleFromDenormalizedSchedule : IAnywherePersonScheduleFromDenormalizedSchedule
	{
		public AnywherePersonScheduleFromDenormalizedSchedule()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(DenormalizedSchedule message)
		{
			if (!message.IsDefaultScenario) return;
			var readModel = _fromDenormalizedScheduleToReadModel.Convert(message);
			_anywherePersonScheduleReadModelRepository.SaveReadModel(readModel);
		}
	}
}
