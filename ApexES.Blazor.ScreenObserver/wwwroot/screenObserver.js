window.browserScreenService = {
    observers: {}, // Store multiple observers
    screenObserver: null, // Special observer for screen size

    observeElement: function (dotNetReference, elementId) {
        // Check if we're already observing this element
        if (this.observers[elementId]) {
            return Math.floor(document.getElementById(elementId)?.offsetWidth || 0);
        }

        const element = document.getElementById(elementId);
        if (!element) {
            console.warn(`Element with ID '${elementId}' not found.`);
            return 0;
        }

        // Debounce setup for this specific element
        let debounceTimer = null;
        const debounceInterval = 250;

        // Create a new observer for this element
        const resizeObserver = new ResizeObserver(entries => {
            const width = Math.floor(entries[0].target.offsetWidth);

            if (debounceTimer) {
                clearTimeout(debounceTimer);
            }

            debounceTimer = setTimeout(() => {
                dotNetReference.invokeMethodAsync('OnElementResized', elementId, width);
            }, debounceInterval);
        });

        // Start observing and store the observer
        resizeObserver.observe(element);
        this.observers[elementId] = {
            observer: resizeObserver,
            debounceTimer: debounceTimer
        };

        // Return initial width
        return Math.floor(element.offsetWidth);
    },

    observeScreenSize: function (dotNetReference) {
        // Check if we're already observing screen size
        if (this.screenObserver) {
            return Math.floor(document.body.offsetWidth);
        }

        // Debounce setup for screen
        let debounceTimer = null;
        const debounceInterval = 250;

        // Create a new observer for document.body
        const resizeObserver = new ResizeObserver(entries => {
            const width = Math.floor(document.body.offsetWidth);

            if (debounceTimer) {
                clearTimeout(debounceTimer);
            }

            debounceTimer = setTimeout(() => {
                dotNetReference.invokeMethodAsync('OnScreenResized', width);
            }, debounceInterval);
        });

        // Start observing and store the observer
        resizeObserver.observe(document.body);
        this.screenObserver = {
            observer: resizeObserver,
            debounceTimer: debounceTimer
        };

        // Return initial width
        return Math.floor(document.body.offsetWidth);
    },

    stopObserving: function (elementId) {
        const observerData = this.observers[elementId];
        if (observerData) {
            observerData.observer.disconnect();
            if (observerData.debounceTimer) {
                clearTimeout(observerData.debounceTimer);
            }
            delete this.observers[elementId];
        }
    },

    stopObservingScreen: function () {
        if (this.screenObserver) {
            this.screenObserver.observer.disconnect();
            if (this.screenObserver.debounceTimer) {
                clearTimeout(this.screenObserver.debounceTimer);
            }
            this.screenObserver = null;
        }
    },

    dispose: function () {
        // Clean up all observers
        Object.keys(this.observers).forEach(elementId => {
            this.stopObserving(elementId);
        });
        this.observers = {};

        // Clean up screen observer
        this.stopObservingScreen();
    }
};