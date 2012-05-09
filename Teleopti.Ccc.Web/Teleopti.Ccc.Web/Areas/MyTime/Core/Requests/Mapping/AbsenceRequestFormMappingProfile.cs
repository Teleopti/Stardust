using System;
using AutoMapper;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class AbsenceRequestFormMappingProfile : Profile
	{
		private readonly Func<IMappingEngine> _mapper;
		private readonly Func<ILoggedOnUser> _loggedOnUser;
		private readonly Func<IUserTimeZone> _userTimeZone;
		private readonly Func<ITypeConverter<AbsenceRequestForm, IPersonRequest>> _absenceRequestFormToPersonRequest;

		public AbsenceRequestFormMappingProfile(Func<IMappingEngine> mapper, Func<ILoggedOnUser> loggedOnUser, Func<IUserTimeZone> userTimeZone, Func<ITypeConverter<AbsenceRequestForm, IPersonRequest>> absenceRequestFormToPersonRequest)
		{
			_mapper = mapper;
			_loggedOnUser = loggedOnUser;
			_userTimeZone = userTimeZone;
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

			public AbsenceRequestFormToPersonRequest(Func<IMappingEngine> mapper, Func<ILoggedOnUser> loggedOnUser)
			{
				_mapper = mapper;
				_loggedOnUser = loggedOnUser;
			}

			public IPersonRequest Convert(ResolutionContext context)
			{
				var source = context.SourceValue as TextRequestForm;
				var destination = context.DestinationValue as IPersonRequest;

				if (destination == null)
				{
					destination = new PersonRequest(_loggedOnUser.Invoke().CurrentUser());
				}

				return destination;
			}
		}
	}
}