using System;
using DryIoc;
using Hangfire;

namespace BMRM.Desktop.Utils;

public class DryIocJobActivator : JobActivator
{
    private readonly IContainer _container;

    public DryIocJobActivator(IContainer container)
    {
        _container = container;
    }

    public override object ActivateJob(Type jobType)
    {
        return _container.Resolve(jobType);
    }
}
