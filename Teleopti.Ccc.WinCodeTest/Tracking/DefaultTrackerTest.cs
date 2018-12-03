#region Imports

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;


#endregion

namespace Teleopti.Ccc.WinCodeTest.Tracking
{

	/// <summary>
	/// Represents the test class for the default tracker.
	/// </summary>
	/// <remarks>
	/// Created by: SavaniN
	/// Created date: 2008-12-03
	/// </remarks>
	[TestFixture]
	public class DefaultTrackerTest
	{
		private ITracker _defaultTracker;

		[SetUp]
		public void Setup()
		{
			_defaultTracker = TrackerView.DefaultTracker;
		}

		/// <summary>
		/// Verifies the tracker.
		/// </summary>
		/// <remarks>
		/// Created by: SavaniN
		/// Created date: 2008-12-03
		/// </remarks>
		[Test]
		public void VerifyTracker()
		{
			Assert.AreEqual(_defaultTracker, TrackerView.DefaultTracker);
		}

		/// <summary>
		/// Verifies the can get description.
		/// </summary>
		/// <remarks>
		/// Created by: SavaniN
		/// Created date: 2008-12-03
		/// </remarks>
		[Test]
		public void VerifyCanGetDescription()
		{
			Description description = new Description(UserTexts.Resources.DefaultTrackerDefaultDescription);
			Assert.AreEqual(_defaultTracker.Description, description);
			Assert.AreSame(_defaultTracker.Description.Name, description.Name);
		}

		[Test]
		public void ShouldReturnZeroTime()
		{
			var result = _defaultTracker.TrackForReset(null, new List<IScheduleDay>());
			Assert.AreEqual(TimeSpan.Zero, result);
		}

		[Test]
		public void VerifyExceptionOnCreatePersonAccount()
		{
			Assert.Throws<NotImplementedException>(() => _defaultTracker.CreatePersonAccount(new DateOnly()));
		}
	}
}
