public record TaskAssignedEvent(Guid TaskId, Guid AssigneeId) : IDomainEvent;
