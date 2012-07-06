using System;
using AutoMapper;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class AbsenceRequestFormMappingProfile : Profile
	{
		private readonly Func<ITypeConverter<AbsenceRequestForm, IPersonRequest>> _absenceRequestFormToPersonRequest;

		public AbsenceRequestFormMappingProfile(Func<ITypeConverter<AbsenceRequestForm, IPersonRequest>> absenceRequestFormToPersonRequest)
		{
			_absenceRequestFormToPersonRequest = absenceRequestFormToPersonRequest;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<AbsenceRequestForm, IPersonRequest>()
				.ConvertUsing(_absenceRequestFormToPersonRequest.Invoke());
		}

		public class AbsenceRequestFormToPersonRequest : ITypeConverter<AbsenceRequestForm, IPersonRequest>
		{
			private readonly Func<IMappingEngine> _mapper;
			private readonly Func<ILoggedOnUser> _loggedOnUser;
			private readonly Func<IAbsenceRepository> _absenceRepository;
			private readonly Func<IUserTimeZone> _userTimeZone;

			public AbsenceRequestFormToPersonRequest(Func<IMappingEngine> mapper, Func<ILoggedOnUser> loggedOnUser, Func<IAbsenceRepository> absenceRepository, Func<IUserTimeZone> userTimeZone)
			{
				_mapper = mapper;
				_loggedOnUser = loggedOnUser;
				_absenceRepository = absenceRepository;
				_userTimeZone = userTimeZone;
			}

			public IPersonRequest Convert(ResolutionContext context)
			{
				var source = context.SourceValue as AbsenceRequestForm;
				var destination = context.DestinationValue as IPersonRequest;

				if (destination == null)
				{
					destination = new PersonRequest(_loggedOnUser.Invoke().CurrentUser()) { Subject = source.Subject };
				}

				DateTimePeriod period;

				if (source.FullDay)
				{
					var startTime = TimeZoneHelper.ConvertToUtc(new DateTime(2012, 5, 11, 0, 0, 0), _userTimeZone.Invoke().TimeZone());
					var endTime = TimeZoneHelper.ConvertToUtc(new DateTime(2012, 5, 11, 23, 59, 0), _userTimeZone.Invoke().TimeZone());
					period = new DateTimePeriod(startTime, endTime);
				}
				else
				{
					period = _mapper.Invoke().Map<DateTimePeriodForm, DateTimePeriod>(source.Period);
				}

				destination.Request = new AbsenceRequest(_absenceRepository.Invoke().Load(source.AbsenceId), period);

				destination.TrySetMessage(source.Message ?? "");

				if (source.EntityId != null)
					destination.SetId(source.EntityId);

				destination.Subject = source.Subject;

				return destination;
			}
		}
	}
}