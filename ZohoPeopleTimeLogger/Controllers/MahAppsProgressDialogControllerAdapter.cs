using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;

namespace ZohoPeopleTimeLogger.Controllers
{
    public class MahAppsProgressDialogControllerAdapter : IProgressDialogController
    {
        private readonly ProgressDialogController adaptee;

        public MahAppsProgressDialogControllerAdapter(ProgressDialogController adaptee)
        {
            this.adaptee = adaptee;
        }

        public void SetIndeterminate()
        {
            adaptee.SetIndeterminate();
        }

        public Task CloseAsync()
        {
            return adaptee.CloseAsync();
        }
    }
}