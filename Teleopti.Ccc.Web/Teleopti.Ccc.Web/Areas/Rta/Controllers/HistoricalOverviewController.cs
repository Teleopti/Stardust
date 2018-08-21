using System;
using System.Collections.Generic;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	//[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
	public class HistoricalOverviewController : ApiController
	{
		[HttpGet, Route("api/HistoricalOverview/Load")]
		public virtual IHttpActionResult Load([FromUri] IEnumerable<Guid> siteIds = null, [FromUri] IEnumerable<Guid> teamIds = null)
		{
			var stuff =  @"[
				{
					Name: 'Denver/Avalanche',
					Agents: [{
						Id: '1234',
						Name: 'Andeen Ashley',
						IntervalAdherence: 73,

						Days: [
							{
								Date: '20180801',
								DisplayDate: '23/12',
								Adherence: 100,
								WasLateForWork: true
							},
							{
								Date: '20180802',
								DisplayDate: '24/12',
								Adherence: 90
							},
							{
								Date: '20180803',
								DisplayDate: '25/12',
								Adherence: 85
							},
							{
								Date: '20180804',
								DisplayDate: '26/12',
								Adherence: 88
							},
							{
								Date: '20180805',
								DisplayDate: '27/12',
								Adherence: 30,
								WasLateForWork: true
							},
							{
								Date: '20180806',
								DisplayDate: '28/12',
								Adherence: 70
							},
							{
								Date: '20180807',
								DisplayDate: '29/12',
								Adherence: 72
							}
						],
						LateForWork:
							{
								Count: 2,
								TotalMinutes: 24
							}
					},
						{
							Id: '1234',
							Name: 'Aneedn Anna',
							IntervalAdherence: 77,
							Days: [
								{
									Date: '20180801',
									DisplayDate: '1/8',
									Adherence: 70,
								},
								{
									Date: '20180802',
									DisplayDate: '1/8',
									Adherence: 56,
									WasLateForWork: true
								},
								{
									Date: '20180803',
									DisplayDate: '1/8',
									Adherence: 83
								},
								{
									Date: '20180804',
									DisplayDate: '1/8',
									Adherence: 71
								},
								{
									Date: '20180805',
									DisplayDate: '1/8',
									Adherence: 95
								},
								{
									Date: '20180806',
									DisplayDate: '1/8',
									Adherence: 77
								},
								{
									Date: '20180807',
									DisplayDate: '1/8',
									Adherence: 84
								}
							],
							LateForWork:
								{
									Count: 1,
									TotalMinutes: 10
								}
						},
						{
							Id: '1234',
							Name: 'Aleed Jane',
							IntervalAdherence: 75,
							Days: [
								{
									Date: '20180801',
									DisplayDate: '1/8',
									Adherence: 83,
								},
								{
									Date: '20180802',
									DisplayDate: '1/8',
									Adherence: 95,
									WasLateForWork: true
								},
								{
									Date: '20180803',
									DisplayDate: '1/8',
									Adherence: 78,
								},
								{
									Date: '20180804',
									DisplayDate: '1/8',
									Adherence: 78,
								},
								{
									Date: '20180805',
									DisplayDate: '1/8',
									Adherence: 98,
									WasLateForWork: true
								},
								{
									Date: '20180806',
									DisplayDate: '1/8',
									Adherence: 95,
									WasLateForWork: true
								},
								{
									Date: '20180807',
									DisplayDate: '1/8',
									Adherence: 85,
								}
							],
							LateForWork:
								{
									Count: 3,
									TotalMinutes: 42
								}
						}

					]
				},
				{
					Id: '1234',
					Name: 'Barcelona/Red',
					Agents: [{
						Name: 'Cndeen Ashley',
						IntervalAdherence: 94,
						Days: [
							{
								Date: '20180801',
								DisplayDate: '1/8',
								Adherence: 92,
								WasLateForWork: true
							},
							{
								Date: '20180802',
								DisplayDate: '1/8',
								Adherence: 97,
							},
							{
								Date: '20180803',
								DisplayDate: '1/8',
								Adherence: 94,
							},
							{
								Date: '20180804',
								DisplayDate: '1/8',
								Adherence: 98,
							},
							{
								Date: '20180806',
								DisplayDate: '1/8',
								Adherence: 99,
							},
							{
								Date: '20180807',
								DisplayDate: '1/8',
								Adherence: 94,
							},
							{
								Date: '20180808',
								DisplayDate: '1/8',
								Adherence: 99,
							}
						],
						LateForWork:
							{
								Count: 1,
								TotalMinutes: 3
							}
					}
					]
				}
			]";
			
			return Ok(JsonConvert.DeserializeObject<dynamic>(stuff));
		}
		
	}
}