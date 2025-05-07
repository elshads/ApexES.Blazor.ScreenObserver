window.browserScreenService = {
    observers: {},
    screenWidth: 0,
    hasScreenObserver: false,
    debounceTimer: null,
    debounceInterval: 250,

    observeElement: function (dotNetReference, elementId) {
        const element = document.getElementById(elementId);
        if (!element) {
            console.warn(`Element with ID '${elementId}' not found.`);
            return 0;
        }

        if (!this.observers[elementId]) {
            const resizeObserver = new ResizeObserver(entries => {
                const width = Math.floor(entries[0].target.offsetWidth);

                if (this.observers[elementId].debounceTimer) {
                    clearTimeout(this.observers[elementId].debounceTimer);
                }

                this.observers[elementId].debounceTimer = setTimeout(() => {
                    dotNetReference.invokeMethodAsync('OnElementWidthChanged', elementId, parseInt(width, 10));
                }, this.debounceInterval);
            });

            resizeObserver.observe(element);

            this.observers[elementId] = {
                observer: resizeObserver,
                dotNetReference: dotNetReference,
                debounceTimer: null
            };
        } else {
            this.observers[elementId].dotNetReference = dotNetReference;
        }
        
        return parseInt(Math.floor(element.offsetWidth), 10);
    },

    observeScreen: function (dotNetReference) {
        if (!this.hasScreenObserver) {
            const resizeObserver = new ResizeObserver(entries => {
                const width = Math.floor(document.body.offsetWidth);
                this.screenWidth = width;

                if (this.debounceTimer) {
                    clearTimeout(this.debounceTimer);
                }

                this.debounceTimer = setTimeout(() => {
                    dotNetReference.invokeMethodAsync('OnScreenWidthChanged', parseInt(width, 10));
                }, this.debounceInterval);
            });

            resizeObserver.observe(document.body);
            this._screenResizeObserver = resizeObserver;
            this.hasScreenObserver = true;
        }

        return parseInt(Math.floor(this.screenWidth || document.body.offsetWidth), 10);
    },

    stopObservingElement: function (elementId) {
        if (this.observers[elementId]) {
            this.observers[elementId].observer.disconnect();
            if (this.observers[elementId].debounceTimer) {
                clearTimeout(this.observers[elementId].debounceTimer);
            }
            delete this.observers[elementId];
        }
    },

    stopObservingScreen: function () {
        if (this._screenResizeObserver) {
            this._screenResizeObserver.disconnect();
            this._screenResizeObserver = null;
            this.hasScreenObserver = false;
        }

        if (this.debounceTimer) {
            clearTimeout(this.debounceTimer);
            this.debounceTimer = null;
        }
    },

    dispose: function () {
        Object.keys(this.observers).forEach(elementId => {
            if (this.observers[elementId]) {
                this.observers[elementId].observer.disconnect();
                if (this.observers[elementId].debounceTimer) {
                    clearTimeout(this.observers[elementId].debounceTimer);
                }
            }
        });
        this.observers = {};

        this.stopObservingScreen();
    }
};