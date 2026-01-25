interface DotNetObjectReference {
    invokeMethodAsync(methodName: string): Promise<void>;
}

interface InfiniteScrollInterop {
    observer: IntersectionObserver | null;
    isLoading: boolean;
    observe(element: Element, dotNetHelper: DotNetObjectReference): void;
    disconnect(): void;
}

const infiniteScrollInterop: InfiniteScrollInterop = {
    observer: null,
    isLoading: false,

    observe(element: Element, dotNetHelper: DotNetObjectReference): void {
        const self = this;
        this.observer = new IntersectionObserver(
            async (entries: IntersectionObserverEntry[]) => {
                if (entries[0].isIntersecting && !self.isLoading) {
                    self.isLoading = true;
                    try {
                        await dotNetHelper.invokeMethodAsync('LoadMore');
                    } finally {
                        // Small delay to prevent rapid re-triggering
                        setTimeout(() => {
                            self.isLoading = false;
                        }, 100);
                    }
                }
            },
            {
                threshold: 0.1,
                rootMargin: '100px' // Trigger slightly before element is fully visible
            }
        );
        this.observer.observe(element);
    },

    disconnect(): void {
        if (this.observer) {
            this.observer.disconnect();
            this.observer = null;
        }
        this.isLoading = false;
    }
};

// Expose to window for Blazor interop
(window as unknown as { infiniteScrollInterop: InfiniteScrollInterop }).infiniteScrollInterop = infiniteScrollInterop;
