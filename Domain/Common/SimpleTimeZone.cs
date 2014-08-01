using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class SimpleTimeZone :  ISimpleTimeZone
	{
		private Int16 _id;
		private string _name;
		private int _distance;

		public Int16 Id
		{
			get { return _id; }
			set { _id = value; }
		}
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}
		public int Distance
		{
			get { return _distance; }
			set { _distance = value; }
		}
	}
}