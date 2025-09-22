// /wwwroot/js/scroll-handler.js
export function registerScrollHandler(containerId, barId, fabId) {
    const el = containerId ? document.getElementById(containerId) : null;
    const useWindow = !el || el.scrollHeight <= el.clientHeight;
    const container = useWindow ? window : el;

    const bar = barId ? document.getElementById(barId) : null;
    const fab = fabId ? document.getElementById(fabId) : null;

    // keep CSS var in sync with the real bar height
    const setBarHeightVar = () => {
        const h = bar ? bar.getBoundingClientRect().height : 56;
        document.documentElement.style.setProperty('--bottom-bar-h', `${h}px`);
    };
    setBarHeightVar();
    window.addEventListener('resize', setBarHeightVar);

    let lastY = useWindow
        ? (window.scrollY || document.documentElement.scrollTop || 0)
        : el.scrollTop;

    const THRESHOLD = 8;

    const setHidden = (hidden) => {
        if (bar) bar.classList.toggle('hide', hidden);
        if (fab) fab.classList.toggle('hide', hidden);
    };

    function onScroll() {
        const y = useWindow
            ? (window.scrollY || document.documentElement.scrollTop || 0)
            : el.scrollTop;

        if (y < 24) {
            setHidden(false);     // near top -> show
            lastY = y;
            return;
        }
        const dy = y - lastY;
        if (Math.abs(dy) >= THRESHOLD) {
            setHidden(dy > 0);    // down -> hide, up -> show
            lastY = y;
        }
    }

    container.addEventListener('scroll', onScroll, { passive: true });
    onScroll();

    return {
        dispose() {
            container.removeEventListener('scroll', onScroll);
            window.removeEventListener('resize', setBarHeightVar);
        }
    };
}
