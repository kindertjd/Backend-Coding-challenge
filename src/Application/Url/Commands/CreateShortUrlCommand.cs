using FluentValidation;
using HashidsNet;
using MediatR;
using UrlShortenerService.Application.Common.Interfaces;

namespace UrlShortenerService.Application.Url.Commands;

public record CreateShortUrlCommand : IRequest<string>
{
    public string Url { get; init; } = default!;
}

public class CreateShortUrlCommandValidator : AbstractValidator<CreateShortUrlCommand>
{
    public CreateShortUrlCommandValidator()
    {
        _ = RuleFor(v => v.Url)
          .NotEmpty()
          .WithMessage("Url is required.");
    }
}

public class CreateShortUrlCommandHandler : IRequestHandler<CreateShortUrlCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IHashids _hashids;

    public CreateShortUrlCommandHandler(IApplicationDbContext context, IHashids hashids)
    {
        _context = context;
        _hashids = hashids;
    }

    public async Task<string> Handle(CreateShortUrlCommand request, CancellationToken cancellationToken)
    {
        // 1. Store the original URL in the database
        var urlEntity = new Domain.Entities.Url
        {
            OriginalUrl = request.Url
        };

        _context.Urls.Add(urlEntity);
        await _context.SaveChangesAsync(cancellationToken);

        // 2. Generate a unique short key using Hashids and the entity's ID
        var shortKey = _hashids.EncodeLong(urlEntity.Id);

        // 3. Return the full short URL (adjust base URL as needed)
        var baseUrl = "https://localhost:5246/u/"; // Change to your deployed base URL if needed
        return $"{baseUrl}{shortKey}";
    }
}
