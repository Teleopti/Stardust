using AutoMapper;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.Mapping
{
    public class MessageViewModelMappingProfile : Profile
    {
        protected override void Configure()
        {

            base.Configure();

            CreateMap<IPushMessageDialogue, MessageViewModel>()
                .ForMember(d => d.Title, o => o.MapFrom(m => m.PushMessage.GetTitle(new NoFormatting())))
                .ForMember(d => d.Sender, o => o.MapFrom(m => m.PushMessage.Sender.Name.ToString()))
                .ForMember(d => d.Date, o => o.MapFrom(m => m.UpdatedOn.Value.ToShortDateTimeString()))
            ;
        }
    }
}