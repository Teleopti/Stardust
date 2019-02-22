using System.Collections.Generic;
using System.ServiceModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Sdk.Logic
{
	public static class ValidationExtensions
	{
		public static void VerifyCountLessThan<T>(this ICollection<T> items, int limit, string messageFormat)
		{
			if (items.Count > limit)
			{
				throw new FaultException(string.Format(System.Globalization.CultureInfo.InvariantCulture, messageFormat, items.Count));
			}
		}

		public static void VerifyCanModifyPeople(ICurrentAuthorization currentAuthorization)
		{
			if (!currentAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage))
			{
				throw new FaultException("You're not allowed to modify person details.");
			}
		}

		public static void VerifyCanBeModifiedByCurrentUser(this IPerson person, ICurrentAuthorization currentAuthorization)
		{
			person.VerifyCanBeModifiedByCurrentUser(DateOnly.Today, currentAuthorization);
		}

		public static void VerifyCanBeModifiedByCurrentUser(this IPerson person, DateOnly dateOnly, ICurrentAuthorization currentAuthorization)
		{
			if (!currentAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, dateOnly, person))
			{
				throw new FaultException("You're not allowed to modify person details.");
			}
		}

		public static void VerifyCanBeModifiedByCurrentUser(this IEnumerable<IPerson> people, DateOnly dateOnly, ICurrentAuthorization currentAuthorization)
		{
			var authorization = currentAuthorization.Current();
			foreach (var person in people)
			{
				if (!authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, dateOnly, person))
				{
					throw new FaultException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "You're not allowed to work with this person ({0}).", person.Name));
				}
			}
		}

		public static void ValidateBusinessUnitConsistency(this IFilterOnBusinessUnit belongsToBusinessUnit, ICurrentBusinessUnit currentBusinessUnit)
		{
			var businessUnit = currentBusinessUnit.CurrentId().GetValueOrDefault();
			if (belongsToBusinessUnit.BusinessUnit != null && businessUnit != belongsToBusinessUnit.BusinessUnit.Id.GetValueOrDefault())
			{
				throw new FaultException(
					$"Adding references to items from a different business unit than the currently specified in the header is not allowed. (Type: {belongsToBusinessUnit.GetType()}, Current business unit id: {currentBusinessUnit}, Attempted business unit id: {belongsToBusinessUnit.BusinessUnit.Id.GetValueOrDefault()})");
			}
		}
	}
}