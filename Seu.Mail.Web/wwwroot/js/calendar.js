// Calendar JavaScript functions for Seu.Mail

// Download file function for import/export
window.downloadFile = (filename, contentType, content) => {
    const blob = new Blob([content], {type: contentType});
    const url = URL.createObjectURL(blob);

    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.style.display = 'none';

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    URL.revokeObjectURL(url);
};

// Initialize Bootstrap dropdowns and tooltips
window.initializeCalendar = () => {
    // Initialize Bootstrap dropdowns
    var dropdownElementList = [].slice.call(document.querySelectorAll('.dropdown-toggle'));
    var dropdownList = dropdownElementList.map(function (dropdownToggleEl) {
        return new bootstrap.Dropdown(dropdownToggleEl);
    });

    // Initialize Bootstrap tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize Bootstrap modals
    var modalElementList = [].slice.call(document.querySelectorAll('.modal'));
    var modalList = modalElementList.map(function (modalEl) {
        return new bootstrap.Modal(modalEl);
    });
};

// Drag and drop functionality for events
window.initializeDragAndDrop = () => {
    const eventItems = document.querySelectorAll('.event-item');
    const timeSlots = document.querySelectorAll('.time-slot');

    eventItems.forEach(event => {
        event.draggable = true;

        event.addEventListener('dragstart', (e) => {
            e.dataTransfer.setData('text/plain', event.dataset.eventId);
            event.classList.add('dragging');
        });

        event.addEventListener('dragend', (e) => {
            event.classList.remove('dragging');
        });
    });

    timeSlots.forEach(slot => {
        slot.addEventListener('dragover', (e) => {
            e.preventDefault();
            slot.classList.add('drag-over');
        });

        slot.addEventListener('dragleave', (e) => {
            slot.classList.remove('drag-over');
        });

        slot.addEventListener('drop', (e) => {
            e.preventDefault();
            slot.classList.remove('drag-over');

            const eventId = e.dataTransfer.getData('text/plain');
            const newTime = slot.dataset.datetime;

            if (eventId && newTime) {
                // Trigger Blazor event
                DotNet.invokeMethodAsync('Seu.Mail', 'HandleEventDrop', parseInt(eventId), newTime);
            }
        });
    });
};

// Auto-resize textarea elements
window.autoResizeTextarea = (element) => {
    element.style.height = 'auto';
    element.style.height = element.scrollHeight + 'px';
};

// Show notification
window.showNotification = (title, message, type = 'info') => {
    // Create toast element
    const toastContainer = document.getElementById('toast-container') || createToastContainer();

    const toastId = 'toast-' + Date.now();
    const toast = document.createElement('div');
    toast.id = toastId;
    toast.className = `toast align-items-center text-white bg-${type} border-0`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');

    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                <strong>${title}</strong><br>
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;

    toastContainer.appendChild(toast);

    const bsToast = new bootstrap.Toast(toast, {
        autohide: true,
        delay: 5000
    });

    bsToast.show();

    // Remove toast element after it's hidden
    toast.addEventListener('hidden.bs.toast', () => {
        toast.remove();
    });
};

// Create toast container if it doesn't exist
function createToastContainer() {
    const container = document.createElement('div');
    container.id = 'toast-container';
    container.className = 'toast-container position-fixed top-0 end-0 p-3';
    container.style.zIndex = '1055';
    document.body.appendChild(container);
    return container;
}

// Print calendar view
window.printCalendar = () => {
    const printContent = document.querySelector('.calendar-content').innerHTML;
    const originalContent = document.body.innerHTML;

    document.body.innerHTML = `
        <html>
        <head>
            <title>Calendar Print</title>
            <style>
                body { font-family: Arial, sans-serif; margin: 20px; }
                .calendar-grid { border-collapse: collapse; width: 100%; }
                .calendar-day { border: 1px solid #ccc; padding: 8px; vertical-align: top; height: 100px; }
                .event-item { background-color: #007bff; color: white; padding: 2px 4px; margin: 1px 0; border-radius: 3px; font-size: 10px; }
                @media print { .no-print { display: none; } }
            </style>
        </head>
        <body>
            ${printContent}
        </body>
        </html>
    `;

    window.print();
    document.body.innerHTML = originalContent;
    window.location.reload();
};

