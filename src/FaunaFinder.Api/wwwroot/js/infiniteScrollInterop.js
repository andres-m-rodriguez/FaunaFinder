"use strict";
const infiniteScrollInterop = {
    observer: null,
    observe(element, dotNetHelper) {
        this.observer = new IntersectionObserver(async (entries) => {
            if (entries[0].isIntersecting) {
                await dotNetHelper.invokeMethodAsync('LoadMore');
            }
        }, { threshold: 0.1 });
        this.observer.observe(element);
    },
    disconnect() {
        if (this.observer) {
            this.observer.disconnect();
            this.observer = null;
        }
    }
};
// Expose to window for Blazor interop
window.infiniteScrollInterop = infiniteScrollInterop;
