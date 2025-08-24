// Email Navigation and Print Functionality

window.setupEmailKeyboardNavigation = (dotNetObjectReference) => {
    // Remove any existing event listeners
    document.removeEventListener("keydown", window.emailKeyboardHandler);

    // Create new keyboard handler
    window.emailKeyboardHandler = function (event) {
        // Don't handle keys if user is typing in an input field
        if (
            event.target.tagName === "INPUT" ||
            event.target.tagName === "TEXTAREA" ||
            event.target.isContentEditable
        ) {
            return;
        }

        // Handle keyboard shortcuts
        switch (event.key) {
            case "j":
            case "ArrowDown":
                event.preventDefault();
                dotNetObjectReference.invokeMethodAsync("HandleKeyPress", event.key);
                break;
            case "k":
            case "ArrowUp":
                event.preventDefault();
                dotNetObjectReference.invokeMethodAsync("HandleKeyPress", event.key);
                break;
            case "ArrowLeft":
            case "ArrowRight":
                event.preventDefault();
                dotNetObjectReference.invokeMethodAsync("HandleKeyPress", event.key);
                break;
            case "Escape":
                event.preventDefault();
                dotNetObjectReference.invokeMethodAsync("HandleKeyPress", event.key);
                break;
        }
    };

    // Add the event listener
    document.addEventListener("keydown", window.emailKeyboardHandler);
};

window.printEmailContent = (subject, content, isHtml, from, to, date) => {
    try {
        // Check if content is empty and provide fallback
        if (!content || content.trim() === "") {
            content = "No content available for this email.";
            isHtml = false;
        }

        // Create a new window for printing
        const printWindow = window.open(
            "",
            "_blank",
            "width=900,height=700,scrollbars=yes,resizable=yes,menubar=no,toolbar=no,location=no,status=no",
        );

        if (!printWindow) {
            // Fallback: try to print in current window if popup is blocked
            if (
                confirm(
                    "Print window blocked. Would you like to print in the current tab instead?",
                )
            ) {
                printInCurrentWindow(subject, content, isHtml, from, to, date);
            }
            return;
        }

        const emailBody = isHtml
            ? content
            : `<pre style="white-space: pre-wrap; font-family: inherit;">${escapeHtml(content)}</pre>`;

        const printContent = `
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="UTF-8">
            <title>Print: ${escapeHtml(subject)}</title>
            <style>
                * {
                    box-sizing: border-box;
                }
                body {
                    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
                    line-height: 1.6;
                    color: #333;
                    max-width: 800px;
                    margin: 20px auto;
                    padding: 20px;
                    background: white;
                }
                .print-header {
                    border-bottom: 3px solid #007bff;
                    padding-bottom: 20px;
                    margin-bottom: 30px;
                }
                .email-subject {
                    font-size: 28px;
                    font-weight: bold;
                    margin-bottom: 15px;
                    color: #333;
                    word-wrap: break-word;
                }
                .email-meta {
                    background: #f8f9fa;
                    padding: 15px;
                    border-radius: 8px;
                    margin-bottom: 20px;
                    border: 1px solid #e9ecef;
                }
                .email-meta-row {
                    display: flex;
                    margin-bottom: 8px;
                }
                .email-meta-label {
                    font-weight: bold;
                    width: 80px;
                    flex-shrink: 0;
                    color: #495057;
                }
                .email-meta-value {
                    color: #333;
                    word-break: break-all;
                }
                .email-body {
                    font-size: 14px;
                    line-height: 1.8;
                    color: #333;
                }
                .email-body img {
                    max-width: 100% !important;
                    height: auto !important;
                    display: block;
                    margin: 10px 0;
                }
                .email-body table {
                    width: 100%;
                    border-collapse: collapse;
                    margin: 15px 0;
                }
                .email-body table td,
                .email-body table th {
                    border: 1px solid #ddd;
                    padding: 8px;
                    word-wrap: break-word;
                }
                .email-body pre {
                    white-space: pre-wrap;
                    word-wrap: break-word;
                    font-family: inherit;
                    margin: 0;
                    background: #f8f9fa;
                    padding: 15px;
                    border-radius: 5px;
                    border: 1px solid #e9ecef;
                }
                .print-footer {
                    margin-top: 40px;
                    padding-top: 20px;
                    border-top: 1px solid #ddd;
                    font-size: 12px;
                    color: #666;
                    text-align: center;
                }
                .print-controls {
                    position: fixed;
                    top: 10px;
                    right: 10px;
                    z-index: 1000;
                }
                .print-controls button {
                    background: #007bff;
                    color: white;
                    border: none;
                    padding: 10px 20px;
                    margin: 0 5px;
                    cursor: pointer;
                    border-radius: 5px;
                    font-size: 14px;
                }
                .print-controls button:hover {
                    background: #0056b3;
                }
                @media print {
                    body {
                        margin: 0;
                        padding: 15mm;
                        font-size: 12pt;
                    }
                    .print-controls { display: none !important; }
                    .print-header { border-bottom-color: #333 !important; }
                    .email-meta { background: white !important; border: 1px solid #333 !important; }
                    .email-body pre { background: white !important; border: 1px solid #333 !important; }
                    .print-footer { page-break-inside: avoid; }
                }
                @page {
                    margin: 15mm;
                    size: A4;
                }
            </style>
        </head>
        <body>
            <div class="print-controls no-print">
                <button onclick="window.print()" title="Print Email">
                    🖨️ Print
                </button>
                <button onclick="window.close()" title="Close Window">
                    ❌ Close
                </button>
            </div>

            <div class="print-header">
                <div class="email-subject">${escapeHtml(subject)}</div>
            </div>

            <div class="email-meta">
                <div class="email-meta-row">
                    <div class="email-meta-label">From:</div>
                    <div class="email-meta-value">${escapeHtml(from)}</div>
                </div>
                <div class="email-meta-row">
                    <div class="email-meta-label">To:</div>
                    <div class="email-meta-value">${escapeHtml(to)}</div>
                </div>
                <div class="email-meta-row">
                    <div class="email-meta-label">Date:</div>
                    <div class="email-meta-value">${escapeHtml(date)}</div>
                </div>
            </div>

            <div class="email-body">
                ${emailBody}
            </div>

            <div class="print-footer">
                <p>Printed from Seu Email on ${new Date().toLocaleDateString()} at ${new Date().toLocaleTimeString()}</p>
            </div>

            <script>
                // Focus the window and set up keyboard shortcuts
                window.focus();

                document.addEventListener('keydown', function(e) {
                    if (e.ctrlKey || e.metaKey) {
                        if (e.key === 'p') {
                            e.preventDefault();
                            window.print();
                        }
                        if (e.key === 'w') {
                            e.preventDefault();
                            window.close();
                        }
                    }
                    if (e.key === 'Escape') {
                        window.close();
                    }
                });

                // Auto-focus for immediate printing if needed
                setTimeout(() => {
                    try {
                        if (confirm('Would you like to print this email now?')) {
                            window.print();
                        }
                    } catch (e) {
                        console.error('Print error:', e);
                    }
                }, 500);
            </script>
        </body>
        </html>
    `;

        printWindow.document.write(printContent);
        printWindow.document.close();
    } catch (error) {
        console.error("Error creating print window:", error);
        alert(
            "Failed to open print window. Please try again or check your popup settings.",
        );
    }
};

