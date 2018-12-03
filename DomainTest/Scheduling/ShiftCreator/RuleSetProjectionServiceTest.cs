using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
	[TestFixture]
	public class RuleSetProjectionServiceTest
	{
        [Test]
		public void ShouldReturnRequiredData()
		{
			var shiftCreatorService = MockRepository.GenerateMock<IShiftCreatorService>();
			var target = new RuleSetProjectionService(shiftCreatorService);
			var workShift = WorkShiftFactory.CreateWithLunch(new TimePeriod(9, 0, 16, 0), new TimePeriod(11, 0, 12, 0));
			shiftCreatorService.Stub(x => x.Generate(null,null)).Return(new List<WorkShiftCollection> { new WorkShiftCollection(null) { workShift } });

			var result = target.ProjectionCollection(null,null);

			result.Single().ContractTime.Should().Be(workShift.Projection.ContractTime());
			result.Single().TimePeriod.Should().Be(workShift.ToTimePeriod().Value);
			result.Single().ShiftCategoryId.Should().Be(workShift.ShiftCategory.Id.Value);
			result.Single().Layers.Select(l => l.ActivityId).Should().Have.SameSequenceAs(
				workShift.Projection.Select(p => p.Payload.Id.Value));
			result.Single().Layers.Select(l => l.Period).Should().Have.SameSequenceAs(
				workShift.Projection.Select(p => p.Period));
		}

		[Test]
		public void ShouldReturnSerializableData()
		{
			var shiftCreatorService = MockRepository.GenerateMock<IShiftCreatorService>();
			var target = new RuleSetProjectionService(shiftCreatorService);
			var workShift = WorkShiftFactory.CreateWithLunch(new TimePeriod(9, 0, 16, 0), new TimePeriod(11, 0, 12, 0));
			shiftCreatorService.Stub(x => x.Generate(null,null)).Return(new List<WorkShiftCollection>{new WorkShiftCollection(null) {workShift}});

			var result = target.ProjectionCollection(null,null);

			var serializer = new BinaryFormatter();
			IWorkShiftProjection[] deserialized;
			using(var stream = new MemoryStream())
			{
				serializer.Serialize(stream, result);
				stream.Position = 0;
				deserialized = (IWorkShiftProjection[])serializer.Deserialize(stream);
			}

			deserialized.Single().ContractTime.Should().Be(result.Single().ContractTime);
			deserialized.Single().Layers.Count().Should().Be(result.Single().Layers.Count());
		}
	}
}