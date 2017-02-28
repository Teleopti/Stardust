using System;

namespace Teleopti.Interfaces.Infrastructure
{
	/// <summary>
	/// A marker attribute telling our builds that the methods/types
	/// this attribute is applied to shouldn't be included in reports
	/// showing non used code/only used in tests.
	/// Can be applied to base types and/or interfaces as well. If implemented
	/// that way, all derived classes is considered "IsNotDeadCode" as well.
	/// </summary>
    public sealed class IsNotDeadCodeAttribute : Attribute
	{
		public IsNotDeadCodeAttribute(string descriptionWhyThisShouldBeKept)
		{
			
		}
	}
}