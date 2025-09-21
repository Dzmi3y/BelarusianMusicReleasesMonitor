using BMRM.Desktop;
using BMRM.Desktop.Views;
using Prism.Ioc;
using Prism.Modularity;

public class DesktopModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<NewReleasesView>();
    }
}


