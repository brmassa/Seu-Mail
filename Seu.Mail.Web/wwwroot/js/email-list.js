// Email List JavaScript Utilities
window.emailListUtils = {
  contextMenuInstance: null,
  clickHandler: null,
  contextMenuHandler: null,

  addContextMenuListener: function (dotNetRef) {
    this.contextMenuInstance = dotNetRef;
    this.clickHandler = this.closeContextMenu.bind(this);
    this.contextMenuHandler = this.handleRightClick.bind(this);
    document.addEventListener("click", this.clickHandler);
    document.addEventListener("contextmenu", this.contextMenuHandler);
  },

  removeContextMenuListener: function () {
    if (this.clickHandler) {
      document.removeEventListener("click", this.clickHandler);
      this.clickHandler = null;
    }
    if (this.contextMenuHandler) {
      document.removeEventListener("contextmenu", this.contextMenuHandler);
      this.contextMenuHandler = null;
    }
    this.contextMenuInstance = null;
  },

  closeContextMenu: function (event) {
    if (event.target.closest(".email-context-menu")) return;
    if (this.contextMenuInstance) {
      this.contextMenuInstance.invokeMethodAsync("CloseContextMenuFromJs");
    }
  },

  handleRightClick: function (event) {
    if (!event.target.closest(".email-item")) return;
    event.preventDefault();
    return false;
  },

  // Keyboard navigation for email list
  handleKeyboardNavigation: function (
    event,
    currentIndex,
    totalCount,
    dotNetRef,
  ) {
    switch (event.key) {
      case "ArrowDown":
        event.preventDefault();
        if (currentIndex < totalCount - 1) {
          dotNetRef.invokeMethodAsync("NavigateToEmail", currentIndex + 1);
        }
        break;

      case "ArrowUp":
        event.preventDefault();
        if (currentIndex > 0) {
          dotNetRef.invokeMethodAsync("NavigateToEmail", currentIndex - 1);
        }
        break;

      case "Enter":
      case " ":
        event.preventDefault();
        dotNetRef.invokeMethodAsync("OpenCurrentEmail");
        break;

      case "Delete":
        event.preventDefault();
        dotNetRef.invokeMethodAsync("DeleteCurrentEmail");
        break;

      case "r":
        if (event.ctrlKey || event.metaKey) {
          event.preventDefault();
          dotNetRef.invokeMethodAsync("ReplyToCurrentEmail");
        }
        break;

      case "a":
        if (event.ctrlKey || event.metaKey) {
          event.preventDefault();
          dotNetRef.invokeMethodAsync("SelectAllEmails");
        }
        break;

      case "f":
        if (event.ctrlKey || event.metaKey) {
          event.preventDefault();
          dotNetRef.invokeMethodAsync("ForwardCurrentEmail");
        }
        break;
    }
  },

  // Initialize keyboard shortcuts for email list
  initializeKeyboardShortcuts: function (dotNetRef) {
    document.addEventListener("keydown", (event) => {
      // Only handle shortcuts when not in input fields
      if (
        event.target.tagName === "INPUT" ||
        event.target.tagName === "TEXTAREA" ||
        event.target.contentEditable === "true"
      ) {
        return;
      }

      this.handleKeyboardNavigation(event, 0, 0, dotNetRef);
    });
  },

  // Smooth scroll to element
  scrollToElement: function (elementId, behavior = "smooth") {
    const element = document.getElementById(elementId);
    if (element) {
      element.scrollIntoView({
        behavior: behavior,
        block: "center",
        inline: "nearest",
      });
    }
  },

  // Get viewport dimensions for context menu positioning
  getViewportDimensions: function () {
    return {
      width: window.innerWidth,
      height: window.innerHeight,
    };
  },

  // Position context menu to stay within viewport
  positionContextMenu: function (x, y, menuWidth = 200, menuHeight = 300) {
    const viewport = this.getViewportDimensions();

    // Adjust X position if menu would go off-screen
    if (x + menuWidth > viewport.width) {
      x = viewport.width - menuWidth - 10;
    }

    // Adjust Y position if menu would go off-screen
    if (y + menuHeight > viewport.height) {
      y = viewport.height - menuHeight - 10;
    }

    // Ensure menu doesn't go off top or left edge
    x = Math.max(10, x);
    y = Math.max(10, y);

    return { x: x, y: y };
  },

  // Debounce function for performance optimization
  debounce: function (func, wait) {
    let timeout;
    return function executedFunction(...args) {
      const later = () => {
        clearTimeout(timeout);
        func(...args);
      };
      clearTimeout(timeout);
      timeout = setTimeout(later, wait);
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
        setTimeout(() => (inThrottle = false), limit);
      }
    };
  },

  // Virtual scrolling helper for large email lists
  calculateVisibleItems: function (
    scrollTop,
    itemHeight,
    containerHeight,
    totalItems,
  ) {
    const startIndex = Math.floor(scrollTop / itemHeight);
    const endIndex = Math.min(
      startIndex + Math.ceil(containerHeight / itemHeight) + 1,
      totalItems,
    );

    return {
      startIndex: Math.max(0, startIndex),
      endIndex: endIndex,
      offsetY: startIndex * itemHeight,
    };
  },

  // Copy text to clipboard
  copyToClipboard: async function (text) {
    try {
      if (navigator.clipboard && navigator.clipboard.writeText) {
        await navigator.clipboard.writeText(text);
        return true;
      } else {
        // Fallback for older browsers
        const textArea = document.createElement("textarea");
        textArea.value = text;
        textArea.style.position = "fixed";
        textArea.style.left = "-999999px";
        textArea.style.top = "-999999px";
        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();
        const result = document.execCommand("copy");
        document.body.removeChild(textArea);
        return result;
      }
    } catch (error) {
      console.error("Failed to copy text to clipboard:", error);
      return false;
    }
  },

  // Show toast notification
  showToast: function (message, type = "info", duration = 3000) {
    // Create toast element
    const toast = document.createElement("div");
    toast.className = `toast toast-${type} show`;
    toast.innerHTML = `
            <div class="toast-header">
                <i class="fas fa-${type === "success" ? "check-circle" : type === "error" ? "exclamation-circle" : "info-circle"} me-2"></i>
                <strong class="me-auto">${type.charAt(0).toUpperCase() + type.slice(1)}</strong>
                <button type="button" class="btn-close" data-bs-dismiss="toast"></button>
            </div>
            <div class="toast-body">${message}</div>
        `;

    // Add to toast container or create one
    let toastContainer = document.querySelector(".toast-container");
    if (!toastContainer) {
      toastContainer = document.createElement("div");
      toastContainer.className =
        "toast-container position-fixed top-0 end-0 p-3";
      toastContainer.style.zIndex = "1080";
      document.body.appendChild(toastContainer);
    }

    toastContainer.appendChild(toast);

    // Auto-hide after duration
    setTimeout(() => {
      toast.classList.remove("show");
      setTimeout(() => {
        if (toast.parentNode) {
          toast.parentNode.removeChild(toast);
        }
      }, 150);
    }, duration);

    return toast;
  },
};

// Initialize when DOM is loaded
document.addEventListener("DOMContentLoaded", function () {
  console.log("Email list utilities loaded");
});

// Cleanup on page unload
window.addEventListener("beforeunload", function () {
  if (window.emailListUtils) {
    window.emailListUtils.removeContextMenuListener();
  }
});
