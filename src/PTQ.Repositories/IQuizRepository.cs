using PTQ.Application;

namespace PTQ.Repositories;

public interface IQuizRepository
{
    IEnumerable<AllTestsDto> GetAllTests();
    SpecificQuizDto GetSpecificTest(int id);
    bool CreateTest(RequestBodyDto request);
}