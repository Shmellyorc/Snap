using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Snap.Content.Abstractions.Interfaces;

namespace Snap.Content.Abstractions;

public sealed class MacBundleContentProvider : IContentProvider
{
    private readonly string _bundlePath;
    private readonly string _resourcePath;
    private readonly bool _isInBundle;

    public bool IsInBundle => _isInBundle;
    public string? ResourcePath => _resourcePath;
    public string? MacOSPath => _isInBundle
        ? Path.Combine(_bundlePath, "Contents", "MacOs")
        : null;

    public MacBundleContentProvider(string? bundlePath = null)
    {
        if (string.IsNullOrEmpty(bundlePath))
        {
            _bundlePath = DetectBundlePath();
            _isInBundle = !string.IsNullOrEmpty(_bundlePath);
        }
        else
        {
            _bundlePath = bundlePath;
            _isInBundle = Directory.Exists(_bundlePath);
        }

        if (_isInBundle)
        {
            _resourcePath = Path.Combine(_bundlePath, "Contents", "Resources");

            if (!Directory.Exists(_resourcePath))
            {
                var possiblePaths = new[]
                {
                    Path.Combine(_bundlePath, "Resources"),
                    Path.Combine(_bundlePath, "Contents", "Resources", "Content"),
                    Path.Combine(_bundlePath, "Contents", "Resources", "Assets")
                };

                _resourcePath = possiblePaths.FirstOrDefault(Directory.Exists) ?? _resourcePath;
            }
        }
        else
        {
            _resourcePath = string.Empty;
        }
    }

    public bool Exists(string path)
    {
        if (!_isInBundle) return false;

        var fullPath = MapPath(path);
        return File.Exists(fullPath);
    }

    public IEnumerable<string> List(string folder)
    {
        if (!_isInBundle) yield break;

        var searchPath = string.IsNullOrEmpty(folder)
            ? _resourcePath
            : Path.Combine(_resourcePath, folder.Replace('/', Path.AltDirectorySeparatorChar));

        if (!Directory.Exists(searchPath)) yield break;

        var baseUri = new Uri(_resourcePath + Path.DirectorySeparatorChar);

        foreach (var file in Directory.EnumerateFiles(searchPath, "*", SearchOption.AllDirectories))
        {
            var fileUri = new Uri(file);
            var relativeUri = baseUri.MakeRelativeUri(fileUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString())
                .Replace('\\', '/');

            yield return relativePath;
        }
    }

    public Stream OpenRead(string path)
    {
        if (!_isInBundle)
            throw new FileNotFoundException($"Not running in a MacOs bundle: {path}");

        var fullPath = MapPath(path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Asset not found in bundle: {path}");

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    private static string DetectBundlePath()
    {
        var excutablePath = AppContext.BaseDirectory;

        var appIndex = excutablePath.IndexOf(".app/", StringComparison.OrdinalIgnoreCase);
        if (appIndex >= 0)
        {
            return excutablePath.Substring(0, appIndex + 4);
        }

        var parentDir = Directory.GetParent(excutablePath);
        if (parentDir?.Name == "MacOs")
        {
            var contentDir = parentDir.Parent;
            if (contentDir?.Name == "Contents")
            {
                return contentDir.Parent?.FullName ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private string MapPath(string logicalPath)
    {
        if (string.IsNullOrWhiteSpace(logicalPath))
            throw new ArgumentException("Path cannot be empty", nameof(logicalPath));

        var cleanPath = logicalPath.Replace('\\', '/').TrimStart('/');
        return Path.GetFullPath(Path.Combine(_resourcePath, cleanPath.Replace('/', Path.DirectorySeparatorChar)));
    }
}
