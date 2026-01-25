interface DotNetObjectReference {
    invokeMethodAsync(methodName: string): Promise<void>;
}

interface InfiniteScrollInterop {
    observer: IntersectionObserver | null;
    observe(element: Element, dotNetHelper: DotNetObjectReference): void;
    disconnect(): void;
}

const infiniteScrollInterop: InfiniteScrollInterop = {
    observer: null,

    observe(element: Element, dotNetHelper: DotNetObjectReference): void {
        this.observer = new IntersectionObserver(
            async (entries: IntersectionObserverEntry[]) => {
                if (entries[0].isIntersecting) {
                    await dotNetHelper.invokeMethodAsync('LoadMore');
                }
            },
            { threshold: 0.1 }
        );
        this.observer.observe(element);
    },

    disconnect(): void {
        if (this.observer) {
            this.observer.disconnect();
            this.observer = null;
        }
    }
};

// Expose to window for Blazor interop
(window as unknown as { infiniteScrollInterop: InfiniteScrollInterop }).infiniteScrollInterop = infiniteScrollInterop;
