using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
    public class AdherenceController : Controller
    {
	    private readonly IAdherencePercentageReadModelPersister _adherencePercentageReadModelPersister;
	    private readonly INow _now;
	    private readonly IHistoricalAdherence _historicalAdherence = new HistoricalAdherence();

	    public AdherenceController(IAdherencePercentageReadModelPersister adherencePercentageReadModelPersister, INow now)
	    {
		    _adherencePercentageReadModelPersister = adherencePercentageReadModelPersister;
		    _now = now;
	    }

	    [UnitOfWorkAction, HttpGet]
		public JsonResult ForToday(Guid personId)
	    {
		    var readModel = _adherencePercentageReadModelPersister.Get(new DateOnly(_now.UtcDateTime()), personId);

		    if (readModel == null)
		    {
			    return Json(new object(), JsonRequestBehavior.AllowGet);
		    }

			var ret = new AdherenceInfo
			{
						  MinutesInAdherence = readModel.MinutesInAdherence,
						  MinutesOutOfAdherence = readModel.MinutesOutOfAdherence,
						  LastTimestamp = readModel.LastTimestamp,
						  AdherencePercent = _historicalAdherence.ForDay(readModel).ValueAsPercent()
					  };

			return Json(ret, JsonRequestBehavior.AllowGet);
        }

    }

	public class AdherenceInfo
	{
		public int MinutesInAdherence { get; set; }
		public int MinutesOutOfAdherence { get; set; }
		public DateTime LastTimestamp { get; set; }
		public double AdherencePercent { get; set; }
	}
}
