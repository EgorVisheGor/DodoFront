using System.Net;
using System.Text;

namespace HTTPServer;

public static class WebHelper
{
    public static async Task ShowStatic(HttpListenerContext context, string? externalPath = null)
    {
        var path = (externalPath ?? context.Request.Url?.LocalPath) 
            ?.Split("/")
            .Skip(1)
            .ToArray();
        
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "public");

        if (path != null)
        {
            for (var i = 0; i < path.Length - 1; i++)
            {
                basePath = Path.Combine(basePath, $@"{path[i]}\");
            }
        }

        basePath = Path.Combine(basePath, path?[^1] ?? string.Empty);
    
        if (File.Exists(basePath))
        {
            await ShowFile(basePath, context);
        }
        else
        {
            await Show404(context);
        }
    }
    
    public static async Task ShowFile(string path, HttpListenerContext context)
    {
        context.Response.StatusCode = 200;
        context.Response.ContentType = Path.GetExtension(path) switch
        {
            ".html" => "text/html",
            ".css" => "text/css",
            _ => "text/plain"
        };
        var file = await File.ReadAllBytesAsync(path);
        await context.Response.OutputStream.WriteAsync(file);
    }
    
    private static async Task Show404(HttpListenerContext context)
    {
        context.Response.ContentType = "text/plain; charset=utf-8";
        context.Response.StatusCode = 404;
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Нужная вам страница не найдена!"));
    }
}