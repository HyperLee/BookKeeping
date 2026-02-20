namespace BookKeeping.Models;

/// <summary>
/// Marker interface for entities with audit timestamps.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Gets or sets the UTC timestamp when this entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this entity was last updated.
    /// </summary>
    DateTime UpdatedAt { get; set; }
}