// Copy calendar event to clipboard
window.copyEventToClipboard = (eventText) => {
    if (navigator.clipboard) {
        navigator.clipboard.writeText(eventText).then(() => {
            showNotification('Copied', 'Event details copied to clipboard', 'success');
        }).catch(() => {
            fallbackCopyToClipboard(eventText);
        });
    } else {
        fallbackCopyToClipboard(eventText);
    }
};

// Fallback copy method for older browsers
function fallbackCopyToClipboard(text) {
    const textArea = document.createElement('textarea');
    textArea.value = text;
    textArea.style.position = 'fixed';
    textArea.style.left = '-999999px';
    textArea.style.top = '-999999px';
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();

    try {
        document.execCommand('copy');
        showNotification('Copied', 'Event details copied to clipboard', 'success');
    } catch (err) {
        showNotification('Error', 'Failed to copy to clipboard', 'danger');
    }

    document.body.removeChild(textArea);
}

// Format date for display
window.formatDate = (date, format = 'YYYY-MM-DD') => {
    const d = new Date(date);

    switch (format) {
        case 'YYYY-MM-DD':
            return d.toISOString().split('T')[0];
        case 'MM/DD/YYYY':
            return (d.getMonth() + 1).toString().padStart(2, '0') + '/' +
                d.getDate().toString().padStart(2, '0') + '/' +
                d.getFullYear();
        case 'DD/MM/YYYY':
            return d.getDate().toString().padStart(2, '0') + '/' +
                (d.getMonth() + 1).toString().padStart(2, '0') + '/' +
                d.getFullYear();
        case 'long':
            return d.toLocaleDateString('en-US', {
                weekday: 'long',
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            });
        default:
            return d.toLocaleDateString();
    }
};

// Get current week number
window.getWeekNumber = (date) => {
    const d = new Date(date);
    d.setUTCDate(d.getUTCDate() + 4 - (d.getUTCDay() || 7));
    const yearStart = new Date(Date.UTC(d.getUTCFullYear(), 0, 1));
    const weekNo = Math.ceil(((d - yearStart) / 86400000 + 1) / 7);
    return weekNo;
};

// Keyboard shortcuts for calendar
window.initializeKeyboardShortcuts = () => {
    document.addEventListener('keydown', (e) => {
        // Only handle shortcuts when not typing in input fields
        if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') {
            return;
        }

        switch (e.key) {
            case 'n':
            case 'N':
                if (e.ctrlKey || e.metaKey) {
                    e.preventDefault();
                    // Trigger new event creation
                    document.querySelector('[data-action="new-event"]')?.click();
                }
                break;
            case 't':
            case 'T':
                e.preventDefault();
                // Go to today
                document.querySelector('[data-action="today"]')?.click();
                break;
            case 'ArrowLeft':
                if (e.ctrlKey || e.metaKey) {
                    e.preventDefault();
                    // Navigate to previous period
                    document.querySelector('[data-action="previous"]')?.click();
                }
                break;
            case 'ArrowRight':
                if (e.ctrlKey || e.metaKey) {
                    e.preventDefault();
                    // Navigate to next period
                    document.querySelector('[data-action="next"]')?.click();
                }
                break;
            case '1':
                if (e.ctrlKey || e.metaKey) {
                    e.preventDefault();
                    // Switch to month view
                    document.querySelector('[data-view="month"]')?.click();
                }
                break;
            case '2':
                if (e.ctrlKey || e.metaKey) {
                    e.preventDefault();
                    // Switch to week view
                    document.querySelector('[data-view="week"]')?.click();
                }
                break;
            case '3':
                if (e.ctrlKey || e.metaKey) {
                    e.preventDefault();
                    // Switch to day view
                    document.querySelector('[data-view="day"]')?.click();
                }
                break;
        }
    });
};

// Initialize all calendar functionality when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    initializeCalendar();
    initializeKeyboardShortcuts();
});

// Blazor component lifecycle hooks
window.blazorCalendar = {
    // Called after Blazor component updates
    afterUpdate: () => {
        initializeDragAndDrop();
        initializeCalendar();
    },

    // Called when component is disposed
    dispose: () => {
        // Clean up event listeners and timers
        const tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        tooltips.forEach(tooltip => {
            const instance = bootstrap.Tooltip.getInstance(tooltip);
            if (instance) {
                instance.dispose();
            }
        });
    }
};
