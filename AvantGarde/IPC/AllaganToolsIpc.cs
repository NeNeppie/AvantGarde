using System;
using Dalamud.Plugin.Ipc;

namespace AvantGarde.IPC;

public class AllaganToolsIpc
{
    private readonly ICallGateSubscriber<bool>? _initializedSubscriber = null;
    private long _lastRefresh = 0;

    public bool IsAvailable
    {
        get
        {
            // Refresh every 10 seconds
            if (_lastRefresh + 10_000 <= Environment.TickCount64)
            {
                if (!_initializedSubscriber?.HasFunction ?? false)
                {
                    field = false;
                }
                else
                {
                    field = _initializedSubscriber?.InvokeFunc() ?? false;
                }

                _lastRefresh = Environment.TickCount64;
            }

            return field;
        }
    } = false;

    public AllaganToolsIpc()
    {
        _initializedSubscriber = Service.PluginInterface.GetIpcSubscriber<bool>("AllaganTools.IsInitialized");
    }

    public void OpenMoreInformation(uint itemId)
    {
        if (IsAvailable)
        {
            Service.CommandManager.ProcessCommand("/moreinfo " + itemId);
        }
    }
}
