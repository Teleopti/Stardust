using System;
using AutoMapper;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.Mapping
{
    public class MessageViewModelMappingProfile : Profile
    {
		private readonly Func<IUserTimeZone> _userTimeZone;

	    public MessageViewModelMappingProfile(Func<IUserTimeZone> timeZone)
		{
			_userTimeZone = timeZone;
		}

	    protected override void Configure()
        {

            base.Configure();

    		CreateMap<IDialogueMessage, DialogueMessageViewModel>()
    			.ForMember(d => d.Text, o => o.MapFrom(m => m.Text))
    			.ForMember(d => d.SenderId, o => o.MapFrom(m => m.Sender.Id))
    			.ForMember(d => d.Sender, o => o.MapFrom(m => m.Sender.Name.ToString()))
				.ForMember(d => d.Created, o => o.MapFrom(m => TimeZoneInfo.ConvertTimeFromUtc(m.Created,_userTimeZone.Invoke().TimeZone()).ToShortDateTimeString()));

            CreateMap<IPushMessageDialogue, MessageViewModel>()
				.ForMember(d => d.MessageType, o => o.MapFrom(m => (int)m.PushMessage.MessageType))
                .ForMember(d => d.MessageId, o => o.MapFrom(m => m.Id.ToString()))
                .ForMember(d => d.Title, o => o.MapFrom(m => m.PushMessage.GetTitle(new NoFormatting())))
				.ForMember(d => d.Message, o => o.ResolveUsing(m =>
                                                          	{
                                                          		var message = m.Message(new NoFormatting());
                                                          		return message;
                                                          	}))
                .ForMember(d => d.Sender, o => o.MapFrom(m => m.PushMessage.Sender.Name.ToString()))
				.ForMember(d => d.Date, o => o.MapFrom(s => s.UpdatedOn.HasValue
																	? TimeZoneInfo.ConvertTimeFromUtc(s.UpdatedOn.Value,_userTimeZone.Invoke().TimeZone()).ToShortDateTimeString()
																	: null))
				.ForMember(d => d.IsRead, o => o.MapFrom(m => m.IsReplied))
				.ForMember(d => d.AllowDialogueReply, o => o.MapFrom(m => m.PushMessage.AllowDialogueReply))
				.ForMember(d => d.DialogueMessages, o => o.MapFrom(m => m.DialogueMessages))
				.ForMember(d => d.ReplyOptions, o => o.MapFrom(m => m.PushMessage.ReplyOptions))
            ;
        }
    }
}