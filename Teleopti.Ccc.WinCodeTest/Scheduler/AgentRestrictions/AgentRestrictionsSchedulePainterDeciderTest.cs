using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsSchedulePainterDeciderTest
	{
		private AgentRestrictionsSchedulePainterDecider _decider;
		private IPreferenceCellData _preferenceCellData;
		private MockRepository _mocks;
		private RestrictionSchedulingOptions _restrictionSchedulingOptions;
		

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_preferenceCellData = _mocks.StrictMock<IPreferenceCellData>();
			_decider = new AgentRestrictionsSchedulePainterDecider();
			_restrictionSchedulingOptions = new RestrictionSchedulingOptions();
		}

		//[Test]
		//public void ShouldNotPaintWhenNotEnabled()
		//{
		//    _restrictionSchedulingOptions.UseScheduling = false;

		//    using (_mocks.Record())
		//    {
		//        Expect.Call(_preferenceCellData.Enabled).Return(false);
		//    }

		//    using (_mocks.Playback())
		//    {
		//        Assert.IsFalse(_decider.ShouldPaint(_preferenceCellData));
		//    }
		//}

		[Test]
		public void ShouldNotPaintWhenNotUseScheduling()
		{
			_restrictionSchedulingOptions.UseScheduling = false;

			using (_mocks.Record())
			{
				//Expect.Call(_preferenceCellData.Enabled).Return(true);
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions);
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_decider.ShouldPaint(_preferenceCellData));
			}	
		}

		[Test]
		public void ShouldPaintWhenDayOff()
		{
			_restrictionSchedulingOptions.UseScheduling = true;

			using(_mocks.Record())
			{
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions);
				Expect.Call(_preferenceCellData.HasDayOff).Return(true);
			}

			using(_mocks.Playback())
			{
				Assert.IsTrue(_decider.ShouldPaint(_preferenceCellData));	
			}		
		}

		[Test]
		public void ShouldPaintWhenShift()
		{
			_restrictionSchedulingOptions.UseScheduling = true;

			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions);
				Expect.Call(_preferenceCellData.HasDayOff).Return(false);
				Expect.Call(_preferenceCellData.HasShift).Return(true);
			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_decider.ShouldPaint(_preferenceCellData));
			}
		}

		[Test]
		public void ShouldPaintWhenFullDayAbsence()
		{
			_restrictionSchedulingOptions.UseScheduling = true;

			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions);
				Expect.Call(_preferenceCellData.HasDayOff).Return(false);
				Expect.Call(_preferenceCellData.HasShift).Return(false);
				Expect.Call(_preferenceCellData.HasFullDayAbsence).Return(true);
			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_decider.ShouldPaint(_preferenceCellData));
			}
		}

		[Test]
		public void ShouldPaintFullDayAbsence()
		{
			_restrictionSchedulingOptions.UseScheduling = true;

			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions);
				//Expect.Call(_preferenceCellData.HasDayOff).Return(false);
				Expect.Call(_preferenceCellData.HasFullDayAbsence).Return(true);
			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_decider.ShouldPaintFullDayAbsence(_preferenceCellData));
			}		
		}

		[Test]
		public void ShouldPaintFullDayAbsenceOnContractDayOff()
		{
			_restrictionSchedulingOptions.UseScheduling = true;

			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions);
				//Expect.Call(_preferenceCellData.HasDayOff).Return(false);
				//Expect.Call(_preferenceCellData.HasFullDayAbsence).Return(true);
				Expect.Call(_preferenceCellData.HasAbsenceOnContractDayOff).Return(true);
			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_decider.ShouldPaintFullDayAbsenceOnContractDayOff(_preferenceCellData));
			}	
		}

		[Test]
		public void ShouldPaintDayOff()
		{
			_restrictionSchedulingOptions.UseScheduling = true;

			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions);
				Expect.Call(_preferenceCellData.HasDayOff).Return(true);
			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_decider.ShouldPaintDayOff(_preferenceCellData));
			}	
		}

		[Test]
		public void ShouldPaintMainShift()
		{
			_restrictionSchedulingOptions.UseScheduling = true;
			_restrictionSchedulingOptions.UseRotations = false;

			using (_mocks.Record())
			{
				Expect.Call(_preferenceCellData.SchedulingOption).Return(_restrictionSchedulingOptions).Repeat.AtLeastOnce();
				Expect.Call(_preferenceCellData.HasFullDayAbsence).Return(false).Repeat.AtLeastOnce();
				Expect.Call(_preferenceCellData.HasDayOff).Return(false);
				Expect.Call(_preferenceCellData.HasShift).Return(true);
			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_decider.ShouldPaintMainShift(_preferenceCellData));
			}	
		}
	}
}
