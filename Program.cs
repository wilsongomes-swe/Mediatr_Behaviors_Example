using MediatR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CreateNewCompanyHandler).Assembly)
        .AddBehavior<AddHashBehavior>()
        .AddBehavior<AddKeyBehavior>());

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapPost("/companies", (CreateNewCompanyInput input, IMediator mediator) 
    => mediator.Send(input, default));

app.Run();

// input and output models
public record CreateCompanyOutput(Guid id, string name, string address, string key, string hash);

public class CreateNewCompanyInput(string name, string address, string key, string hash)
    : IRequest<CreateCompanyOutput>
{
    public string Name { get; init; } = name;
    public string Address { get; init; } = address;
    public string Key { get; set; } = key;
    public string Hash { get; set; } = hash;
}

// handler
public class CreateNewCompanyHandler(ILogger<CreateNewCompanyHandler> _logger) : IRequestHandler<CreateNewCompanyInput, CreateCompanyOutput>
{
    public Task<CreateCompanyOutput> Handle(CreateNewCompanyInput request, CancellationToken cancellationToken)
    {
        // processed...
        _logger.LogInformation("Creating new company");
        var output = new CreateCompanyOutput(Guid.NewGuid(), request.Name, request.Address, request.Key, request.Hash);
        return Task.FromResult(output);
    }
}

// Behaviors
public class AddKeyBehavior(ILogger<AddKeyBehavior> _logger) : 
    IPipelineBehavior<CreateNewCompanyInput, CreateCompanyOutput>
{
    public async Task<CreateCompanyOutput> Handle(CreateNewCompanyInput req, RequestHandlerDelegate<CreateCompanyOutput> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding key to request");
        req.Key = $"123456789-{Guid.NewGuid().ToString()[..20]}";
        var output = await next();
        return output;
    }
}

public class AddHashBehavior(ILogger<AddHashBehavior> _logger) : 
    IPipelineBehavior<CreateNewCompanyInput, CreateCompanyOutput>
{
    public Task<CreateCompanyOutput> Handle(CreateNewCompanyInput req, RequestHandlerDelegate<CreateCompanyOutput> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding hash to request");
        req.Hash = Guid.NewGuid().ToString();
        var output = next();
        return output;
    }
}