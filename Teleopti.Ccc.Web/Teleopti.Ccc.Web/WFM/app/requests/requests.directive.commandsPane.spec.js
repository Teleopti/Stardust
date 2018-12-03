'use strict';
describe('[RequestsCommandPaneDirectiveTests]', function() {
	var $compile, $rootScope, $q, $controller;
	var requestsDataService,
		requestsNotificationService,
		requestCommandParamsHolder,
		_notificationResult,
		requestsController,
		mockSignalRBackendServer = {},
		signalRService,
		currentUserInfo,
		requestsPermissions,
		replyMessage,
		fakeState = {
			current: {
				name: ''
			}
		},
		toggles = {
			Wfm_Requests_People_Search_36294: true,
			Wfm_Requests_Approve_Based_On_Budget_Allotment_39626: true
		};

	beforeEach(function() {
		module('wfm.templates');
		module('wfm.requests');

		requestsDataService = new FakeRequestsDataService();
		requestsNotificationService = new FakeRequestsNotificationService();
		signalRService = new FakeSingalRService();
		currentUserInfo = new FakeCurrentUserInfo();
		requestsPermissions = new FakeRequestsPermissions();
		module(function($provide) {
			$provide.service('Toggle', function() {
				toggles.togglesLoaded = $q(function(resolve, reject) {
					resolve();
				});
				return toggles;
			});
			$provide.service('requestsDataService', function() {
				return requestsDataService;
			});
			$provide.service('requestsNotificationService', function() {
				return requestsNotificationService;
			});
			$provide.service('signalRSVC', function() {
				return signalRService;
			});
			$provide.service('CurrentUserInfo', function() {
				return currentUserInfo;
			});
			$provide.service('workingHoursPickerDirective', function() {
				return null;
			});
			$provide.service('showWeekdaysFilter', function() {
				return null;
			});
			$provide.service('requestsPermissions', function() {
				return requestsPermissions;
			});
			$provide.service('groupPageService', function () {
				return new FakeGroupPageService();
			});
			$provide.service('$state', function() {
				return fakeState;
			});
		});
	});

	beforeEach(inject(function(_$rootScope_, _$controller_, _$compile_, _requestCommandParamsHolder_, _$q_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$controller = _$controller_;
		requestCommandParamsHolder = _requestCommandParamsHolder_;
		$q = _$q_;
	}));

	afterEach(function() {
		fakeState.current.name = '';
	});

	it('approve requests success, should notify the result', function() {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }]
		};
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.approveRequests();

		expect(_notificationResult[0].changedRequestsCount).toEqual(1);
		expect(_notificationResult[0].requestsCount).toEqual(2);
	});

	it('approve requests, should handle message if there is one', function() {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }]
		};
		requestsDataService.setRequestCommandHandlingResult(handleResult);
		var message = 'message for approve';

		test.requestCommandPaneScope.approveRequests(message);

		expect(replyMessage).toEqual(message);
	});

	it('approve requests fail, should notify the result', function() {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: false,
			ErrorMessages: ['A request that is New cannot be Approved.']
		};
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.approveRequests();

		expect(_notificationResult[0]).toEqual('A request that is New cannot be Approved.');
	});

	it('should approve base on budget command enabled in absence tab', function() {
		var test = setUpTarget(false);
		expect(test.requestCommandPaneScope.isApproveBasedOnBusinessRulesEnabled()).toEqual(true);
	});

	it('should hide approve base on budget command in shift trade tab', function() {
		var test = setUpTarget(true);
		expect(test.requestCommandPaneScope.isApproveBasedOnBusinessRulesEnabled()).toEqual(false);
	});

	it('should hide approve base on budget command in overtime tab', function() {
		fakeState.current.name = 'requests.overtime';

		var test = setUpTarget();
		expect(test.requestCommandPaneScope.isApproveBasedOnBusinessRulesEnabled()).toEqual(false);
	});

	it('approveBasedOnBusinessRules command submit scucess, should notify the result', function() {
		var test = setUpTarget();
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }],
			CommandTrackId: '13'
		};
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		requestsDataService.submitCommandIsASucess(true);
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.approveBasedOnBusinessRules();

		expect(handleResult.CommandTrackId).toEqual(test.requestCommandPaneScope.commandTrackId);
		expect(_notificationResult[0]).toEqual('SubmitApproveBasedOnBusinessRulesSuccess');
	});

	it('should notify the result when approveBasedOnBusinessRules command is finished', function() {
		var test = setUpTarget();
		test.requestCommandPaneScope.commandTrackId = '13';
		mockSignalRBackendServer.notifyClients('IApproveRequestsWithValidatorsEventMessage', {
			TrackId: test.requestCommandPaneScope.commandTrackId
		});
		expect(_notificationResult[0]).toEqual('ApproveBasedOnBusinessRulesFinished');
		expect(requestCommandParamsHolder.getSelectedRequestsIds(false).length).toEqual(0);
	});

	it('deny requests, should handle message if there is one', function() {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }]
		};
		requestsDataService.setRequestCommandHandlingResult(handleResult);
		var message = 'message for deny';

		test.requestCommandPaneScope.denyRequests(message);

		expect(replyMessage).toEqual(message);
	});

	it('deny requests success, should notify the result', function() {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }]
		};
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.denyRequests();

		expect(_notificationResult[0].changedRequestsCount).toEqual(1);
		expect(_notificationResult[0].requestsCount).toEqual(2);
	});

	it('deny requests fail, should notify the result', function() {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: false,
			ErrorMessages: ['something is wrong with this deny']
		};
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.denyRequests();

		expect(_notificationResult[0]).toEqual('something is wrong with this deny');
	});

	it('cancel requests, should handle message if there is one', function() {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }]
		};
		requestsDataService.setRequestCommandHandlingResult(handleResult);
		var message = 'message for cancel';

		test.requestCommandPaneScope.cancelRequests(message);

		expect(replyMessage).toEqual(message);
	});

	it('cancel requests success, should notify the result', function() {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }]
		};
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.cancelRequests();

		expect(_notificationResult[0].changedRequestsCount).toEqual(1);
		expect(_notificationResult[0].requestsCount).toEqual(2);
	});

	it('cancel requests fail, should notify the result', function() {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: false,
			ErrorMessages: ['something is wrong with this cancel']
		};
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.denyRequests();

		expect(_notificationResult[0]).toEqual('something is wrong with this cancel');
	});

	it('should hide cancel command in shift trade tab', function() {
		var test = setUpTarget(true);
		expect(test.requestCommandPaneScope.isCommandEnabledWithoutShiftTradeView('cancel')).toEqual(false);
	});

	it('should hide cancel command in overtime tab', function() {
		fakeState.current.name = 'requests.overtime';

		var test = setUpTarget();
		expect(test.requestCommandPaneScope.isCommandEnabledWithoutShiftTradeView('cancel')).toEqual(false);
	});

	it('should hide processWaitlistedRequests command in shift trade tab', function() {
		var test = setUpTarget(true);
		expect(test.requestCommandPaneScope.isCommandEnabledWithoutShiftTradeView('processWaitlist')).toEqual(false);
	});

	it('should hide processWaitlistedRequests command in overtime tab', function() {
		fakeState.current.name = 'requests.overtime';

		var test = setUpTarget();
		expect(test.requestCommandPaneScope.isCommandEnabledWithoutShiftTradeView('processWaitlist')).toEqual(false);
	});

	it('processWaitlistedRequests command submit scucess, should notify the result', function() {
		var test = setUpTarget();
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }],
			CommandTrackId: '12'
		};

		requestsDataService.submitCommandIsASucess(true);
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.processWaitlistRequests();

		expect(handleResult.CommandTrackId).toEqual(test.requestCommandPaneScope.commandTrackId);
		expect(_notificationResult[0]).toEqual('Submit process waitlisted requests command success');
	});

	it('should notify the result when processWaitlistedRequests command is finished', function() {
		var test = setUpTarget();
		test.requestCommandPaneScope.commandTrackId = '12';
		mockSignalRBackendServer.notifyClients('IRunRequestWaitlistEventMessage', {
			TrackId: test.requestCommandPaneScope.commandTrackId,
			StartDate: 'D2016-07-15T12:00:44.357',
			EndDate: 'D2016-07-15T12:00:44.357'
		});
		expect(_notificationResult[0]).toEqual('ProcessWaitlistedRequestsFinished');
	});

	it('reply requests, should handle message', function() {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }]
		};
		requestsDataService.setRequestCommandHandlingResult(handleResult);
		var message = 'message for reply';

		test.requestCommandPaneScope.replyRequests(message);

		expect(replyMessage).toEqual(message);
	});

	it('should not get message for reply when there is more than one request selected', function() {
		var test = setUpTarget();
		var selectedRequestId = '1';
		var message = ['message for id 1'];
		var requestIds = ['1', '2'];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		requestCommandParamsHolder.setSelectedIdAndMessage(selectedRequestId, message);

		test.requestCommandPaneScope.displayReplyDialog();
		expect(test.requestCommandPaneScope.selectedRequestMessage).toEqual('');
	});

	it('should get message for reply when there is only one request selected', function() {
		var test = setUpTarget();
		var selectedRequestId = ['1'];
		var message = '  message for id 1';
		requestCommandParamsHolder.setSelectedRequestIds(selectedRequestId);
		requestCommandParamsHolder.setSelectedIdAndMessage(selectedRequestId, [message]);

		test.requestCommandPaneScope.displayReplyDialog();

		expect(test.requestCommandPaneScope.selectedRequestMessage).toEqual(message.substr(2));
	});

	it('should command enabled in shift trade tab', function() {
		var test = setUpTarget(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds, true);
		expect(test.requestCommandPaneScope.disableCommands()).toEqual(false);
	});

	it('should hide reply command in shift trade tab', function() {
		var test = setUpTarget(true);
		expect(test.requestCommandPaneScope.isCommandEnabledWithoutShiftTradeView('reply')).toEqual(false);
	});

	it('should hide reply command in overtime tab', function() {
		fakeState.current.name = 'requests.overtime';

		var test = setUpTarget();
		expect(test.requestCommandPaneScope.isCommandEnabledWithoutShiftTradeView('reply')).toEqual(false);
	});

	it('reply requests success, should notify the result', function() {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }],
			ReplySuccessCount: 1
		};
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.replyRequests();

		expect(_notificationResult[0]).toEqual('ReplySuccess');
	});

	it('reply requests fail, should notify the result', function() {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: false,
			ErrorMessages: ['something is wrong with this reply']
		};
		requestsDataService.setRequestCommandHandlingResult(handleResult);

		test.requestCommandPaneScope.denyRequests();

		expect(_notificationResult[0]).toEqual(handleResult.ErrorMessages[0]);
	});

	it('reply and approve success, should notify both results', function() {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }],
			ReplySuccessCount: 1
		};
		requestsDataService.setRequestCommandHandlingResult(handleResult);
		var message = 'message for reply and approve';

		test.requestCommandPaneScope.approveRequests(message);

		expect(_notificationResult[0].changedRequestsCount).toEqual(1);
		expect(_notificationResult[0].requestsCount).toEqual(2);
		expect(_notificationResult[1]).toEqual('ReplySuccess');
	});

	it('reply and deny success, should notify both results', function() {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }],
			ReplySuccessCount: 1
		};
		requestsDataService.setRequestCommandHandlingResult(handleResult);
		var message = 'message for reply and deny';

		test.requestCommandPaneScope.denyRequests(message);

		expect(_notificationResult[0].changedRequestsCount).toEqual(1);
		expect(_notificationResult[0].requestsCount).toEqual(2);
		expect(_notificationResult[1]).toEqual('ReplySuccess');
	});

	it('reply and cancel success, should notify both results', function() {
		var test = setUpTarget();
		requestsDataService.submitCommandIsASucess(true);
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		var handleResult = {
			Success: true,
			AffectedRequestIds: [{ id: 1 }],
			ReplySuccessCount: 1
		};
		requestsDataService.setRequestCommandHandlingResult(handleResult);
		var message = 'message for reply and cancel';

		test.requestCommandPaneScope.cancelRequests(message);

		expect(_notificationResult[0].changedRequestsCount).toEqual(1);
		expect(_notificationResult[0].requestsCount).toEqual(2);
		expect(_notificationResult[1]).toEqual('ReplySuccess');
	});

	it('should show siteopenhour command in absence and text tab', function() {
		var test = setUpTarget();
		expect(test.requestCommandPaneScope.isCommandEnabled('siteOpenHour')).toEqual(true);
	});

	it('should show siteopenhour command in shift trade tab', function() {
		var test = setUpTarget(true);
		expect(test.requestCommandPaneScope.isCommandEnabled('siteOpenHour')).toEqual(true);
	});

	it('should show siteopenhour dialog when click on siteopenhour command', function() {
		var test = setUpTarget(true);

		// By default the site open hour dialog should not show
		var result = test.targetElement[0].querySelectorAll('requests-site-open-hours');
		expect(result.length).toEqual(0);

		test.requestCommandPaneScope.showSiteOpenHour();
		test.targetElement.isolateScope().$digest();

		result = test.targetElement[0].querySelectorAll('requests-site-open-hours');
		expect(result.length).toEqual(1);
	});

	it('should show siteopenhour command in overtime tab', function() {
		fakeState.current.name = 'requests.overtime';

		var test = setUpTarget();
		expect(test.requestCommandPaneScope.isCommandEnabled('siteOpenHour')).toEqual(true);
	});

	it('should show viewAllowance command in absence and text tab', function() {
		var test = setUpTarget();
		expect(test.requestCommandPaneScope.isCommandEnabled('viewAllowance')).toEqual(true);
	});

	it('should show viewAllowance command in shift trade tab', function() {
		var test = setUpTarget(true);
		expect(test.requestCommandPaneScope.isCommandEnabled('viewAllowance')).toEqual(true);
	});

	it('should hide viewAllowance command in overtime tab', function() {
		fakeState.current.name = 'requests.overtime';

		var test = setUpTarget();
		expect(test.requestCommandPaneScope.isCommandEnabled('viewAllowance')).toEqual(false);
	});

	it('submit any command is a fail, should notify the result', function() {
		var test = setUpTarget();
		var requestIds = [{ id: 1 }, { id: 2 }];
		requestCommandParamsHolder.setSelectedRequestIds(requestIds);
		requestsDataService.submitCommandIsASucess(false);

		test.requestCommandPaneScope.denyRequests();

		expect(_notificationResult[0]).toEqual('submit error');
	});

	function FakeRequestsNotificationService() {
		this.notifySubmitProcessWaitlistedRequestsSuccess = function() {
			_notificationResult.push('Submit process waitlisted requests command success');
		};
		this.notifyProcessWaitlistedRequestsFinished = function() {
			_notificationResult.push('ProcessWaitlistedRequestsFinished');
		};
		this.notifyCommandError = function(error) {
			_notificationResult.push(angular.isUndefined(error) ? 'submit error' : error);
		};
		this.notifyApproveRequestsSuccess = function(changedRequestsCount, requestsCount) {
			_notificationResult.push({
				changedRequestsCount: changedRequestsCount,
				requestsCount: requestsCount
			});
		};
		this.notifyDenyRequestsSuccess = function(changedRequestsCount, requestsCount) {
			_notificationResult.push({
				changedRequestsCount: changedRequestsCount,
				requestsCount: requestsCount
			});
		};
		this.notifyCancelledRequestsSuccess = function(changedRequestsCount, requestsCount) {
			_notificationResult.push({
				changedRequestsCount: changedRequestsCount,
				requestsCount: requestsCount
			});
		};
		this.notifyReplySuccess = function() {
			_notificationResult.push('ReplySuccess');
		};
		this.notifySubmitApproveBasedOnBusinessRulesSuccess = function() {
			_notificationResult.push('SubmitApproveBasedOnBusinessRulesSuccess');
		};
		this.notifyApproveBasedOnBusinessRulesFinished = function() {
			_notificationResult.push('ApproveBasedOnBusinessRulesFinished');
		};
	}

	function FakeRequestsDataService() {
		var _commandIsASucess, _handleResult;
		var successCallback = function(callback) {
			if (_commandIsASucess) callback(_handleResult);
		};

		var getMessageCallback = function(selectedRequestIdsAndMessage) {
			replyMessage = selectedRequestIdsAndMessage.ReplyMessage;
		};
		var errorCallback = function(callback) {
			if (!_commandIsASucess) callback(_handleResult);
		};
		this.approveWithValidatorsPromise = function() {
			return { success: successCallback, error: errorCallback };
		};
		this.processWaitlistRequestsPromise = function() {
			return { success: successCallback, error: errorCallback };
		};
		this.approveRequestsPromise = function(selectedRequestIdsAndMessage) {
			getMessageCallback(selectedRequestIdsAndMessage);
			return { success: successCallback, error: errorCallback };
		};
		this.denyRequestsPromise = function(selectedRequestIdsAndMessage) {
			getMessageCallback(selectedRequestIdsAndMessage);
			return { success: successCallback, error: errorCallback };
		};
		this.cancelRequestsPromise = function(selectedRequestIdsAndMessage) {
			getMessageCallback(selectedRequestIdsAndMessage);
			return { success: successCallback, error: errorCallback };
		};
		this.replyRequestsPromise = function(selectedRequestIdsAndMessage) {
			getMessageCallback(selectedRequestIdsAndMessage);
			return { success: successCallback, error: errorCallback };
		};
		this.getSitesPromise = function() {
			return { success: successCallback, error: errorCallback };
		};
		this.getRequestLicense = function(successCallback) {
			return {
				then: function() {
					successCallback && successCallback();
				}
			};
		};
		this.submitCommandIsASucess = function(result) {
			_commandIsASucess = result;
		};
		this.setRequestCommandHandlingResult = function(handleResult) {
			_handleResult = handleResult;
		};
		this.getAllBusinessRulesForApproving = function() {
			return [
				{
					Id: 1,
					Checked: true,
					Name: 'BudgetAllotmentValidator',
					Description: 'ValidateRequestsBasedOnBudgetAllotment',
					Enabled: true
				},
				{
					Id: 2,
					Checked: false,
					Name: 'IntradayValidator',
					Description: 'ValidateRequestsBasedOnIntraday',
					Enabled: true
				}
			];
		};
		this.getBudgetGroupsPromise = function() {
			return [];
		};
		this.hierarchy = function() {
			return $q(function(resolve) {
				resolve({ Children: [] });
			});
		};
		this.getPermissionsPromise = function() {
			return {
				then: function(callback) {
					callback &&
						callback({
							data: {
								HasApproveOrDenyPermission: true,
								HasCancelPermission: true,
								HasReplyPermission: true
							}
						});
				}
			};
		};
	}

	function FakeRequestsPermissions() {
		var permissions;

		this.set = function setPermissions(data) {
			permissions = data;
		};

		this.all = function getPermissions() {
			return permissions;
		};
	}

	function FakeSingalRService() {
		mockSignalRBackendServer.subscriptions = [];
		mockSignalRBackendServer.notifyClients = function(domainType, message) {
			var eventHandler = this.subscriptions[domainType];
			eventHandler(message);
		};
		this.subscribe = function(options, eventHandler) {
			mockSignalRBackendServer.subscriptions[options.DomainType] = eventHandler;
		};
	}

	function FakeCurrentUserInfo() {
		this.CurrentUserInfo = function() {
			return {
				DefaultTimeZone: 'Europe/Berlin'
			};
		};
	}

	function FakeGroupPageService() {
		this.period = {};
		this.fetchAvailableGroupPages = function (startDate, endDate) {
			this.period = { StartDate: startDate, EndDate: endDate };
			return {
				then: function (cb) {
				}
			}
		}
	}

	function setUpTarget(isShiftTradeViewActived) {
		if (angular.isUndefined(isShiftTradeViewActived)) {
			isShiftTradeViewActived = false;
		}
		var scope = $rootScope.$new();
		requestsController = $controller('RequestsController', {
			$scope: scope,
			requestsNotificationService: requestsNotificationService,
			requestsDataService: requestsDataService,
			CurrentUserInfo: currentUserInfo,
			requestsPermissions: requestsPermissions
		});
		scope.$digest();

		_notificationResult = [];
		var targetScope = $rootScope.$new();
		targetScope.onCommandSuccess = requestsController.onCommandSuccess;
		targetScope.onErrorMessages = requestsController.onErrorMessages;
		targetScope.onCommandError = requestsController.onCommandError;
		targetScope.onProcessWaitlistFinished = requestsController.onProcessWaitlistFinished;
		targetScope.onApproveBasedOnBusinessRulesFinished = requestsController.onApproveBasedOnBusinessRulesFinished;
		var targetElement = $compile(
			'<requests-commands-pane ' +
				'after-command-success="onCommandSuccess(commandType, changedRequestsCount, requestsCount, waitlistPeriod) "' +
				'on-error-messages="onErrorMessages(errorMessages) "' +
				'after-command-error="onCommandError(error)"' +
				'on-process-waitlist-finished="onProcessWaitlistFinished(message)"' +
				'on-approve-based-on-business-rules-finished="onApproveBasedOnBusinessRulesFinished(message)"' +
				'is-shift-trade-view-active="' +
				isShiftTradeViewActived +
				'"' +
				'"></requests-commands-pane>'
		)(targetScope);
		targetScope.$digest();
		return { targetElement: targetElement, requestCommandPaneScope: getRequestCommandPaneScope(targetElement) };
	}

	function getRequestCommandPaneScope(targetElement) {
		return targetElement.isolateScope().requestsCommandsPane;
	}
});
