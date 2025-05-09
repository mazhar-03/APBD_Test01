namespace PTQ.Repositories;

public class QuizRepository:IQuizRepository
{
    private readonly string _connectionString;

    public QuizRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
}