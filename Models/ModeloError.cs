namespace ControlViveresApp.Models;

public class ModeloError
{
    public string? IdSolicitud { get; set; }

    public bool MostrarIdSolicitud => !string.IsNullOrEmpty(IdSolicitud);
}
