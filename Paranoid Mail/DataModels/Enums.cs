
namespace Paranoid
{
    public enum FileCompression : byte
    {
        Uncompressed = 0,
        LZMA = 1
    }

    public enum MessageTextFormat : byte
    {
        PlainText = 0,
        RTF = 1,
    }

    public enum AttachmentSource : int
    {
        File = 0,
        Forward = 1
    }
}
