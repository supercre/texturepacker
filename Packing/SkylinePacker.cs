using System;
using System.Collections.Generic;
using Point = SixLabors.ImageSharp.Point;

namespace TexturePacker.Packing;

public class SkylinePacker
{
    private readonly List<SkylineNode> _skyline;
    private readonly int _width;
    private readonly int _height;
    private readonly int _padding;

    public SkylinePacker(int width, int height, int padding)
    {
        _width = width;
        _height = height;
        _padding = padding;
        _skyline = new List<SkylineNode> { new SkylineNode(0, 0, width) };
    }

    public (bool success, Point position, bool rotated) Insert(int width, int height, bool enableRotation)
    {
        var bestHeight = int.MaxValue;
        var bestWidth = int.MaxValue;
        var bestY = int.MaxValue;
        var bestX = int.MaxValue;
        var bestIndex = -1;
        var rotated = false;

        // Try without rotation
        var result = FindBestPosition(width, height);
        if (result.success)
        {
            bestHeight = result.height;
            bestWidth = width;
            bestY = result.y;
            bestX = result.x;
            bestIndex = result.index;
        }

        // Try with rotation if enabled
        if (enableRotation)
        {
            result = FindBestPosition(height, width);
            if (result.success && (result.height < bestHeight || (result.height == bestHeight && result.x < bestX)))
            {
                bestHeight = result.height;
                bestWidth = height;
                bestY = result.y;
                bestX = result.x;
                bestIndex = result.index;
                rotated = true;
            }
        }

        if (bestIndex == -1)
            return (false, new Point(0, 0), false);

        // Add the new node
        var node = new SkylineNode(bestX, bestY, bestWidth);
        _skyline.Insert(bestIndex, node);

        // Merge nodes if possible
        MergeNodes();

        return (true, new Point(bestX + _padding, bestY + _padding), rotated);
    }

    private (bool success, int x, int y, int height, int index) FindBestPosition(int width, int height)
    {
        var bestHeight = int.MaxValue;
        var bestWidth = int.MaxValue;
        var bestY = int.MaxValue;
        var bestX = int.MaxValue;
        var bestIndex = -1;

        for (var i = 0; i < _skyline.Count; i++)
        {
            var y = _skyline[i].y;
            var x = _skyline[i].x;

            if (x + width > _width)
                continue;

            var maxHeight = y;
            var j = i;
            while (j < _skyline.Count && _skyline[j].x < x + width)
            {
                maxHeight = Math.Max(maxHeight, _skyline[j].y);
                j++;
            }

            if (maxHeight + height > _height)
                continue;

            if (maxHeight < bestHeight || (maxHeight == bestHeight && x < bestX))
            {
                bestHeight = maxHeight;
                bestWidth = width;
                bestY = maxHeight;
                bestX = x;
                bestIndex = i;
            }
        }

        return (bestIndex != -1, bestX, bestY, bestHeight, bestIndex);
    }

    private void MergeNodes()
    {
        for (var i = 0; i < _skyline.Count - 1; i++)
        {
            if (_skyline[i].y == _skyline[i + 1].y)
            {
                _skyline[i].width += _skyline[i + 1].width;
                _skyline.RemoveAt(i + 1);
                i--;
            }
        }
    }

    private class SkylineNode
    {
        public int x;
        public int y;
        public int width;

        public SkylineNode(int x, int y, int width)
        {
            this.x = x;
            this.y = y;
            this.width = width;
        }
    }
} 