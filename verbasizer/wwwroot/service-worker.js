const CACHE_NAME = "verbasizer-v1";
const APP_SHELL = [
    "/",
    "/about",
    "/manifest.webmanifest",
    "/pwa-192.png",
    "/pwa-512.png",
    "/apple-touch-icon.png",
    "/favicon.png",
    "/app.css",
    "/bootstrap/bootstrap.min.css"
];

self.addEventListener("install", event => {
    event.waitUntil(
        caches.open(CACHE_NAME).then(cache => cache.addAll(APP_SHELL))
    );

    self.skipWaiting();
});

self.addEventListener("activate", event => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(
                keys
                    .filter(key => key !== CACHE_NAME)
                    .map(key => caches.delete(key))
            ))
            .then(() => self.clients.claim())
    );
});

self.addEventListener("fetch", event => {
    const request = event.request;
    const url = new URL(request.url);

    if (request.method !== "GET" || url.origin !== self.location.origin) {
        return;
    }

    if (url.pathname.startsWith("/_framework/") || url.pathname.startsWith("/_blazor")) {
        return;
    }

    if (request.mode === "navigate") {
        event.respondWith(
            fetch(request)
                .then(response => {
                    const copy = response.clone();
                    caches.open(CACHE_NAME).then(cache => cache.put("/", copy));
                    return response;
                })
                .catch(() => caches.match(request).then(match => match || caches.match("/")))
        );

        return;
    }

    event.respondWith(
        caches.match(request).then(cachedResponse => {
            if (cachedResponse) {
                return cachedResponse;
            }

            return fetch(request).then(networkResponse => {
                const copy = networkResponse.clone();
                caches.open(CACHE_NAME).then(cache => cache.put(request, copy));
                return networkResponse;
            });
        })
    );
});
