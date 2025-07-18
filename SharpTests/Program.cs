var options = new Options
{
    Iterations = 50,
    Chunks = 
    [
        new(Percentual: 0, 10),
        new(Percentual: 10, 20),
        new(Percentual: 20, 30),
        new(Percentual: 30, 40),
        new(Percentual: 40, 50),
        new(Percentual: 50, 60),
        new(Percentual: 60, 70),
        new(Percentual: 70, 80),
        new(Percentual: 80, 90),
        new(Percentual: 90, 100)
    ]
};
await Test(options, async http =>
{
    return await http.Get("https://quickpizza.grafana.com/");
});

// await Test(async http =>
// {
//     var response = await http.Get("https://quickpizza.grafana.com/");
//     Check(response, new()
//     {
//         ["response code was 200"] = r => r.StatusCode == 201,
//         ["response content type is text/html"] = r => r.Header.ContentType == "text/html",
//         ["response content is not empty"] = r => !string.IsNullOrEmpty(response.Body.Content),
//         ["response reason phrase is OK"] = r => r.Header.ReasonPhrase == "OK",
//         ["body size was larger than 123 bytes"] = r => r.Body.Length > 123,
//     });
//     return response;
// });

// await Test(async (http) =>
// {
//     var payload = new
//     {
//         username = "default",
//         password = "12345678",
//     };
//     var headers = new[]
//     {
//         "Content-Type", "application/json"
//     };
//     var response = await http.Post("https://quickpizza.grafana.com/api/users/token/login",headers, payload);
//     Check(response, new()
//     {
//         ["response code was 200"] = r => r.StatusCode == 200
//     });
//     return response;
// });

// await Test(async http =>
// {
//     var username = "user";
//     var password = "password";
//     var credentials = Base64(username, password);
//     var headers = new[]
//     {
//         "Authorization", $"Basic {credentials}"
//     };
//     var response = await http.Get($"https://quickpizza.grafana.com/api/basic-auth/{username}/{password}", headers);
//
//     Check(response, new()
//     {
//         ["status is 200"] = r => r.StatusCode == 200,
//         ["is authenticated"] = r => r.Body.Payload.authenticated == true,
//         ["is correct user"] = r => r.Body.Payload.user == username
//     });
//
//     return response;
// });