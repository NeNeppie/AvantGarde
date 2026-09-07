using System;
using Dalamud.Plugin.Ipc;

namespace AvantGarde.IPC;

public class ItemVendorLocationIpc
{
    private readonly ICallGateSubscriber<uint, object?>? _openUiSubscriber = null;
    private long _lastRefresh = 0;

    public bool IsAvailable
    {
        get
        {
            // Refresh every 10 seconds
            if (_lastRefresh + 10_000 <= Environment.TickCount64)
            {
                field = _openUiSubscriber?.HasFunction ?? false;
                _lastRefresh = Environment.TickCount64;
            }

            return field;
        }
    } = false;

    public ItemVendorLocationIpc()
    {
        _openUiSubscriber = Service.PluginInterface.GetIpcSubscriber<uint, object?>("ItemVendorLocation.OpenVendorResults");
    }

    public void OpenVendorResults(uint itemId) => _openUiSubscriber?.InvokeFunc(itemId);
}
