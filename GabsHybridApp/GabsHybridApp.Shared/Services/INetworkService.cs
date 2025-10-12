namespace GabsHybridApp.Shared.Services;

public interface INetworkService
{
    bool IsOnline { get; }
    bool IsWifi { get; }
    bool IsCellular { get; }

    /// <summary>Refresh cached values from the host platform.</summary>
    Task RefreshAsync(CancellationToken ct = default);
    Task OpenSettingsAsync();
}

public sealed class NullNetworkService : INetworkService
{
    public bool IsOnline => true;
    public bool IsWifi => true;
    public bool IsCellular => true;

    public Task RefreshAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task OpenSettingsAsync() => Task.CompletedTask;
}
