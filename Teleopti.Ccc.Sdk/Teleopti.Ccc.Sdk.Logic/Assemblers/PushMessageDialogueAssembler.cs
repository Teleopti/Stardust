using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PushMessageDialogueAssembler : Assembler<IPushMessageDialogue, PushMessageDialogueDto>
    {
        public IAssembler<IPerson, PersonDto> PersonAssembler { get; set; }
        public IAssembler<IPushMessage, PushMessageDto> PushMessageAssembler { get; set; }
        public IAssembler<IDialogueMessage, DialogueMessageDto> DialogueMessageAssembler { get; set; }

        public override PushMessageDialogueDto DomainEntityToDto(IPushMessageDialogue entity)
        {
            PushMessageDialogueDto dto = new PushMessageDialogueDto();
            dto.Id = entity.Id;
            dto.PushMessage = PushMessageAssembler.DomainEntityToDto(entity.PushMessage);

            foreach (IDialogueMessage reply in entity.DialogueMessages)
            {
                dto.Messages.Add(DialogueMessageAssembler.DomainEntityToDto(reply));
            }
            dto.IsReplied = entity.IsReplied;
            dto.Receiver = PersonAssembler.DomainEntityToDto(entity.Receiver);
            TimeZoneInfo TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(dto.Receiver.TimeZoneId);
            int cultureId = CultureInfo.CurrentCulture.LCID;
            if (dto.Receiver.CultureLanguageId.HasValue)
                cultureId = dto.Receiver.CultureLanguageId.Value;
            dto.OriginalDate = "";
            CultureInfo c = new CultureInfo(cultureId);
            if (entity.CreatedOn != null)
                dto.OriginalDate = TimeZoneHelper.ConvertFromUtc((DateTime)entity.CreatedOn, TimeZoneInfo).ToString(c);
            dto.Message = entity.Message(new NormalizeText());

            return dto;
        }

        public override IPushMessageDialogue DtoToDomainEntity(PushMessageDialogueDto dto)
        {
            throw new NotSupportedException("This operation is not supported yet.");
        }
    }
}
