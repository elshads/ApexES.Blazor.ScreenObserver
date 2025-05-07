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
    <script src="_content/ApexES.Blazor.ScreenObserver/ScreenObserver.js"></script>
</body>
```

Add using statement to your `_Imports.razor` file:
```razor
@using ApexES.Blazor.ScreenObserver.Components
```

## Usage

### Example 1: Handling screen width changes

```csharp
@implements IAsyncDisposable
@inject IBrowserScreenService BrowserScreenService

<div id="redElement" class="red-element">
    <div id="blueElement" class="blue-element">
        @_blueElementWidth
    </div>
    @_redElementWidth (Max-Width: 1024px)
</div>

<ul>
    <li>Screen Width: @(_screenWidth)</li>
    <li>Red Element Width: @(_redElementWidth)</li>
    <li>Blue Element Width: @(_blueElementWidth)</li>
</ul>

<div>Or get the current width directly from the service: @BrowserScreenService.CurrentScreenWidth</div>

@code {
    int _screenWidth;
    int _redElementWidth;
    int _blueElementWidth;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Initialize the screen width and element widths
            // Subscribe to screen width changes
            _screenWidth = await BrowserScreenService.ObserveScreen();
            BrowserScreenService.ScreenWidthChanged += OnScreenChanged;

            // Initialize and Subscribe to element width changes
            _redElementWidth = await BrowserScreenService.ObserveElement("redElement", OnRedElementChanged);
            _blueElementWidth = await BrowserScreenService.ObserveElement("blueElement", OnBlueElementChanged);

            StateHasChanged();
        }
    }

    void OnScreenChanged(object? sender, int width)
    {
        _screenWidth = width;
        StateHasChanged();
    }

    void OnRedElementChanged(int width)
    {
        _redElementWidth = width;
        StateHasChanged();
    }

    void OnBlueElementChanged(int width)
    {
        _blueElementWidth = width;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        // Unsubscribe from screen width changes when the component is disposed
        BrowserScreenService.ScreenWidthChanged -= OnScreenChanged;

        // Unsubscribe from element changes when the component is disposed
        await BrowserScreenService.StopObservingElement("redElement", OnRedElementChanged);
        await BrowserScreenService.StopObservingElement("blueElement", OnBlueElementChanged);
    }
}


<style>
    .red-element {
        display: flex;
        align-items: center;
        max-width: 1024px;
        width: 100%;
        padding: 16px;
        color: white;
        background-color: red;
    }

    .blue-element {
        width: 50%;
        padding: 16px;
        margin-right: 16px;
        color: white;
        background-color: blue;
    }
</style>
```

### Example 2: MediaQuery component

`<AxMediaQuery>` public members:
- `int? MaxWidth`: The maximum width of the screen or element to match.
- `int? MinWidth`: The minimum width of the screen or element to match.
- `EventCallback<int> WidthChanged`: An event that is triggered when the width of the screen or element changes. Returns the width as an integer.
- `string? ElementId`: The ID of the HTML element to observe. If not provided, the screen width is observed.
- `RenderFragment<int>? ChildContent`: The content to render. int `context` is passed to the content, which represents the width of the screen or element.

Below is an example of how to use the `AxMediaQuery` component in different scenarios:

```csharp
<div id="testId" style="border:1px solid red; padding:1rem;">
    <AxMediaQuery ElementId="testId">
        <div>Element width: @context</div>
    </AxMediaQuery>
</div>


<AxMediaQuery MaxWidth="540">
    <div>Screen width XS: @context</div>
</AxMediaQuery>

<AxMediaQuery MinWidth="541" MaxWidth="780">
    <div>Screen width SM: @context</div>
</AxMediaQuery>

<AxMediaQuery MinWidth="781" MaxWidth="968">
    <div>Screen width MD: @context</div>
</AxMediaQuery>

<AxMediaQuery MinWidth="969" MaxWidth="1200">
    <div>Screen width LG: @context</div>
</AxMediaQuery>

<AxMediaQuery MinWidth="1201">
    <div>Screen width XL: @context</div>
</AxMediaQuery>

<AxMediaQuery>
    @if (context > 1200)
    {
    <div>XL result: @context</div>
    }
    else if (context > 968)
    {
        <div>LG result: @context</div>
    }
    else if (context > 780)
    {
        <div>MD result: @context</div>
    }
    else if (context > 540)
    {
        <div>SM result: @context</div>
    }
    else
    {
        <div>XS result: @context</div>
    }
</AxMediaQuery>

<AxMediaQuery WidthChanged="OnWidthChanged" />
<div>Width changed: @_width</div>


@code {
    int _width;

    void OnWidthChanged(int width)
    {
        _width = width;
    }
}
```