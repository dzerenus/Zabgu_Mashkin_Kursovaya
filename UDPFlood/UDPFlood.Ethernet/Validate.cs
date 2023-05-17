namespace UDPFlood.Ethernet;

public static class Validate
{
    const string MacAlphabet = "0123456789ABCDEF";

    public static bool IsValidMAC(string input)
    {
        var parts = input.Split(':');

        if (parts.Length != 6)
            return false;

        foreach (var part in parts)
        {
            if (part.Length != 2)
                return false;

            foreach (var c in part)
                if (!MacAlphabet.Contains(char.ToUpper(c)))
                    return false;
        }

        return true;
    }

    public static bool IsValidIP(string input)
    {
        var parts = input.Split(".");

        if (parts.Length != 4)
            return false;

        foreach (var part in parts)
        {
            if (!int.TryParse(part, out var octet))
                return false;

            if (octet < 0 || octet > 255) 
                return false;
        }

        return true;
    }

    public static bool IsValidPort(string input)
    {
        if (!int.TryParse(input, out var port))
            return false;

        if (port < 0 || port > 65535)
            return false;

        return true;
    }
}