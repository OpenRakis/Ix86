namespace Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Instructions.IncReg;

using Spice86.Core.Emulator.CPU.CfgCpu.ParsedInstruction.Prefix;
using Spice86.Shared.Emulator.Memory;

public class IncReg16 : InstructionWithRegisterIndex {
    public IncReg16(SegmentedAddress address, InstructionField<byte> opcodeField, List<InstructionPrefix> prefixes,
        int registerIndex) : base(address, opcodeField, prefixes, registerIndex) {
    }

    public override void Visit(ICfgNodeVisitor visitor) {
        visitor.Accept(this);
    }
}