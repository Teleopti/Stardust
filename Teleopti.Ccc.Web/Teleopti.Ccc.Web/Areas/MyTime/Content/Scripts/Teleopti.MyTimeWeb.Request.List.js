Teleopti.MyTimeWeb.Request.List = (function($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var pageViewModel;

	function RequestItemViewModel(requestPageViewModel) {
		var self = this;

		self.isProcessing = ko.observable(false);
		self.Subject = ko.observable();
		self.RequestType = ko.observable();
		self.RequestTypeEnum = ko.observable();
		self.RequestPayload = ko.observable();
		self.Status = ko.observable();

		self.StartDateTime = ko.observable();
		self.EndDateTime = ko.observable();

		self.IsSingleDay = ko.observable();
		self.UpdatedOn = ko.observable();
		self.TextSegments = ko.observableArray();
		self.DenyReason = ko.observable();
		self.ListText = ko.observable();
		self.Link = ko.observable();
		self.Id = ko.observable();
		self.IsMouseOver = ko.observable(false);
		self.IsSelected = ko.observable(false);
		self.IsLoading = ko.observable(false);
		self.CanDelete = ko.observable(true);
		self.CanCancel = ko.observable(true);
		self.IsEditable = ko.observable(false);
		self.IsDeletePending = ko.observable(false);
		self.IsCancelPending = ko.observable(false);
		self.ErrorMessage = ko.observable();

		self.StatusClass = ko.observable();
		self.DetailItem = undefined;
		self.parent = requestPageViewModel;

		self.Type = ko.computed(function() {
			var payload = self.RequestPayload() !== '' ? ', ' + self.RequestPayload() : '';
			return self.RequestType() + payload;
		});

		self.isReferred = ko.observable(false);
		self.isCreatedByUser = ko.observable(false);

		self.IsFullDay = ko.observable();
		self.IsShiftTradeRequest = function() {
			return self.RequestTypeEnum() === 2;
		};

		self.GetDateDisplay = function() {
			if (!(self.StartDateTime() && self.EndDateTime())) {
				return null;
			}

			if (self.IsSingleDay() && (self.IsShiftTradeRequest() || self.IsFullDay())) {
				return Teleopti.MyTimeWeb.Common.FormatDate(self.StartDateTime());
			}

			var showTimes = !self.IsShiftTradeRequest() && !self.IsFullDay();
			return Teleopti.MyTimeWeb.Common.FormatDatePeriod(self.StartDateTime(), self.EndDateTime(), showTimes);
		};

		self.Dates = ko.computed(function() {
			return self.GetDateDisplay();
		});

		self.ShowDetails = function() {
			if (self.IsSelected()) {
				self.IsSelected(false);
				return;
			}

			if (self.DetailItem === undefined) {
				ajax.Ajax({
					url: self.Link(),
					dataType: 'json',
					type: 'GET',
					beforeSend: function() {
						self.IsLoading(true);
					},
					complete: function() {
						self.IsLoading(false);
					},
					success: function(data) {
						self.DetailItem = Teleopti.MyTimeWeb.Request.RequestDetail.ShowRequest(
							data,
							self.successUpdatingRequest
						);
						self.IsSelected(true);
					}
				});
			} else {
				self.IsSelected(true);
			}
		};

		self.successUpdatingRequest = function(data) {
			self.IsSelected(false);
			self.DetailItem = undefined;
			self.parent.AddRequest(data, false);
		};

		self.ToggleMouseOver = function() {
			self.IsMouseOver(!self.IsMouseOver());
		};

		self.isValid = function() {
			return !(self.isReferred() && !self.isCreatedByUser());
		};
	}

	function RequestPageViewModel(readyForInteraction, completelyLoaded) {
		var self = this;

		self.Ready = readyForInteraction;
		self.Completed = completelyLoaded;
		self.IsUpdate = ko.computed(function() {
			return false;
		});

		self.Template = ko.computed(function() {
			return 'request-detail-not-set';
		});

		self.Requests = ko.observableArray();

		self.MoreToLoad = ko.observable(false);

		self.isLoadingRequests = ko.observable(true);
		self.hideRequestsOnPhone = ko.observable(false);
		self.filters = [
			{ name: requestsMessagesUserTexts.CURRENT_REQUESTS, show: 'true' },
			{ name: requestsMessagesUserTexts.ALL_REQUESTS, show: 'false' }
		];
		self.sorters = [
			{ name: requestsMessagesUserTexts.BY_STARTDATE, show: 'false' },
			{ name: requestsMessagesUserTexts.BY_UPDATEDATE, show: 'true' }
		];
		self.ShowRequests = function(data) {
			ko.utils.arrayForEach(data, function(item) {
				ko.utils.arrayForEach(self.Requests(), function(request) {
					if (item !== undefined && request.Id() === item.Id) {
						var index = data.indexOf(item);
						if (index !== -1) {
							delete data[index];
						}
					}
				});
			});

			ko.utils.arrayForEach(data, function(item) {
				var vm = new RequestItemViewModel(self);
				vm.Initialize(item, false);
				self.Requests.push(vm);
			});
		};

		self.ColumnRequests = ko.computed(function() {
			var list = ko.utils.arrayFilter(self.Requests(), function(request) {
				return request.isValid();
			});
			var result = [];
			var index = 0;
			ko.utils.arrayForEach(list, function(i) {
				if (index % 2 === 0) {
					result.push({ Items: [i] });
				} else {
					result[result.length - 1].Items.push(i);
				}
				index++;
			});
			return result;
		});

		ko.eventAggregator.subscribe(
			function(cancelRequestMessage) {
				ko.utils.arrayFirst(self.Requests(), function(r) {
					if (r.Id() === cancelRequestMessage.id) {
						self.Delete(r);
					}
				});
			},
			null,
			'cancel_request'
		);

		self.clearAllPromptsFromOtherRequestItemViewModels = function(requestItemViewModel) {
			ko.utils.arrayForEach(self.Requests(), function(request) {
				if (request !== requestItemViewModel) {
					request.IsCancelPending(false);
					request.IsDeletePending(false);
					request.ErrorMessage(null);
				}
			});
		};

		self.SwitchDeleteConfirmationVisibility = function(requestItemViewModel) {
			requestItemViewModel.IsDeletePending(!requestItemViewModel.IsDeletePending());
			requestItemViewModel.IsCancelPending(false);
			requestItemViewModel.ErrorMessage(null);

			self.clearAllPromptsFromOtherRequestItemViewModels(requestItemViewModel);
		};

		self.SwitchCancelConfirmationVisibility = function(requestItemViewModel) {
			requestItemViewModel.IsCancelPending(!requestItemViewModel.IsCancelPending());
			requestItemViewModel.IsDeletePending(false);
			requestItemViewModel.ErrorMessage(null);

			self.clearAllPromptsFromOtherRequestItemViewModels(requestItemViewModel);
		};

		self.SwitchErrorMessageVisibility = function(requestItemViewModel) {
			requestItemViewModel.ErrorMessage(null);
			requestItemViewModel.IsCancelPending(false);
			requestItemViewModel.IsDeletePending(false);

			self.clearAllPromptsFromOtherRequestItemViewModels(requestItemViewModel);
		};

		self.Delete = function(requestItemViewModel) {
			var url = requestItemViewModel.Link();

			ajax.Ajax({
				url: url,
				dataType: 'json',
				contentType: 'application/json; charset=utf-8',
				type: 'DELETE',
				success: function() {
					self.Requests.remove(requestItemViewModel);
				},
				error: function(jqXHR, textStatus) {
					Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
				}
			});
		};

		self.Cancel = function(requestItemViewModel) {
			var selectedViewModel = requestItemViewModel;
			var requestId = requestItemViewModel.Id();
			if (!requestId) return;

			ajax.Ajax({
				url: 'Requests/CancelRequest',
				dataType: 'json',
				contentType: 'application/json; charset=utf-8',
				type: 'PUT',
				data: JSON.stringify({ id: requestId }),
				success: function(result) {
					if (result.Success) {
						self.Requests.remove(requestItemViewModel);
						selectedViewModel.Initialize(result.RequestViewModel);
						self.Requests.unshift(selectedViewModel);
					} else {
						if (result.ErrorMessages && result.ErrorMessages !== null) {
							requestItemViewModel.ErrorMessage(result.ErrorMessages[0]);
						}
					}
					requestItemViewModel.IsCancelPending(false);
				},
				error: function(jqXHR, textStatus) {
					Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
				}
			});
		};

		self.AddRequest = function(request, isProcessing) {
			var selectedViewModel = ko.utils.arrayFirst(self.Requests(), function(item) {
				return item.Id() === request.Id;
			});

			if (selectedViewModel) {
				self.Requests.remove(selectedViewModel);
			} else {
				selectedViewModel = new RequestItemViewModel(self);
				selectedViewModel.Initialize(request);
			}

			if (isProcessing) {
				selectedViewModel.Initialize(request, false);
				self.Requests.unshift(selectedViewModel);
			} else {
				ajax.Ajax({
					url: selectedViewModel.Link(),
					dataType: 'json',
					type: 'GET',
					success: function(data) {
						selectedViewModel.Initialize(data, isProcessing);
						self.Requests.unshift(selectedViewModel);
					}
				});
			}
		};

		self.hideOldRequests = ko.observable('true');
		self.hideOldRequests.subscribe(function() {
			self.Requests([]);
			self.pages = 0;
			self.LoadPage();
		});

		self.IsSortByUpdateDate = ko.observable('true');
		self.IsSortByUpdateDate.subscribe(function() {
			self.Requests([]);
			self.pages = 0;
			self.LoadPage();
		});

		self.pages = 0;
		self.loadMoreRequests = function() {
			self.MoreToLoad(false);
			self.LoadPage();
		};
		self.LoadPage = function() {
			var skip = self.pages * 20;
			var take = 20;
			ajax.Ajax({
				url: 'Requests/Requests',
				dataType: 'json',
				type: 'GET',
				data: {
					Take: take,
					Skip: skip,
					HideOldRequest: self.hideOldRequests(),
					IsSortByUpdateDate: self.IsSortByUpdateDate()
				},
				beforeSend: function() {
					self.isLoadingRequests(true);
				},
				success: function(data) {
					self.MoreToLoad(data.length === take);
					self.ShowRequests(data);
					if (data.length !== 0) {
						self.pages++;
					}
				},
				complete: function() {
					if (self.Ready) {
						self.Ready();
						self.Ready = null;
					}
					if (self.Completed) {
						self.Completed();
						self.Completed = null;
					}
					self.isLoadingRequests(false);
				}
			});
		};

		self.CanDelete = ko.observable(true);
		self.CanCancel = ko.observable(true);
	}

	_classFromStatus = function(data) {
		if (data.IsApproved) return 'label-success';
		if (data.IsDenied) return 'label-danger';

		return 'label-warning';
	};

	_setAgentName = function(message, name) {
		// replace agent name (as a work-around is inside [] so they can be found inside the message SLOB)
		var placeholderReg = /^\[(.?|.+?)\]/gi;
		return message.replace(placeholderReg, '[' + name + ']');
	};

	_splitChunk = function(textSegs) {
		var splitedTextSegs = [];
		$.each(textSegs, function(index, line) {
			var len = 60;
			var curr = len;
			var prev = 0;
			if (line.length > len) {
				while (line[curr]) {
					if (line[curr++] === ' ') {
						splitedTextSegs.push(line.substring(prev, curr));
						prev = curr;
						curr += len;
					}
				}
				splitedTextSegs.push(line.substr(prev));
			} else splitedTextSegs.push(line);
		});

		return splitedTextSegs;
	};

	ko.utils.extend(RequestItemViewModel.prototype, {
		Initialize: function(data, isProcessing) {
			if (data == undefined) return;
			if (data.Text !== null) {
				var textSegs = data.Text.split('\n');
			}
			var textNoBr = '';
			var messageInList = [];

			var splitedTextSegs = _splitChunk(textSegs);

			//remove line breaks for summary display...
			$.each(textSegs, function(index, text) {
				if (text !== undefined && text !== '') {
					var updatedText = text;
					if (index === 0) {
						if (data.From !== '') updatedText = _setAgentName(text, data.From);
					} else {
						if (data.To !== '') updatedText = _setAgentName(text, data.To);
					}
					textNoBr = textNoBr + updatedText + ' ';
					messageInList.push(updatedText + '\n');
				}
			});

			var self = this;
			self.isProcessing(isProcessing);
			self.Subject(data.Subject === null ? '' : data.Subject);
			self.RequestType(data.Type);
			self.RequestTypeEnum(data.TypeEnum);
			self.Status(data.Status);
			self.IsFullDay(data.IsFullDay);

			self.StartDateTime(moment(data.DateTimeFrom));
			self.EndDateTime(moment(data.DateTimeTo));
			self.IsSingleDay(moment(data.DateTimeFrom).isSame(moment(data.DateTimeTo), 'day'));

			self.UpdatedOn(Teleopti.MyTimeWeb.Common.FormatDate(data.UpdatedOnDateTime));

			self.TextSegments(messageInList);
			self.ListText(textNoBr.length > 50 ? textNoBr.substring(0, 50) + '...' : textNoBr);
			self.DenyReason(data.DenyReason === undefined ? '' : data.DenyReason.replace(/\n/g, '<br/>'));
			self.Link(data.Link.href);
			self.Id(data.Id);
			self.StatusClass(_classFromStatus(data));
			self.RequestPayload(data.Payload);
			self.CanDelete(data.Link.Methods.indexOf('DELETE') !== -1);
			self.IsEditable(data.Link.Methods.indexOf('PUT') !== -1);
			self.CanCancel(data.Link.Methods.indexOf('CANCEL') !== -1);
			self.isCreatedByUser(data.IsCreatedByUser);
			self.isReferred(data.IsReferred);
			self.IsSelected(false);
			self.IsCancelPending(false);
			self.ErrorMessage(null);

			if (self.IsShiftTradeRequest() && !data.IsCreatedByUser) self.CanDelete(false);
		}
	});

	function _initScrollPaging() {
		pageViewModel.LoadPage();
		$(window).scroll(_loadAPageIfRequired);
	}

	function _loadAPageIfRequired() {
		if (!pageViewModel.MoreToLoad()) {
			return;
		}

		var jqWindow = $(window);
		var jqDocument = $(window.document);
		if (_isAtBottom(jqDocument, jqWindow)) {
			$(window).off('scroll');
			pageViewModel.LoadPage();
			setTimeout(function() {
				$(window).scroll(_loadAPageIfRequired);
			}, 100);
		}
	}

	function _isAtBottom(jqDocument, jqWindow) {
		var totalContentHeight = jqDocument.height();
		var inViewContentHeight = jqWindow.height();
		var aboveViewContentHeight = jqWindow.scrollTop();
		return totalContentHeight - inViewContentHeight - aboveViewContentHeight <= 0;
	}

	function _unbind() {
		var element = $('#Requests-data-binding-area')[0];
		if (element) ko.cleanNode(element);

		element = $('#Request-add-data-binding-area')[0];
		if (element) ko.cleanNode(element);

		Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.Dispose();
	}

	return {
		Init: function(readyForInteractionCallback, completelyLoadedCallback, parseAjax) {
			if (parseAjax) ajax = parseAjax;

			pageViewModel = new RequestPageViewModel(readyForInteractionCallback, completelyLoadedCallback);

			_initScrollPaging();
			var element = $('#Requests-data-binding-area')[0];
			if (element) ko.applyBindings(pageViewModel, element);
		},
		AddItemAtTop: function (request, isProcessing) {
			if (pageViewModel == null) return;
			pageViewModel.AddRequest(request, isProcessing);
		},
		GetRequestItemViewModel: function() {
			return new RequestItemViewModel(null);
		},
		Dispose: function() {
			_unbind();
		},
		HideRequests: function(show) {
			if (pageViewModel != null) pageViewModel.hideRequestsOnPhone(show);
		}
	};
})(jQuery);
