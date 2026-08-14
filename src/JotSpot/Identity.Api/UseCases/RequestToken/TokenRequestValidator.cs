namespace Identity.Api.UseCases.RequestToken;

using FluentValidation;

internal class TokenRequestValidator : AbstractValidator<TokenRequest>
{
    public TokenRequestValidator()
    {
        RuleFor(x => x.Login).NotEmpty().Length(4, 20).Matches("^[a-zA-Z0-9]*$");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(7);
    }
}