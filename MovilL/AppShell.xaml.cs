using Microsoft.Extensions.DependencyInjection;
using MovilL.views;

namespace MovilL
{
    public partial class AppShell : Shell
    {
        public AppShell(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            LoginShellContent.ContentTemplate = CreatePageTemplate<LoginPage>(serviceProvider);
            InicioShellContent.ContentTemplate = CreatePageTemplate<Inicio>(serviceProvider);
            MovimientoShellContent.ContentTemplate = CreatePageTemplate<NuevoMovimientoPage>(serviceProvider);
            ResumenShellContent.ContentTemplate = CreatePageTemplate<ResumenPage>(serviceProvider);

            Routing.RegisterRoute("Perfil", typeof(Perfil));
            Routing.RegisterRoute("Configuracion", typeof(Configuracion));
        }

        private static DataTemplate CreatePageTemplate<TPage>(IServiceProvider serviceProvider)
            where TPage : Page
        {
            return new DataTemplate(() => serviceProvider.GetRequiredService<TPage>());
        }
    }
}

