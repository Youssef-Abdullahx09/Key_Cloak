using FastEndpoints;

namespace KeyCloakSolution.Controllers;

public class EndpointTest : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Post("/create-item");
        AllowAnonymous();

    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await SendAsync(new Response { IsSuccess = false }, 400);

        await SendAsync(new Response()
        {
            Data = new { FullName = req.FirstName + " " + req.LastName },
            IsSuccess = true
        });
    }


}