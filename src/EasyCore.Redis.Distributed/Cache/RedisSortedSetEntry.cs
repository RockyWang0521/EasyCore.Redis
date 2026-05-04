namespace EasyCore.Redis.Distributed
{
    /// <summary>
    /// A sorted-set member paired with its score.
    /// </summary>
    /// <param name="Member">Sorted-set member value.</param>
    /// <param name="Score">Score used for ordering.</param>
    public readonly record struct RedisSortedSetEntry(string Member, double Score);
}
