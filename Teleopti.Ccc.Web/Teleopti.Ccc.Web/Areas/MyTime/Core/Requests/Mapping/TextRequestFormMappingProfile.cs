using System;
using AutoMapper;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class TextRequestFormMappingProfile : Profile
	{
		private readonly Func<IMappingEngine> _mapper;
		private readonly Func<ILoggedOnUser> _loggedOnUser;
		private readonly Func<IUserTimeZone> _userTimeZone;
		private readonly Func<ITypeConverter<TextRequestForm, IPersonRequest>> _textRequestFormToPersonRequest;

		public TextRequestFormMappingProfile(Func<IMappingEngine> mapper, Func<ILoggedOnUser> loggedOnUser, Func<IUserTimeZone> userTimeZone, Func<ITypeConverter<TextRequestForm, IPersonRequest>> textRequestFormToPersonRequest)
		{
			_mapper = mapper;
			_loggedOnUser = loggedOnUser;
			_userTimeZone = userTimeZone;
			_textRequestFormToPersonRequest = textRequestFormToPersonRequest;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<TextRequestForm, IPersonRequest>()
				.ConvertUsing(_textRequestFormToPersonRequest.Invoke());

			CreateMap<DateTimePeriodForm, DateTimePeriod>()
				.ConvertUsing(s =>
				              	{
									if (s == null)
										return new DateTimePeriod();
									var fromTime = s.StartDate.Date.Add(s.StartTime.Time);
									var toTime = s.EndDate.Date.Add(s.EndTime.Time);
									var fromTimeUtc = TimeZoneHelper.ConvertToUtc(fromTime, _userTimeZone.Invoke().TimeZone());
									var toTimeUtc = TimeZoneHelper.ConvertToUtc(toTime, _userTimeZone.Invoke().TimeZone());
									return new DateTimePeriod(fromTimeUtc, toTimeUtc);
								});
		}

		public class TextRequestFormToPersonRequest : ITypeConverter<TextRequestForm, IPersonRequest>
		{
			private readonly Func<IMappingEngine> _mapper;
			private readonly Func<ILoggedOnUser> _loggedOnUser;

			public TextRequestFormToPersonRequest(Func<IMappingEngine> mapper, Func<ILoggedOnUser> loggedOnUser)
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
					var person = _loggedOnUser.Invoke().CurrentUser();
					destination = new PersonRequest(person);
					destination.Pending();
				}


				var period = _mapper.Invoke().Map<DateTimePeriodForm, DateTimePeriod>(source.Period);
				var textRequest = new TextRequest(period);
				destination.Request = textRequest;

				destination.TrySetMessage(source.Message ?? "");

				if (source.EntityId != null)
					destination.SetId(source.EntityId);

				destination.Subject = source.Subject;

				return destination;
			}
		}
	}
}