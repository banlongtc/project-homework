using Microsoft.AspNetCore.SignalR;

namespace MPLUS_GW_WebCore
{
    public class MaterialTaskHub : Hub
    {
        public async Task SendInputValue(string inputClass, string inputValue, int parentValue)
        {
            await Clients.All.SendAsync("ReceiveInputValue", inputClass, inputValue, parentValue);
        }
    }
}
