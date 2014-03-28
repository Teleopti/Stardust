using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Util;
using Microsoft.IdentityModel.Protocols.WSFederation;

namespace Teleopti.Ccc.Web.Core
{
	public class AllowTokenPostRequestValidator : RequestValidator
	{
		protected override bool IsValidRequestString(HttpContext context, string value,
		                                             RequestValidationSource requestValidationSource,
		                                             string collectionKey, out int validationFailureIndex)
		{
			validationFailureIndex = 0;

			if (requestValidationSource == RequestValidationSource.Form &&
			    collectionKey.Equals(WSFederationConstants.Parameters.Result, StringComparison.Ordinal))
			{
				var baseUrl = WSFederationMessage.GetBaseUrl(context.Request.Url);
				Func<NameValueCollection> formGetter, queryStringGetter;
				Microsoft.Web.Infrastructure.DynamicValidationHelper.ValidationUtility.GetUnvalidatedCollections(context, out formGetter, out queryStringGetter);
				if (WSFederationMessage.CreateFromNameValueCollection(baseUrl, formGetter()) is SignInResponseMessage)
					return true;
			}
			return base.IsValidRequestString(context, value, requestValidationSource, collectionKey,
			                                 out validationFailureIndex);
		}
	}
}