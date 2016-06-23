'use strict';
describe('RequestsControllerTests', function () {
	var $compile,
		$rootScope,
		$controller;
	var requestsDataService, requestsNotificationService, requestCommandParamsHolder, _notificationResult;

	beforeEach(function () {
		module('wfm.templates');
		module('wfm.requests');

		requestsDataService = new FakeRequestsDataService();
		requestsNotificationService = new FakeRequestsNotificationService();
		module(function ($provide) {

			$provide.service('Toggle', function () {
				return {
					Wfm_Requests_Basic_35986: true,
					Wfm_Requests_People_Search_36294: true,
					Wfm_Requests_Performance_36295: true,
					Wfm_Requests_ApproveDeny_36297: true,
					togglesLoaded: {
						then: function (cb) { cb(); }
					}
				}
			});
			$provide.service('requestsDataService', function () {
				return requestsDataService;
			});
			$provide.service('requestsNotificationService', function () {
				return requestsNotificationService;
			});
		});
	});

	beforeEach(inject(function (_$rootScope_, _$controller_, _$compile_, _requestCommandParamsHolder_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$controller = _$controller_;
		requestCommandParamsHolder = _requestCommandParamsHolder_;
	}));

	it('processWaitlistedRequests command sunbmit scucess, should notify the result', function () {
		var test = setUpTarget();
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }]
		}
		requestsDataService.submitCommandIsASucess(true);
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.processWaitlistRequests();

		expect(_notificationResult).toEqual('Submit process waitlisted requests command success');
	});

	it('approve requests success, should notify the result', function () {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }]
		}
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.approveRequests();

		expect(_notificationResult.changedRequestsCount).toEqual(1);
		expect(_notificationResult.requestsCount).toEqual(2);
	});

	it('approve requests fail, should notify the result', function () {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: false,
			ErrorMessages: ['A request that is New cannot be Approved.']
		}
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.approveRequests();

		expect(_notificationResult).toEqual("A request that is New cannot be Approved.");
	});

	it('deny requests success, should notify the result', function () {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }]
		}
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.denyRequests();

		expect(_notificationResult.changedRequestsCount).toEqual(1);
		expect(_notificationResult.requestsCount).toEqual(2);
	});

	it('deny requests fail, should notify the result', function () {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: false,
			ErrorMessages: ['something is wrong with this deny']
		}
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.denyRequests();

		expect(_notificationResult).toEqual("something is wrong with this deny");
	});

	it('cancel requests success, should notify the result', function () {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }]
		}
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.cancelRequests();

		expect(_notificationResult.changedRequestsCount).toEqual(1);
		expect(_notificationResult.requestsCount).toEqual(2);
	});

	it('cancel requests fail, should notify the result', function () {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: false,
			ErrorMessages: ['something is wrong with this cancel']
		}
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.denyRequests();

		expect(_notificationResult).toEqual("something is wrong with this cancel");
	});

	it('submit any command is a fail, should notify the result', function () {
		var test = setUpTarget();
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		requestsDataService.submitCommandIsASucess(false);

		test.requestCommandPaneScope.denyRequests();

		expect(_notificationResult).toEqual('submit error');
	});

	it('should command enabled in shift trade tab', function() {
		var test = setUpTarget(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds, true);
		expect(test.requestCommandPaneScope.disableCommands()).toEqual(false);
	});

	function FakeRequestsNotificationService() {
		this.notifySubmitProcessWaitlistedRequestsSuccess = function () {
			_notificationResult = "Submit process waitlisted requests command success";
		}
		this.notifyCommandError = function (error) {
			_notificationResult = error == undefined ? 'submit error' : error;
		}
		this.notifyApproveRequestsSuccess = function (changedRequestsCount, requestsCount) {
			_notificationResult = {
				changedRequestsCount: changedRequestsCount,
				requestsCount: requestsCount
			}
		}
		this.notifyDenyRequestsSuccess = function (changedRequestsCount, requestsCount) {
			_notificationResult = {
				changedRequestsCount: changedRequestsCount,
				requestsCount: requestsCount
			}
		}
		this.notifyCancelledRequestsSuccess = function (changedRequestsCount, requestsCount) {
			_notificationResult = {
				changedRequestsCount: changedRequestsCount,
				requestsCount: requestsCount
			}
		}
	}

	function FakeRequestsDataService() {
		var _commandIsASucess, _handleResult;
		var successCallback = function (callback) {
			if (_commandIsASucess)
				callback(_handleResult);
		};
		var errorCallback = function (callback) {
			if (!_commandIsASucess)
				callback(_handleResult);
		}
		this.processWaitlistRequestsPromise = function () {
			return { success: successCallback, error: errorCallback }
		}
		this.approveRequestsPromise = function () {
			return { success: successCallback, error: errorCallback }
		}
		this.denyRequestsPromise = function () {
			return { success: successCallback, error: errorCallback }
		}
		this.cancelRequestsPromise = function () {
			return { success: successCallback, error: errorCallback }
		}
		this.submitCommandIsASucess = function (result) {
			_commandIsASucess = result;
		}
		this.setRequestCommandHandlingResult = function (handleResult) {
			_handleResult = handleResult;
		}
	}

	function setUpTarget(isShiftTradeViewActived) {
		if (isShiftTradeViewActived == undefined) {
			isShiftTradeViewActived = false;
		}
		var scope = $rootScope.$new();
		var target = $controller('RequestsCtrl', {
			$scope: scope
		});
		_notificationResult = '';
		var targetScope = $rootScope.$new();
		targetScope.onCommandSuccess = target.onCommandSuccess;
		targetScope.onErrorMessages = target.onErrorMessages;
		targetScope.onCommandError = target.onCommandError;
		var targetElement = $compile('<requests-commands-pane ' +
			'after-command-success="onCommandSuccess(commandType, changedRequestsCount, requestsCount, commandId, waitlistPeriod) "' +
			'on-error-messages="onErrorMessages(errorMessages) "' +
			'after-command-error="onCommandError(error)"' +
			'is-shift-trade-view-active="' + isShiftTradeViewActived + '"' +
			'"></requests-commands-pane>')(targetScope);
		targetScope.$digest();
		return { targetElement: targetElement, requestCommandPaneScope: getRequestCommandPaneScope(targetElement) }
	}

	function getRequestCommandPaneScope(targetElement) {
		return angular.element(targetElement).scope().$$childTail.$$childTail.requestsCommandsPane;
	}
});
