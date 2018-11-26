using System;
using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Percent value object
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-11-01
    /// </remarks>
	 [Serializable]
    public struct Percent : IEquatable<Percent>, IComparable<Percent>
    {
        private readonly double _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Percent"/> struct.
        /// Value should be entered as decimal value; 10% = 0.1.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-01
        /// </remarks>
        public Percent(double value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-01
        /// </remarks>
        public double Value => _value;

		public double ValueAsPercent()
	    {
		    return Value*100;
	    }

        /// <summary>
        /// Returns the value in readable format
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> string.
        /// </returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-01
        /// </remarks>
        public override string ToString()
        {
            return _value.ToString("P0", CultureInfo.CurrentCulture);
        }

		public static Percent Zero
		{
			get { return new Percent(0); }
		}

		#region Equals and GetHashCode stuff
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-01
        /// </remarks>
        public static bool operator ==(Percent obj1, Percent obj2)
        {
            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-01
        /// </remarks>
        public static bool operator !=(Percent obj1, Percent obj2)
        {
            return !obj1.Equals(obj2);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-01
        /// </remarks>
        public override bool Equals(object obj)
        {
			if (obj is Percent percent) return Equals(percent);
			return false;
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
        public bool Equals(Percent other)
        {
            return (Value == other.Value);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-01
        /// </remarks>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        #endregion

        #region IComparable
        ///<summary>
        ///Compares the current object with another object of the same type.
        ///</summary>
        ///
        ///<returns>
        ///A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the other parameter.Zero This object is equal to other. Greater than zero This object is greater than other. 
        ///</returns>
        ///
        ///<param name="other">An object to compare with this object.</param>
        public int CompareTo(Percent other)
        {
            return Value.CompareTo(other.Value);
        }

        /// <summary>
        /// Implements the operator &gt;.
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-01
        /// </remarks>
        public static bool operator >(Percent obj1, Percent obj2)
        {
            return obj1.CompareTo(obj2) > 0;
        }
        /// <summary>
        /// Implements the operator &lt;.
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-01
        /// </remarks>
        public static bool operator <(Percent obj1, Percent obj2)
        {
            return !(obj1 > obj2);
        }
        
        #endregion

        /// <summary>
        /// Tries the parse.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-12
        /// </remarks>
        public static bool TryParse(string value, out Percent result, bool tryDifferentCultureForDoubleValue = false)
        {
            value = value.Replace(CultureInfo.CurrentCulture.NumberFormat.PercentSymbol, "");
            double valueAsDouble;			

			if (tryDifferentCultureForDoubleValue)
			{
				CultureInfo CultureInfo = new CultureInfo("en-US");
				var numberInfoFormat = (NumberFormatInfo)CultureInfo.NumberFormat.Clone();
				if (double.TryParse(value, NumberStyles.AllowDecimalPoint, numberInfoFormat, out valueAsDouble))
				{
					result = new Percent(valueAsDouble / 100d);
					return true;
				}

				numberInfoFormat.NumberDecimalSeparator = ",";
				if (double.TryParse(value, NumberStyles.AllowDecimalPoint, numberInfoFormat, out valueAsDouble))
				{
					result = new Percent(valueAsDouble / 100d);
					return true;
				}

			} else
			{
				if (double.TryParse(value, out valueAsDouble))
				{
					result = new Percent(valueAsDouble / 100d);
					return true;
				}
			}
			
            result = new Percent();
            return false;
        }

        /// <summary>
        /// Returns the value in readable format
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns>A <see cref="T:System.String"/> string.</returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-28
        /// </remarks>
        public string ToString(IFormatProvider provider)
        {
            return _value.ToString("P", provider);
        }
    }
}