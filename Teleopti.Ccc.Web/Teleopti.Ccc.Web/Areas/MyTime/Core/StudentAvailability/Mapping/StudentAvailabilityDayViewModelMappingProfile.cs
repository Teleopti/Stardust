using System;
using System.Globalization;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping
{
	public class StudentAvailabilityDayViewModelMappingProfile : Profile
	{
		private readonly Func<IStudentAvailabilityProvider> _studentAvailabilityProvider;

		public StudentAvailabilityDayViewModelMappingProfile(Func<IStudentAvailabilityProvider> studentAvailabilityProvider)
		{
			_studentAvailabilityProvider = studentAvailabilityProvider;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<IStudentAvailabilityDay, StudentAvailabilityDayViewModel>()
				.ForMember(d => d.Date, o => o.MapFrom(s => s.RestrictionDate.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.AvailableTimeSpan, o => o.ResolveUsing(s =>
				                                                    	{
																			var studentAvailabilityRestriction = _studentAvailabilityProvider().GetStudentAvailabilityForDay(s);
				                                                    		return studentAvailabilityRestriction == null ? string.Empty : string.Format(
				                                                    			CultureInfo.InvariantCulture,
				                                                    			"{0} - {1}", studentAvailabilityRestriction.StartTimeLimitation.StartTimeString,
				                                                    			studentAvailabilityRestriction.EndTimeLimitation.EndTimeString);
				                                                    	}))
				;
		}
	}
}