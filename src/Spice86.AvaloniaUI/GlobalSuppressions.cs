﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This barely affects performance, and breaks the public APIs contract if the method is public")]
[assembly: SuppressMessage("Style", "IDE0007:Use implicit type", Justification = "We don't like 'var' around these parts, partner...")]
[assembly: SuppressMessage("Naming", "ET001:Type name does not match file name", Justification = "Code-behind file", Scope = "type", Target = "~T:Spice86.AvaloniaUI.App")]
[assembly: SuppressMessage("Naming", "ET001:Type name does not match file name", Justification = "Code-behind file", Scope = "type", Target = "~T:Spice86.AvaloniaUI.Views.MainWindow")]
[assembly: SuppressMessage("Naming", "ET001:Type name does not match file name", Justification = "Code-behind file", Scope = "type", Target = "~T:Spice86.AvaloniaUI.Views.VideoBufferView")]
