using Microsoft.JSInterop;

namespace ApexES.Blazor.ScreenObserver;

public class BrowserScreenService : IBrowserScreenService, IAsyncDisposable
{
    IJSRuntime _jsRuntime;
    DotNetObjectReference<BrowserScreenService>? _dotNetReference;
    bool _disposed = false;
    Dictionary<string, List<Action<int>>> _elementSubscriptions = new();
    List<Action<int>> _screenSubscriptions = new();

    public BrowserScreenService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _dotNetReference = DotNetObjectReference.Create(this);
    }

    // Subscribe to element resize events
    public async Task<int> ObserveElement(string elementId, Action<int> callback)
    {
        if (string.IsNullOrEmpty(elementId))
        {
            throw new ArgumentException("Element ID cannot be null or empty", nameof(elementId));
        }

        // Add the callback to the subscribers list
        if (!_elementSubscriptions.ContainsKey(elementId))
        {
            _elementSubscriptions[elementId] = new List<Action<int>>();
        }
        _elementSubscriptions[elementId].Add(callback);

        // Start observing if this is the first subscriber
        if (_elementSubscriptions[elementId].Count == 1)
        {
            try
            {
                // Start observing and get initial width
                return await _jsRuntime.InvokeAsync<int>(
                    "browserScreenService.observeElement",
                    _dotNetReference,
                    elementId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting observation for element '{elementId}': {ex.Message}");
                return 0;
            }
        }
        else
        {
            // Already observing, just return current width
            try
            {
                return await _jsRuntime.InvokeAsync<int>(
                    "document.getElementById",
                    elementId,
                    "offsetWidth");
            }
            catch
            {
                return 0;
            }
        }
    }

    // Subscribe to screen/body resize events
    public async Task<int> ObserveScreenSize(Action<int> callback)
    {
        // Add the callback to the subscribers list
        _screenSubscriptions.Add(callback);

        // Start observing if this is the first subscriber
        if (_screenSubscriptions.Count == 1)
        {
            try
            {
                // Start observing and get initial width
                return await _jsRuntime.InvokeAsync<int>(
                    "browserScreenService.observeScreenSize",
                    _dotNetReference);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting observation for screen: {ex.Message}");
                return 0;
            }
        }
        else
        {
            // Already observing, just return current width
            try
            {
                return await _jsRuntime.InvokeAsync<int>(
                    "document.body.offsetWidth");
            }
            catch
            {
                return 0;
            }
        }
    }

    // Unsubscribe from element resize events
    public async Task StopObservingElement(string elementId, Action<int> callback)
    {
        if (!_elementSubscriptions.ContainsKey(elementId))
            return;

        _elementSubscriptions[elementId].Remove(callback);

        // If no more subscribers, stop observing
        if (_elementSubscriptions[elementId].Count == 0)
        {
            _elementSubscriptions.Remove(elementId);
            await _jsRuntime.InvokeVoidAsync("browserScreenService.stopObserving", elementId);
        }
    }

    // Unsubscribe from screen resize events
    public async Task StopObservingScreen(Action<int> callback)
    {
        _screenSubscriptions.Remove(callback);

        // If no more subscribers, stop observing
        if (_screenSubscriptions.Count == 0)
        {
            await _jsRuntime.InvokeVoidAsync("browserScreenService.stopObservingScreen");
        }
    }

    // This method is called from JavaScript for element resize
    [JSInvokable]
    public void OnElementResized(string elementId, int width)
    {
        if (_elementSubscriptions.TryGetValue(elementId, out var callbacks))
        {
            foreach (var callback in callbacks)
            {
                callback(width);
            }
        }
    }

    // This method is called from JavaScript for screen resize
    [JSInvokable]
    public void OnScreenResized(int width)
    {
        foreach (var callback in _screenSubscriptions)
        {
            callback(width);
        }
    }

    // Dispose method to clean up resources
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        try
        {
            await _jsRuntime.InvokeVoidAsync("browserScreenService.dispose");
            _elementSubscriptions.Clear();
            _screenSubscriptions.Clear();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disposing BrowserScreenService: {ex.Message}");
        }
        finally
        {
            _dotNetReference?.Dispose();
            _dotNetReference = null;
            _disposed = true;
        }
    }
}

