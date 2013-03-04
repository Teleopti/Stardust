using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping
{
	public interface ICreateHourText
	{
		string CreateText(DateTime time);
	}
}