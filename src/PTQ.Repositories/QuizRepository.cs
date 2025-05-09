using Microsoft.Data.SqlClient;
using PTQ.Application;

namespace PTQ.Repositories;

public class QuizRepository : IQuizRepository
{
    private readonly string _connectionString;

    public QuizRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<AllTestsDto> GetAllTests()
    {
        var quizzes = new List<AllTestsDto>();
        var sql = "SELECT Id, Name FROM Quiz";

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand(sql, connection);
            var reader = command.ExecuteReader();
            try
            {
                if (reader.HasRows)
                    while (reader.Read())
                    {
                        var quiz = new AllTestsDto
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        };
                        quizzes.Add(quiz);
                    }
            }
            finally
            {
                reader.Close();
            }
        }

        return quizzes;
    }

    public SpecificQuizDto GetSpecificTest(int quizId)
    {
        var quiz = new SpecificQuizDto();
        var sql = @"SELECT q.Id, q.Name, pt.Name, q.PathFile 
                    FROM Quiz q JOIN PotatoTeacher pt ON q.PotatoTeacherId = pt.Id
                    WHERE q.Id = @quizId";

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@quizId", quizId);
            var reader = command.ExecuteReader();
            try
            {
                if (reader.HasRows)
                    while (reader.Read())
                        quiz = new SpecificQuizDto
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            PotatoTeacherName = reader.GetString(2),
                            Path = reader.GetString(3)
                        };
            }
            finally
            {
                reader.Close();
            }
        }
        return quiz;
    }

    public bool CreateTest(RequestBodyDto request)
    {
        const string selectTeacherSql = "SELECT Id FROM PotatoTeacher WHERE Name = @teacherName";
        const string insertTeacherSql = "INSERT INTO PotatoTeacher (Name) OUTPUT INSERTED.Id VALUES (@teacherName)";
        const string insertQuizSql = @"INSERT INTO Quiz (Name, PotatoTeacherId, PathFile)
                                   VALUES (@quizName, @teacherId, @pathFile)";

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            int teacherId;

            using (var selectCmd = new SqlCommand(selectTeacherSql, connection, transaction))
            {
                selectCmd.Parameters.AddWithValue("@teacherName", request.PotatoTeacherName);
                var result = selectCmd.ExecuteScalar();
                if (result != null)
                {
                    teacherId = (int)result;
                }
                else
                {
                    using (var insertCmd = new SqlCommand(insertTeacherSql, connection, transaction))
                    {
                        selectCmd.Parameters.AddWithValue("@teacherName", request.PotatoTeacherName);
                        teacherId = (int)insertCmd.ExecuteScalar();
                    }
                }
            }

            using (var insertQuizCmd = new SqlCommand(insertQuizSql, connection, transaction))
            {
                insertQuizCmd.Parameters.AddWithValue("@quizName", request.Name);
                insertQuizCmd.Parameters.AddWithValue("@teacherId", teacherId);
                insertQuizCmd.Parameters.AddWithValue("@pathFile", request.Path);
                insertQuizCmd.ExecuteNonQuery();
            }
            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }
}