namespace ApexES.Blazor.ScreenObserver;

public interface IBrowserScreenService
{
    Task<int> ObserveElement(string elementId, Action<int> callback);
    Task<int> ObserveScreenSize(Action<int> callback);
    Task StopObservingElement(string elementId, Action<int> callback);
    Task StopObservingScreen(Action<int> callback);
    void OnElementResized(string elementId, int width);
    void OnScreenResized(int width);
    ValueTask DisposeAsync();
}
