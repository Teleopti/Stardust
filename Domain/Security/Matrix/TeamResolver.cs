using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Matrix
{
	public interface ITeamResolver
	{
		HashSet<MatrixPermissionHolder> ResolveTeams(IApplicationRole role, DateOnly queryDate);
	}

	public class TeamResolver : ITeamResolver
	{
		private readonly IPerson _person;
		private readonly IEnumerable<ISite> _sites;
		private HashSet<MatrixPermissionHolder> _result;

		public TeamResolver(IPerson person, IEnumerable<ISite> sites)
		{
			_person = person;
			_sites = sites;
		}

		private void add(MatrixPermissionHolder holder)
		{
			if (holder.Team.BusinessUnitExplicit.Id.Value == ServiceLocator_DONTUSE.CurrentBusinessUnit.Current().Id.GetValueOrDefault())
				_result.Add(holder);
		}

		public HashSet<MatrixPermissionHolder> ResolveTeams(IApplicationRole role, DateOnly queryDate)
		{
			_result = new HashSet<MatrixPermissionHolder>();

			IAvailableData availableData = role.AvailableData;

			if (availableData == null)
				return _result;

			// Add other teams than myOwn
			foreach (ITeam team in role.AvailableData.AvailableTeams)
			{
				add(new MatrixPermissionHolder(_person, team, false));
			}

			ITeam myTeam = _person.MyTeam(queryDate);

			if (myTeam == null
				&& (availableData.AvailableDataRange == AvailableDataRangeOption.MyTeam || availableData.AvailableDataRange == AvailableDataRangeOption.MySite))
				return _result;

			switch (availableData.AvailableDataRange)
			{
				case AvailableDataRangeOption.None:
					return _result;
				case AvailableDataRangeOption.MyOwn:
					addMyOwnPermissions(myTeam);
					return _result;
				case AvailableDataRangeOption.MyTeam:
					add(new MatrixPermissionHolder(_person, myTeam, false));
					break;
				case AvailableDataRangeOption.MySite:
					ISite mySite = myTeam.Site;
					foreach (ITeam team in mySite.TeamCollection)
					{
						add(new MatrixPermissionHolder(_person, team, false));
					}
					break;
				case AvailableDataRangeOption.MyBusinessUnit:
				case AvailableDataRangeOption.Everyone:
					foreach (ISite site in _sites)
					{
						foreach (ITeam team in site.TeamCollection)
						{
							add(new MatrixPermissionHolder(_person, team, false));
						}
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(role), "Unknown enumerator in AvailableData");
			}

			addMyOwnPermissions(myTeam);

			return _result;
		}

		private void addMyOwnPermissions(ITeam myTeam)
		{
			if (myTeam != null)
				add(new MatrixPermissionHolder(_person, myTeam, true));
		}
	}
}