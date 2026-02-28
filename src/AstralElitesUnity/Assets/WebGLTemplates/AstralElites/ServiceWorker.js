const cacheName = "fydar-astralelites-v{{{ PRODUCT_VERSION }}}";
const contentToCache = [
    "./",
    "Build/{{{ LOADER_FILENAME }}}",
    "Build/{{{ FRAMEWORK_FILENAME }}}",
    "Build/{{{ DATA_FILENAME }}}",
    "Build/{{{ CODE_FILENAME }}}",
    "StreamingAssets/AssetBundles/music.data",
    "StreamingAssets/AssetBundles/sfx.data"
];

self.addEventListener('install', function (e) {
    e.waitUntil((async function () {
        const cache = await caches.open(cacheName);
        await cache.addAll(contentToCache);
    })());
});

const putInCache = async (request, response) => {
    const cache = await caches.open(cacheName);
    await cache.put(request, response);
};

const cacheFirst = async ({ request }) => {
    const responseFromCache = await caches.match(request);
    if (responseFromCache) {
        return responseFromCache;
    }

    try {
        const responseFromNetwork = await fetch(request);
        putInCache(request, responseFromNetwork.clone());
        return responseFromNetwork;
    } catch (error) {
        return new Response("Network error happened", {
            status: 408,
            headers: { "Content-Type": "text/plain" },
        });
    }
};

self.addEventListener("fetch", (event) => {
    event.respondWith(
        cacheFirst({
            request: event.request
        }),
    );
});
