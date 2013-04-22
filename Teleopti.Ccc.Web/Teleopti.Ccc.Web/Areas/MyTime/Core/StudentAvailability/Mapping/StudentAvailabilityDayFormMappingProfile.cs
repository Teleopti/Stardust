using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping
{
	public class StudentAvailabilityDayFormMappingProfile : Profile
	{
		protected override void Configure()
		{
			base.Configure();

			CreateMap<StudentAvailabilityDayInput, IStudentAvailabilityDay>()
				.ConvertUsing<StudentAvailabilityDayFormToStudentAvailabilityDay>()
				;

			CreateMap<StudentAvailabilityDayInput, IStudentAvailabilityRestriction>()
				.ConstructUsing((StudentAvailabilityDayInput s) => new StudentAvailabilityRestriction())
				.ForMember(d => d.StartTimeLimitation, o => o.ResolveUsing(s => new StartTimeLimitation(s.StartTime.Time, null)))
				.ForMember(d => d.EndTimeLimitation, o => o.ResolveUsing(s => new EndTimeLimitation(null,
																	 TimeHelper.ParseTimeSpanFromTimeOfDay(s.EndTime.Time, s.NextDay))))
				.ForMember(d => d.WorkTimeLimitation, o => o.Ignore())
				;
		}

		public class StudentAvailabilityDayFormToStudentAvailabilityDay : ITypeConverter<StudentAvailabilityDayInput, IStudentAvailabilityDay>
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
				var source = context.SourceValue as StudentAvailabilityDayInput;
				var destination = context.DestinationValue as IStudentAvailabilityDay;
				if (destination == null)
				{
					var person = _loggedOnUser.CurrentUser();
					var restriction = _mapper.Map<StudentAvailabilityDayInput, IStudentAvailabilityRestriction>(source);
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