/**
 * Header JavaScript Functions
 * Handles theme toggle, sticky header, search functionality, and responsive behavior
 */

$(document).ready(function () {
    // Initialize header functionality
    initializeHeader();
    initializeThemeToggle();
    initializeSearch();
    initializeStickyHeader();
    initializeNotifications();
    initializeResponsiveMenu();
});

/**
 * Initialize main header functionality
 */
function initializeHeader() {
    // Set active navigation item based on current URL
    setActiveNavigationItem();

    // Initialize tooltips
    initializeTooltips();

    // Handle dropdown menus
    initializeDropdowns();
}

/**
 * Set active navigation item based on current page
 */
function setActiveNavigationItem() {
    const currentPath = window.location.pathname.toLowerCase();

    $('.nav-link-custom').each(function () {
        const href = $(this).attr('href');
        if (href && currentPath.includes(href.toLowerCase())) {
            $(this).addClass('active');
        }
    });
}

/**
 * Initialize tooltips for better UX
 */
function initializeTooltips() {
    $('[title]').tooltip({
        placement: 'bottom',
        trigger: 'hover'
    });
}

/**
 * Initialize dropdown menus with custom behavior
 */
function initializeDropdowns() {
    // Close dropdown when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.dropdown').length) {
            $('.dropdown-menu').removeClass('show');
        }
    });

    // Add hover effect for desktop
    if (window.innerWidth > 991) {
        $('.dropdown').hover(
            function () {
                $(this).find('.dropdown-menu').addClass('show');
            },
            function () {
                $(this).find('.dropdown-menu').removeClass('show');
            }
        );
    }
}

/**
 * Theme toggle functionality
 */
function initializeThemeToggle() {
    const themeToggle = $('#themeToggle');
    const body = $('body');

    // Load saved theme
    const savedTheme = localStorage.getItem('theme') || 'light';
    setTheme(savedTheme);

    // Theme toggle click handler
    themeToggle.on('click', function () {
        const currentTheme = body.hasClass('dark-mode') ? 'dark' : 'light';
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';
        setTheme(newTheme);
        localStorage.setItem('theme', newTheme);
    });

    function setTheme(theme) {
        if (theme === 'dark') {
            body.addClass('dark-mode');
            themeToggle.find('i').removeClass('fa-moon').addClass('fa-sun');
            themeToggle.attr('title', 'Chuyển sang chế độ sáng');
        } else {
            body.removeClass('dark-mode');
            themeToggle.find('i').removeClass('fa-sun').addClass('fa-moon');
            themeToggle.attr('title', 'Chuyển sang chế độ tối');
        }
    }
}

/**
 * Search functionality
 */
function initializeSearch() {
    const searchForm = $('.search-form');
    const searchInput = $('.search-input');
    const searchBtn = $('.search-btn');

    // Search form submission
    searchForm.on('submit', function (e) {
        e.preventDefault();
        const query = searchInput.val().trim();

        if (query) {
            performSearch(query);
        }
    });

    // Search input focus/blur effects
    searchInput.on('focus', function () {
        $(this).parent().addClass('focused');
    }).on('blur', function () {
        $(this).parent().removeClass('focused');
    });

    // Auto-complete functionality (basic implementation)
    let searchTimeout;
    searchInput.on('input', function () {
        clearTimeout(searchTimeout);
        const query = $(this).val().trim();

        if (query.length > 2) {
            searchTimeout = setTimeout(() => {
                showSearchSuggestions(query);
            }, 300);
        } else {
            hideSearchSuggestions();
        }
    });

    function performSearch(query) {
        // Add loading state
        searchBtn.html('<i class="fas fa-spinner fa-spin"></i>');

        // Simulate search (replace with actual search implementation)
        setTimeout(() => {
            console.log('Searching for:', query);
            searchBtn.html('<i class="fas fa-search"></i>');

            // Redirect to search results page or show results
            // window.location.href = `/search?q=${encodeURIComponent(query)}`;
        }, 1000);
    }

    function showSearchSuggestions(query) {
        // Basic search suggestions (replace with actual API call)
        const suggestions = [
            'Mẫu CV chuyên nghiệp',
            'Tạo CV bằng AI',
            'Chỉnh sửa CV online',
            'Template CV miễn phí'
        ].filter(item => item.toLowerCase().includes(query.toLowerCase()));

        if (suggestions.length > 0) {
            let suggestionHtml = '<div class="search-suggestions">';
            suggestions.forEach(suggestion => {
                suggestionHtml += `<div class="suggestion-item">${suggestion}</div>`;
            });
            suggestionHtml += '</div>';

            // Show suggestions (implement dropdown)
            console.log('Suggestions:', suggestions);
        }
    }

    function hideSearchSuggestions() {
        $('.search-suggestions').remove();
    }
}

