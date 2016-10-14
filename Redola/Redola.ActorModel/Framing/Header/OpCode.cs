namespace Redola.ActorModel.Framing
{
    // The opcode denotes the frame type of the Actor frame.
    // The opcode is an integer number between 0 and 15, inclusive.
    // Opcode 	Meaning 	Reference 
    // 1	Unassigned	
    // 2	Binary Frame
    // 3-7	Unassigned	
    // 8	Unassigned	
    // 9	Ping Frame
    // 10	Pong Frame
    // 11-15	Unassigned
    public enum OpCode : byte
    {
        Binary = 2,
        Ping = 9,
        Pong = 10,
    }
}
