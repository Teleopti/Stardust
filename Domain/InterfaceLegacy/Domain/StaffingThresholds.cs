using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Thresholds for staffing figures
	/// </summary>
	/// <remarks>
	/// Created by: robink
	/// Created date: 2008-09-16
	/// </remarks>
	public struct StaffingThresholds : IEquatable<StaffingThresholds>
	{
		private readonly Percent _seriousUnderstaffing;
		private readonly Percent _understaffing;
		private readonly Percent _overstaffing;
		private readonly Percent _understaffingFor;

		/// <summary>
		/// Initializes a new instance of the <see cref="StaffingThresholds"/> struct.
		/// </summary>
		/// <param name="seriousUnderstaffing">The serious understaffing.</param>
		/// <param name="understaffing">The understaffing.</param>
		/// <param name="overstaffing">The overstaffing.</param>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-09-16
		/// </remarks>
		public StaffingThresholds(Percent seriousUnderstaffing, Percent understaffing, Percent overstaffing)
		{
			_seriousUnderstaffing = seriousUnderstaffing;
			_understaffing = understaffing;
			_overstaffing = overstaffing;
			_understaffingFor = new Percent(1);
		}

		///<summary>
		/// Initializes a new instance of the <see cref="StaffingThresholds"/> struct if understaffingFor available.
		///</summary>
		/// <param name="seriousUnderstaffing">The serious understaffing.</param>
		/// <param name="understaffing">The understaffing.</param>
		/// <param name="overstaffing">The overstaffing.</param>
		/// <param name="understaffingFor">The understaffing realm</param>
		public StaffingThresholds(Percent seriousUnderstaffing, Percent understaffing, Percent overstaffing, Percent understaffingFor)
		{
			_seriousUnderstaffing = seriousUnderstaffing;
			_understaffing = understaffing;
			_overstaffing = overstaffing;
			_understaffingFor = understaffingFor;
		}

		/// <summary>
		/// Gets the threshold for serious understaffing.
		/// </summary>
		/// <value>The threshold for serious understaffing.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-09-16
		/// </remarks>
		public Percent SeriousUnderstaffing
		{
			get { return _seriousUnderstaffing; }
		}

		/// <summary>
		/// Gets the threshold for understaffing.
		/// </summary>
		/// <value>The threshold for understaffing.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-09-16
		/// </remarks>
		public Percent Understaffing
		{
			get { return _understaffing; }
		}

		/// <summary>
		/// Gets the threshold for overstaffing.
		/// </summary>
		/// <value>The threshold for overstaffing.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-09-16
		/// </remarks>
		public Percent Overstaffing
		{
			get { return _overstaffing; }
		}

		///<summary>
		/// Gets the threshold for understaffing realm.
		///</summary>
		public Percent UnderstaffingFor
		{
			get { return _understaffingFor; }
		}

		/// <summary>
		/// Defaults the values.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-10-27
		/// </remarks>
		public static StaffingThresholds DefaultValues()
		{
			return new StaffingThresholds(new Percent(-0.2), new Percent(-0.1), new Percent(0.1));
		}

		///<summary>
		///Indicates whether the current object is equal to another object of the same type.
		///</summary>
		///
		///<returns>
		///true if the current object is equal to the other parameter; otherwise, false.
		///</returns>
		///
		///<param name="other">An object to compare with this object.</param>
		public bool Equals(StaffingThresholds other)
		{
			return (_seriousUnderstaffing == other._seriousUnderstaffing && 
					_understaffing == other._understaffing &&
					_overstaffing == other._overstaffing && _understaffingFor == other._understaffingFor);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">Another object to compare to.</param>
		/// <returns>
		/// true if obj and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			return obj is StaffingThresholds thresholds && Equals(thresholds);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return _seriousUnderstaffing.GetHashCode() ^ _understaffing.GetHashCode() ^ _overstaffing.GetHashCode() ^ _understaffingFor.GetHashCode();
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="staffingThresholds1">The staffing thresholds1.</param>
		/// <param name="staffingThresholds2">The staffing thresholds2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(StaffingThresholds staffingThresholds1, StaffingThresholds staffingThresholds2)
		{
			return staffingThresholds1.Equals(staffingThresholds2);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="staffingThresholds1">The staffing thresholds1.</param>
		/// <param name="staffingThresholds2">The staffing thresholds2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(StaffingThresholds staffingThresholds1, StaffingThresholds staffingThresholds2)
		{
			return !staffingThresholds1.Equals(staffingThresholds2);
		}
	}
}