using System;
using System.Threading.Tasks;

namespace AppTool.Contracts.Models
{
    public interface IProcesser<T, S>
    {
        Task<S> ExecuteAsync(IProgress<T> progress);
    }
}