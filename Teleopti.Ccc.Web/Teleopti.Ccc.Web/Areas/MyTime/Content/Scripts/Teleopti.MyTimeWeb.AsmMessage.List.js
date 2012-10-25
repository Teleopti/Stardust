/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Content/Scripts/knockout-2.1.0.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.AsmMessageDetail.js"/>

if (typeof (Teleopti) === 'undefined') {
    Teleopti = {};
    if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
        Teleopti.MyTimeWeb = {};
    }
}

Teleopti.MyTimeWeb.AsmMessageList = (function ($) {

	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;

	function asmMessageListViewModel() {
		var self = this;

		self.asmMessageList = ko.observableArray();
		self.chosenMessage = ko.observable();
		self.chosenMessageId = ko.observable();
		self.shouldShowMessage = ko.observable(false);
		self.CreateAsmMessageList = function (dataList) {
			var asmMessageItems = new Array();
			$.each(dataList, function (position, element) {
				asmMessageItems.push(new asmMessageItemViewModel(element));
			});

			self.asmMessageList($.merge(self.asmMessageList(), asmMessageItems));
		};

		self.chosenMessageId.subscribe(function () {
			var match;
			ko.utils.arrayForEach(self.asmMessageList(), function (item) {
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
				Teleopti.MyTimeWeb.AsmMessageDetail.HideEditSection();
			}
		});

		self.asmMessageList.subscribe(function () {
			if (self.asmMessageList().length == 0)
				self.shouldShowMessage(true);
			else {
				self.shouldShowMessage(false);
			}
		});
	}

	function asmMessageItemViewModel(item) {
		var maxLength = 250;
		var self = this;
		self.title = ko.observable(item.Title);
		self.message = ko.observable(item.Message);
		self.date = ko.observable(item.Date);
		self.sender = ko.observable(item.Sender);
		self.messageId = ko.observable(item.MessageId);
		self.isRead = ko.observable(item.IsRead);
		self.isSelected = ko.observable(false);
		self.allowDialogueReply = ko.observable(item.AllowDialogueReply);
		self.isSending = ko.observable(false);
		self.reply = ko.observable('');

		self.replyIsTooLong = ko.computed(function () {
			return self.reply().length > maxLength;
		});
		self.remainingCharacters = ko.computed(function () {
			if (self.allowDialogueReply()) {
				return ('(' + (maxLength - self.reply().length) + ')');
			}
			return "";
		});
		self.dialogueMessages = ko.utils.arrayMap(item.DialogueMessages, function (data) {
			return new dialogueMessageViewModel(data);
		});
		self.isRead.subscribe(function () {
			vm.asmMessageList.remove(self);
			vm.chosenMessage(null);
		});
		self.confirmReadMessage = function (data, event) {
			self.isSending(true);
			_replyToMessage(self);
		};
		self.isConfirmButtonEnabled = ko.computed(function () {
			if (self.isSending() || (self.allowDialogueReply() && self.reply().length == 0) || (self.allowDialogueReply() && self.replyIsTooLong())) {
				return false;
			}
			return true;
		});
	}

	var dialogueMessageViewModel = function (dialogueMessage) {
		var self = this;
		self.text = ko.observable(dialogueMessage.Text);
	};

	function _addNewMessageAtTop(messageItem) {
		vm.asmMessageList.unshift(new asmMessageItemViewModel(messageItem));
		$('.asmMessage-list li')
            .first()
            .click(function () {
            	vm.chosenMessageId($(this).find('span[data-bind$="messageId"]').text());
            	_showAsmMessage($(this));
            })
            .hover(function () {
            	$(this).find('.asmMessage-arrow-right').css({ opacity: 1.0 });
            },
            function () {
            	$(this).find('.asmMessage-arrow-right').css({ opacity: 0.1 });
            })
            .find('.asmMessage-arrow-right').css({ opacity: 0.1 })
            ;
	};

	function _replyToMessage(messageItem) {
		ajax.Ajax({
			url: "Message/Reply",
			dataType: "json",
			type: 'POST',
			contentType: 'application/json; charset=utf-8',
			beforeSend: function () {
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

	function _initScrollPaging() {
		vm = new asmMessageListViewModel();
		ko.applyBindings(vm, document.getElementById('AsmMessage-body-inner'));
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
		var skip = $('#AsmMessages-list li:not(.template)').length;
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
				vm.CreateAsmMessageList(data);

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
		return $('.asmMessage-list .arrow-down').is(':visible');
	}

	function _loading() {
		$('.asmMessage-list .arrow-down').hide();
		$('.asmMessage-list .loading-gradient').show();
	}

	function _noMoreToLoad() {
		$('.asmMessage-list .arrow-down').hide();
		$('.asmMessage-list .loading-gradient').hide();
	}

	function _moreToLoad() {
		$('.asmMessage-list .arrow-down').show();
		$('.asmMessage-list .loading-gradient').hide();
	}

	function _initListClick() {
		$('#AsmMessages-list li')
			.unbind('click')
            .click(function () {
            	vm.chosenMessageId($(this).find('span[data-bind$="messageId"]').text());
            	_showAsmMessage($(this));
            })
            .hover(function () {
            	$(this).find('.asmMessage-arrow-right').css({ opacity: 1.0 });
            },
        	function () {
        		$(this).find('.asmMessage-arrow-right').css({ opacity: 0.1 });
        	})
            .find('.asmMessage-arrow-right').css({ opacity: 0.1 })
            ;
	}

	function _showAsmMessage(listItem) {
		var bindListItemClick = function _bindClick() {
			listItem.bind('click', function () {
				vm.chosenMessageId($(this).find('span[data-bind$="messageId"]').text());
				_showAsmMessage($(this));
			});
		};

		listItem.unbind('click');
		Teleopti.MyTimeWeb.AsmMessageDetail.FadeEditSection(bindListItemClick);
		Teleopti.MyTimeWeb.AsmMessageDetail.ShowAsmMessage(listItem.position().top - 30);
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
