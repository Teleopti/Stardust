Teleopti.MyTimeWeb.AsmMessage = (function ($) {

	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var currentPage = 'Teleopti.MyTimeWeb.AsmMessage';

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
		var messageSpan = $('a[href*="#MessageTab"] > span');
		if (messageCount == 0) {
			messageSpan.parent().removeClass("asm-new-message-indicator");
			messageSpan.hide();
			return;
		}

		messageSpan.text(messageCount);
		messageSpan.removeClass('hide');
		messageSpan.show();

		messageSpan.parent().addClass("asm-new-message-indicator");
	}

	function _setAddNewMessageAtTopOfList(messageItem) {
		if (messageItem != null && !messageItem.IsRead)
			Teleopti.MyTimeWeb.AsmMessageList.AddNewMessageAtTop(messageItem);
	}

	function _listenForEvents(callbackForMessages) {

		Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker({
			successCallback: callbackForMessages,
			domainType: 'IPushMessageDialogue',
			page: currentPage
		});
	};

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
		MessagePartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
			readyForInteractionCallback();
			completelyLoadedCallback();
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