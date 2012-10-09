using System;
using AutoMapper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider
{
	public class PushMessageDialoguePersister : IPushMessageDialoguePersister
	{
		private readonly IPushMessageDialogueRepository _pushMessageDialogueRepository;
		private readonly IMappingEngine _mapper;

		public PushMessageDialoguePersister(IPushMessageDialogueRepository pushMessageDialogueRepository, IMappingEngine mapper)
		{
			_pushMessageDialogueRepository = pushMessageDialogueRepository;
			_mapper = mapper;
		}

		public MessageViewModel Persist(string messageId)
		{
			var pushMessageDialogue = _pushMessageDialogueRepository.Get(new Guid(messageId));

			return _mapper.Map<IPushMessageDialogue, MessageViewModel>(pushMessageDialogue);
		}
	}
}