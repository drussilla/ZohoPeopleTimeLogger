using System.Threading.Tasks;
using ZohoPeopleClient;
using ZohoPeopleClient.Exceptions;
using ZohoPeopleTimeLogger.Model;
using ZohoPeopleTimeLogger.Services;

namespace ZohoPeopleTimeLogger.Controllers
{
    public class LoginController : ILoginController
    {
        private readonly IDialogService dialogService;

        private readonly IZohoClient zohoClient;

        public LoginController(IZohoClient zohoClient, IDialogService dialogService)
        {
            this.zohoClient = zohoClient;
            this.dialogService = dialogService;
        }

        public async Task<AuthenticationData> LoginWithPassword()
        {
            var loginDetails = await dialogService.ShowLogin();

            // cancel was pressed
            if (loginDetails == null)
            {
                return null;
            }

            var progress = await dialogService.ShowProgress("Authentication", "Wait for response from server");
            progress.SetIndeterminate();

            var isError = false;
            var errorMessage = "";
            var token = "";
            try
            {
                token = await zohoClient.LoginAsync(loginDetails.Username, loginDetails.Password);
            }
            catch (ApiLoginErrorException exception)
            {
                isError = true;
                errorMessage = exception.Response;
            }

            await progress.CloseAsync();

            if (isError)
            {
                await dialogService.ShowMessageAsync("Authentication error", errorMessage);
                return null;
            }

            return new AuthenticationData {UserName = loginDetails.Username, Token = token};
        }

        public void LoginWithToken(string token)
        {
            zohoClient.Login(token);
        }
    }
}