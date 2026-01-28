using System.Text;

namespace FaunaFinder.Pagination.Contracts;

public static class CursorHelper
{
    public static string Encode(int id)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(id.ToString()));
    }

    public static int Decode(string cursor)
    {
        var bytes = Convert.FromBase64String(cursor);
        var idString = Encoding.UTF8.GetString(bytes);
        return int.Parse(idString);
    }

    public static bool TryDecode(string cursor, out int id)
    {
        try
        {
            id = Decode(cursor);
            return true;
        }
        catch
        {
            id = 0;
            return false;
        }
    }
}
