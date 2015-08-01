using System.Threading.Tasks;

namespace ZohoPeopleTimeLogger.Controllers
{
    public interface IProgressDialogController
    {
        void SetIndeterminate();
        Task CloseAsync();
    }
}