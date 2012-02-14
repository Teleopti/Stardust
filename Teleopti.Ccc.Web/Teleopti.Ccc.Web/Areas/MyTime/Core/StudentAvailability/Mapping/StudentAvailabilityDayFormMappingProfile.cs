using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping
{
	public class StudentAvailabilityDayFormMappingProfile : Profile
	{
		protected override void Configure()
		{
			base.Configure();

			CreateMap<StudentAvailabilityDayForm, IStudentAvailabilityDay>()
				.ConvertUsing<StudentAvailabilityDayFormToStudentAvailabilityDay>()
				;

			CreateMap<StudentAvailabilityDayForm, IStudentAvailabilityRestriction>()
				.ConstructUsing(s => new StudentAvailabilityRestriction())
				.ForMember(d => d.StartTimeLimitation, o => o.MapFrom(s => s))
				.ForMember(d => d.EndTimeLimitation, o => o.MapFrom(s => s))
				.ForMember(d => d.WorkTimeLimitation, o => o.Ignore())
				;

			CreateMap<StudentAvailabilityDayForm, StartTimeLimitation>()
				.ForMember(d => d.StartTime, o => o.MapFrom(s => s.StartTime.Time))
				.ForMember(d => d.EndTime, o => o.Ignore())
				.ForMember(d => d.StartTimeString, o => o.Ignore())
				.ForMember(d => d.EndTimeString, o => o.Ignore())
				;

			CreateMap<StudentAvailabilityDayForm, EndTimeLimitation>()
				.ForMember(d => d.StartTime, o => o.Ignore())
				.ForMember(d => d.EndTime, o => o.MapFrom(s => TimeHelper.ParseTimeSpanFromTimeOfDay(s.EndTime.Time, s.NextDay)))
				.ForMember(d => d.StartTimeString, o => o.Ignore())
				.ForMember(d => d.EndTimeString, o => o.Ignore())
				;

		}

		public class StudentAvailabilityDayFormToStudentAvailabilityDay : ITypeConverter<StudentAvailabilityDayForm, IStudentAvailabilityDay>
		{
			private readonly ILoggedOnUser _loggedOnUser;
			private readonly IMappingEngine _mapper;

			public StudentAvailabilityDayFormToStudentAvailabilityDay(ILoggedOnUser loggedOnUser, IMappingEngine mapper)
			{
				_loggedOnUser = loggedOnUser;
				_mapper = mapper;
			}

			public IStudentAvailabilityDay Convert(ResolutionContext context)
			{
				var source = context.SourceValue as StudentAvailabilityDayForm;
				var destination = context.DestinationValue as IStudentAvailabilityDay;
				if (destination == null)
				{
					var person = _loggedOnUser.CurrentUser();
					var restriction = _mapper.Map<StudentAvailabilityDayForm, IStudentAvailabilityRestriction>(source);
					destination = new StudentAvailabilityDay(person, source.Date, new List<IStudentAvailabilityRestriction> { restriction });
				}
				else
				{
					_mapper.Map(source, destination.RestrictionCollection.Single());
				}
				return destination;
			}
		}
	}
}