namespace Spice86.Core.Emulator.Devices.Video.Registers.CrtController;

/// <summary>
/// Represents the 8 bit Text Cursor End register.
/// </summary>
public class TextCursorEndRegister : Register8 {
    /// <summary>
    ///     This two-bit field specifies a delay, in character clocks, from the Text Cursor Location specified in CRE and
    ///     CRF to the actual cursor.
    /// </summary>
    public byte TextCursorSkew {
        get => GetBits(6, 5);
        set => SetBits(6, 5, value);
    }

    /// <summary>
    ///     This field specifies the scanline within the Character where the text cursor is to end. A value greater than
    ///     the character cell height yields an effective ending value equal to the cell height.
    /// </summary>
    public byte TextCursorEnd {
        get => GetBits(4, 0);
        set => SetBits(4, 0, value);
    }
}