using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class OvertimeRequestFormMapper
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
		private readonly IUserTimeZone _userTimeZone;

		public OvertimeRequestFormMapper(ILoggedOnUser loggedOnUser,
			IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository,
			IUserTimeZone userTimeZone)
		{
			_loggedOnUser = loggedOnUser;
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
			_userTimeZone = userTimeZone;
		}

		public IPersonRequest Map(OvertimeRequestForm source)
		{
			var definitionSet = _multiplicatorDefinitionSetRepository.Get(source.MultiplicatorDefinitionSet);
			var period = new DateTimePeriodFormMapper(_userTimeZone).Map(source.Period);
			var overtimeRequest = new OvertimeRequest(definitionSet, period);

			var destination = new PersonRequest(_loggedOnUser.CurrentUser())
			{
				Subject = source.Subject,
				Request = overtimeRequest
			};

			destination.TrySetMessage(source.Message ?? "");

			return destination;
		}
	}
}