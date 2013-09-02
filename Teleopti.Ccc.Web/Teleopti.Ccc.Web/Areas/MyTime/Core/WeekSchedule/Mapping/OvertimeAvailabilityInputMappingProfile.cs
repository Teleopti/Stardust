using System;
using AutoMapper;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class OvertimeAvailabilityInputMappingProfile : Profile
	{
		public OvertimeAvailabilityInputMappingProfile()
		{
			
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<OvertimeAvailabilityInput, IOvertimeAvailability>()
				.ConvertUsing<OvertimeAvailabilityInputToOvertimeAvailability>();
		}

		public class OvertimeAvailabilityInputToOvertimeAvailability : ITypeConverter<OvertimeAvailabilityInput, IOvertimeAvailability>
		{
			private readonly Func<ILoggedOnUser> _loggedOnUser;
			private readonly Func<IMappingEngine> _mapper;

			public OvertimeAvailabilityInputToOvertimeAvailability(Func<IMappingEngine> mapper, Func<ILoggedOnUser> loggedOnUser)
			{
				_loggedOnUser = loggedOnUser;
				_mapper = mapper;
			}

			public IOvertimeAvailability Convert(ResolutionContext context)
			{
				var source = context.SourceValue as OvertimeAvailabilityInput;
				var destination = context.DestinationValue as IOvertimeAvailability;
				if (destination == null)
				{
					var person = _loggedOnUser.Invoke().CurrentUser();
					destination = new OvertimeAvailability(person, source.Date, source.StartTime.ToTimeSpan(), source.EndTime.ToTimeSpan());
				}
				else
				{
					//destination.
					//_mapper.Invoke().Map(source, destination.Restriction);
				}
				return destination;
			}
		}
	}
}