@webapi
Feature: Health check
  Operators need a health endpoint so orchestrators can verify the service is ready.

Scenario: Health endpoint reports a successful status
  When the health endpoint is requested
  Then the response status code is 200
