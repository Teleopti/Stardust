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
		private readonly Func<ITypeConverter<TextRequestForm, IPersonRequest>> _textRequestFormToPersonRequest;

		public TextRequestFormMappingProfile(Func<ITypeConverter<TextRequestForm, IPersonRequest>> textRequestFormToPersonRequest)
		{
			_textRequestFormToPersonRequest = textRequestFormToPersonRequest;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<TextRequestForm, IPersonRequest>()
				.ConvertUsing(_textRequestFormToPersonRequest.Invoke());
		}

		public class TextRequestFormToPersonRequest : ITypeConverter<TextRequestForm, IPersonRequest>
		{
			private readonly Func<IMappingEngine> _mapper;
			private readonly Func<ILoggedOnUser> _loggedOnUser;
			private readonly Func<IUserTimeZone> _userTimeZone;

			public TextRequestFormToPersonRequest(Func<IMappingEngine> mapper, Func<ILoggedOnUser> loggedOnUser, Func<IUserTimeZone> userTimeZone)
			{
				_mapper = mapper;
				_loggedOnUser = loggedOnUser;
				_userTimeZone = userTimeZone;
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