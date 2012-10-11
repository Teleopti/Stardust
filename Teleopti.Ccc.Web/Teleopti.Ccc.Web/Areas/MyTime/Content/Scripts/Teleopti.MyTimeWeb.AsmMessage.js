/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.MessageBroker.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Communication.List.js"/>
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
        _getUnreadMessagesCount();
    };

    function _setMessageNotificationOnTab(messageCount) {
        var countString = '';

        //alert('count: ' + messageCount);

        if (messageCount > 0)
            countString = ' (' + messageCount + ')';

        var messageTab = $('a[href*="#MessageTab"]');
        messageTab.addClass("asm-new-message-indicator");
        var pos = messageTab.text().indexOf(' (', 0);
        if (pos == -1) {
            pos = messageTab.text().length;
        }
        messageTab.text(messageTab.text().substring(0, pos) + countString);
    }

    function test() {
        console.log('rgsgfsdf');
        //console.log('fel');
    }

    function _listenForEvents() {
        ajax.Ajax({
            url: 'MessageBroker/FetchUserData',
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

    function _getUnreadMessagesCount() {
        ajax.Ajax({
            url: '/MyTime/Message/MessagesCount',
            dataType: "json",
            type: 'GET',
            success: function (data) {
                _setMessageNotificationOnTab(data.UnreadMessagesCount);
            },
            error: function (error) {
                //console.log(error);
            }
        });
    }

    return {
        Init: function () {
            _listenForEvents();
        },
        OnMessageBrokerEvent: function () {
            _onMessageBrokerEvent();
        },
        SetMessageNotificationOnTab: function (messageCount) {
            _setMessageNotificationOnTab(messageCount);
        }
    };
})(jQuery);