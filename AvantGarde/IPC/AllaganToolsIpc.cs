using System;
using Dalamud.Plugin.Ipc;

namespace AvantGarde.IPC;

public class AllaganToolsIpc
{
    private readonly ICallGateSubscriber<bool>? _initializedSubscriber = null;
    private readonly ICallGateSubscriber<uint, bool, uint[], bool, uint> _itemCountOwnedByCategorySub;
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
        _itemCountOwnedByCategorySub = Service.PluginInterface.GetIpcSubscriber<uint, bool, uint[], bool, uint>("AllaganTools.ItemCountOwnedByCategory");
    }

    /// <summary>
    /// Utilizes Allagan Tools' /moreinfo command to open an informative window for the given item.
    /// </summary>
    /// <param name="itemId">ID of item to query</param>
    public void OpenMoreInformation(uint itemId)
    {
        if (IsAvailable)
        {
            Service.CommandManager.ProcessCommand("/moreinfo " + itemId);
        }
    }

    /// <summary>
    /// Counts how much of an item is owned by the current character, and returns whether said count is above zero.
    /// Searches for both NQ and HQ items.
    /// If IPC is not available, this function will always return `true`.
    /// </summary>
    /// <param name="itemId">ID of item to look for</param>
    /// <param name="includeSharedStorage">Whether to include free company chests and housing storage</param>
    public bool FindOwnedItem(uint itemId, bool includeSharedStorage = false)
    {
        if (!IsAvailable || !_itemCountOwnedByCategorySub.HasFunction)
            return true;

        return _itemCountOwnedByCategorySub.InvokeFunc(itemId, true, [], includeSharedStorage) > 0;
    }
}
