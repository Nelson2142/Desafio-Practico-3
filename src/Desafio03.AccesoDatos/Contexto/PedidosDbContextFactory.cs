using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Desafio03.AccesoDatos.Contexto;

/// <summary>
/// Permite usar los comandos de migraciones (dotnet ef) sobre este proyecto
/// sin necesidad de arrancar la API.
/// La cadena se puede cambiar con la variable de entorno ConnectionStrings__ConexionSql.
/// </summary>
public class PedidosDbContextFactory : IDesignTimeDbContextFactory<PedidosDbContext>
{
    public PedidosDbContext CreateDbContext(string[] args)
    {
        var cadena = Environment.GetEnvironmentVariable("ConnectionStrings__ConexionSql")
                     ?? "Server=localhost,1433;Database=PedidosDB;User Id=sa;Password=Desafio03#2026;TrustServerCertificate=True";

        var opciones = new DbContextOptionsBuilder<PedidosDbContext>()
            .UseSqlServer(cadena)
            .Options;

        return new PedidosDbContext(opciones);
    }
}
