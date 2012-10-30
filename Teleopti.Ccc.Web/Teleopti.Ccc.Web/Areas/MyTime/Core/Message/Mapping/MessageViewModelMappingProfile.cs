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
    			.ForMember(d => d.SenderId, o => o.MapFrom(m => m.Sender.Id));
				
            CreateMap<IPushMessageDialogue, MessageViewModel>()
                .ForMember(d => d.MessageId, o => o.MapFrom(m => m.Id.ToString()))
                .ForMember(d => d.Title, o => o.MapFrom(m => m.PushMessage.GetTitle(new NoFormatting())))
                .ForMember(d => d.Message, o => o.MapFrom(m =>
                                                          	{
                                                          		var message = m.PushMessage.GetMessage(new NoFormatting());
                                                          		if (message.Length > 50)
                                                          			return message.Substring(0, 50) + "...";
                                                          		return message;
                                                          	}))
                .ForMember(d => d.Sender, o => o.MapFrom(m => m.PushMessage.Sender.Name.ToString()))
				.ForMember(d => d.Date, o => o.MapFrom(s => s.UpdatedOn.HasValue
																	? _userTimeZone.Invoke().TimeZone().ConvertTimeFromUtc(
																		s.UpdatedOn.Value).ToShortDateTimeString()
																	: null))
				.ForMember(d => d.IsRead, o => o.MapFrom(m => m.IsReplied))
				.ForMember(d => d.AllowDialogueReply, o => o.MapFrom(m => m.PushMessage.AllowDialogueReply))
				.ForMember(d => d.DialogueMessages, o => o.MapFrom(m => m.DialogueMessages))
				.ForMember(d => d.ReplyOptions, o => o.MapFrom(m => m.PushMessage.ReplyOptions))
            ;
        }
    }
}