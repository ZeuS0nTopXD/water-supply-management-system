using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Domain.Entities;

public sealed class Notification : Entity
{
    private Notification()
    {
    }

    public Notification(int id, int residentId, string title, string message, NotificationType notificationType, DateTime createdAt)
        : base(id)
    {
        if (residentId <= 0) throw new DomainValidationException("A valid resident is required.");
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message)) throw new DomainValidationException("Notification title and message are required.");
        ResidentId = residentId;
        Title = title.Trim();
        Message = message.Trim();
        NotificationType = notificationType;
        CreatedAt = createdAt;
    }

    public int ResidentId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public NotificationType NotificationType { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    public void MarkRead() => ReadAt = DateTime.UtcNow;
}