/**
 * Sticky header functionality
 */
function initializeStickyHeader() {
    const header = $('.main-header');
    let lastScrollTop = 0;

    $(window).scroll(function () {
        const scrollTop = $(this).scrollTop();

        // Add/remove scrolled class
        if (scrollTop > 50) {
            header.addClass('scrolled');
        } else {
            header.removeClass('scrolled');
        }

        // Hide/show header on scroll (optional)
        if (scrollTop > lastScrollTop && scrollTop > 100) {
            // Scrolling down
            header.css('transform', 'translateY(-100%)');
        } else {
            // Scrolling up
            header.css('transform', 'translateY(0)');
        }

        lastScrollTop = scrollTop;
    });
}

/**
 * Notification functionality
 */
function initializeNotifications() {
    const notificationDropdown = $('#notificationDropdown');
    const notificationBadge = $('.notification-badge');

    // Load notifications
    loadNotifications();

    // Mark notifications as read when dropdown is opened
    notificationDropdown.on('shown.bs.dropdown', function () {
        markNotificationsAsRead();
    });

    function loadNotifications() {
        // Simulate loading notifications (replace with actual API call)
        const notifications = [
            {
                id: 1,
                message: 'CV của bạn đã được tạo thành công',
                time: '2 phút trước',
                read: false
            },
            {
                id: 2,
                message: 'Có template mới được cập nhật',
                time: '1 giờ trước',
                read: false
            },
            {
                id: 3,
                message: 'Hệ thống sẽ bảo trì vào 2:00 AM',
                time: '3 giờ trước',
                read: true
            }
        ];

        updateNotificationBadge(notifications);
    }

    function updateNotificationBadge(notifications) {
        const unreadCount = notifications.filter(n => !n.read).length;

        if (unreadCount > 0) {
            notificationBadge.text(unreadCount).show();
        } else {
            notificationBadge.hide();
        }
    }

    function markNotificationsAsRead() {
        // Simulate marking as read (replace with actual API call)
        setTimeout(() => {
            notificationBadge.hide();
        }, 1000);
    }
}

/**
 * Responsive menu functionality
 */
function initializeResponsiveMenu() {
    const navbarToggler = $('.navbar-toggler');
    const navbarCollapse = $('.navbar-collapse');

    // Close mobile menu when clicking on nav links
    $('.nav-link').on('click', function () {
        if (window.innerWidth < 992) {
            navbarCollapse.collapse('hide');
        }
    });

    // Handle window resize
    $(window).resize(function () {
        if (window.innerWidth >= 992) {
            navbarCollapse.removeClass('show');
            initializeDropdowns(); // Re-initialize hover effects for desktop
        }
    });

    // Smooth scroll for anchor links
    $('a[href^="#"]').on('click', function (e) {
        const target = $(this.getAttribute('href'));
        if (target.length) {
            e.preventDefault();
            $('html, body').stop().animate({
                scrollTop: target.offset().top - 80
            }, 1000);
        }
    });
}

/**
 * Loading states for navigation links
 */
function showLoadingState(element) {
    $(element).addClass('loading').prop('disabled', true);
}

