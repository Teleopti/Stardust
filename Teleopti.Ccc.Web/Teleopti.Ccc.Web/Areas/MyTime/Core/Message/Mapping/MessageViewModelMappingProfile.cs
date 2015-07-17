using System;
using AutoMapper;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.Mapping
{
    public class MessageViewModelMappingProfile : Profile
    {
		private readonly Func<IUserTimeZone> _userTimeZone;
		private readonly IPersonNameProvider _personNameProvider;

	    public MessageViewModelMappingProfile(Func<IUserTimeZone> timeZone, IPersonNameProvider personNameProvider)
	    {
		    _userTimeZone = timeZone;
		    _personNameProvider = personNameProvider;
	    }

	    protected override void Configure()
        {

            base.Configure();

    		CreateMap<IDialogueMessage, DialogueMessageViewModel>()
    			.ForMember(d => d.Text, o => o.MapFrom(m => m.Text))
    			.ForMember(d => d.SenderId, o => o.MapFrom(m => m.Sender.Id))
				.ForMember(d => d.Sender, o => o.MapFrom(m => _personNameProvider.BuildNameFromSetting(m.Sender.Name)))
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
				.ForMember(d => d.Sender, o => o.MapFrom(m => _personNameProvider.BuildNameFromSetting(m.PushMessage.Sender.Name)))
				.ForMember(d => d.Date, o => o.ResolveUsing(m =>
				{
					if (m.UpdatedOn.HasValue)
					{
						return TimeZoneInfo.ConvertTimeFromUtc(m.UpdatedOn.Value, _userTimeZone.Invoke().TimeZone());
					}
					return null;
				}))

				//.ForMember(d => d.Date, o => o.MapFrom(s => s.UpdatedOn.HasValue
				//													? TimeZoneInfo.ConvertTimeFromUtc(s.UpdatedOn.Value, _userTimeZone.Invoke().TimeZone()).ToShortDateTimeString()
				//													: null))
				.ForMember(d => d.IsRead, o => o.MapFrom(m => m.IsReplied))
				.ForMember(d => d.AllowDialogueReply, o => o.MapFrom(m => m.PushMessage.AllowDialogueReply))
				.ForMember(d => d.DialogueMessages, o => o.MapFrom(m => m.DialogueMessages))
				.ForMember(d => d.ReplyOptions, o => o.MapFrom(m => m.PushMessage.ReplyOptions))
            ;
        }
    }
}