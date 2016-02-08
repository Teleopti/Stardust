using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Common
{
	[TestFixture]
	public class ScheduleDataLoaderTest
	{
		private ScheduleDataLoader _scheduleDataLoader;
		private MockRepository _mockRepository;
		private IUnitOfWork _unitOfWork;
		private ISchedulerStateHolder _schedulerStateHolder;
		private IPerson _person;
		private IScheduleStorage _scheduleStorage;
		private IPersonProvider _personProvider;
	    private IScheduleDictionaryLoadOptions _scheduleDictionaryLoadOptions;
		private IScheduleDateTimePeriod _scheduleDateTimePeriod;

		[SetUp]
		public void Setup()
		{
			
			_mockRepository = new MockRepository();
			_unitOfWork = _mockRepository.StrictMock<IUnitOfWork>();
			_schedulerStateHolder = _mockRepository.StrictMock<ISchedulerStateHolder>();
			_scheduleDataLoader = new ScheduleDataLoader(_schedulerStateHolder);
			_person = _mockRepository.StrictMock<IPerson>();
			_scheduleStorage = _mockRepository.StrictMock<IScheduleStorage>();
			_personProvider = _mockRepository.StrictMock<IPersonProvider>();
		    _scheduleDictionaryLoadOptions = _mockRepository.StrictMock<IScheduleDictionaryLoadOptions>();
			_scheduleDateTimePeriod = _mockRepository.StrictMock<IScheduleDateTimePeriod>();
		}

		[Test]
		public void ShouldLoadSchedule()
		{
			using(_mockRepository.Record())
			{
				Expect.Call(() => _schedulerStateHolder.LoadSchedules(_scheduleStorage, _personProvider, _scheduleDictionaryLoadOptions, _scheduleDateTimePeriod)).IgnoreArguments();
			}

			using(_mockRepository.Playback())
			{
				_scheduleDataLoader.LoadSchedule(_unitOfWork, new DateTimePeriod(2011,1,1,2011,1,1), _person);
			}
		}
	}
}
