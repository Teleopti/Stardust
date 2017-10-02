using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Wfm.Administration.Models.Stardust;

namespace Teleopti.Wfm.AdministrationTest
{
	//need to set up IOC for the tests.. but need to push so I can move from laptop to desktop..
	public class AnotherTemporaryFakeStardustSender : IStardustSender
	{
		private readonly FakeStardustRepository _stardustRepository;

		public AnotherTemporaryFakeStardustSender(FakeStardustRepository stardustRepository)
		{
			_stardustRepository = stardustRepository;
		}
		public Guid Send(IEvent @event)
		{
			var job = new Job();
			_stardustRepository.Has(job);
			return job.JobId;
		}
	}
}