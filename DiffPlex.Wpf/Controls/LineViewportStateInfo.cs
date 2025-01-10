using DiffPlex.DiffBuilder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffPlex.Wpf.Controls;

/// <summary>
/// Visibility levels.
/// </summary>
public enum VisibilityLevels : byte
{
    /// <summary>
    /// Any part of visual in viewport.
    /// </summary>
    Any = 0,

    /// <summary>
    /// Half at least in viewport.
    /// </summary>
    Half = 1,

    /// <summary>
    /// All visual in viewport.
    /// </summary>
    All = 2
}

/// <summary>
/// The state information of the line viewport.
/// </summary>
/// <param name="model">The diff piece instance.</param>
/// <param name="isInViewport">true if the element is in viewport; otherwise, false.</param>
/// <param name="isCollapsed">true if the visibility of the element is to collapse; otherwise, false.</param>
internal struct LineViewportStateInfo(DiffPiece model, bool isInViewport, bool isCollapsed)
{
    /// <summary>
    /// Gets the diff piece instance.
    /// </summary>
    public DiffPiece Model { get; } = model;

    /// <summary>
    /// Gets a value indicating whether the element is in viewport.
    /// </summary>
    public bool IsInViewport { get; } = isInViewport;

    /// <summary>
    /// Gets a value indicating whether the visibility of the element is to collapse.
    /// </summary>
    public bool IsCollapsed { get; } = isCollapsed;
}
