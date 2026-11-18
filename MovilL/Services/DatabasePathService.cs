namespace MovilL.Services;

public class DatabasePathService : IDatabasePathService
{
    public string GetDatabasePath()
    {
        return Path.Combine(FileSystem.AppDataDirectory, "MovilL.db3");
    }
}
