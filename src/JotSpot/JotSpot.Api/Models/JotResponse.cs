namespace JotSpot.Api.Models;

public record JotResponse(Guid Id, string Title, string Text)
{
    public static JotResponse FromDomain(Jot jot)
    {
        return new JotResponse(jot.Id, jot.Title, jot.Text);
    }
}