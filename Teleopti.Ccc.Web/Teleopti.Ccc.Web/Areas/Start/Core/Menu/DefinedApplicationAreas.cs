using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Menu
{
	using System.Collections.Generic;

	using Domain.Security.AuthorizationData;
	using Models.Menu;

	public static class DefinedApplicationAreas
	{
		private static readonly List<ApplicationArea> ApplicationApplicationAreas = new List<ApplicationArea>
		                                                                            	{
		                                                                            		new ApplicationArea
		                                                                            			{
		                                                                            				ApplicationFunctionPath =
		                                                                            					DefinedRaptorApplicationFunctionPaths
		                                                                            					.MyTimeWeb,
		                                                                            				Area = "MyTime",
		                                                                            				Name = Resources.MyTime
		                                                                            			},
		                                                                            		new ApplicationArea
		                                                                            			{
		                                                                            				ApplicationFunctionPath =
		                                                                            					DefinedRaptorApplicationFunctionPaths
		                                                                            					.Anywhere,
		                                                                            				Area = "Anywhere",
		                                                                            				Name = GetMenuText(DefinedRaptorApplicationFunctionPaths.Anywhere)
		                                                                            			}
		                                                                            	};

		private static string GetMenuText(string applicationFunctionPath)
		{
			var factory = new DefinedRaptorApplicationFunctionFactory();
			var rawResourceKey = ApplicationFunction.FindByPath(factory.ApplicationFunctionList, applicationFunctionPath).FunctionDescription;

			return Resources.ResourceManager.GetString(rawResourceKey.Replace("xx", string.Empty));
		}

		public static IEnumerable<ApplicationArea> ApplicationAreas
		{
			get
			{
				return ApplicationApplicationAreas;
			}
		}
	}
}