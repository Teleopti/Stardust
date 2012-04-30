using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
	[TestFixture]
	public class RuleSetProjectionServiceTest
	{
		[Test]
		public void ShouldReturnSerializableData()
		{
			var shiftCreatorService = MockRepository.GenerateMock<IShiftCreatorService>();
			var target = new RuleSetProjectionService(shiftCreatorService);
			var workShift = WorkShiftFactory.CreateWithLunch(new TimePeriod(9, 0, 16, 0), new TimePeriod(11, 0, 12, 0));
			shiftCreatorService.Stub(x => x.Generate(null)).Return(new List<IWorkShift>(new[] {workShift}));

			var result = target.ProjectionCollection(null);

			var serializer = new BinaryFormatter();
			var stream = new MemoryStream();
			serializer.Serialize(stream, result);
			stream.Position = 0;
			var deserialized = (IWorkShiftProjection[]) serializer.Deserialize(stream);

			deserialized.Single().ContractTime.Should().Be(workShift.Projection.ContractTime());
			deserialized.Single().TimePeriod.Should().Be(workShift.ToTimePeriod().Value);
			deserialized.Single().ShiftCategoryId.Should().Be(workShift.ShiftCategory.Id.Value);
			deserialized.Single().Layers.Select(l => l.ActivityId).Should().Have.SameSequenceAs(
				workShift.Projection.Select(p => p.Payload.Id.Value));
			deserialized.Single().Layers.Select(l => l.Period).Should().Have.SameSequenceAs(
				workShift.Projection.Select(p => p.Period));
		}
	}
}