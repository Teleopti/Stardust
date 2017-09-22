using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PushMessageAssembler : Assembler<IPushMessage, PushMessageDto>
    {
        public IAssembler<IPerson, PersonDto> PersonAssembler { get; set; }

        public override PushMessageDto DomainEntityToDto(IPushMessage entity)
        {
        	var normalizeText = new NormalizeText();
			PushMessageDto dto = new PushMessageDto();
			dto.Title = entity.GetTitle(normalizeText);
            dto.Message = entity.GetMessage(normalizeText);
            dto.AllowDialogueReply = entity.AllowDialogueReply;
            dto.Sender = PersonAssembler.DomainEntityToDto(entity.Sender);
            dto.ReplyOptions.Clear();
            foreach (var replyOption in entity.ReplyOptions)
            {
                dto.ReplyOptions.Add(replyOption);
            }
            return dto;
        }

        public override IPushMessage DtoToDomainEntity(PushMessageDto dto)
        {
            throw new NotSupportedException("Not implemented yet.");
        }
    }
}
