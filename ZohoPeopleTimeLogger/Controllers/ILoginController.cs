using System.Threading.Tasks;
using ZohoPeopleTimeLogger.Model;

namespace ZohoPeopleTimeLogger.Controllers
{
    public interface ILoginController
    {
        Task<AuthenticationData> LoginWithPassword();

        void LoginWithToken(string token);
    }
}