﻿/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.RequestDetail.js"/>
/// <reference path="jquery.ui.connector.js"/>
/// <reference path="jquery.ui.connector.js"/>
/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js" />

Teleopti.MyTimeWeb.Request.List = (function ($) {

    var ajax = new Teleopti.MyTimeWeb.Ajax();
    var pageViewModel;

    function RequestItemViewModel() {

        var self = this;

	    self.isProcessing = ko.observable(false);
        self.Subject = ko.observable();
        self.RequestType = ko.observable();
        self.RequestPayload = ko.observable();
        self.Status = ko.observable();
        self.Dates = ko.observable();
        self.UpdatedOn = ko.observable();
        self.Text = ko.observable();
        self.Link = ko.observable();
        self.Id = ko.observable();
        self.IsMouseOver = ko.observable(false);
        self.IsSelected = ko.observable(false);
        self.IsLoading = ko.observable(false);
        self.CanDelete = ko.observable(true);
        self.StatusClass = ko.observable();
        self.DetailItem = undefined;

        self.Type = ko.computed(function () {
            var payload = (self.RequestPayload() != '') ? ', ' + self.RequestPayload() : '';
            return self.RequestType() + payload;
        });

        self.ShowDetails = function (viewmodel, event) {
            if (self.IsSelected()) {
                self.IsSelected(false);
                return;
            }
            if (self.DetailItem === undefined) {
                ajax.Ajax({
                    url: self.Link(),
                    dataType: "json",
                    type: 'GET',
                    beforeSend: function() {
                        self.IsLoading(true);
                    },
                    complete: function() {
                        self.IsLoading(false);
                    },
                    success: function(data) {
                        self.DetailItem = Teleopti.MyTimeWeb.Request.RequestDetail.ShowRequest(data);
                        self.DetailItem.AddRequestCallback = function () {
                            self.IsSelected(false);
                            self.DetailItem = undefined;
                        };
                        self.IsSelected(true);
                    }
                });
            } else {
                self.IsSelected(true);
            }
        };

        self.ToggleMouseOver = function () {
            self.IsMouseOver(!self.IsMouseOver());
        };

    }

    function RequestPageViewModel(readyForInteraction, completelyLoaded) {

        var self = this;

        self.Ready = readyForInteraction;
        self.Completed = completelyLoaded;
        self.IsUpdate = ko.computed(function () {
            return false;
        });
        
        self.Template = ko.computed(function () {
            return "request-detail-not-set";
        });

        self.Requests = ko.observableArray();
        
        self.MoreToLoad = ko.observable(false);

        self.isLoadingRequests = ko.observable(true);
        
        self.ShowRequests = function (data) {
            ko.utils.arrayForEach(data, function (item) {
                var vm = new RequestItemViewModel();
                vm.Initialize(item,false);
                self.Requests.push(vm);
            });
        };

        self.ColumnRequests = ko.computed(function() {
            var list = self.Requests();

            var result = [];
            var index = 0;
            ko.utils.arrayForEach(list, function(i) {
                if (index % 2 == 0) {
                    result.push({ Items: [i] });
                } else {
                    result[result.length - 1].Items.push(i);
                }
                index++;
            });
            return result;
        });

        self.Delete = function (requestItemViewModel) {

            var url = requestItemViewModel.Link();
            ajax.Ajax({
                url: url,
                dataType: "json",
                contentType: 'application/json; charset=utf-8',
                type: "DELETE",
                success: function () {
                    self.Requests.remove(requestItemViewModel);
                },
                error: function (jqXHR, textStatus) {
                    Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
                }
            });
        };

        self.AddRequest = function (request,isProcessing) {
            var selectedViewModel = ko.utils.arrayFirst(self.Requests(), function (item) {
                return item.Id() == request.Id;
            });

            if (selectedViewModel) {
                self.Requests.remove(selectedViewModel);
            }
            else {
                selectedViewModel = new RequestItemViewModel();
                selectedViewModel.Initialize(request);
            }

            ajax.Ajax({
                url: selectedViewModel.Link(),
                dataType: "json",
                type: 'GET',
                success: function (data) {
                	selectedViewModel.Initialize(data, isProcessing);
                    self.Requests.unshift(selectedViewModel);
                }
            });
        };

        self.LoadPage = function () {
            var skip = self.Requests().length;
            var take = 20;
            ajax.Ajax({
                url: "Requests/Requests",
                dataType: "json",
                type: 'GET',
                data: {
                    Take: take,
                    Skip: skip
                },
                beforeSend: function() {
                    self.isLoadingRequests(true);
                },
                success: function (data) {
                    self.MoreToLoad(data.length == take);
                    self.ShowRequests(data);
                },
                complete: function () {
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
    }

    _classFromStatus = function(data) {
        if (data.IsApproved)
            return 'label-success';
        if (data.IsDenied)
            return 'label-important';

        return 'label-warning';
    };
    
    ko.utils.extend(RequestItemViewModel.prototype, {
        Initialize: function (data, isProcessing) {
        	var self = this;
	        self.isProcessing(isProcessing);
            self.Subject(data.Subject == null ? '<br>' : data.Subject);
            self.RequestType(data.Type);
            self.Status(data.Status);
            self.Dates(data.Dates);
            self.UpdatedOn(data.UpdatedOn);
            self.Text(data.Text);
            self.Link(data.Link.href);
            self.Id(data.Id);
            self.StatusClass(_classFromStatus(data));
            self.RequestPayload(data.Payload);
			self.CanDelete((data.Link.Methods.indexOf("DELETE") != -1) && data.IsCreatedByUser);
        }
    });

    function _initScrollPaging() {
        pageViewModel.LoadPage();
        $(window).scroll(_loadAPageIfRequired);
    }

    function _loadAPageIfRequired() {
        var jqWindow = $(window);
        var jqDocument = $(window.document);
        if (_isAtBottom(jqDocument, jqWindow)) {
            pageViewModel.LoadPage();
        }
    }

    function _isAtBottom(jqDocument, jqWindow) {

        var totalContentHeight = jqDocument.height();
        var inViewContentHeight = jqWindow.height();
        var aboveViewContentHeight = jqWindow.scrollTop();
        return totalContentHeight - inViewContentHeight - aboveViewContentHeight <= 0;
    }

    return {
        Init: function (readyForInteractionCallback, completelyLoadedCallback) {
            pageViewModel = new RequestPageViewModel(readyForInteractionCallback, completelyLoadedCallback);

            _initScrollPaging();
            var element = $('#Requests-data-binding-area')[0];

            if (element) ko.applyBindings(pageViewModel, element);
        },
        AddItemAtTop: function (request,isProcessing) {
        	pageViewModel.AddRequest(request, isProcessing);
        }
    };

})(jQuery);

