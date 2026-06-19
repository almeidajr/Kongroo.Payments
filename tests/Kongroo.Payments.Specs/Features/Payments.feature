@webapi
Feature: Payment processing
  The service consumes a placed order, simulates a payment decision against a
  configurable limit, and publishes the result.

Scenario: A payment under the limit is approved
  When an order is placed for "grace@example.com" with amount 500.00 "BRL"
  Then a payment processed event is published with approval "true"

Scenario: A payment over the limit is rejected
  When an order is placed for "grace@example.com" with amount 1500.00 "BRL"
  Then a payment processed event is published with approval "false"
