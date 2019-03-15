using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoinbasePro.Application.Extend
{
    public static class ExtendTask
    {
        //https://stackoverflow.com/a/22864616/3744570
        public static async void Forget(this Task task, params Type[] acceptableExceptions)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // TODO: consider whether derived types are also acceptable.
                if (!acceptableExceptions.Contains(ex.GetType()))
                    throw;
            }
        }
    }
}
