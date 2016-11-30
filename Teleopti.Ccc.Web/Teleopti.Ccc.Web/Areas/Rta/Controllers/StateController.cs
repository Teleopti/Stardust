using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Castle.Core.Internal;
using Common.Logging;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
    public class RtaToolController : ApiController
    {
        private readonly IAgentStatePersister _persister;
        private readonly IPersonRepository _persons;
        private readonly IRtaStateGroupRepository _stateGroups;

        public RtaToolController(IAgentStatePersister persister, IPersonRepository persons, IRtaStateGroupRepository stateGroups)
        {
            _persister = persister;
            _persons = persons;
            _stateGroups = stateGroups;
        }

        [UnitOfWork, HttpGet, Route("RtaTool/Agents/For")]
        public virtual IHttpActionResult GetAgents()
        {
            var external = _persister.FindAll();
            var persons = _persons.FindPeople(external.Select(x => x.PersonId));
            return Ok((from e in external
                from p in persons
                where p.Id == e.PersonId
                select new
                {
                    Name = p.Name.FirstName + " " + p.Name.LastName,
                    e.UserCode
                }).ToArray());
        }

        [UnitOfWork, HttpGet, Route("RtaTool/PhoneStates/For")]
        public virtual IHttpActionResult GetPhoneStates()
        {
            return Ok(_stateGroups.LoadAllCompleteGraph()
                .Where(x => !x.StateCollection.IsNullOrEmpty())
                .Select(x => new
                {
                    Name = x.Name,
                    State = x.StateCollection.First().StateCode
                }).ToArray());
        }
    }


	public class StateController : ApiController
	{
		private readonly Domain.ApplicationLayer.Rta.Service.Rta _rta;

		public StateController(Domain.ApplicationLayer.Rta.Service.Rta rta)
		{
			_rta = rta;
		}

		[HttpPost, Route("Rta/State/Change")]
		public IHttpActionResult Change([FromBody]ExternalUserStateWebModel input)
		{
			try
			{
                _rta.SaveState(
					new StateInputModel
					{
						AuthenticationKey = input.AuthenticationKey,
						PlatformTypeId = input.PlatformTypeId,
						SourceId = input.SourceId,
						UserCode = input.UserCode,
						StateCode = input.StateCode,
						StateDescription = input.StateDescription,
						SnapshotId = parseSnapshotId(input.SnapshotId),
					});
			}
			catch (InvalidAuthenticationKeyException e)
			{
				return BadRequest(e.Message);
			}
			catch (LegacyAuthenticationKeyException e)
			{
				return BadRequest(e.Message);
			}
			catch (InvalidSourceException e)
			{
				return BadRequest(e.Message);
			}
			catch (InvalidPlatformException e)
			{
				return BadRequest(e.Message);
			}
			catch (InvalidUserCodeException e)
			{
				return BadRequest(e.Message);
			}

			return Ok();
		}

		[HttpPost, Route("Rta/State/Batch")]
		public IHttpActionResult Batch([FromBody]IEnumerable<ExternalUserStateWebModel> input)
		{

			try
			{
				var states = input.Select(i => new BatchStateInputModel
				{
					UserCode = i.UserCode,
					StateCode = i.StateCode,
					StateDescription = i.StateDescription,
				}).ToArray();

				var root = input.First();

				_rta.SaveStateBatch(new BatchInputModel
				{
					AuthenticationKey = root.AuthenticationKey,
					PlatformTypeId = root.PlatformTypeId,
					SourceId = root.SourceId,
					SnapshotId = parseSnapshotId(root.SnapshotId),
					States = states
				});

			}
			catch (InvalidAuthenticationKeyException e)
			{
				return BadRequest(e.Message);
			}
			catch (LegacyAuthenticationKeyException e)
			{
				return BadRequest(e.Message);
			}
			catch (InvalidSourceException e)
			{
				return BadRequest(e.Message);
			}
			catch (InvalidPlatformException e)
			{
				return BadRequest(e.Message);
			}
			catch (InvalidUserCodeException e)
			{
				return BadRequest(e.Message);
			}

			return Ok();
		}

		private static DateTime? parseSnapshotId(string snapshotId)
		{
			DateTime parsed;
			DateTime.TryParse(snapshotId, out parsed);
			return parsed == DateTime.MinValue ? (DateTime?)null : parsed;
		}

	}
}
