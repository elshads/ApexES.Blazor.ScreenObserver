using Microsoft.JSInterop;

namespace ApexES.Blazor.ScreenObserver;

public class BrowserScreenService : IBrowserScreenService, IAsyncDisposable
{
    IJSRuntime _jsRuntime;
    DotNetObjectReference<BrowserScreenService>? _dotNetReference;
    bool _disposed = false;
    Dictionary<string, List<Action<int>>> _elementSubscriptions = new();
    bool _isObservingScreen = false;

    public BrowserScreenService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _dotNetReference = DotNetObjectReference.Create(this);
    }

    public int CurrentScreenWidth { get; private set; }
    public event EventHandler<int>? ScreenWidthChanged;

    public async Task<int> ObserveElement(string elementId, Action<int> callback)
    {
        if (string.IsNullOrEmpty(elementId))
            throw new ArgumentException("Element ID cannot be null or empty", nameof(elementId));

        try
        {
            if (!_elementSubscriptions.ContainsKey(elementId))
            {
                _elementSubscriptions[elementId] = new List<Action<int>>();
            }

            if (!_elementSubscriptions[elementId].Contains(callback))
            {
                _elementSubscriptions[elementId].Add(callback);
            }

            var width = await _jsRuntime.InvokeAsync<int>(
                "browserScreenService.observeElement",
                _dotNetReference,
                elementId);

            return width;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error observing element '{elementId}': {ex.Message}");
            return 0;
        }
    }

    public async Task<int> ObserveScreen()
    {
        try
        {
            if (!_isObservingScreen)
            {
                CurrentScreenWidth = await _jsRuntime.InvokeAsync<int>(
                    "browserScreenService.observeScreen",
                    _dotNetReference);

                _isObservingScreen = true;
            }

            return CurrentScreenWidth;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error observing screen size {ex.Message}");
            return 0;
        }
    }

    public async Task StopObservingElement(string elementId, Action<int> callback)
    {
        if (!_elementSubscriptions.ContainsKey(elementId))
            return;

        _elementSubscriptions[elementId].Remove(callback);

        if (_elementSubscriptions[elementId].Count == 0)
        {
            _elementSubscriptions.Remove(elementId);
            await _jsRuntime.InvokeVoidAsync("browserScreenService.stopObservingElement", elementId);
        }
    }

    [JSInvokable]
    public void OnElementWidthChanged(string elementId, int width)
    {
        if (_elementSubscriptions.TryGetValue(elementId, out var callbacks))
        {
            foreach (var callback in callbacks.ToArray())
            {
                try
                {
                    callback(width);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in callback for element '{elementId}': {ex.Message}");
                }
            }
        }
    }

    [JSInvokable]
    public void OnScreenWidthChanged(int width)
    {
        CurrentScreenWidth = width;

        try
        {
            ScreenWidthChanged?.Invoke(this, width);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in screen resize event handler: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        try
        {
            await _jsRuntime.InvokeVoidAsync("browserScreenService.dispose");
            _elementSubscriptions.Clear();
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

