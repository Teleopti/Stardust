﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.Data
{
	public class MultiplicatorDefinitionSetProvider : IMultiplicatorDefinitionSetProvider
	{
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly INow _now;

		public MultiplicatorDefinitionSetProvider(IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository, ILoggedOnUser loggedOnUser, INow now)
		{
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
			_loggedOnUser = loggedOnUser;
			_now = now;
		}

		public IList<MultiplicatorDefinitionSetViewModel> GetAllOvertimeDefinitionSets()
		{
			var multilicatorDefinitionSet = _multiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions();
			return convertToViewModel(multilicatorDefinitionSet);
		}

		public IList<MultiplicatorDefinitionSetViewModel> GetDefinitionSets(IPerson person, DateOnly date)
		{

			var multiplicatorDefinitionSets = person.Period(date)?.PersonContract?.Contract?.MultiplicatorDefinitionSetCollection;
			return multiplicatorDefinitionSets != null
				? convertToViewModel(multiplicatorDefinitionSets)
				: new List<MultiplicatorDefinitionSetViewModel>();
		}
		public IList<MultiplicatorDefinitionSetViewModel> GetDefinitionSetsForCurrentUser()
		{
			var timezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var today = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), timezone));
			return GetDefinitionSets(_loggedOnUser.CurrentUser(), today);
		}

		private IList<MultiplicatorDefinitionSetViewModel> convertToViewModel(
			IList<IMultiplicatorDefinitionSet> multilicatorDefinitionSet)
		{
			return multilicatorDefinitionSet.Select(s => new MultiplicatorDefinitionSetViewModel
			{
				Id = s.Id.GetValueOrDefault(),
				Name = s.Name
			}).ToList();
		}
	}
}