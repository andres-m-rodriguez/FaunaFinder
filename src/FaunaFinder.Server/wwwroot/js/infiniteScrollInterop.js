"use strict";
const infiniteScrollInterop = {
    observer: null,
    isLoading: false,
    observe(element, dotNetHelper) {
        const self = this;
        this.observer = new IntersectionObserver(async (entries) => {
            if (entries[0].isIntersecting && !self.isLoading) {
                self.isLoading = true;
                try {
                    await dotNetHelper.invokeMethodAsync('LoadMore');
                }
                finally {
                    // Small delay to prevent rapid re-triggering
                    setTimeout(() => {
                        self.isLoading = false;
                    }, 100);
                }
            }
        }, {
            threshold: 0.1,
            rootMargin: '100px' // Trigger slightly before element is fully visible
        });
        this.observer.observe(element);
    },
    disconnect() {
        if (this.observer) {
            this.observer.disconnect();
            this.observer = null;
        }
        this.isLoading = false;
    }
};
// Expose to window for Blazor interop
window.infiniteScrollInterop = infiniteScrollInterop;
