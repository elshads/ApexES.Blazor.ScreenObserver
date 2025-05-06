# ApexES.Blazor.ScreenObserver

An open-source Blazor library that returns the width of screen or any other html element.

## Installation

```csharp
dotnet add package ApexES.Blazor.ScreenObserver
```

## Setup

Register the service in your `Program.cs` file:
```csharp
using ApexES.Blazor.ScreenObserver;
//...
builder.Services.AddScreenObserver();
```

Add the JavaScript file to your project. You can do this by adding the following line to your `_Host.cshtml`, `App.razor` or `index.html` file:
```html
<body>
    @* ... *@
    <script src="_content/ApexES.Blazor.ScreenObserver/screenObserver.js"></script>
</body>
```

## Usage

```csharp
@implements IAsyncDisposable
@using ApexES.Blazor.ScreenObserver
@inject IBrowserScreenService ScreenService


<div id="parentElement" style="max-width:800px;width:100%;padding:16px;background-color:red;">
    <div id="childElement" style="width:50%;padding:16px;background-color:blue;"></div>
</div>

<ul>
    <li>Screen Width: @(_screenWidth)</li>
    <li>Parent Width: @(_parentWidth)</li>
    <li>Child Width: @(_childWidth)</li>
</ul>


@code {
    int _screenWidth;
    int _parentWidth;
    int _childWidth;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Subscribe to screen size changes
            _screenWidth = await ScreenService.ObserveScreenSize(OnScreenChanged);
            _parentWidth = await ScreenService.ObserveElement("parentElement", OnParentElementChanged);
            _childWidth = await ScreenService.ObserveElement("childElement", OnChildElementChanged);

            StateHasChanged();
        }
    }

    void OnScreenChanged(int width)
    {
        _screenWidth = width;
        StateHasChanged();
    }

    void OnParentElementChanged(int width)
    {
        _parentWidth = width;
        StateHasChanged();
    }

    void OnChildElementChanged(int width)
    {
        _childWidth = width;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        // Unsubscribe when component is disposed
        await ScreenService.StopObservingScreen(OnScreenChanged);
        await ScreenService.StopObservingElement("parentElement", OnParentElementChanged);
        await ScreenService.StopObservingElement("childElement", OnChildElementChanged);

        // Or..
        // Dispose all
        await ScreenService.DisposeAsync();
    }
}
```