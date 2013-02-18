/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.MessageBroker.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.AsmMessage.List.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.AsmMessage = (function ($) {

	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var userId;

	function _onMessageBrokerEvent(notification) {
		if (notification.DomainUpdateType == 2)
			Teleopti.MyTimeWeb.AsmMessageList.DeleteMessage(notification.DomainId);
		_getMessageInformation(notification.DomainId, _organizeMessages);
	};

	function _organizeMessages(data) {
		_setMessageNotificationOnTab(data.UnreadMessagesCount);
		_setAddNewMessageAtTopOfList(data.MessageItem);
	};

	function _setMessageNotificationOnTab(messageCount) {
		var countString = '';

		if (messageCount > 0)
			countString = ' (' + messageCount + ')';

		var messageTab = $('a[href*="#MessageTab"]');
		
		if (messageCount > 0) { messageTab.addClass("asm-new-message-indicator"); }
		else { messageTab.removeClass("asm-new-message-indicator"); }

		var pos = messageTab.text().indexOf(' (', 0);
		if (pos == -1) {
			pos = messageTab.text().length;
		}
		messageTab.text(messageTab.text().substring(0, pos) + countString);
	}

	function _setAddNewMessageAtTopOfList(messageItem) {
		if (messageItem != null && !messageItem.IsRead)
			Teleopti.MyTimeWeb.AsmMessageList.AddNewMessageAtTop(messageItem);
	}

	function _listenForEvents(callbackForMessages) {
		ajax.Ajax({
			url: 'MessageBroker/FetchUserData',
			dataType: "json",
			type: 'GET',
			success: function (data) {
				Teleopti.MyTimeWeb.MessageBroker.AddSubscription({
					url: data.Url,
					callback: callbackForMessages,
					errCallback: function(){},
					domainType: 'IPushMessageDialogue',
					businessUnitId: data.BusinessUnitId,
					datasource: data.DataSourceName,
					referenceId: data.AgentId
				});
				userId = data.AgentId;
			}
		});
	}

	function _getMessageInformation(messageId, callbackForMessageInformation) {
		ajax.Ajax({
			url: 'Message/Message',
			dataType: "json",
			type: 'GET',
			data: {
				messageId: messageId
			},
			success: callbackForMessageInformation,
			error: function (error) {
			}
		});
	}

	return {
		Init: function () {
			_listenForEvents(_onMessageBrokerEvent);
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Message/Index', Teleopti.MyTimeWeb.AsmMessage.MessagePartialInit);
		},
		OnMessageBrokerEvent: function () {
			_onMessageBrokerEvent();
		},
		SetMessageNotificationOnTab: function (messageCount) {
			_setMessageNotificationOnTab(messageCount);
		},
		MessagePartialInit: function () {
			Teleopti.MyTimeWeb.AsmMessageList.Init();
		},
		ListenForMessages: function (callbackForMessages) {
			_listenForEvents(callbackForMessages);
		},
		FetchMessageInfo: function (messageId, callbackForMessageInfo) {
			_getMessageInformation(messageId, callbackForMessageInfo);
		}
	};
})(jQuery);