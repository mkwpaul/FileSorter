﻿using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Caching;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileSorter.Services;

public sealed class ImageCache
{
    readonly ILogger _logger;
    readonly MemoryCache _cache;

    readonly CacheItemPolicy policy;

    public ImageCache(ILogger logger)
    {
        policy = new CacheItemPolicy
        {
            SlidingExpiration = TimeSpan.FromSeconds(15),
            RemovedCallback = OnRemoveFromCache,
        };
        _logger = logger.ForContext<ImageCache>();
        _cache = new MemoryCache("imageCache");
    }

    public void Add(string key, ImageSource image)
    {
        _cache.AddOrGetExisting(key, image, policy);
    }

    void OnRemoveFromCache(CacheEntryRemovedArguments arguments)
    {
        _logger.Warning("Removed {file} from Cache", arguments.CacheItem.Key);
        if (arguments.CacheItem.Value is not BitmapImage image)
            return;

        if (image.StreamSource is not MemoryStream memoryStream)
            return;

        image.StreamSource = null;
        memoryStream.Dispose();
    }

    public bool TryGetImageFromCache(string key, [NotNullWhen(true)] out ImageSource? image)
    {
        var item = _cache.GetCacheItem(key);
        if (item?.Value is not ImageSource source)
        {
            image = null;
            return false;
        }

        image = source;
        return true;
    }

    public void Remove(string key)
    {
        _cache.Remove(key, CacheEntryRemovedReason.Removed);
    }
}
