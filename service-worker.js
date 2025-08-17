// Minimal Service Worker for KnjiznicaBlazor
// Prevents 404 errors during development

const CACHE_NAME = 'knjiznica-v1';

// Install event
self.addEventListener('install', event => {
    console.log('Service Worker: Installing...');
    self.skipWaiting();
});

// Activate event
self.addEventListener('activate', event => {
    console.log('Service Worker: Activated');
    self.clients.claim();
});

// Fetch event - basic implementation
self.addEventListener('fetch', event => {
    // Only handle GET requests
    if (event.request.method !== 'GET') {
        return;
    }

    // Let all requests pass through for now
    event.respondWith(
        fetch(event.request).catch(error => {
            console.log('Service Worker: Fetch failed', error);
            // Return a basic offline response if needed
            return new Response('Offline', { status: 503 });
        })
    );
});

console.log('📚 Service Worker: Knjižnica service worker loaded')