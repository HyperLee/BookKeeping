namespace BookKeeping.Models;

/// <summary>
/// Marker interface for entities that support soft delete.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets or sets a value indicating whether this entity is deleted.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this entity was deleted.
    /// </summary>
    DateTime? DeletedAt { get; set; }
}
