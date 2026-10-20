namespace InventarioTienda;

/// <summary>
/// Punto de entrada: crea y muestra la ventana principal.
/// </summary>
static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FormPrincipal());
    }
}
