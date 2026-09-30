using Microsoft.EntityFrameworkCore;

namespace TripSV.Datos
{
    public static class ExtensionesContexto
    {
        public static async Task<bool> GuardarSinConflictoAsync(this DbContext contexto)
        {
            try
            {
                await contexto.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                contexto.ChangeTracker.Clear();
                return false;
            }
        }
    }
}
