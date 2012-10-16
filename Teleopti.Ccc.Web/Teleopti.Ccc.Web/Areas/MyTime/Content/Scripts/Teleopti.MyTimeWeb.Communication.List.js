/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Content/Scripts/knockout-2.1.0.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js"/>

if (typeof (Teleopti) === 'undefined') {
    Teleopti = {};
    if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
        Teleopti.MyTimeWeb = {};
    }
}

Teleopti.MyTimeWeb.CommunicationList = (function ($) {

    var ajax = new Teleopti.MyTimeWeb.Ajax();
    var vm;

    function communicationListViewModel() {
        var self = this;

        self.communicationList = ko.observableArray();
        self.chosenMessage = ko.observable();
        self.chosenMessageId = ko.observable();
        self.shouldShowMessage = ko.observable(false);

        self.CreateCommunicationList = function (dataList) {
            var communicationItems = new Array();
            $.each(dataList, function (position, element) {
                communicationItems.push(new communicationItemViewModel(element));
            });

            self.communicationList($.merge(self.communicationList(), communicationItems));
        };

        self.chosenMessageId.subscribe(function () {
            var match;
            ko.utils.arrayForEach(self.communicationList(), function (item) {
                item.isSelected(false);
                if (item.messageId() === self.chosenMessageId()) {
                    item.isSelected(true);
                    match = item;

                }
            });

            self.chosenMessage(match);
        });

        self.chosenMessage.subscribe(function () {
            if (self.chosenMessage() == null) {
                Teleopti.MyTimeWeb.CommunicationDetail.HideEditSection();
            }
        });

        self.communicationList.subscribe(function () {
            if (self.communicationList().length == 0)
                self.shouldShowMessage(true);
            else {
                self.shouldShowMessage(false);
            }
        });
    }

    function communicationItemViewModel(item) {
        var self = this;
        self.title = ko.observable(item.Title);
        self.message = ko.observable(item.Message);
        self.date = ko.observable(item.Date);
        self.sender = ko.observable(item.Sender);
        self.messageId = ko.observable(item.MessageId);
        self.isRead = ko.observable(item.IsRead);
        self.isConfirmButtonEnabled = ko.observable(true);
        self.isSelected = ko.observable(false);

        self.isRead.subscribe(function () {
            vm.communicationList.remove(self);
            vm.chosenMessage(null);
        });
        self.confirmReadMessage = function (data, event) {
            _replyToMessage(self);
        };
    }

    function _addNewMessageAtTop(messageItem) {
        vm.communicationList.unshift(new communicationItemViewModel(messageItem));
        $('.communication-list li')
            .first()
            .click(function () {
                vm.chosenMessageId($(this).find('span[data-bind$="messageId"]').text());
                _showCommunication($(this));
            })
            .hover(function() {
                $(this).find('.communication-arrow-right').css({ opacity: 1.0 });
            },
            function() {
                $(this).find('.communication-arrow-right').css({ opacity: 0.1 });
            })
            .find('.communication-arrow-right').css({ opacity: 0.1 })
            ;
    };

    function _replyToMessage(messageItem) {
        ajax.Ajax({
            url: "Message/Reply",
            dataType: "json",
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            beforeSend: function () {
                _setConfirmButtonState(messageItem, false);
                _loading();
            },
            data: JSON.stringify({
                messageId: messageItem.messageId()
            }),
            success: function (data, textStatus, jqXHR) {
                messageItem.isRead(true);
                _noMoreToLoad();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert('felfelfel! ' + messageItem.messageId());
            }
        });
    }

    function _setConfirmButtonState(messageItem, isEnabled) {
        messageItem.isConfirmButtonEnabled(isEnabled);
    }

    function _initScrollPaging() {
        vm = new communicationListViewModel();
        ko.applyBindings(vm, document.getElementById('Communication-body-inner'));
        _loadAPage();
        $(window).scroll(_loadAPageIfRequired);
    }

    function _loadAPageIfRequired() {
        if (!_hasMoreToLoad())
            return;
        var jqWindow = $(window);
        var jqDocument = $(window.document);
        if (_isAtBottom(jqDocument, jqWindow)) {
            _loadAPage();
        }
    }

    function _isAtBottom(jqDocument, jqWindow) {
        var totalContentHeight = jqDocument.height();
        var inViewContentHeight = jqWindow.height();
        var aboveViewContentHeight = jqWindow.scrollTop();
        return totalContentHeight - inViewContentHeight - aboveViewContentHeight <= 0;
    }

    function _loadAPage() {
        var skip = $('#Communications-list li:not(.template)').length;
        var take = 20;
        ajax.Ajax({
            url: "Message/Messages",
            dataType: "json",
            type: 'GET',
            beforeSend: _loading,
            data: {
                Take: take,
                Skip: skip
            },
            success: function (data, textStatus, jqXHR) {
                vm.CreateCommunicationList(data);

                if (data.length == 0 || data.length < take) {
                    _noMoreToLoad();
                } else {
                    _moreToLoad();
                }
                _initListClick();
            },
            error: function () {
            }
        });
    }

    function _hasMoreToLoad() {
        return $('.communication-list .arrow-down').is(':visible');
    }

    function _loading() {
        $('.communication-list .arrow-down').hide();
        $('.communication-list .loading-gradient').show();
    }

    function _noMoreToLoad() {
        $('.communication-list .arrow-down').hide();
        $('.communication-list .loading-gradient').hide();
    }

    function _moreToLoad() {
        $('.communication-list .arrow-down').show();
        $('.communication-list .loading-gradient').hide();
    }

    function _initListClick() {
        var item = $('#Communications-list li');
        $('#Communications-list li')
			.unbind('click')
            .click(function () {
                vm.chosenMessageId($(this).find('span[data-bind$="messageId"]').text());
                _showCommunication($(this));
            })
            .hover(function () {
                $(this).find('.communication-arrow-right').css({ opacity: 1.0 });
            },
        	function () {
        	    $(this).find('.communication-arrow-right').css({ opacity: 0.1 });
        	})
            .find('.communication-arrow-right').css({ opacity: 0.1 })
            ;
    }

    function _showCommunication(listItem) {
        var bindListItemClick = function _bindClick() {
            listItem.bind('click', function () {
                vm.chosenMessageId($(this).find('span[data-bind$="messageId"]').text());
                _showCommunication($(this));
            });
        };

        listItem.unbind('click');
        Teleopti.MyTimeWeb.CommunicationDetail.FadeEditSection(bindListItemClick);
        Teleopti.MyTimeWeb.CommunicationDetail.ShowCommunication(listItem.position().top - 30);
    }

    return {
        Init: function () {
            _initScrollPaging();
        },
        AddNewMessageAtTop: function (messageItem) {
            _addNewMessageAtTop(messageItem);
        }
    };

})(jQuery);
