﻿Feature: Routing
	In order make it easy to browse to the site
	As a user
	I want to be redirected to the correct locations

Background:
	Given there is a role with
	| Field                    | Value               |
	| Name                     | Access to all areas |
	| Access to mytime web     | true                |
	| Access to anywhere       | true                |
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Access to report only |
	| Access to mytime web     | false                 |
	| Access to anywhere       | false                 |
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Access to mytime only |
	| Access to mytime web     | true                  |
	| Access to anywhere       | false                 |
	Given there is a role with
	| Field                    | Value                   |
	| Name                     | Access to anywhere only |
	| Access to mytime web     | false                   |
	| Access to anywhere       | true                    |
	Given there is a role with
	| Field                    | Value                         |
	| Name                     | Access to all except anywhere |
	| Access to mytime web     | true                          |
	| Access to anywhere       | false                         |

Scenario: Browse to root
	Given I am not signed in
	When I navigate to the site's root
	Then I should see the global sign in page
 
Scenario: Browse to root and sign in to MyTime
	Given I have the role 'Access to mytime only'
	When I navigate to the site's root
	And I sign in
	Then I should see MyTime

Scenario: Browse to descendent of MyTime, CiscoWidget in this case
    Given I have the role 'Access to mytime only'
	Given I am not signed in
	When I navigate to CiscoWidget
	And I sign in
	Then I should see CiscoWidget

Scenario: Browse to MyTime and sign in
	Given I have the role 'Access to all areas'
	When I navigate to MyTime
	And I sign in
	Then I should see MyTime
	
Scenario: Browse to Anywhere and sign in
	Given I have the role 'Access to all areas'
	When I navigate to Anywhere
	And I sign in
	Then I should see Anywhere

Scenario: keep url
	Given I have the role 'Access to all areas'
	When I navigate to Anywhere
	And I navigate to MyTime
	And I sign in
	Then I should see MyTime