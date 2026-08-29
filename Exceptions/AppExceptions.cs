namespace Magazin_cosmetice_COSMETICO.Exceptions;

/// <summary>Clasa de baza pentru erorile "asteptate" ale aplicatiei.</summary>
public abstract class AppException : Exception
{
    public abstract int StatusCode { get; }
    protected AppException(string message) : base(message) { }
}

/// <summary>Resursa nu exista -> 404.</summary>
public class NotFoundException : AppException
{
    public override int StatusCode => StatusCodes.Status404NotFound;

    public NotFoundException(string resource, object key)
        : base($"{resource} cu identificatorul '{key}' nu a fost gasit.") { }

    public NotFoundException(string message) : base(message) { }
}

/// <summary>Regula de business incalcata (stoc insuficient etc.) -> 400.</summary>
public class BusinessRuleException : AppException
{
    public override int StatusCode => StatusCodes.Status400BadRequest;
    public BusinessRuleException(string message) : base(message) { }
}

/// <summary>Utilizator autentificat, dar nu are voie pe resursa asta -> 403.</summary>
public class ForbiddenException : AppException
{
    public override int StatusCode => StatusCodes.Status403Forbidden;
    public ForbiddenException(string message) : base(message) { }
}

