﻿namespace Spice86.Ui;

using Avalonia.Input;
using Spice86.Emulator.Devices.Video;
using System;
using System.Collections.Generic;

/// <summary>
/// TODO : complete it, along with all the UI framework.
/// </summary>
public class Gui {

    internal void Draw(byte[] vs, Rgb[] rgbs) {
        throw new NotImplementedException();
    }

    internal int GetHeight() {
        throw new NotImplementedException();
    }

    internal Key GetLastKeyCode() {
        throw new NotImplementedException();
    }

    internal int GetMouseX() {
        throw new NotImplementedException();
    }

    internal int GetMouseY() {
        throw new NotImplementedException();
    }

    internal int GetWidth() {
        throw new NotImplementedException();
    }

    internal bool IsKeyPressed(Key keyCode) {
        throw new NotImplementedException();
    }

    internal bool IsLeftButtonClicked() {
        throw new NotImplementedException();
    }

    internal bool IsRightButtonClicked() {
        throw new NotImplementedException();
    }

    internal void SetMouseX(int x) {
        throw new NotImplementedException();
    }

    internal void SetMouseY(int y) {
        throw new NotImplementedException();
    }

    internal void SetOnKeyPressedEvent(Action v) {
        throw new NotImplementedException();
    }

    internal void SetOnKeyReleasedEvent(Action v) {
        throw new NotImplementedException();
    }

    internal void SetResolution(int videoWidth, int videoHeight, int v) {
        throw new NotImplementedException();
    }

    internal void RemoveBuffer(int address) {
        throw new NotImplementedException();
    }

    internal VideoBuffer[] GetVideoBuffers() {
        throw new NotImplementedException();
    }

    internal void AddBuffer(int address, double scale, int v1, int v2, object p) {
        throw new NotImplementedException();
    }
}
