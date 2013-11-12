using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeScheduleLayerViewModelMapperTest
	{
		[Test]
		public void ShouldMapLayerFromReadModelLayer()
		{
			var readModelLayer = new SimpleLayer
			{
				Start = DateTime.Now,
				End = DateTime.Now.AddHours(1),
				Minutes = 60,
				Color = "green",
				Description = "Phone",
				IsAbsenceConfidential = false
			};

			var target = new ShiftTradeScheduleLayerViewModelMapper();
			var result = target.Map(new[] { readModelLayer });

			var mappedlayer = result.First();
			mappedlayer.Start.Should().Be.EqualTo(readModelLayer.Start);
			mappedlayer.End.Should().Be.EqualTo(readModelLayer.End);
			mappedlayer.LengthInMinutes.Should().Be.EqualTo(readModelLayer.Minutes);
			mappedlayer.Color.Should().Be.EqualTo(readModelLayer.Color);
			mappedlayer.IsAbsenceConfidential.Should().Be.EqualTo(readModelLayer.IsAbsenceConfidential);
		}

		[Test, SetCulture("en-US")]
		public void ShouldMapLayerTitleToUsCulture()
		{
			var startDateTime = DateTime.Now;
			var readModelLayer = new SimpleLayer
			{
				Start = startDateTime,
				End = startDateTime.AddHours(1),
				Description = "Phone"
			};

			var target = new ShiftTradeScheduleLayerViewModelMapper();
			var result = target.Map(new[] { readModelLayer });

			var expectedTitle = string.Format(CultureInfo.CurrentCulture, "{0}\n{1}-{2}", readModelLayer.Description,
			                                  readModelLayer.Start.ToShortTimeString(), readModelLayer.End.ToShortTimeString());
			result.First().Title.Should().Be.EqualTo(expectedTitle);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapLayerTitleToSwedishCulture()
		{
			var startDateTime = DateTime.Now;
			var readModelLayer = new SimpleLayer
			{
				Start = startDateTime,
				End = startDateTime.AddHours(1),
				Description = "Phone"
			};

			var target = new ShiftTradeScheduleLayerViewModelMapper();
			var result = target.Map(new[] { readModelLayer });

			var expectedTitle = string.Format(CultureInfo.CurrentCulture, "{0}\n{1}-{2}", readModelLayer.Description,
											  readModelLayer.Start.ToShortTimeString(), readModelLayer.End.ToShortTimeString());
			result.First().Title.Should().Be.EqualTo(expectedTitle);
		}
	}
}
