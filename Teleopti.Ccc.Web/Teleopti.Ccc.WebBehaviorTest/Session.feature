﻿Feature: Session
	In order to be able to work with the application
	As an agent
	I want the application to handle my login session approprietly

Scenario: Stay signed in after server restart
	Given I am viewing an application page
	Then I should be signed in
	When I navigate the internet
	And the server restarts
	And I navigate to an application page
	Then I should be signed in

Scenario: Signed out when cookie expires while I browse the internet
	Given I am viewing an application page
	Then I should be signed in
	When my cookie expires
	And I navigate the internet
	And I navigate to an application page
	Then I should be signed out

Scenario: Signed out when cookie expires while I have the browser open
	Given I am viewing an application page
	Then I should be signed in
	When my cookie expires
	And I navigate to an application page
	Then I should be signed out

Scenario: Signed out when saving preference when cookie is expired
	Given I am an agent without access to extended preferences
	And I have an open workflow control set with an allowed standard preference
	And I am viewing preferences
	When my cookie expires
	And I select an editable day without preference
	And I try to select a standard preference
	Then I should be signed out

Scenario: Signed out when navigating to next period when cookie is expired
	Given I am an agent
	And I have several virtual schedule periods
	And I am viewing preferences
	When my cookie expires
	And I click next virtual schedule period button
	Then I should be signed out

Scenario: Corrupt cookie due to upgrade should be overwritten by a logon
	Given I am viewing an application page
	Then I should be signed in
	When My cookie gets corrupt
	And I navigate to an application page
	Then I should be signed out
	When I sign in again
	Then I should be signed in

Scenario: Corrupt cookie due to no longer existing database should be overwritten by a logon
	Given I am viewing an application page
	Then I should be signed in
	When My cookie gets pointed to non existing database
	And I navigate to an application page
	Then I should be signed out
	When I sign in again
	Then I should be signed in