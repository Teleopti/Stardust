using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.MatrixLockers
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class MatrixNotPermittedLockerTest
	{
		private MockRepository _mocks;
		private IMatrixNotPermittedLocker _target;
		private IScheduleMatrixPro _matrix;
		private IList<IScheduleMatrixPro> _matrixList;
		private IAuthorization _authorization;
		private string _function;
		private IPerson _person;
		private IScheduleDayPro _scheduleDayPro;
		private IScheduleDay _scheduleDay;
		private IPersistableScheduleData _persistableScheduleData;
		private IList<IPersistableScheduleData> _persistableScheduleDataList;
		
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_authorization = _mocks.StrictMock<IAuthorization>();
			_target = new MatrixNotPermittedLocker(new ThisAuthorization(_authorization));
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrixList = new List<IScheduleMatrixPro>{_matrix};
			_person = PersonFactory.CreatePerson();
			_function = DefinedRaptorApplicationFunctionPaths.ModifySchedule;
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_persistableScheduleData = _mocks.StrictMock<IPersistableScheduleData>();
			_persistableScheduleDataList = new List<IPersistableScheduleData>{_persistableScheduleData};
		}

		[Test]
		public void ShouldLockIfNoPermission()
		{
			using (_mocks.Record())
			{
				commonMocks();

				Expect.Call(_authorization.IsPermitted("", DateOnly.Today, _person)).IgnoreArguments().Return(false);
				
				//Assert
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2000, 1, 2)).Repeat.Twice();
				Expect.Call(() => _matrix.LockDay(new DateOnly(2000, 1, 2)));
			}

			using (_mocks.Playback())
			{
					_target.Execute(_matrixList);
			}
		}

		[Test]
		public void ShouldNotLockIfPermission()
		{
			using (_mocks.Record())
			{
				commonMocks();

				Expect.Call(_authorization.IsPermitted("", DateOnly.Today, _person)).IgnoreArguments().Return(true);
			}

			using (_mocks.Playback())
			{
				_target.Execute(_matrixList);
			}
		}

		private void commonMocks()
		{
			Expect.Call(_matrix.UnlockedDays)
				.Return(new [] {_scheduleDayPro});
			Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
			Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(_persistableScheduleDataList);
			Expect.Call(_persistableScheduleData.FunctionPath).Return(_function);
			Expect.Call(_persistableScheduleData.Period).Return(new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			Expect.Call(_persistableScheduleData.Person).Return(_person).Repeat.Any();
		}
	}

}