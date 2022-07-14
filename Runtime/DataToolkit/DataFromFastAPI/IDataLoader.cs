
using System.Threading.Tasks;
using UnityEngine;

namespace Cameo
{
    public abstract class IDataLoader<T> : MonoBehaviour where T:class
    {
        public virtual async Task<T> Load()
        {
            await Task.Yield();
            return null;
        }

        public virtual async Task<T> LoadWithParams(params object[] loadParams)
        {
            await Task.Yield();
            return null;
        }
    }
}
