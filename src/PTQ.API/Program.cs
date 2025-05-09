using PTQ.Repositories;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("UniversityConnection");

builder.Services.AddTransient<IQuizRepository, QuizRepository>(
    _ => new QuizRepository(connectionString));
builder.Services.AddControllers();
builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();


app.MapGet("/api/quizzes", (IQuizRepository quizRepository) =>
{
    try
    {
        var results = quizRepository.GetAllTests();
        return Results.Ok(results);
    }
    catch
    {
        return Results.BadRequest("Something went wrong");
    }
});

app.MapGet("/api/quizzes/{id}", (int id, IQuizRepository quizRepository) =>
{
    try
    {
        var results = quizRepository.GetSpecificTest(id);
        return Results.Ok(results);
    }
    catch
    {
        return Results.BadRequest("Something went wrong");
    }
});


app.Run();