using AmisAdminDotNet.Models;

namespace AmisAdminDotNet.Services;

public interface IUserStore
{
    PagedResult<UserRecord> Query(string? keywords, int page, int perPage);
    UserRecord Create(SaveUserRequest request);
    UserRecord? Update(int id, SaveUserRequest request);
    bool Delete(int id);
}
