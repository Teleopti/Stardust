using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[TestFixture]
	public class ProjectionChangedEventShiftExtensionTest
	{

		[Test]
		public void ShouldFindStartIndex()
		{
			var start = new DateTime(2014, 12, 01, 8, 0, 0);
			var list = createLayers(start, new[] { 60, 15, 60 });

			ProjectionChangedEventShiftExtension.FindStartIndex(list, start).Should().Be.EqualTo(0);
			ProjectionChangedEventShiftExtension.FindStartIndex(list, start.AddMinutes(15)).Should().Be.EqualTo(0);
			ProjectionChangedEventShiftExtension.FindStartIndex(list, start.AddHours(1)).Should().Be.EqualTo(1);
			ProjectionChangedEventShiftExtension.FindStartIndex(list, start.AddHours(5)).Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldFilterOutOneSingleLayer()
		{
			var start = new DateTime(2014, 12, 01, 8, 0, 0,DateTimeKind.Utc);
			var list = createLayers(start, new[] {60, 15, 60});

			var shift = new ProjectionChangedEventShift {Layers = list};
			var layers = shift.FilterLayers(new DateTimePeriod(start, start.AddMinutes(15)));
			layers.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldFilterOutTwoLayers()
		{
			var start = new DateTime(2014, 12, 01, 8, 0, 0, DateTimeKind.Utc);
			var list = createLayers(start, new[] { 5, 20 });

			var shift = new ProjectionChangedEventShift { Layers = list };
			var layers = shift.FilterLayers(new DateTimePeriod(start, start.AddMinutes(15))).ToList();

			layers.Count().Should().Be.EqualTo(2);
			layers[0].StartDateTime.Should().Be.EqualTo(start);
			layers[0].EndDateTime.Should().Be.EqualTo(start.AddMinutes(5));
			layers[1].StartDateTime.Should().Be.EqualTo(start.AddMinutes(5));
			layers[1].EndDateTime.Should().Be.EqualTo(start.AddMinutes(15));
		}

		[Test]
		public void ShouldFilterOutLayerStartingInInterval()
		{
			var start = new DateTime(2014, 12, 01, 8, 0, 0, DateTimeKind.Utc);
			var list = createLayers(start.AddMinutes(10), new[] { 20, 20 });

			var shift = new ProjectionChangedEventShift { Layers = list };
			var layers = shift.FilterLayers(new DateTimePeriod(start, start.AddMinutes(15))).ToList();

			layers.Count().Should().Be.EqualTo(1);
			layers[0].StartDateTime.Should().Be.EqualTo(start.AddMinutes(10));
			layers[0].EndDateTime.Should().Be.EqualTo(start.AddMinutes(15));
			
		}

		[Test]
		public void ShouldSetContractTimeCorrect()
		{
			var start = new DateTime(2014, 12, 01, 8, 0, 0, DateTimeKind.Utc);
			var list = createLayers(start.AddMinutes(10), new[] { 20, 20 });

			var shift = new ProjectionChangedEventShift { Layers = list };
			var layers = shift.FilterLayers(new DateTimePeriod(start, start.AddMinutes(15))).ToList();

			layers.Count().Should().Be.EqualTo(1);
			layers[0].ContractTime.TotalMinutes.Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldSetWorkTimeCorrect()
		{
			var start = new DateTime(2014, 12, 01, 8, 0, 0, DateTimeKind.Utc);
			var list = createLayers(start.AddMinutes(10), new[] { 20, 20 });

			var shift = new ProjectionChangedEventShift { Layers = list };
			var layers = shift.FilterLayers(new DateTimePeriod(start, start.AddMinutes(15))).ToList();

			layers.Count().Should().Be.EqualTo(1);
			layers[0].WorkTime.TotalMinutes.Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldSetOvertimeCorrect()
		{
			var start = new DateTime(2014, 12, 01, 8, 0, 0, DateTimeKind.Utc);
			var list = createLayers(start.AddMinutes(10), new[] { 20 });

			var shift = new ProjectionChangedEventShift { Layers = list };
			var layers = shift.FilterLayers(new DateTimePeriod(start, start.AddMinutes(15))).ToList();

			layers.Count().Should().Be.EqualTo(1);
			layers[0].Overtime.TotalMinutes.Should().Be.EqualTo(5);
		}


		[Test]
		public void ShouldSetPropertiesCorrect()
		{
			var start = new DateTime(2014, 12, 01, 8, 0, 0, DateTimeKind.Utc);
			var list = createLayers(start, new[] { 15 });

			var shift = new ProjectionChangedEventShift { Layers = list };
			var layers = shift.FilterLayers(new DateTimePeriod(start, start.AddMinutes(15))).ToList();

			layers.Count().Should().Be.EqualTo(1);
			var layer = list[0];

			layers[0].DisplayColor.Should().Be.EqualTo(layer.DisplayColor);
			layers[0].Name.Should().Be.EqualTo(layer.Name);
			layers[0].ShortName.Should().Be.EqualTo(layer.ShortName);
			layers[0].PayloadId.Should().Be.EqualTo(layer.PayloadId);
			layers[0].IsAbsence.Should().Be.EqualTo(layer.IsAbsence);
		}
		private IList<ProjectionChangedEventLayer> createLayers(DateTime startOfShift, IEnumerable<int> lengthCollection)
		{
			int accStart = 0;
			var layerList = new List<ProjectionChangedEventLayer>();

			foreach (int length in lengthCollection)
			{
				layerList.Add(
					new ProjectionChangedEventLayer
					{
						StartDateTime = startOfShift.AddMinutes(accStart),
						EndDateTime = startOfShift.AddMinutes(accStart).AddMinutes(length),
						ContractTime = new TimeSpan(0,0,length), 
						WorkTime = new TimeSpan(0,0,length),
						Overtime = new TimeSpan(0,0,length),
						DisplayColor = Color.Brown.ToArgb(),
						IsAbsence = true,
						IsAbsenceConfidential = true,
						Name = "Jonas",
						ShortName = "JN",
						PayloadId = Guid.NewGuid()
					}
					);
				accStart += length;
			}
			return layerList;
		}
	}
}
