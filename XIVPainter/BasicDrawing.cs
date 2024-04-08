using Dalamud.Plugin.Services;

namespace XIVPainter;

/// <summary>
/// The basic drawing thing.
/// </summary>
public abstract class BasicDrawing : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// If it is enabled.
    /// </summary>
    public virtual bool Enable { get; set; } = true;

    /// <summary>
    /// The time that it will disappear.
    /// </summary>
    public DateTime DeadTime { get; set; } = DateTime.MinValue;

    private protected BasicDrawing()
    {
        Service.Framework.Update += Framework_Update;
    }

    private void Framework_Update(IFramework framework)
    {
        if (DeadTime != DateTime.MinValue && DeadTime < DateTime.Now)
        {
            Dispose();
            return;
        }

        AdditionalUpdate();
    }

    private protected virtual void AdditionalUpdate()
    {

    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if(_disposed) return;
        _disposed = true;

        Service.Framework.Update -= Framework_Update;
        AdditionalDispose();
        GC.SuppressFinalize(this);
    }

    private protected virtual void AdditionalDispose()
    {

    }
}
