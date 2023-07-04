using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Umbrella.Api.Contexts;
using Umbrella.Api.Entities;

namespace Umbrella.Api.Utils;

public class UniqueAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        Regex regex = new(@"(^\d{2}\.\d{3}\.\d{3}\/\d{4}\-\d{2}$)|(^\d{14})");

        using RepositoryContext db = new();

        if (!regex.IsMatch((value as string)!) || !TextUtil.IsCnpj((value as string)!))
        {
            return new ValidationResult($"{validationContext.MemberName} is not a valid CNPJ!");
        }

        Client? client = db.Clients.SingleOrDefault(client => client.Cnpj == TextUtil.UnformatCNPJ((string) value!));

        return client != null
                   ? new ValidationResult($"{validationContext.MemberName} must be unique! ")
                   : ValidationResult.Success;
    }
}