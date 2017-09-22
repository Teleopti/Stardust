using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsPreferencePainterDeciderTest
	{
		private AgentRestrictionsPreferencePainterDecider _decider;
		private IPreferenceCellData _preferenceCellData;
		private MockRepository _mocks;
		private RestrictionSchedulingOptions _restrictionSchedulingOptions;
		private IEffectiveRestriction _effectiveRestriction;
		private IDayOffTemplate _dayOffTemplate;
		private IShiftCategory _shiftCategory;
		private IAbsence _absence;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_preferenceCellData = _mocks.StrictMock<IPreferenceCellData>();
			_decider = new AgentRestrictionsPreferencePainterDecider();
			_restrictionSchedulingOptions = new RestrictionSchedulingOptions();
			_effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			_dayOffTemplate = _mocks.StrictMock<IDayOffTemplate>();
			_shiftCategory = _mocks.StrictMock<IShiftCategory>();
			_absence = _mocks.StrictMock<IAbsence>();
		}

		
		[Test]
		public void ShouldThrowExceptionOnNullArgumentShouldPaint()
		{
			Assert.Throws<ArgumentNullException>(() => _decider.ShouldPaint(null));
		}

		[Test]
		public void ShouldThrowExceptionOnNullArgumentShouldPaintPreferredDayOff()
		{
			Assert.Throws<ArgumentNullException>(() => _decider.ShouldPaintPreferredDayOff(null));
		}

		[Test]
		public void ShouldThrowExceptionOnNullArgumentShouldPaintPreferredShiftCategory()
		{
			Assert.Throws<ArgumentNullException>(() => _decider.ShouldPaintPreferredDayOff(null));
		}

		[Test]
		public void ShouldThrowExceptionOnNullArgumentShouldPaintPreferredAbsence()
		{
			Assert.Throws<ArgumentNullException>(() => _decider.ShouldPaintPreferredAbsence(null));
		}

		[Test]
		public void ShouldThrowExceptionOnNullArgumentShouldPaintPreferredAbsenceOnContractDayOff()
		{
			Assert.Throws<ArgumentNullException>(() => _decider.ShouldPaintPreferredAbsenceOnContractDayOff(null));
		}

		[Test]
		public void ShouldThrowExceptionOnNullArgumentShouldPaintPreferredExtended()
		{
			Assert.Throws<ArgumentNullException>(() => _decider.ShouldPaintPreferredExtended(null));
		}

		[Test]
		public void ShouldNotPaintWhenSchedule()
		{
			_restrictionSchedulingOptions.UseScheduling = true;

			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions);
				Expect.Call(_preferenceCellData.HasFullDayAbsence).Return(true);
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_decider.ShouldPaint(_preferenceCellData));
			}	
		}

		[Test]
		public void ShouldNotPaintWhenNotSelected()
		{
			_restrictionSchedulingOptions.UseScheduling = true;
			_restrictionSchedulingOptions.UsePreferences = false;
			_restrictionSchedulingOptions.UseRotations = false;
			_restrictionSchedulingOptions.UseAvailability = false;
			_restrictionSchedulingOptions.UseStudentAvailability= false;

			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.HasFullDayAbsence).Return(false);
				Expect.Call(_preferenceCellData.HasDayOff).Return(false);
				Expect.Call(_preferenceCellData.HasShift).Return(false);
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_decider.ShouldPaint(_preferenceCellData));
			}	
		}

		[Test]
		public void ShouldPaintPreferredDayOff()
		{
			_restrictionSchedulingOptions.UseScheduling = true;
			_restrictionSchedulingOptions.UsePreferences = true;
			_restrictionSchedulingOptions.UseRotations = true;
			
			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.EffectiveRestriction).Return(_effectiveRestriction).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestriction.DayOffTemplate).Return(_dayOffTemplate);
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestriction.ShiftCategory).Return(null);
			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_decider.ShouldPaintPreferredDayOff(_preferenceCellData));
			}	
		}

		[Test]
		public void ShouldNotPaintPreferredDayOffWhenNoRestriction()
		{
			_restrictionSchedulingOptions.UseScheduling = true;
			_restrictionSchedulingOptions.UsePreferences = true;
			_restrictionSchedulingOptions.UseRotations = true;

			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.EffectiveRestriction).Return(null).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_decider.ShouldPaintPreferredDayOff(_preferenceCellData));
			}
		}

		[Test]
		public void ShouldNotPaintPreferredDayOffWhenNoDayOffTemplate()
		{
			_restrictionSchedulingOptions.UseScheduling = true;
			_restrictionSchedulingOptions.UsePreferences = true;
			_restrictionSchedulingOptions.UseRotations = true;

			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.EffectiveRestriction).Return(_effectiveRestriction).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestriction.DayOffTemplate).Return(null);
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_decider.ShouldPaintPreferredDayOff(_preferenceCellData));
			}
		}

		[Test]
		public void ShouldPaintPreferredShiftCategory()
		{
			_restrictionSchedulingOptions.UseScheduling = true;
			_restrictionSchedulingOptions.UsePreferences = true;
			_restrictionSchedulingOptions.UseRotations = false;


			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.EffectiveRestriction).Return(_effectiveRestriction).Repeat.AtLeastOnce();
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestriction.ShiftCategory).Return(_shiftCategory);
			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_decider.ShouldPaintPreferredShiftCategory(_preferenceCellData));
			}	
		}

		[Test]
		public void ShouldNotPaintPreferredShiftCategoryWhenNoEffectiveRestriction()
		{
			_restrictionSchedulingOptions.UseScheduling = true;
			_restrictionSchedulingOptions.UsePreferences = true;
			_restrictionSchedulingOptions.UseRotations = false;


			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.EffectiveRestriction).Return(null).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_decider.ShouldPaintPreferredShiftCategory(_preferenceCellData));
			}
		}

		[Test]
		public void ShouldNotPaintPreferredShiftCategoryWhenNoShiftCategory()
		{
			_restrictionSchedulingOptions.UseScheduling = true;
			_restrictionSchedulingOptions.UsePreferences = true;
			_restrictionSchedulingOptions.UseRotations = false;


			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.EffectiveRestriction).Return(_effectiveRestriction).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestriction.ShiftCategory).Return(null);
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_decider.ShouldPaintPreferredShiftCategory(_preferenceCellData));
			}
		}

		[Test]
		public void ShouldPaintPreferredAbsence()
		{
			_restrictionSchedulingOptions.UseScheduling = true;
			_restrictionSchedulingOptions.UsePreferences = true;
			_restrictionSchedulingOptions.UseRotations = true;


			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.EffectiveRestriction).Return(_effectiveRestriction).Repeat.AtLeastOnce();
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestriction.Absence).Return(_absence);
				Expect.Call(_preferenceCellData.HasAbsenceOnContractDayOff).Return(false);
				Expect.Call(_effectiveRestriction.DayOffTemplate).Return(null);
			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_decider.ShouldPaintPreferredAbsence(_preferenceCellData));
			}
		}

		[Test]
		public void ShouldNotPaintPreferredAbsenceWhenNoEffectiveRestriction()
		{
			_restrictionSchedulingOptions.UseScheduling = true;
			_restrictionSchedulingOptions.UsePreferences = true;
			_restrictionSchedulingOptions.UseRotations = true;


			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.EffectiveRestriction).Return(null).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_decider.ShouldPaintPreferredAbsence(_preferenceCellData));
			}
		}

		[Test]
		public void ShouldNotPaintPreferredAbsenceWhenNoAbsence()
		{
			_restrictionSchedulingOptions.UseScheduling = true;
			_restrictionSchedulingOptions.UsePreferences = true;
			_restrictionSchedulingOptions.UseRotations = true;


			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.EffectiveRestriction).Return(_effectiveRestriction).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestriction.Absence).Return(null);
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_decider.ShouldPaintPreferredAbsence(_preferenceCellData));
			}
		}

		[Test]
		public void ShouldNotPaintPreferredAbsenceWhenOnContractOnDayOff()
		{
			_restrictionSchedulingOptions.UseScheduling = true;
			_restrictionSchedulingOptions.UsePreferences = true;
			_restrictionSchedulingOptions.UseRotations = true;


			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.EffectiveRestriction).Return(_effectiveRestriction).Repeat.AtLeastOnce();
				Expect.Call(_preferenceCellData.HasAbsenceOnContractDayOff).Return(true);
				Expect.Call(_effectiveRestriction.Absence).Return(_absence);
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_decider.ShouldPaintPreferredAbsence(_preferenceCellData));
			}
		}

		[Test]
		public void ShouldPaintPreferredAbsenceOnContractDayOff()
		{
			_restrictionSchedulingOptions.UseScheduling = true;
			_restrictionSchedulingOptions.UsePreferences = true;
			_restrictionSchedulingOptions.UseRotations = true;


			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.EffectiveRestriction).Return(_effectiveRestriction).Repeat.AtLeastOnce();
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestriction.Absence).Return(_absence);
				Expect.Call(_preferenceCellData.HasAbsenceOnContractDayOff).Return(true);
				Expect.Call(_effectiveRestriction.DayOffTemplate).Return(null);
			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_decider.ShouldPaintPreferredAbsenceOnContractDayOff(_preferenceCellData));
			}
		}

		[Test]
		public void ShouldNotPaintPreferredAbsenceOnContractDayOffWhenNoEffectiveRestriction()
		{
			_restrictionSchedulingOptions.UseScheduling = true;
			_restrictionSchedulingOptions.UsePreferences = true;
			_restrictionSchedulingOptions.UseRotations = true;


			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.EffectiveRestriction).Return(null).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_decider.ShouldPaintPreferredAbsenceOnContractDayOff(_preferenceCellData));
			}
		}

		[Test]
		public void ShouldNotPaintPreferredAbsenceOnContractDayOffWhenNoAbsence()
		{
			_restrictionSchedulingOptions.UseScheduling = true;
			_restrictionSchedulingOptions.UsePreferences = true;
			_restrictionSchedulingOptions.UseRotations = true;


			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.EffectiveRestriction).Return(_effectiveRestriction).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestriction.Absence).Return(null);
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_decider.ShouldPaintPreferredAbsenceOnContractDayOff(_preferenceCellData));
			}
		}

		[Test]
		public void ShouldPaintPreferredExtended()
		{
			_restrictionSchedulingOptions.UseScheduling = false;
			_restrictionSchedulingOptions.UsePreferences = true;

			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions).Repeat.AtLeastOnce();
				Expect.Call(_preferenceCellData.HasExtendedPreference).Return(true);
				Expect.Call(_preferenceCellData.EffectiveRestriction).Return(_effectiveRestriction);
				Expect.Call(_effectiveRestriction.DayOffTemplate).Return(null);
			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_decider.ShouldPaintPreferredExtended(_preferenceCellData));
			}
		}

		[Test]
		public void ShouldNotPaintPreferredExtendedWhenNotUsePreferences()
		{
			_restrictionSchedulingOptions.UsePreferences = false;

			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_decider.ShouldPaintPreferredExtended(_preferenceCellData));
			}
		}

		[Test]
		public void ShouldNotPaintPreferredExtendedWhenNoExtendedPreferences()
		{
			_restrictionSchedulingOptions.UsePreferences = true;

			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions).Repeat.AtLeastOnce();
				Expect.Call(_preferenceCellData.HasExtendedPreference).Return(false);
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_decider.ShouldPaintPreferredExtended(_preferenceCellData));
			}
		}

		//[Test]
		//public void ShouldNotPaintPreferredExtendedWhenUseSchedules()
		//{
		//    _restrictionSchedulingOptions.UsePreferences = true;
		//    _restrictionSchedulingOptions.UseScheduling = true;

		//    using (_mocks.Record())
		//    {
		//        Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions).Repeat.AtLeastOnce();
		//        Expect.Call(_preferenceCellData.HasExtendedPreference).Return(true);
		//    }

		//    using (_mocks.Playback())
		//    {
		//        Assert.IsFalse(_decider.ShouldPaintPreferredExtended(_preferenceCellData));
		//    }
		//}
	}
}
