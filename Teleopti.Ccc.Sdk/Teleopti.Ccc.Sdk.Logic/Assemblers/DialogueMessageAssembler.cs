using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class DialogueMessageAssembler : Assembler<IDialogueMessage,DialogueMessageDto>
    {
        public IAssembler<IPerson, PersonDto> PersonAssembler { get; set; }

        public override DialogueMessageDto DomainEntityToDto(IDialogueMessage entity)
        {
            DialogueMessageDto dialogueMessageDto = new DialogueMessageDto();
            dialogueMessageDto.Text = entity.Text;
            SetCreatedOn(dialogueMessageDto, entity);
            dialogueMessageDto.Sender = PersonAssembler.DomainEntityToDto(entity.Sender);

            return dialogueMessageDto;
        }

        public override IDialogueMessage DtoToDomainEntity(DialogueMessageDto dto)
        {
            throw new NotSupportedException("This operation is not supported yet.");
        }

        private static void SetCreatedOn(DialogueMessageDto dto, IDialogueMessage reply)
        {
            //Todo: Fix so the client user gets the corect time according to the time zone, the commented lines will fix this, but 
            //we dont have the time to fix it and test it correctly now, 2009-06-01
            //var pushMessageDialogue = (PushMessageDialogue)((DialogueMessage)reply).Parent;
            ////Reciver...
            //IPerson receiver = pushMessageDialogue.Receiver;
            //TimeZoneInfo TimeZoneInfo = receiver.PermissionInformation.DefaultTimeZone();
            //CreatedOn = TimeZoneHelper.ConvertFromUtc(reply.Created, TimeZoneInfo).ToShortDateString();
            dto.CreatedOn = reply.Created.ToString(CultureInfo.CurrentCulture);
        }
    }
}
