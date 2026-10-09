using MovilL.views;

namespace MovilL
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("Perfil", typeof(Perfil));
            Routing.RegisterRoute("Configuracion", typeof(Configuracion));
        }
    }
}

