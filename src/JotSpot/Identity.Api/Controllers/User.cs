namespace Identity.Api.Controllers;

record User(
    int UserId,
    string Login,
    string FirstName,
    string LastName,
    string City);