using System.ComponentModel;
using System.Security.Claims;
using Kongroo.Payments.Application;
using Kongroo.Payments.Presentation.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Kongroo.Payments.Presentation;

public static class EndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public RouteGroupBuilder MapPaymentEndpoints()
        {
            var routeGroup = endpoints.MapGroup("/").WithTags("Payments");

            routeGroup
                .MapGet("/", GetPaymentsAsync)
                .RequireAuthorization()
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .WithName("GetPayments")
                .WithSummary("Get payments")
                .WithDescription(
                    "Returns the authenticated caller's payments ordered by most recent. "
                        + "Admins may pass ?customerId= to view another customer's payments."
                );

            routeGroup
                .MapGet("/{orderId:guid}", GetPaymentAsync)
                .RequireAuthorization()
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .WithName("GetPaymentByOrderId")
                .WithSummary("Get a payment")
                .WithDescription("Returns the payment for a single order owned by the authenticated caller.");

            return routeGroup;
        }
    }

    private static async Task<Ok<IReadOnlyList<PaymentResponse>>> GetPaymentsAsync(
        ClaimsPrincipal user,
        [Description("Admin-only: the customer whose payments to list.")] Guid? customerId,
        GetPaymentsQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var effectiveCustomerId = user.IsInRole("Admin") && customerId.HasValue ? customerId.Value : user.GetUserId();

        var response = await handler.HandleAsync(new GetPaymentsQuery(effectiveCustomerId), cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<Ok<PaymentResponse>> GetPaymentAsync(
        [Description("Unique identifier of the order whose payment to retrieve.")] Guid orderId,
        ClaimsPrincipal user,
        GetPaymentQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var response = await handler.HandleAsync(
            new GetPaymentQuery(orderId, user.GetUserId(), user.IsInRole("Admin")),
            cancellationToken
        );

        return TypedResults.Ok(response);
    }
}
