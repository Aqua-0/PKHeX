using System;

namespace PKHeX.Core;

internal static class MoveInfo7Expansion
{
    private static readonly Memory<byte> PPData = Util.GetBinaryResource("move_pp_uu_expansion");
    private static readonly Memory<byte> TypeData = Util.GetBinaryResource("move_type_uu_expansion");

    public static ReadOnlySpan<byte> PP => PPData.Span;
    public static ReadOnlySpan<byte> Type => TypeData.Span;
}
