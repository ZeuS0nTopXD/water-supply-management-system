# Water Supply PWA

Phase 3 extends the ASP.NET application for mobile users. The PWA assets live in `ASPNETApplication/WaterSupply.Web/wwwroot` because authentication and business routes are shared with the server-rendered application.

- `manifest.webmanifest` enables installation and standalone display.
- `service-worker.js` caches the application shell and provides `/offline.html` when the network is unavailable.
- `js/pwa-notifications.js` registers the service worker and exposes a browser notification permission flow.
- Responsive Bootstrap forms and tables are used throughout the application.

Use HTTPS or localhost, open the web app in a PWA-capable browser, and choose the browser's install action.
