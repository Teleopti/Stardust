using System;
using System.Collections.Specialized;
using System.IdentityModel.Services;
using System.Web;
using System.Web.Util;
using Teleopti.Ccc.Web.Auth;

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
				var baseUrl = WSFederationMessage.GetBaseUrl(context.Request.UrlConsideringLoadBalancerHeaders());
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