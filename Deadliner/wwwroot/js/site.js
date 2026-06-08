(function () {
    const STORAGE_KEY = 'deadliner-theme';

    function getPreferredTheme() {
        const saved = localStorage.getItem(STORAGE_KEY);
        if (saved === 'light' || saved === 'dark') {
            return saved;
        }
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        document.querySelectorAll('.theme-toggle').forEach(function (toggle) {
            toggle.setAttribute('aria-pressed', theme === 'dark' ? 'true' : 'false');
            toggle.setAttribute('title', theme === 'dark' ? 'Светлая тема' : 'Тёмная тема');
        });
    }

    function toggleTheme() {
        const current = document.documentElement.getAttribute('data-theme') || 'light';
        const next = current === 'dark' ? 'light' : 'dark';
        localStorage.setItem(STORAGE_KEY, next);
        applyTheme(next);
    }

    function prefersReducedMotion() {
        return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    }

    function initActiveNav() {
        const path = window.location.pathname.toLowerCase();

        document.querySelectorAll('.header__link').forEach(function (link) {
            const href = (link.getAttribute('href') || '').toLowerCase();
            if (!href) return;

            const controller = href.split('/').filter(Boolean)[0] || 'dashboard';
            const isDashboard = controller === 'dashboard';
            const isMatch = isDashboard
                ? path === '/' || path.includes('/dashboard')
                : path.includes('/' + controller);

            if (isMatch) {
                link.classList.add('header__link--active');
            }
        });
    }

    function initScrollProgress() {
        const bar = document.querySelector('.scroll-progress');
        if (!bar) return;

        function update() {
            const scrollTop = window.scrollY;
            const docHeight = document.documentElement.scrollHeight - window.innerHeight;
            const progress = docHeight > 0 ? (scrollTop / docHeight) * 100 : 0;
            bar.style.width = progress + '%';
        }

        window.addEventListener('scroll', update, { passive: true });
        update();
    }

    function initCursorGlow() {
        const glow = document.querySelector('.cursor-glow');
        if (!glow || prefersReducedMotion() || window.matchMedia('(max-width: 768px)').matches) {
            return;
        }

        let visible = false;
        document.addEventListener('mousemove', function (e) {
            glow.style.left = e.clientX + 'px';
            glow.style.top = e.clientY + 'px';
            if (!visible) {
                visible = true;
                document.body.classList.add('cosmic-ready');
            }
        }, { passive: true });

        document.addEventListener('mouseleave', function () {
            glow.style.opacity = '0';
        });

        document.addEventListener('mouseenter', function () {
            glow.style.opacity = '';
        });
    }

    function initStarfield() {
        const canvas = document.querySelector('.cosmic-bg__canvas');
        if (!canvas || prefersReducedMotion()) return;

        const ctx = canvas.getContext('2d');
        if (!ctx) return;

        let stars = [];
        let width = 0;
        let height = 0;
        let animationId = 0;

        function resize() {
            width = window.innerWidth;
            height = window.innerHeight;
            canvas.width = width;
            canvas.height = height;

            const count = Math.min(180, Math.floor((width * height) / 8000));
            stars = Array.from({ length: count }, function () {
                return {
                    x: Math.random() * width,
                    y: Math.random() * height,
                    radius: Math.random() * 1.4 + 0.3,
                    speed: Math.random() * 0.25 + 0.05,
                    alpha: Math.random() * 0.6 + 0.2,
                    twinkle: Math.random() * Math.PI * 2
                };
            });
        }

        function draw() {
            ctx.clearRect(0, 0, width, height);
            const isDark = document.documentElement.getAttribute('data-theme') === 'dark';

            stars.forEach(function (star) {
                star.y -= star.speed;
                star.twinkle += 0.02;

                if (star.y < -2) {
                    star.y = height + 2;
                    star.x = Math.random() * width;
                }

                const flicker = 0.5 + Math.sin(star.twinkle) * 0.5;
                ctx.beginPath();
                ctx.arc(star.x, star.y, star.radius, 0, Math.PI * 2);
                ctx.fillStyle = isDark
                    ? 'rgba(200, 215, 255, ' + (star.alpha * flicker) + ')'
                    : 'rgba(74, 108, 247, ' + (star.alpha * flicker * 0.45) + ')';
                ctx.fill();
            });

            animationId = requestAnimationFrame(draw);
        }

        resize();
        draw();

        window.addEventListener('resize', function () {
            cancelAnimationFrame(animationId);
            resize();
            draw();
        });
    }

    function animateCounter(element) {
        const target = parseInt(element.textContent, 10);
        if (isNaN(target) || target === 0) return;

        const duration = 900;
        const start = performance.now();

        function step(now) {
            const progress = Math.min((now - start) / duration, 1);
            const eased = 1 - Math.pow(1 - progress, 3);
            element.textContent = Math.round(target * eased);
            if (progress < 1) {
                requestAnimationFrame(step);
            }
        }

        element.textContent = '0';
        requestAnimationFrame(step);
    }

    function initCounterAnimation() {
        if (prefersReducedMotion()) return;

        document.querySelectorAll('.counter__value').forEach(function (el) {
            animateCounter(el);
        });
    }

    function initCardTilt() {
        if (prefersReducedMotion() || window.matchMedia('(max-width: 768px)').matches) return;

        const cards = document.querySelectorAll('.counter, .task-card, .tag-card');
        cards.forEach(function (card) {
            card.addEventListener('mousemove', function (e) {
                const rect = card.getBoundingClientRect();
                const x = (e.clientX - rect.left) / rect.width - 0.5;
                const y = (e.clientY - rect.top) / rect.height - 0.5;
                card.style.transform = 'perspective(600px) rotateY(' + (x * 6) + 'deg) rotateX(' + (-y * 6) + 'deg) translateY(-3px)';
            });

            card.addEventListener('mouseleave', function () {
                card.style.transform = '';
            });
        });
    }

    applyTheme(getPreferredTheme());

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('.theme-toggle').forEach(function (button) {
            button.addEventListener('click', toggleTheme);
        });

        initActiveNav();
        initScrollProgress();
        initCursorGlow();
        initStarfield();
        initCounterAnimation();
        initCardTilt();
    });
})();