// Fallback function to print in current window
function printInCurrentWindow(subject, content, isHtml, from, to, date) {
    try {
        const originalContent = document.body.innerHTML;
        const emailBody = isHtml
            ? content
            : `<pre style="white-space: pre-wrap; font-family: inherit;">${escapeHtml(content)}</pre>`;

        const printContent = `
      <div style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; max-width: 800px; margin: 20px auto; padding: 20px;">
        <div style="border-bottom: 3px solid #007bff; padding-bottom: 20px; margin-bottom: 30px;">
          <h1 style="font-size: 28px; margin-bottom: 15px;">${escapeHtml(subject)}</h1>
        </div>
        <div style="background: #f8f9fa; padding: 15px; border-radius: 8px; margin-bottom: 20px;">
          <div><strong>From:</strong> ${escapeHtml(from)}</div>
          <div><strong>To:</strong> ${escapeHtml(to)}</div>
          <div><strong>Date:</strong> ${escapeHtml(date)}</div>
        </div>
        <div style="line-height: 1.8;">${emailBody}</div>
        <div style="margin-top: 40px; padding-top: 20px; border-top: 1px solid #ddd; text-align: center; font-size: 12px; color: #666;">
          Printed from Seu Email on ${new Date().toLocaleDateString()} at ${new Date().toLocaleTimeString()}
        </div>
      </div>`;

        document.body.innerHTML = printContent;
        window.print();
        document.body.innerHTML = originalContent;
    } catch (error) {
        console.error("Error printing in current window:", error);
        alert("Failed to print email. Please try again.");
    }
}

// Helper function to escape HTML for safe insertion
function escapeHtml(text) {
    if (!text) return "";
    const map = {
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#039;",
    };
    return text.replace(/[&<>"']/g, function (m) {
        return map[m];
    });
}

// Initialize resizable panes
window.initializeResizablePanes = () => {
    const container = document.querySelector(".email-split-container");
    const emailList = document.querySelector(".email-list-section");
    const emailView = document.querySelector(".email-view-section");

    if (!container || !emailList || !emailView) return;

    let isResizing = false;
    let startX = 0;
    let startWidthList = 0;
    let startWidthView = 0;

    // Create resize handle
    const resizeHandle = document.createElement("div");
    resizeHandle.className = "resize-handle";
    resizeHandle.innerHTML = '<div class="resize-handle-line"></div>';

    // Insert resize handle between list and view
    emailList.insertAdjacentElement("afterend", resizeHandle);

    resizeHandle.addEventListener("mousedown", (e) => {
        isResizing = true;
        startX = e.clientX;
        startWidthList = parseInt(window.getComputedStyle(emailList).width, 10);
        startWidthView = parseInt(window.getComputedStyle(emailView).width, 10);

        document.addEventListener("mousemove", handleResize);
        document.addEventListener("mouseup", stopResize);
        document.body.style.cursor = "col-resize";
        e.preventDefault();
    });

    function handleResize(e) {
        if (!isResizing) return;

        const containerWidth = container.offsetWidth;
        const deltaX = e.clientX - startX;
        const newListWidth = startWidthList + deltaX;
        const newViewWidth = startWidthView - deltaX;

        // Ensure minimum widths
        const minWidth = 250;
        if (newListWidth >= minWidth && newViewWidth >= minWidth) {
            const listPercent = (newListWidth / containerWidth) * 100;
            const viewPercent = (newViewWidth / containerWidth) * 100;

            emailList.style.width = `${listPercent}%`;
            emailView.style.width = `${viewPercent}%`;
        }
    }

    function stopResize() {
        isResizing = false;
        document.removeEventListener("mousemove", handleResize);
        document.removeEventListener("mouseup", stopResize);
        document.body.style.cursor = "default";
    }
};

// Initialize when DOM is loaded
document.addEventListener("DOMContentLoaded", () => {
    // Small delay to ensure Blazor components are rendered
    setTimeout(() => {
        window.initializeResizablePanes();
    }, 500);
});
