using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
	///<summary>
	/// Struct for TimeSpan that has to be positive or zero
	///</summary>
	/// <remarks>
	/// Created by: Mattias E
	/// Date: 2011-03-16
	/// </remarks>
	[Serializable]
	public struct PositiveTimeSpan
	{
		private TimeSpan _timeSpan;

		///<summary>
		/// The actual time span
		///</summary>
		public TimeSpan TimeSpan
		{
			get { return _timeSpan; }

			set
			{
				_timeSpan = value;
				Validate(_timeSpan);
			}
		}

		///<summary>
		/// Initiates a new instance of <see cref="PositiveTimeSpan"/> struct
		///</summary>
		///<param name="timeSpan"> The TimeSpan to base it on</param>
		public PositiveTimeSpan(TimeSpan timeSpan)
		{
			_timeSpan = timeSpan;
			Validate(_timeSpan);
		}

		///<summary>
		/// Initiates a new instance of <see cref="PositiveTimeSpan"/> struct
		///</summary>
		///<param name="hours"> Number of hours </param>
		///<param name="minutes"> Number of minutes </param>
		///<param name="seconds"> Number of seconds </param>
		public PositiveTimeSpan(int hours, int minutes, int seconds)
		{
			_timeSpan = new TimeSpan(hours, minutes, seconds);
			Validate(_timeSpan);
		}
		
		///<summary>
		/// Explicit cast from TimeSpan
		///</summary>
		///<param name="value"> The TimeSpan to cast </param>
		///<returns></returns>
		public static explicit operator PositiveTimeSpan?(TimeSpan value)
		{
			return new PositiveTimeSpan(value);
		}

		///<summary>
		/// Implicit cast to TimeSpan
		///</summary>
		///<param name="value"> The PositiveTimeSpan </param>
		///<returns></returns>
		public static implicit operator TimeSpan?(PositiveTimeSpan value)
		{
			return value.TimeSpan;
		}

		///<summary>
		/// Converts a PostiveTimeSpan to a TimeSpan
		///</summary>
		///<param name="positiveTimeSpan"> The PostiveTimeSpan to convert </param>
		///<returns> The TimeSpan corresponding to postitiveTimeSpan</returns>
		public static TimeSpan ToTimeSpan(PositiveTimeSpan positiveTimeSpan)
		{
			return positiveTimeSpan.TimeSpan;
		}

		private static bool Validate(TimeSpan timeSpan)
		{
			if (timeSpan < TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException("timeSpan", timeSpan, "Time span cannot be negative");
			}	
			return true;
		}

		///<summary>
		/// Compares to PositiveTimeSpans
		///</summary>
		///<param name="positiveTimeSpan1"> PostiveTimeSpan 1 </param>
		///<param name="positiveTimeSpan2"> PostiveTimeSpan 2 </param>
		///<returns></returns>
		public static bool operator ==(PositiveTimeSpan positiveTimeSpan1, PositiveTimeSpan positiveTimeSpan2)
		{
			return positiveTimeSpan1.TimeSpan == positiveTimeSpan2.TimeSpan;
		}

		///<summary>
		/// Compares to PositiveTimeSpans
		///</summary>
		///<param name="positiveTimeSpan1"> PostiveTimeSpan 1 </param>
		///<param name="positiveTimeSpan2"> PostiveTimeSpan 2 </param>
		///<returns></returns>
		public static bool operator !=(PositiveTimeSpan positiveTimeSpan1, PositiveTimeSpan positiveTimeSpan2)
		{
			return !(positiveTimeSpan1 == positiveTimeSpan2);
		}

		///<summary>
		/// Compares to PositiveTimeSpans
		///</summary>
		///<param name="positiveTimeSpan1"> PostiveTimeSpan 1 </param>
		///<param name="positiveTimeSpan2"> PostiveTimeSpan 2 </param>
		///<returns></returns>
		public static bool operator >(PositiveTimeSpan positiveTimeSpan1, PositiveTimeSpan positiveTimeSpan2)
		{
			return positiveTimeSpan1.TimeSpan > positiveTimeSpan2.TimeSpan;
		}

		///<summary>
		/// Compares to PositiveTimeSpans
		///</summary>
		///<param name="positiveTimeSpan1"> PostiveTimeSpan 1 </param>
		///<param name="positiveTimeSpan2"> PostiveTimeSpan 2 </param>
		///<returns></returns>
		public static bool operator <(PositiveTimeSpan positiveTimeSpan1, PositiveTimeSpan positiveTimeSpan2)
		{
			return positiveTimeSpan1.TimeSpan < positiveTimeSpan2.TimeSpan;
		}

		///<summary>
		/// Compares to PositiveTimeSpans
		///</summary>
		///<param name="positiveTimeSpan1"> PostiveTimeSpan 1 </param>
		///<param name="positiveTimeSpan2"> PostiveTimeSpan 2 </param>
		///<returns></returns>
		public static bool operator >=(PositiveTimeSpan positiveTimeSpan1, PositiveTimeSpan positiveTimeSpan2)
		{
			return positiveTimeSpan1.TimeSpan >= positiveTimeSpan2.TimeSpan;
		}

		///<summary>
		/// Compares to PositiveTimeSpans
		///</summary>
		///<param name="positiveTimeSpan1"> PostiveTimeSpan 1 </param>
		///<param name="positiveTimeSpan2"> PostiveTimeSpan 2 </param>
		///<returns></returns>
		public static bool operator <=(PositiveTimeSpan positiveTimeSpan1, PositiveTimeSpan positiveTimeSpan2)
		{
			return positiveTimeSpan1.TimeSpan <= positiveTimeSpan2.TimeSpan;
		}

		///<summary>
		/// Compares to PositiveTimeSpans
		///</summary>
		///<param name="other"> The PostiveTimeSpan to compare with </param>
		///<returns></returns>
		public bool Equals(PositiveTimeSpan other)
		{
			return other.TimeSpan.Equals(TimeSpan);
		}

		///<summary>
		/// Compares two PositiveTimeSpans
		///</summary>
		///<param name="positiveTimeSpan1"> The first PostitiveTimeSpan </param>
		///<param name="positiveTimeSpan2"> The second PostitiveTimeSpan </param>
		///<returns>
		/// -1 postitiveTimeSpan1 is less postitiveTimeSpan2
		/// 1 postitiveTimeSpan1 is greater than postitiveTimeSpan2
		/// 0 postitiveTimeSpan1 is equal to postitiveTimeSpan2
		///  </returns>
		public static int Compare(PositiveTimeSpan positiveTimeSpan1, PositiveTimeSpan positiveTimeSpan2)
		{
			if (positiveTimeSpan1 < positiveTimeSpan2) return -1;
			if (positiveTimeSpan1 > positiveTimeSpan2) return 1;
			return 0;
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		/// <param name="obj">Another object to compare to. 
		///                 </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (obj.GetType() != typeof (PositiveTimeSpan)) return false;
			return Equals((PositiveTimeSpan) obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode()
		{
			return _timeSpan.GetHashCode();
		}
	}
}
