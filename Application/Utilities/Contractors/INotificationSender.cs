using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Contractors
{
    public interface INotificationSender
    {
        Task SendAsync(string to, string title, string body, Dictionary<string, string> Data=null);
    }
}
