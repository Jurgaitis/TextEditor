using System.Text;

namespace FilesHandler;
public static class FileHandlers
{
    public static void RemovePunctuations(StreamReader reader, StreamWriter writer)
    {
        while (!reader.EndOfStream)
        {
            char a = (char)reader.Read();

            if (!Char.IsPunctuation(a))
                writer.Write(a);
        }
    }

    public static void TruncateText(StreamReader reader, StreamWriter writer, int minWordLength)
    {
        StringBuilder stringBuilder = new();
        int lettersNum = 0;
        while (!reader.EndOfStream)
        {
            char c = (char)reader.Read();
            stringBuilder.Append(c);

            if (Char.IsLetterOrDigit(c))
            {
                lettersNum++;
            }
            else if (Char.IsWhiteSpace(c))
            {

                if (lettersNum >= minWordLength)
                    writer.Write(stringBuilder.ToString());

                lettersNum = 0;
                stringBuilder.Clear();
            }
        }
    }
}
