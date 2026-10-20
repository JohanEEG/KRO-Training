using Microsoft.EntityFrameworkCore;

namespace KROTraining.Models;

public class MembresiaEstadoViewModel
{
    // INEXISTENTE | VENCIDA | POR_VENCER | VIGENTE
    public string Estado { get; set; } = "INEXISTENTE";
    public string Mensaje { get; set; } = "";
    public string? Plan { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public int DiasRestantes { get; set; }
}

public static class MembresiaHelper
{
    public const int DiasAviso = 7;

    public static async Task<MembresiaEstadoViewModel> ObtenerEstadoAsync(KroTrainingContext context, int clienteId)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var ultima = await context.Membresia
            .Include(m => m.Plan)
            .Where(m => m.ClienteId == clienteId)
            .OrderByDescending(m => m.FechaFin)
            .FirstOrDefaultAsync();

        if (ultima == null)
        {
            return new MembresiaEstadoViewModel
            {
                Estado = "INEXISTENTE",
                Mensaje = "Aún no tienes una membresía registrada."
            };
        }

        var vm = new MembresiaEstadoViewModel
        {
            Plan = ultima.Plan?.Nombre,
            FechaInicio = ultima.FechaInicio,
            FechaFin = ultima.FechaFin,
            DiasRestantes = ultima.FechaFin.DayNumber - hoy.DayNumber
        };

        if (ultima.Estado != "ACTIVA" || ultima.FechaFin < hoy)
        {
            vm.Estado = "VENCIDA";
            vm.Mensaje = $"Tu membresía venció el {ultima.FechaFin:dd/MM/yyyy}.";
        }
        else if (vm.DiasRestantes <= DiasAviso)
        {
            vm.Estado = "POR_VENCER";
            vm.Mensaje = vm.DiasRestantes == 0
                ? "Tu membresía vence hoy."
                : $"Tu membresía vence en {vm.DiasRestantes} día(s), el {ultima.FechaFin:dd/MM/yyyy}.";
        }
        else
        {
            vm.Estado = "VIGENTE";
            vm.Mensaje = $"Tu membresía está vigente hasta el {ultima.FechaFin:dd/MM/yyyy}.";
        }

        return vm;
    }

    public static async Task RegistrarNotificacionAsync(KroTrainingContext context, int usuarioId, MembresiaEstadoViewModel estado)
    {
        string tipo = estado.Estado == "VENCIDA" ? "MEMBRESIA_VENCIDA" : "MEMBRESIA_POR_VENCER";

        bool yaExiste = await context.Notificacions
            .AnyAsync(n => n.UsuarioId == usuarioId && n.Tipo == tipo && n.Fecha >= DateTime.Today);

        if (yaExiste) return;

        context.Notificacions.Add(new Notificacion
        {
            UsuarioId = usuarioId,
            Tipo = tipo,
            Mensaje = estado.Mensaje,
            Fecha = DateTime.Now,
            Estado = "NO_LEIDA"
        });
        await context.SaveChangesAsync();
    }
}