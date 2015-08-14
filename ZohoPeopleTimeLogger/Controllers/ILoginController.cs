using System.Threading.Tasks;
using ZohoPeopleTimeLogger.Model;

namespace ZohoPeopleTimeLogger.Controllers
{
    public interface ILoginController
    {
        Task<AuthenticationData> LoginWithPassword();

        Task<bool> LoginWithToken(AuthenticationData authData);

        Task<string> GetEmployeeId(string username);
    }
}