function hideLoadingState(element) {
    $(element).removeClass('loading').prop('disabled', false);
}

/**
 * Utility functions
 */
const HeaderUtils = {
    // Debounce function for performance optimization
    debounce: function (func, wait, immediate) {
        let timeout;
        return function () {
            const context = this, args = arguments;
            const later = function () {
                timeout = null;
                if (!immediate) func.apply(context, args);
            };
            const callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(context, args);
        };
    },

    // Throttle function for scroll events
    throttle: function (func, limit) {
        let inThrottle;
        return function () {
            const args = arguments;
            const context = this;
            if (!inThrottle) {
                func.apply(context, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    },

    // Format time for notifications
    formatTime: function (timestamp) {
        const now = new Date();
        const time = new Date(timestamp);
        const diff = now - time;

        const minutes = Math.floor(diff / 60000);
        const hours = Math.floor(diff / 3600000);
        const days = Math.floor(diff / 86400000);

        if (minutes < 1) return 'Vừa xong';
        if (minutes < 60) return `${minutes} phút trước`;
        if (hours < 24) return `${hours} giờ trước`;
        return `${days} ngày trước`;
    },

    // Show toast notifications
    showToast: function (message, type = 'info') {
        const toastHtml = `
            <div class="toast-notification toast-${type}" data-auto-hide="true">
                <div class="toast-content">
                    <i class="fas fa-${this.getToastIcon(type)} me-2"></i>
                    <span>${message}</span>
                </div>
                <button type="button" class="toast-close" onclick="this.parentElement.remove()">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;

        // Add to toast container or create one
        let toastContainer = $('.toast-container');
        if (!toastContainer.length) {
            toastContainer = $('<div class="toast-container"></div>').appendTo('body');
        }

        const toast = $(toastHtml).appendTo(toastContainer);

        // Auto-hide after 5 seconds
        setTimeout(() => {
            toast.fadeOut(() => toast.remove());
        }, 5000);
    },

    getToastIcon: function (type) {
        const icons = {
            'success': 'check-circle',
            'error': 'exclamation-circle',
            'warning': 'exclamation-triangle',
            'info': 'info-circle'
        };
        return icons[type] || 'info-circle';
    }
};

/**
 * Advanced search functionality with API integration
 */
function initializeAdvancedSearch() {
    const searchInput = $('.search-input');
    const searchResults = $('<div class="search-results-dropdown"></div>');

    // Create search results dropdown
    searchInput.parent().append(searchResults);

    // Advanced search with API call
    const debouncedSearch = HeaderUtils.debounce(function (query) {
        if (query.length < 2) {
            searchResults.hide();
            return;
        }

        // Simulate API call (replace with actual endpoint)
        $.ajax({
            url: '/api/search/suggestions',
            method: 'GET',
            data: { q: query },
            beforeSend: function () {
                searchResults.html('<div class="search-loading">Đang tìm kiếm...</div>').show();
            },
            success: function (data) {
                displaySearchResults(data);
            },
            error: function () {
                searchResults.html('<div class="search-error">Có lỗi xảy ra khi tìm kiếm</div>').show();
            }
        });
    }, 300);

    searchInput.on('input', function () {
        debouncedSearch($(this).val().trim());
    });

    function displaySearchResults(results) {
        if (!results || results.length === 0) {
            searchResults.html('<div class="search-no-results">Không tìm thấy kết quả</div>').show();
            return;
        }

        let html = '<div class="search-results-list">';
        results.forEach(result => {
            html += `
                <div class="search-result-item" data-url="${result.url}">
                    <div class="result-title">${result.title}</div>
                    <div class="result-description">${result.description}</div>
                </div>
            `;
        });
        html += '</div>';

        searchResults.html(html).show();

        // Handle result item clicks
        $('.search-result-item').on('click', function () {
            window.location.href = $(this).data('url');
        });
    }

    // Hide search results when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.search-form').length) {
            searchResults.hide();
        }
    });
}

/**
 * Accessibility improvements
 */
function initializeAccessibility() {
    // Keyboard navigation for dropdowns
    $('.dropdown-toggle').on('keydown', function (e) {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            $(this).dropdown('toggle');
        }
    });

    // Skip to content link
    if (!$('.skip-to-content').length) {
        $('body').prepend(`
            <a href="#main-content" class="skip-to-content sr-only sr-only-focusable">
                Bỏ qua đến nội dung chính
            </a>
        `);
    }

    // ARIA labels for better screen reader support
    $('.nav-btn').each(function () {
        if (!$(this).attr('aria-label')) {
            const title = $(this).attr('title') || $(this).text().trim();
            $(this).attr('aria-label', title);
        }
    });
}

/**
 * Performance monitoring
 */
function initializePerformanceMonitoring() {
    // Monitor header load time
    const startTime = performance.now();

    $(window).on('load', function () {
        const loadTime = performance.now() - startTime;
        console.log(`Header loaded in ${loadTime.toFixed(2)}ms`);

        // Send performance data to analytics (if needed)
        if (window.gtag) {
            gtag('event', 'timing_complete', {
                name: 'header_load',
                value: Math.round(loadTime)
            });
        }
    });
}

/**
 * Initialize all header functionality
 */
function initializeAllHeaderFeatures() {
    initializeHeader();
    initializeThemeToggle();
    initializeSearch();
    initializeStickyHeader();
    initializeNotifications();
    initializeResponsiveMenu();
    initializeAdvancedSearch();
    initializeAccessibility();
    initializePerformanceMonitoring();

    // Add custom CSS for additional features
    addCustomStyles();
}

/**
 * Add additional CSS styles programmatically
 */
function addCustomStyles() {
    const customStyles = `
        <style>
        .toast-container {
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
        }
        
        .toast-notification {
            background: white;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            margin-bottom: 10px;
            padding: 15px;
            display: flex;
            align-items: center;
            justify-content: space-between;
            min-width: 300px;
            animation: slideInRight 0.3s ease;
        }
        
        .toast-success { border-left: 4px solid #28a745; }
        .toast-error { border-left: 4px solid #dc3545; }
        .toast-warning { border-left: 4px solid #ffc107; }
        .toast-info { border-left: 4px solid #17a2b8; }
        
        .toast-close {
            background: none;
            border: none;
            color: #6c757d;
            font-size: 14px;
            cursor: pointer;
            padding: 0;
            margin-left: 15px;
        }
        
        .search-results-dropdown {
            position: absolute;
            top: 100%;
            left: 0;
            right: 0;
            background: white;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            z-index: 1000;
            display: none;
            max-height: 400px;
            overflow-y: auto;
        }
        
        .search-result-item {
            padding: 12px 16px;
            border-bottom: 1px solid #f8f9fa;
            cursor: pointer;
            transition: background-color 0.2s;
        }
        
        .search-result-item:hover {
            background-color: #f8f9fa;
        }
        
        .result-title {
            font-weight: 600;
            color: #495057;
            margin-bottom: 4px;
        }
        
        .result-description {
            font-size: 14px;
            color: #6c757d;
        }
        
        .skip-to-content {
            position: absolute;
            top: -40px;
            left: 6px;
            background: #000;
            color: #fff;
            padding: 8px;
            text-decoration: none;
            border-radius: 4px;
            z-index: 10000;
        }
        
        .skip-to-content:focus {
            top: 6px;
        }
        
        @keyframes slideInRight {
            from { transform: translateX(100%); opacity: 0; }
            to { transform: translateX(0); opacity: 1; }
        }
        </style>
    `;

    if (!$('#header-custom-styles').length) {
        $('head').append(customStyles.replace('<style>', '<style id="header-custom-styles">'));
    }
}

// Initialize everything when DOM is ready
$(document).ready(function () {
    initializeAllHeaderFeatures();
});

// Export functions for external use
window.HeaderUtils = HeaderUtils;
window.showLoadingState = showLoadingState;
window.hideLoadingState = hideLoadingState;