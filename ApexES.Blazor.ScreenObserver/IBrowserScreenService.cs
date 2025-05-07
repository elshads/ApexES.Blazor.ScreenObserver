namespace ApexES.Blazor.ScreenObserver;

public interface IBrowserScreenService
{
    int CurrentScreenWidth { get; }
    event EventHandler<int>? ScreenWidthChanged;
    Task<int> ObserveElement(string elementId, Action<int> callback);
    Task<int> ObserveScreen();
    Task StopObservingElement(string elementId, Action<int> callback);
    void OnElementWidthChanged(string elementId, int width);
    void OnScreenWidthChanged(int width);
    ValueTask DisposeAsync();
}
