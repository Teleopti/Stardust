using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Model
{
	public class PersonInfoGenericResultModel
	{
		public PersonInfoGenericResultModel()
		{
			ResultList = new List<PersonInfoGenericModel>();
		}
		public List<PersonInfoGenericModel> ResultList { get; set; }
	}

	public class PersonInfoGenericModel
	{
		public Guid PersonId { get; set; }
		public string MessageId { get; set; }
		
	}
}