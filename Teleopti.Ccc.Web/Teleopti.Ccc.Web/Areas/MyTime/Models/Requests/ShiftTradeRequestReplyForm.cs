using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
    public class ShiftTradeRequestReplyForm
    {
		 public Guid ID { get; set; }

		 [StringLength(2000, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MessageTooLong")]
		 public string Message { get; set; }

    }
}
