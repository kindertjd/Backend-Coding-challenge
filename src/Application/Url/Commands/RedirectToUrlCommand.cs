using FluentValidation;
using HashidsNet;
using MediatR;
using UrlShortenerService.Application.Common.Interfaces;

namespace UrlShortenerService.Application.Url.Commands;

public record RedirectToUrlCommand : IRequest<string>
{
    public string Id { get; init; } = default!;
}

public class RedirectToUrlCommandValidator : AbstractValidator<RedirectToUrlCommand>
{
    public RedirectToUrlCommandValidator()
    {
        _ = RuleFor(v => v.Id)
          .NotEmpty()
          .WithMessage("Id is required.");
    }
}

public class RedirectToUrlCommandHandler : IRequestHandler<RedirectToUrlCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IHashids _hashids;

    public RedirectToUrlCommandHandler(IApplicationDbContext context, IHashids hashids)
    {
        _context = context;
        _hashids = hashids;
    }

    public async Task<string> Handle(RedirectToUrlCommand request, CancellationToken cancellationToken)
    {
        // 1. Decode the short key to get the original database ID
        var ids = _hashids.DecodeLong(request.Id);
        if (ids == null || ids.Length == 0)
        {
            // Invalid or non-existent short key
            return null!;
        }

        var urlId = ids[0];

        // 2. Look up the original URL by ID
        var urlEntity = await _context.Urls.FindAsync(new object[] { urlId }, cancellationToken);

        // 3. Return the original URL if found, otherwise null
        return urlEntity?.OriginalUrl ?? null!;
    }
}
