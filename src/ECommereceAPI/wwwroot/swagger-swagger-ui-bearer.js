(function () {
  const SCRIPT_ID = 'swagger-bearer-injector';
  const TOPBAR_ID = 'swagger-bearer-topbar';
  const INPUT_ID = 'input_bearer_token_topbar';

  function createUI() {
    if (document.getElementById(SCRIPT_ID)) return null;

    const div = document.createElement('div');
    div.id = SCRIPT_ID;
    div.style.display = 'flex';
    div.style.alignItems = 'center';
    div.style.gap = '8px';

    const input = document.createElement('input');
    input.id = 'input_bearer_token';
    input.placeholder = 'JWT token';
    input.style.width = '320px';
    input.style.padding = '6px 8px';
    input.style.borderRadius = '4px';
    input.style.border = '1px solid #ccc';
    input.style.background = 'white';

    const btn = document.createElement('button');
    btn.id = 'btn_bearer';
    btn.textContent = 'Set Bearer';
    btn.style.padding = '6px 10px';
    btn.style.borderRadius = '4px';

    const btnClear = document.createElement('button');
    btnClear.id = 'btn_clear_bearer';
    btnClear.textContent = 'Clear';
    btnClear.style.padding = '6px 10px';
    btnClear.style.borderRadius = '4px';

    div.appendChild(input);
    div.appendChild(btn);
    div.appendChild(btnClear);

    // load saved token if exists
    const saved = localStorage.getItem('bearer_token') || '';
    if (saved) input.value = saved.replace(/^Bearer\s+/i, '');

    btn.addEventListener('click', function () {
      const token = input.value.trim();
      if (!token) return;
      const bearer = 'Bearer ' + token;
      localStorage.setItem('bearer_token', bearer);
      if (window.ui && typeof window.ui.preauthorizeApiKey === 'function') {
        try { window.ui.preauthorizeApiKey('Bearer', bearer); } catch { }
      }
      console.info('Bearer token set for Swagger UI requests');
    });

    btnClear.addEventListener('click', function () {
      localStorage.removeItem('bearer_token');
      input.value = '';
      console.info('Bearer token cleared');
    });

    return div;
  }

  function createTopbarNode() {
    if (document.getElementById(TOPBAR_ID)) return document.getElementById(TOPBAR_ID);

    const wrapper = document.createElement('div');
    wrapper.id = TOPBAR_ID;
    wrapper.style.display = 'flex';
    wrapper.style.alignItems = 'center';
    wrapper.style.gap = '8px';
    wrapper.style.marginLeft = '12px';

    const input = document.createElement('input');
    input.id = INPUT_ID;
    input.placeholder = 'JWT token';
    input.style.width = '340px';
    input.style.padding = '6px 8px';
    input.style.borderRadius = '4px';
    input.style.border = '1px solid rgba(0,0,0,0.2)';

    const btn = document.createElement('button');
    btn.id = 'btn_bearer_topbar';
    btn.textContent = 'Set Bearer';
    btn.style.padding = '6px 10px';
    btn.style.borderRadius = '4px';

    const btnClear = document.createElement('button');
    btnClear.id = 'btn_clear_bearer_topbar';
    btnClear.textContent = 'Clear';
    btnClear.style.padding = '6px 10px';
    btnClear.style.borderRadius = '4px';

    wrapper.appendChild(input);
    wrapper.appendChild(btn);
    wrapper.appendChild(btnClear);

    // populate saved token
    const saved = localStorage.getItem('bearer_token') || '';
    if (saved) input.value = saved.replace(/^Bearer\s+/i, '');

    btn.addEventListener('click', function () {
      const token = input.value.trim();
      if (!token) return;
      const bearer = 'Bearer ' + token;
      localStorage.setItem('bearer_token', bearer);
      if (window.ui && typeof window.ui.preauthorizeApiKey === 'function') {
        try { window.ui.preauthorizeApiKey('Bearer', bearer); } catch { }
      }
      console.info('Bearer token set for Swagger UI requests (topbar)');
    });

    btnClear.addEventListener('click', function () {
      localStorage.removeItem('bearer_token');
      input.value = '';
      console.info('Bearer token cleared (topbar)');
    });

    return wrapper;
  }

  function tryInsert(node) {
    const selectors = [
      '.swagger-ui .topbar',
      '.topbar',
      '.swagger-ui > .topbar',
      '.swagger-container .topbar'
    ];
    for (const sel of selectors) {
      const el = document.querySelector(sel);
      if (el) {
        // ensure wrapper styling does not break topbar
        el.style.display = 'flex';
        el.style.alignItems = 'center';
        el.appendChild(node);
        console.info('Swagger bearer input inserted into topbar using selector', sel);
        return true;
      }
    }
    return false;
  }

  // Intercept fetch to add Authorization header
  (function hookFetch() {
    const originalFetch = window.fetch;
    window.fetch = function (input, init) {
      try {
        const bearer = localStorage.getItem('bearer_token');
        if (bearer) {
          init = init || {};
          init.headers = init.headers || {};
          if (init.headers instanceof Headers) {
            init.headers.set('Authorization', bearer);
          } else if (Array.isArray(init.headers)) {
            init.headers.push(['Authorization', bearer]);
          } else {
            init.headers['Authorization'] = bearer;
          }
        }
      } catch { }
      return originalFetch(input, init);
    };
  })();

  // Create node and try immediate insertion
  const node = createTopbarNode();
  if (tryInsert(node)) return;

  // If not inserted, observe DOM for topbar
  const observer = new MutationObserver((mutations, obs) => {
    if (tryInsert(node)) {
      obs.disconnect();
    }
  });

  observer.observe(document.documentElement || document.body, { childList: true, subtree: true });

  // Fallback: expose helper on window so user can manually insert from console
  window.__swaggerBearerHelper = {
    insertNow: function () { tryInsert(node); },
    inputId: INPUT_ID
  };

})();
