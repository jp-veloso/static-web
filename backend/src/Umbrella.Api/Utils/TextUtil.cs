using System.Text.RegularExpressions;

namespace Umbrella.Api.Utils;

public static class TextUtil
{
    public static bool IsCnpj(string cnpj)
    {
        int[] multiply1 = {5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2};
        int[] multiply2 = {6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2};

        int sum = 0;

        cnpj = cnpj.Trim();
        cnpj = UnformatCNPJ(cnpj);

        if (cnpj.Length != 14)
        {
            return false;
        }

        string tempCnpj = cnpj[..12];

        for (int i = 0; i < 12; i++)
        {
            sum += int.Parse(tempCnpj[i]
                                .ToString()) * multiply1[i];
        }

        int rest = sum % 11;

        if (rest < 2)
        {
            rest = 0;
        }
        else
        {
            rest = 11 - rest;
        }

        string digit = rest.ToString();

        tempCnpj += digit;

        sum = 0;
        for (int i = 0; i < 13; i++)
        {
            sum += int.Parse(tempCnpj[i]
                                .ToString()) * multiply2[i];
        }

        rest = sum % 11;

        if (rest < 2)
        {
            rest = 0;
        }
        else
        {
            rest = 11 - rest;
        }

        digit += rest.ToString();

        return cnpj.EndsWith(digit);
    }

    public static string UnformatCNPJ(string value)
    {
        return value.Replace(".", string.Empty)
                    .Replace("/", string.Empty)
                    .Replace("-", string.Empty);
    }

    public static string FormatCNPJ(string cnpj)
    {
        return Regex.Replace(cnpj, @"(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})", "$1.$2.$3/$4-$5");
    }
}