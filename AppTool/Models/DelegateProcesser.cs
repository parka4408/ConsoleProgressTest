using AppTool.Contracts.Models;
using System;
using System.Threading.Tasks;

namespace AppTool.Models;

public class DelegateProcesser<T, S> : IProcesser<T, S>
{
    private readonly Func<IProgress<T>, S> _func;

    public DelegateProcesser(Func<IProgress<T>, S> func)
    {
        _func = func;
    }

    public async Task<S> ExecuteAsync(IProgress<T> progress)
    {
        return await Task.Run(() =>
        {
            return _func.Invoke(progress);
        });
    }
}
