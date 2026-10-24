using Kid.Mcp;
using System.Runtime.InteropServices;

namespace Kid.Mcp.Tests;

public sealed class KeyInputParserTests
{
    [Fact]
    public void ParseProducesBalancedModifierChord()
    {
        var events = KeyInputParser.Parse("CTRL+L");

        Assert.Equal(
            new[]
            {
                new KeyInputParser.KeyboardEvent(0x11, false),
                new KeyInputParser.KeyboardEvent(0x4C, false),
                new KeyInputParser.KeyboardEvent(0x4C, true),
                new KeyInputParser.KeyboardEvent(0x11, true)
            },
            events);
    }

    [Theory]
    [InlineData("F5", 0x74)]
    [InlineData("ENTER", 0x0D)]
    [InlineData("a", 0x41)]
    public void ParseProducesKeyDownAndKeyUpEvents(string input, int expectedVirtualKey)
    {
        var events = KeyInputParser.Parse(input);

        Assert.Equal(2, events.Count);
        Assert.Equal((ushort)expectedVirtualKey, events[0].VirtualKey);
        Assert.False(events[0].KeyUp);
        Assert.Equal((ushort)expectedVirtualKey, events[1].VirtualKey);
        Assert.True(events[1].KeyUp);
    }

    [Theory]
    [InlineData("")]
    [InlineData("CTRL+")]
    [InlineData("+L")]
    [InlineData("CTRL+CTRL+L")]
    [InlineData("CTRL+SHIFT")]
    [InlineData("F13")]
    [InlineData("NO_SUCH_KEY")]
    public void ParseRejectsUnsupportedOrMalformedInput(string input)
    {
        Assert.Throws<ArgumentException>(() => KeyInputParser.Parse(input));
    }

    [Fact]
    public void InputStructMatchesWin32InputSizeForCurrentArchitecture()
    {
        var expectedSize = IntPtr.Size == 8 ? 40 : 28;

        Assert.Equal(expectedSize, Marshal.SizeOf<NativeMethods.Input>());
        Assert.Equal(expectedSize, NativeMethods.InputSize);
    }

    [Fact]
    public void InputUnionMatchesLargestWin32InputMember()
    {
        var expectedUnionSize = IntPtr.Size == 8 ? 32 : 24;

        Assert.Equal(expectedUnionSize, Marshal.SizeOf<NativeMethods.InputUnion>());
    }

    [Fact]
    public void InputFieldsMatchWin32Offsets()
    {
        var expectedUnionOffset = IntPtr.Size == 8 ? 8 : 4;

        Assert.Equal(0, Marshal.OffsetOf<NativeMethods.Input>(nameof(NativeMethods.Input.Type)).ToInt32());
        Assert.Equal(expectedUnionOffset, Marshal.OffsetOf<NativeMethods.Input>(nameof(NativeMethods.Input.Data)).ToInt32());
        Assert.Equal(0, Marshal.OffsetOf<NativeMethods.InputUnion>(nameof(NativeMethods.InputUnion.Keyboard)).ToInt32());
        Assert.Equal(0, Marshal.OffsetOf<NativeMethods.InputUnion>(nameof(NativeMethods.InputUnion.Mouse)).ToInt32());
        Assert.Equal(0, Marshal.OffsetOf<NativeMethods.InputUnion>(nameof(NativeMethods.InputUnion.Hardware)).ToInt32());
    }
}
