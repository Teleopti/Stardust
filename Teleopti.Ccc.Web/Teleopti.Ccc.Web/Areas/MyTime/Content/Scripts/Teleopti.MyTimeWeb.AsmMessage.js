/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.MessageBroker.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Communication.List.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.AsmMessage = (function () {

    var userId;

    function _onMessageBrokerEvent(notification) {
        var messageCountFromDb = _getUnreadMessages();
        var countString = '';
        
        var messageTab = $('a[href*="#MessageTab"]');
        messageTab.addClass("asm-new-message-indicator");
        
        if (messageCountFromDb > 0) {
            countString = ' (' + messageCountFromDb + ')';
        }
        
        messageTab.text(messageTab.text() + countString);
    };

    function test() {
        //console.log('fel');
    }

    function _listenForEvents() {
        Teleopti.MyTimeWeb.Ajax.Ajax({
            url: '/MyTime/MessageBroker/FetchUserData',
            dataType: "json",
            type: 'GET',
            success: function (data) {
                Teleopti.MyTimeWeb.MessageBroker.AddSubscription({
                    url: data.Url,
                    callback: _onMessageBrokerEvent,
                    errCallback: test,
                    domainType: 'IPushMessageDialogue',
                    businessUnitId: data.BusinessUnitId,
                    datasource: data.DataSourceName,
                    referenceId: data.AgentId
                });
                userId = data.AgentId;
            }
        });
    }

    function _getUnreadMessages() {
        var count;
        Teleopti.MyTimeWeb.Ajax.Ajax({
            url: '/MyTime/Message/MessagesCount',
            dataType: "json",
            type: 'GET',
            success: function (data) {
                count = data.UnreadMessagesCount;
            }
        });
        return count;
    }

    return {
        Init: function () {
            _listenForEvents();
        },
        OnMessageBrokerEvent: function () {
            _onMessageBrokerEvent();
        }
    };
})(jQuery);