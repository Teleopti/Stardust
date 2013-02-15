using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface ICreateHourText
	{
		string CreateText(DateTime time);
	}
}