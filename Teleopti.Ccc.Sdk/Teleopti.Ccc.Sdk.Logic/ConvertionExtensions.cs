using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Sdk.Logic
{
	public static class ConvertionExtensions
	{
		public static Teleopti.Interfaces.Domain.DetailLevel Convert(this DetailLevel value) =>
			value.ConvertEnumTo<Teleopti.Interfaces.Domain.DetailLevel>();

		public static DetailLevel Convert(this Teleopti.Interfaces.Domain.DetailLevel value) =>
			value.ConvertEnumTo<DetailLevel>();

		public static Teleopti.Interfaces.Domain.EmploymentType Convert(this EmploymentType value) =>
			value.ConvertEnumTo<Teleopti.Interfaces.Domain.EmploymentType>();

		public static T ConvertEnumTo<T>(this object value)
			where T : struct, IConvertible
		{
			var sourceType = value.GetType();
			if (!sourceType.IsEnum)
				throw new ArgumentException("Source type is not enum");
			if (!typeof(T).IsEnum)
				throw new ArgumentException("Destination type is not enum");
			return (T) Enum.Parse(typeof(T), value.ToString());
		}

		public static Teleopti.Interfaces.Domain.DateOnly Convert(this DateOnly value) =>
			new Teleopti.Interfaces.Domain.DateOnly(value.Date);
		public static DateOnly Convert(this Teleopti.Interfaces.Domain.DateOnly value) =>
			new DateOnly(value.Date);

		public static Teleopti.Interfaces.Domain.DateOnlyPeriod Convert(this DateOnlyPeriod value) =>
			new Teleopti.Interfaces.Domain.DateOnlyPeriod(value.StartDate.Convert(), value.EndDate.Convert());
		public static DateOnlyPeriod Convert(this Teleopti.Interfaces.Domain.DateOnlyPeriod value) =>
			new DateOnlyPeriod(value.StartDate.Convert(), value.EndDate.Convert());

		public static Teleopti.Interfaces.Domain.DateTimePeriod Convert(this DateTimePeriod value) =>
			new Teleopti.Interfaces.Domain.DateTimePeriod(value.StartDateTime, value.EndDateTime);
	}
}