using App.Application.Data;
using MediatR;

namespace App.Application.EventDispatcher;
public class DomainEventDispatcher
{
    private readonly IMediator _mediator;

    public DomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DispatchEvents(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var domainEntities = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        domainEntities.ForEach(x => x.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            Console.WriteLine("NEW DOMAIN EVEEEEEEEEEENT !!!!!!!!!!" + domainEvent);
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
