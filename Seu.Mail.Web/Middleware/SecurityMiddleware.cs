namespace Seu.Mail.Web.Middleware;

/// <summary>
/// Middleware that provides security features including security headers, request logging, rate limiting, and payload size validation.
/// </summary>
public class SecurityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next request delegate in the pipeline.</param>
    /// <param name="logger">The logger instance for logging security events.</param>
    public SecurityMiddleware(RequestDelegate next, ILogger<SecurityMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Processes the HTTP request through the security middleware.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        AddSecurityHeaders(context);

        // Log suspicious requests
        LogSuspiciousActivity(context);

        // Rate limiting (basic implementation)
        if (IsRateLimited(context))
        {
            context.Response.StatusCode = 429; // Too Many Requests
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }

        // Validate request size
        if (context.Request.ContentLength > 52428800) // 50MB limit
        {
            context.Response.StatusCode = 413; // Payload Too Large
            await context.Response.WriteAsync("Request too large");
            return;
        }

        await _next(context);
    }

    /// <summary>
    /// Adds security headers to the HTTP response to enhance client-side security.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    private void AddSecurityHeaders(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Prevent clickjacking
        headers["X-Frame-Options"] = "DENY";

        // Prevent MIME type sniffing
        headers["X-Content-Type-Options"] = "nosniff";

        // XSS Protection
        headers["X-XSS-Protection"] = "1; mode=block";

        // Referrer Policy
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Content Security Policy
        headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
            "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' https://cdnjs.cloudflare.com; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none';";

        // Strict Transport Security (if HTTPS)
        if (context.Request.IsHttps) headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

        // Permissions Policy
        headers["Permissions-Policy"] = "camera=(), microphone=(), location=(), usb=()";
    }

    /// <summary>
    /// Logs suspicious request patterns for security monitoring.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    private void LogSuspiciousActivity(HttpContext context)
    {
        var request = context.Request;
        var userAgent = request.Headers.UserAgent.ToString();
        var path = request.Path.ToString();
        var method = request.Method;

        // Log suspicious patterns
        var suspiciousPatterns = new[]
        {
            "script", "javascript:", "vbscript:", "data:", "onload=", "onerror=",
            "../", "..\\", "%2e%2e", "%2f", "%5c", "union", "select", "drop",
            "delete", "insert", "update", "exec", "sp_", "xp_"
        };

        var queryString = request.QueryString.ToString().ToLower();
        var isSuspicious = suspiciousPatterns.Any(pattern =>
            path.ToLower().Contains(pattern) ||
            queryString.Contains(pattern) ||
            userAgent.ToLower().Contains(pattern));

        if (isSuspicious)
            _logger.LogWarning(
                "Suspicious request detected. Method: {Method}, Path: {Path}, Query: {Query}, UserAgent: {UserAgent}, IP: {IP}",
                method, path, queryString, userAgent, context.Connection.RemoteIpAddress);

        // Log requests to sensitive endpoints
        var sensitiveEndpoints = new[] { "/accounts", "/settings", "/api" };
        if (sensitiveEndpoints.Any(endpoint => path.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase)))
            _logger.LogInformation("Access to sensitive endpoint. Method: {Method}, Path: {Path}, IP: {IP}",
                method, path, context.Connection.RemoteIpAddress);
    }

    /// <summary>
    /// Determines if the current request should be rate limited.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>True if the request should be rate limited, false otherwise.</returns>
    private bool IsRateLimited(HttpContext context)
    {
        // Simple in-memory rate limiting (for production, use Redis or similar)
        var key = $"{context.Connection.RemoteIpAddress}_{context.Request.Path}";
        var now = DateTime.UtcNow;

        // This is a simplified implementation - in production, use proper rate limiting
        // like AspNetCoreRateLimit or similar libraries

        return false; // Placeholder - implement based on your requirements
    }
}

/// <summary>
/// Extension methods for configuring the Security middleware.
/// </summary>
public static class SecurityMiddlewareExtensions
{
    /// <summary>
    /// Adds the Security middleware to the application pipeline.
    /// </summary>
    /// <param name="builder">The application builder instance.</param>
    /// <returns>The application builder for method chaining.</returns>
    public static IApplicationBuilder UseSecurityMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityMiddleware>();
    }
}