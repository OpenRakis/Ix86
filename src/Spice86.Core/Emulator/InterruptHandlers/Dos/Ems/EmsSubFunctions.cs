namespace Spice86.Core.Emulator.InterruptHandlers.Dos.Ems; 

public static class EmsSubFunctions {
    public const byte HandleNameGet = 0x00;
    public const byte HandleNameSet = 0x01;
    public const byte GetUnallocatedRawPages = 0x01;
    public const byte GetHardwareConfigurationArray = 0x00;
    public const byte MoveExchangeMove = 0x00;
}