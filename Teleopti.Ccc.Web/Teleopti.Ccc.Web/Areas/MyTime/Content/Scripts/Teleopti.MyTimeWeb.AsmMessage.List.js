/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Content/Scripts/knockout-2.2.1.js" />
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
		var self = this;
		self.title = ko.observable(item.Title);
		self.message = ko.observable(item.Message);
		self.listMessage = ko.observable(item.Message.substring(0, 50) + '...');
		self.date = ko.observable(item.Date);
		self.sender = ko.observable(item.Sender);
		self.messageId = ko.observable(item.MessageId);
		self.isRead = ko.observable(item.IsRead);
		self.isSelected = ko.observable(false);
		self.allowDialogueReply = ko.observable(item.AllowDialogueReply);
		self.isSending = ko.observable(false);
		self.reply = ko.observable('');
		self.selectedReply = ko.observable();

		self.replyOptions = ko.utils.arrayMap(item.ReplyOptions, function (data) {
			return new replyOptionViewModel(data, self);
		});

		self.dialogueMessages = ko.observableArray(ko.utils.arrayMap(item.DialogueMessages, function (data) {
			return new dialogueMessageViewModel(data);
		}));
		self.isRead.subscribe(function () {
			vm.asmMessageList.remove(self);
			vm.chosenMessage(null);
		});
		self.confirmReadMessage = function (data, event) {
			if (self.selectedReply() == undefined) self.selectedReply(self.replyOptions[0].text());
			self.isSending(true);
			_replyToMessage(self);
		};
		self.userMustSelectReplyOption = ko.computed(function () {
			return self.replyOptions.length > 1;
		});

		self.canConfirm = ko.computed(function () {
			if (self.isSending() || (self.allowDialogueReply() && self.reply().length == 0 && (self.selectedReply() == undefined || self.selectedReply() == 'OK')) || self.selectedReply() == undefined && self.userMustSelectReplyOption()) {
				return false;
	}
			return true;
		});

		self.updateItem = function (itemToUpdate) {
			self.date(itemToUpdate.Date);
			var dialogueMessageArray = new Array();
			$.each(itemToUpdate.DialogueMessages, function (position, element) {
				dialogueMessageArray.push(new dialogueMessageViewModel(element));
			});

			self.dialogueMessages(dialogueMessageArray);
		};
	}

	var dialogueMessageViewModel = function (dialogueMessage) {
		var self = this;
		self.text = ko.observable(dialogueMessage.Text);
		self.sender = ko.observable(dialogueMessage.Sender);
		self.created = ko.observable(dialogueMessage.Created);
	};

	var replyOptionViewModel = function (replyOption, parent) {
		var self = this;
		self.text = ko.observable(replyOption);
		self.selected = ko.observable();
		self.selected.subscribe(function () {
			parent.selectedReply(self.text());
		});
	};

	function _addNewMessageAtTop(messageItem) {
		if (typeof vm !== 'undefined') {
			var result = $.grep(vm.asmMessageList(), function (list) {
				return list.messageId() == messageItem.MessageId;
			});
			if (result.length == 1) {
				var messageIndex = $.inArray(result[0], vm.asmMessageList());
				vm.asmMessageList.splice(messageIndex, 1);
				vm.asmMessageList.unshift(result[0]);
				result[0].updateItem(messageItem);
			} else {
				vm.asmMessageList.unshift(new asmMessageItemViewModel(messageItem));
			}
			$('.asmMessage-list li')
				.first()
				.click(function () {
					vm.chosenMessageId($(this).find('span[data-bind$="messageId"]').text());
					_showAsmMessage($(this));
				})
				.hover(function () {
					$(this).find('.asmMessage-arrow-right').animate({
						opacity: 1.0
					}, 300);
				},
					function () {
						$(this).find('.asmMessage-arrow-right').animate({
							opacity: 0.1
						}, 600);
					})
				.find('.asmMessage-arrow-right').css({ opacity: 0.1 });
		}
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
				Id: messageItem.messageId(),
				Reply: messageItem.reply(),
				ReplyOption: messageItem.selectedReply()
			}),
			success: function (data, textStatus, jqXHR) {
				messageItem.isRead(true);
				_noMoreToLoad();
			},
			error: function (jqXHR, textStatus, errorThrown) {
				_noMoreToLoad();
				if (jqXHR.status == 400) {
					alert(jqXHR.responseText);
					var data = $.parseJSON(jqXHR.responseText);
					_displayValidationError(data);
					messageItem.isSending(false);
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
				messageItem.isSending(false);
			}
		});
	}

	function _displayValidationError(data) {
		var message = data.Errors.join('</br>');
		$('#AsmMessage-detail-error').html(message || '');
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

	function _deleteMessage(messageId) {
		var result = $.grep(vm.asmMessageList(), function (list) {
			return list.messageId() == messageId;
		});
		if (result.length == 1) {
			if (vm.chosenMessageId() == messageId) {
				vm.chosenMessage(null);
			}
			var messageIndex = $.inArray(result[0], vm.asmMessageList());
			vm.asmMessageList.splice(messageIndex, 1);
		}
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
            	$(this).find('.asmMessage-arrow-right').animate({
            		opacity: 1.0
            	}, 100);
            	$(this).animate({
            		width: '+=20'
            	}, 100);
            },
        	function () {
        		$(this).find('.asmMessage-arrow-right').animate({
        			opacity: 0.1
        		}, 300);
        		$(this).animate({
        			width: '-=20'
        		}, 300);
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
		},
		//For testing:
		AddReplyText: function (theReply) {
			$.each(vm.asmMessageList(), function (index, value) {
				value.reply(theReply);
			});
		},
		DeleteMessage: function (messageId) {
			_deleteMessage(messageId);

		}
	};

})(jQuery);
