using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping
{
	public class StudentAvailabilityAndScheduleDayViewModelMappingProfile : Profile
	{
		private readonly IProjectionProvider _projectionProvider;		

		public StudentAvailabilityAndScheduleDayViewModelMappingProfile(IProjectionProvider projectionProvider)
		{
			_projectionProvider = projectionProvider;
		}		
		
		protected override void Configure()
		{
			CreateMap<IScheduleDay, StudentAvailabilityAndScheduleDayViewModel>()
				.ForMember(d => d.Date, o => o.MapFrom(s => s.DateOnlyAsPeriod.DateOnly.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.StudentAvailability, o => o.MapFrom(
					s => s.PersonRestrictionCollection() == null ? null
							 : s.PersonRestrictionCollection().OfType<IStudentAvailabilityDay>().FirstOrDefault()))
				.ForMember(d => d.ContractTimeMinutes, o => o.MapFrom(s => _projectionProvider.Projection(s).ContractTime().TotalMinutes))
				;			
		}
	}
}