@WatiN
Feature: Security
	In order to protect my information
	As a user that is not signed in
	I want to make sure I cant access the site

Scenario: Browse to site home page as a user that is not signed in
	Given I am not signed in
	When I navigate to the site home page
	Then I should be redirected to the sign in page

Scenario: Browse to an application page as a user that is not signed in
	Given I am not signed in
	When I navigate to an application page
	Then I should be redirected to the sign in page

Scenario: Browse to an application page as a signed in user
	Given I am signed in
	When I navigate to an application page
	Then I should see an application page
