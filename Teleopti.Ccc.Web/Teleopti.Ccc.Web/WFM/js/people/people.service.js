'use strict';

var peopleService = angular.module('restService', ['ngResource']);
peopleService.service('People', ['$resource', function ($resource) {
	var peopleService = {};

	peopleService.peopleList = [
			{
				"FirstName": "Ada",
				"LastName": "Clinton",
				"Phone": "123123123",
				"Email": "ada@teleopti.com",
				"EmploymentNumber": "100",
				"DefaultTimeZone": "W. Europe Standard Time"
			},
			{
				"FirstName": "Ada",
				"LastName": "Russo",
				"Phone": "3546364758",
				"Email": "ada@teleopti.com",
				"EmploymentNumber": "7999",
				"DefaultTimeZone": "W. Europe Standard Time"
			},
			{
				"FirstName": "Adam",
				"LastName": "Bundy",
				"Phone": "567849369",
				"Email": "adam@teleopti.com",
				"EmploymentNumber": "8000",
				"DefaultTimeZone": "W. Europe Standard Time"
			},
			{
				"FirstName": "Aileen",
				"LastName": "Broccardo",
				"Phone": "7934759347593",
				"Email": "Aileen.Broccardo@insurance.com",
				"EmploymentNumber": "137956",
				"DefaultTimeZone": "W. Europe Standard Time"
			},
			{
				"FirstName": "Alfred",
				"LastName": "Bork",
				"Phone": "04958204385",
				"Email": "Alfred.Bork@insurance.com",
				"EmploymentNumber": "137784",
				"DefaultTimeZone": "W. Europe Standard Time"
			}
	];

	return peopleService;
}]);