using System;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping
{
	public class StudentAvailabilityDayFormResultMappingProfile : Profile
	{
		private readonly Func<IStudentAvailabilityProvider> _studentAvailabilityProvider;

		public StudentAvailabilityDayFormResultMappingProfile(Func<IStudentAvailabilityProvider> studentAvailabilityProvider)
		{
			_studentAvailabilityProvider = studentAvailabilityProvider;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<IStudentAvailabilityDay, StudentAvailabilityDayFormResult>()
				.ForMember(d => d.Date, o => o.MapFrom(s => s.RestrictionDate))
				.ForMember(d => d.AvailableTimeSpan, o => o.MapFrom(s =>
				                                                    	{
																			var studentAvailabilityRestriction = _studentAvailabilityProvider().GetStudentAvailabilityForDay(s);
				                                                    		return studentAvailabilityRestriction.FormatLimitationTimes();
				                                                    	}))
				;

			CreateMap<DateOnly, string>()
				.ConvertUsing(s => s.ToFixedClientDateOnlyFormat())
				;
		}
	}
}