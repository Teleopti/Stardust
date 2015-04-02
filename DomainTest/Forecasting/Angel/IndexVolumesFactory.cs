using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class IndexVolumesFactory
	{
		public static IVolumeYear CreateMonthOfYear()
		{
			var monthOfYear = MockRepository.GenerateMock<IVolumeYear>();
			monthOfYear.Stub(x => x.TaskIndex(Arg<DateOnly>.Is.Anything)).Return(1d);
			monthOfYear.Stub(x => x.AfterTalkTimeIndex(Arg<DateOnly>.Is.Anything)).Return(1d);
			monthOfYear.Stub(x => x.TalkTimeIndex(Arg<DateOnly>.Is.Anything)).Return(1d);
			return monthOfYear;
		}

		public static IVolumeYear CreateWeekOfMonth()
		{
			var weekOfMonth = MockRepository.GenerateMock<IVolumeYear>();
			weekOfMonth.Stub(x => x.TaskIndex(Arg<DateOnly>.Is.Anything)).Return(1.1d);
			weekOfMonth.Stub(x => x.AfterTalkTimeIndex(Arg<DateOnly>.Is.Anything)).Return(1.1d);
			weekOfMonth.Stub(x => x.TalkTimeIndex(Arg<DateOnly>.Is.Anything)).Return(1.1d);
			return weekOfMonth;
		}

		public static IVolumeYear CreateDayOfWeek()
		{
			var dayOfWeek = MockRepository.GenerateMock<IVolumeYear>();
			dayOfWeek.Stub(x => x.TaskIndex(Arg<DateOnly>.Is.Anything)).Return(1.2d);
			dayOfWeek.Stub(x => x.AfterTalkTimeIndex(Arg<DateOnly>.Is.Anything)).Return(1.2d);
			dayOfWeek.Stub(x => x.TalkTimeIndex(Arg<DateOnly>.Is.Anything)).Return(1.2d);
			return dayOfWeek;
		}

		public static IVolumeYear[] Create()
		{
			return new[] {CreateMonthOfYear(), CreateWeekOfMonth(), CreateDayOfWeek()};
		}
	}
}